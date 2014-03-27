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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RemoteRobotController
{
    /// <summary>
    /// Interaction logic for XboxGamepadWindow.xaml
    /// </summary>
    public partial class XboxGamepadWindow : Window
    {
        private Dispatcher _dispatcher;

        System.Threading.Timer time;

        char lastDirectionSet;
        String lastDirectionSetter;

        public char direction;

        CommunicationsWindow communicationsWindow = new CommunicationsWindow();

        char lastSteering = 'g';
        char currentDirection = 'g';
        char currentSteering = 'g';
        char lastDirection = 'g';


        SolidColorBrush transparentBrush = new SolidColorBrush(Colors.Transparent);
        SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);


        public XboxGamepadWindow()
        {
            InitializeComponent();

            _dispatcher = this.Dispatcher;

            TimerCallback tcb = this.Update;
            AutoResetEvent ar = new AutoResetEvent(true);
            time = new System.Threading.Timer(tcb, ar, 250, 250);

            SetDirection('p', "init");

            communicationsWindow.Show();

            communicationsWindow.bumpTime = 10;
            communicationsWindow.holeTime = 10;

        }

        private void Update(Object stateInfo)
        {
            _dispatcher.BeginInvoke((Action)(() =>
            {
                if (communicationsWindow.bumpTime > 10)
                {
                    imageBumpAlert.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    imageBumpAlert.Visibility = System.Windows.Visibility.Visible;
                }


                if (communicationsWindow.holeTime > 10)
                {
                    imageHoleAlert.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    imageHoleAlert.Visibility = System.Windows.Visibility.Visible;
                }
            }));

            try
            {
                // Get the game pad state.
                GamePadState currentState = GamePad.GetState(PlayerIndex.One);

                if (currentState.IsConnected)
                {
                    // Allows the game to exit
                    //if (currentState.Buttons.Back == ButtonState.Pressed)
                    //    this.Close();

                    char direction = 'p';
                    char steering = 'p';

                    String msgThumbstick = "";
                    String msgButtons = "";

                    float forwardTrigger = currentState.Triggers.Right;
                    float backwardTrigger = currentState.Triggers.Left;

                    if (forwardTrigger > 0.5 && backwardTrigger < 0.5)
                        direction = 'w';
                    else if (forwardTrigger < 0.5 && backwardTrigger < 0.5)
                        direction = 'p';
                    else if (forwardTrigger < 0.5 && backwardTrigger > 0.5)
                        direction = 's';

                    // Rotate the model using the left thumbstick, and scale it down
                    float xValue = currentState.ThumbSticks.Left.X * 0.10f;
                    float yValue = currentState.ThumbSticks.Left.Y * 0.10f;

                    if (xValue < -0.09)
                        msgThumbstick = "a";
                    else if (xValue > 0.09)
                        msgThumbstick = "d";
                    else
                        msgThumbstick = direction.ToString();

                    if (currentState.DPad.Left == ButtonState.Pressed)
                        msgButtons = "a";
                    else if (currentState.DPad.Right == ButtonState.Pressed)
                        msgButtons = "d";
                    else
                        msgButtons = direction.ToString();


                    if (msgThumbstick == "p" && msgButtons == "p" && lastDirectionSetter == "controller")
                    {
                        SetDirection('p', "controller");
                    }
                    else if (msgThumbstick == "a" || msgButtons == "a")
                    {
                        SetDirection('a', "controller");
                    }
                    else if (msgThumbstick == "d" || msgButtons == "d")
                    {
                        SetDirection('d', "controller");
                    }
                    else if (msgThumbstick == "w" || msgButtons == "w")
                    {
                        SetDirection('w', "controller");
                    }
                    else if (msgThumbstick == "s" || msgButtons == "s")
                    {
                        SetDirection('s', "controller");
                    }


                    //Tilt control
                    if ((currentState.ThumbSticks.Right.Y * 0.10f) < -0.09)
                        communicationsWindow.SendMessage("j");
                    else if ((currentState.ThumbSticks.Right.Y * 0.10f) > 0.09)
                        communicationsWindow.SendMessage("u"); 
                    else if ((currentState.ThumbSticks.Right.X * 0.10f) < -0.09)
                        communicationsWindow.SendMessage("h");
                    else if ((currentState.ThumbSticks.Right.X * 0.10f) > 0.09)
                        communicationsWindow.SendMessage("k");

                    //Speed control
                    if (currentState.Buttons.LeftShoulder == ButtonState.Pressed)
                        communicationsWindow.SendMessage("-");
                    else if (currentState.Buttons.RightShoulder == ButtonState.Pressed)
                        communicationsWindow.SendMessage("+");

                    if(currentState.Buttons.RightStick == ButtonState.Pressed)
                        communicationsWindow.SendMessage("f");

                    if (currentState.Buttons.B == ButtonState.Pressed)
                        communicationsWindow.SendMessage("b");




                    if (communicationsWindow.bumpTime < 3 || communicationsWindow.holeTime < 3)
                    {
                        GamePad.SetVibration(PlayerIndex.One,
                                40,
                                40);
                    }
                    else
                    {
                        GamePad.SetVibration(PlayerIndex.One,
                                0,
                                0);
                    }
                }
            }
            catch
            {

            }

            communicationsWindow.bumpTime++;
            communicationsWindow.holeTime++;
        }

        public void SetDirection(char newDirection, String setter)
        {
            direction = newDirection;

            _dispatcher.BeginInvoke((Action)(() =>
            {
                String msg = "";
                msg += direction;

                if (msg != "" && direction != lastDirectionSet)
                    communicationsWindow.SendMessage(msg);

                lastDirectionSet = direction;
                lastDirectionSetter = setter;
            }));
        }

        #region Window
        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        private void maximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
            {
                this.WindowState = WindowState.Maximized;
                MainCanvas.Background = transparentBrush;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                MainCanvas.Background = whiteBrush;
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
