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
using System.IO.Ports;
using System.Diagnostics;

namespace RemoteRobotController
{
    /// <summary>
    /// Interaction logic for SteeringWheelWindow.xaml
    /// </summary>
    public partial class SteeringWheelWindow : Window
    {
        private Dispatcher _dispatcher;

        System.Threading.Timer time;

        char lastDirectionSet;
        String lastDirectionSetter;

        public char direction;

        CommunicationsWindow communicationsWindow = new CommunicationsWindow();

        private System.IO.Ports.SerialPort serialPort;

        public SteeringWheelWindow()
        {
            InitializeComponent();

            _dispatcher = this.Dispatcher;

            TimerCallback tcb = this.Update;
            AutoResetEvent ar = new AutoResetEvent(true);
            time = new System.Threading.Timer(tcb, ar, 250, 250);

            SetDirection('p', "init");

            communicationsWindow.Show();

            //availabe COM ports
            SerialPort tmp;
            foreach (string str in SerialPort.GetPortNames())
            {
                tmp = new SerialPort(str);
                if (tmp.IsOpen == false)
                    comboBoxSerialPort.Items.Add(str);
            }

            serialPort = new SerialPort();
        }

        private void Update(Object stateInfo)
        {
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


        #region Serial Port
        private void comboBoxSerialPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _dispatcher.BeginInvoke((Action)(() =>
            {
                serialPort.PortName = comboBoxSerialPort.SelectedItem.ToString();

                //open serial port
                serialPort.Open();
                comboBoxSerialPort.IsEnabled = false;
                buttonCloseSerialPort.IsEnabled = true;
                buttonFindSerialPort.IsEnabled = false;
                buttonOpenSerialPort.IsEnabled = false;

                serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);

                expanderControls.IsExpanded = true;
                expanderSerialPort.IsExpanded = false;
            }));
        }

        private void buttonFindSerialPort_Click(object sender, RoutedEventArgs e)
        {
            //availabe COM ports
            SerialPort tmp;
            foreach (string str in SerialPort.GetPortNames())
            {
                tmp = new SerialPort(str);

                int sameItemNumber = 0;
                int i = 0;
                while (i < comboBoxSerialPort.Items.Count)
                {
                    if (str == comboBoxSerialPort.Items.GetItemAt(i).ToString())
                        sameItemNumber++;

                    i++;
                }

                if (sameItemNumber == 0)
                {
                    if (tmp.IsOpen == false)
                        comboBoxSerialPort.Items.Add(str);
                }
            }
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // blocks until TERM_CHAR is received
            //String msg = serialPort.ReadExisting();
            String msg = "";
            msg += serialPort.ReadExisting();

            if (msg == "\n" || msg == "\r\n")
                return;

           _dispatcher.BeginInvoke((Action)(() =>
            {
                communicationsWindow.SendMessage(msg);

                listbox.Items.Add(msg.First());
            }));
        }

        private void buttonCloseSerialPort_Click(object sender, RoutedEventArgs e)
        {
            serialPort.Close();

            _dispatcher.BeginInvoke((Action)(() =>
            {
                serialPort.Close();

                buttonCloseSerialPort.IsEnabled = false;
                buttonFindSerialPort.IsEnabled = true;
                buttonOpenSerialPort.IsEnabled = true;
                comboBoxSerialPort.IsEnabled = true;
                expanderControls.IsExpanded = false;
            }));


        }

        private void buttonOpenSerialPort_Click(object sender, RoutedEventArgs e)
        {
            _dispatcher.BeginInvoke((Action)(() =>
            {
                serialPort.PortName = comboBoxSerialPort.SelectedItem.ToString();

                //open serial port
                serialPort.Open();

                comboBoxSerialPort.IsEnabled = false;
                buttonCloseSerialPort.IsEnabled = true;
                buttonOpenSerialPort.IsEnabled = false;
                buttonFindSerialPort.IsEnabled = false;

                serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);

                expanderControls.IsExpanded = true;
                expanderSerialPort.IsExpanded = false;
            }));
        }
        #endregion
    }
}
