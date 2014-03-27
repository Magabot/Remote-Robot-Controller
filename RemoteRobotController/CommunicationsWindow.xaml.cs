using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using agsXMPP;
using SKYPE4COMLib;

namespace RemoteRobotController
{
    /// <summary>
    /// Interaction logic for CommunicationsWindow.xaml
    /// </summary>
    public partial class CommunicationsWindow : Window
    {
        GeneralControlsWindow controlsWindow;

        Skype skype;
        string selectedSkypeChatUser;
        int skypeChats = 0;
        int skypeCalls = 0;

        XmppClientConnection googleChat;

        System.Threading.Timer time;

        Dispatcher _dispatcher;

        String communication;

        public int bumpTime;
        public int holeTime;

        public CommunicationsWindow()
        {
            InitializeComponent();

            _dispatcher = this.Dispatcher;

            Properties.Settings.Default.Reload();
            passwordBoxGoogleChat.Password = Properties.Settings.Default.googleChatPassword;
            textBoxGoogleChatUsername.Text = Properties.Settings.Default.googleChatEmail;

            // Update after 1/4 second, and every 1/4 second thereafter.
            TimerCallback tcb = this.Update;
            AutoResetEvent ar = new AutoResetEvent(true);
            time = new System.Threading.Timer(tcb, ar, 250, 250);

            bumpTime = 20;
            holeTime = 20;
        }

        private void Update(Object stateInfo)
        {
            if (communication == "Skype")
            {
                _dispatcher.BeginInvoke((Action)(() =>
                {
                    UpdateSkype(stateInfo);
                }));
            }
            else if (communication == "Google Chat")
            {

            }
        }

        public void SendMessage(String body)
        {
            _dispatcher.BeginInvoke((Action)(() =>
            {
                if (communication == "Skype")
                {
                    if (comboBoxSelectedUser.SelectedItem != null && comboBoxSelectedUser.SelectedItem.ToString() != "")
                        skype.SendMessage(comboBoxSelectedUser.SelectedItem.ToString(), body);

                    listbox.Items.Add(body.First());
                }
                else if (communication == "Google Chat")
                {
                    if (comboBoxSelectedUser.SelectedItem != null && comboBoxSelectedUser.SelectedItem.ToString() != "")
                    {
                        agsXMPP.protocol.client.Message message = new agsXMPP.protocol.client.Message();
                        message.Type = agsXMPP.protocol.client.MessageType.chat;
                        message.To = new Jid(comboBoxSelectedUser.SelectedItem.ToString());
                        message.Body = body;
                        googleChat.Send(message);

                        listbox.Items.Add(body.First());
                    }
                }
                else
                {
                    listbox.Items.Add("Failed to send: " + body.First());
                }
            }));
        }

        private void generalControlsButton_Click(object sender, RoutedEventArgs e)
        {
            controlsWindow = new GeneralControlsWindow();
            controlsWindow.Show();
        }

        #region Skype
        private void skypeButton_Click(object sender, RoutedEventArgs e)
        {
            communication = "Skype";

            _dispatcher.BeginInvoke((Action)(() =>
            {
                listbox.Items.Add("Connecting to skype...");
            }));

            skype = new Skype();
            skype.MessageStatus += new _ISkypeEvents_MessageStatusEventHandler(skype_MessageStatus);

            _dispatcher.BeginInvoke((Action)(() =>
            {
                listbox.Items.Add("Connected to " + skype.CurrentUserHandle + " account.");
            }));

            googleChatButton.IsEnabled = false;
        }

        private void UpdateSkype(Object stateInfo)
        {
            try
            {
                skypeChats = skype.ActiveChats.Count;
                skypeCalls = skype.ActiveCalls.Count;
            }
            catch (InvalidCastException e) { }

            try
            {
                if (skype.ActiveCalls.Count > 0)
                {
                    foreach (Call call in skype.ActiveCalls)
                    {
                        if (call.Status == TCallStatus.clsInProgress)
                        {
                            _dispatcher.BeginInvoke((Action)(() =>
                            {
                                if (!comboBoxSelectedUser.Items.Cast<string>().Contains(call.PartnerHandle.ToString())) // New chat
                                {
                                    comboBoxSelectedUser.Items.Add(call.PartnerHandle.ToString());
                                    comboBoxSelectedUser.SelectedItem = call.PartnerHandle.ToString();
                                }
                            }));
                            selectedSkypeChatUser = call.PartnerHandle.ToString();
                        }
                    }
                }
            }
            catch (InvalidCastException e)
            {
            }
        }

        public void skype_MessageStatus(ChatMessage msg, TChatMessageStatus Status)
        {
            if (Status == TChatMessageStatus.cmsReceived)
            {
                _dispatcher.BeginInvoke((Action)(() =>
                {
                    listbox.Items.Add(String.Format("Message from {0}: {1}", msg.FromHandle, msg.Body));
                    listbox.SelectedIndex = listbox.Items.Count - 1;

                    if (msg.Sender.Handle == selectedSkypeChatUser)
                    {
                        if (msg.Body == "b" || msg.Body == Properties.Settings.Default.bumperMessage)
                        {
                            bumpTime = 0;
                        }
                        else if (msg.Body == "h" || msg.Body == Properties.Settings.Default.holeMessage)
                        {
                            holeTime = 0;
                        }
                    }

                    if (comboBoxSelectedUser.SelectedItem == null || comboBoxSelectedUser.SelectedItem.ToString() == "") // First chat
                        comboBoxSelectedUser.SelectedItem = msg.FromHandle;
                }));
            }
        }
        #endregion

        #region Google Chat
        private void googleChatButton_Click(object sender, RoutedEventArgs e)
        {
            communication = "Google Chat";

            if (expanderGoogleChatSignIn.IsExpanded)
                expanderGoogleChatSignIn.IsExpanded = false;
            else
                expanderGoogleChatSignIn.IsExpanded = true;
        }

        private void buttonGoogleChatSignIn_Click(object sender, RoutedEventArgs e)
        {
            googleChat = new XmppClientConnection();
            // Subscribe to Events
            googleChat.OnLogin += new ObjectHandler(googleChat_OnLogin);
            googleChat.OnRosterStart += new ObjectHandler(googleChat_OnRosterStart);
            googleChat.OnRosterEnd += new ObjectHandler(googleChat_OnRosterEnd);
            googleChat.OnRosterItem += new XmppClientConnection.RosterHandler(googleChat_OnRosterItem);
            googleChat.OnPresence += new agsXMPP.protocol.client.PresenceHandler(googleChat_OnPresence);
            googleChat.OnAuthError += new XmppElementHandler(googleChat_OnAuthError);
            googleChat.OnError += new ErrorHandler(googleChat_OnError);
            googleChat.OnClose += new ObjectHandler(googleChat_OnClose);
            googleChat.OnMessage += new agsXMPP.protocol.client.MessageHandler(googleChat_OnMessage);

            String googleChatEmail = "";

            if (textBoxGoogleChatUsername.Text.Contains('@'))
            {
                if (textBoxGoogleChatUsername.Text.Contains("@gmail.com"))
                {
                    googleChatEmail = textBoxGoogleChatUsername.Text;
                }
                else
                {
                    MessageBox.Show(
                    @"This only works with gmail accounts.
Sorry!",
                    "Not supported e-mail",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                    return;
                }
            }
            else
            {
                if (textBoxGoogleChatUsername.Text != "")
                {
                    googleChatEmail = textBoxGoogleChatUsername.Text += "@gmail.com";
                }
                else
                {
                    MessageBox.Show(
                    @"Please fill the textboxes before.",
                    "Not supported e-mail",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                    return;
                }
            }

            Jid googleChatUser = new Jid(googleChatEmail);

            googleChat.Username = googleChatUser.User;

            googleChat.Server = googleChatUser.Server;

            googleChat.Password = passwordBoxGoogleChat.Password;
            googleChat.AutoResolveConnectServer = true;

            googleChat.Open();


            if (checkBoxGoogleChatRememberMe.IsChecked == true)
            {
                Properties.Settings.Default.googleChatPassword = passwordBoxGoogleChat.Password;
                Properties.Settings.Default.googleChatEmail = textBoxGoogleChatUsername.Text;

                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.googleChatPassword = "";
                Properties.Settings.Default.googleChatEmail = "";

                Properties.Settings.Default.Save();
            }

            skypeButton.IsEnabled = false;
            expanderGoogleChatSignIn.IsExpanded = false;

            textBoxGoogleChatUsername.IsEnabled = false;
            passwordBoxGoogleChat.IsEnabled = false;

            buttonGoogleChatSignIn.IsEnabled = false;
            checkBoxGoogleChatRememberMe.IsEnabled = false;
            buttonGoogleChatSignOut.IsEnabled = true;

            expanderGoogleChatSignIn.IsExpanded = false;
        }

        private void buttonGoogleChatSignOut_Click(object sender, RoutedEventArgs e)
        {
            skypeButton.IsEnabled = true;

            // close the xmpp connection
            googleChat.Close();

            textBoxGoogleChatUsername.IsEnabled = true;
            passwordBoxGoogleChat.IsEnabled = true;

            buttonGoogleChatSignIn.IsEnabled = true;
            checkBoxGoogleChatRememberMe.IsEnabled = true;
            buttonGoogleChatSignOut.IsEnabled = false;

            comboBoxSelectedUser.Items.Clear();
        }

        private void googleChat_OnLogin(object sender)
        {
            _dispatcher.BeginInvoke((Action)(() =>
            {
                listbox.Items.Add("OnLogin");
                listbox.SelectedIndex = listbox.Items.Count - 1;
            }));
        }

        private void googleChat_OnRosterStart(object sender)
        {
            _dispatcher.BeginInvoke((Action)(() =>
            {
                listbox.Items.Add("Connecting to Google Chat...");
                listbox.SelectedIndex = listbox.Items.Count - 1;
            }));
        }

        private void googleChat_OnRosterEnd(object sender)
        {
            _dispatcher.BeginInvoke((Action)(() =>
            {
                listbox.Items.Add("Connected with " + googleChat.MyJID + " account.");
                listbox.SelectedIndex = listbox.Items.Count - 1;
            }));

            // Send our own presence to teh server, so other epople send us online
            // and the server sends us the presences of our contacts when they are
            // available
            googleChat.SendMyPresence();
        }

        private void googleChat_OnClose(object sender)
        {
            _dispatcher.BeginInvoke((Action)(() =>
            {
                listbox.Items.Add("Google Chat connection closed");
                listbox.SelectedIndex = listbox.Items.Count - 1;
            }));
        }

        private void googleChat_OnError(object sender, Exception ex)
        {
            _dispatcher.BeginInvoke((Action)(() =>
            {
                listbox.Items.Add("Google Chat Error!");
                listbox.SelectedIndex = listbox.Items.Count - 1;
            }));
        }

        private void googleChat_OnAuthError(object sender, agsXMPP.Xml.Dom.Element e)
        {
            _dispatcher.BeginInvoke((Action)(() =>
            {
                listbox.Items.Add("Google Chat Auth Error!");
                listbox.SelectedIndex = listbox.Items.Count - 1;
            }));
        }

        private void googleChat_OnRosterItem(object sender, agsXMPP.protocol.iq.roster.RosterItem item)
        {
            _dispatcher.BeginInvoke((Action)(() =>
            {
                //listbox.Items.Add(String.Format("Received Contact {0}", item.Jid.Bare));
                listbox.SelectedIndex = listbox.Items.Count - 1;
            }));

        }

        private void googleChat_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        {
            // ignore empty messages (events)
            if (msg.Body == null || msg.From.Bare == "")
                return;

            _dispatcher.BeginInvoke((Action)(() =>
            {
                listbox.Items.Add(String.Format("Message from {0}: {1}", msg.From.User, msg.Body));
                listbox.SelectedIndex = listbox.Items.Count - 1;


                if (msg.From.ToString() == comboBoxSelectedUser.SelectedItem.ToString())
                {
                    if (msg.Body == "b" || msg.Body == Properties.Settings.Default.bumperMessage)
                    {
                        bumpTime = 0;
                    }
                    else if (msg.Body == "h" || msg.Body == Properties.Settings.Default.holeMessage)
                    {
                        holeTime = 0;
                    }
                }
            }));

            _dispatcher.BeginInvoke((Action)(() =>
            {
                if (comboBoxSelectedUser.SelectedItem == null) // First chat
                    comboBoxSelectedUser.SelectedItem = msg.From.ToString();
            }));
        }

        private void googleChat_OnPresence(object sender, agsXMPP.protocol.client.Presence pres)
        {
            _dispatcher.BeginInvoke((Action)(() =>
            {
                if (!comboBoxSelectedUser.Items.Cast<string>().Contains(pres.From.ToString())) // New chat
                {
                    comboBoxSelectedUser.Items.Add(pres.From.ToString());
                }

                //listbox.Items.Add(String.Format("Received Presence from:{0}", pres.From.Bare));
            }));
        }
        #endregion
    }
}
