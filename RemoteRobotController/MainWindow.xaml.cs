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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using agsXMPP;
using SKYPE4COMLib;

namespace RemoteRobotController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Window controlsWindow;

        System.Threading.Timer time;

        private Dispatcher _dispatcher;

        public MainWindow()
        {
            InitializeComponent();

            _dispatcher = this.Dispatcher;

            // Update after 1/4 second, and every 1/4 second thereafter.
            TimerCallback tcb = this.Update;
            AutoResetEvent ar = new AutoResetEvent(true);
            time = new System.Threading.Timer(tcb, ar, 250, 250);
        }

        private void Update(Object stateInfo)
        {

        }

        private void generalControlsButton_Click(object sender, MouseButtonEventArgs e)
        {
            controlsWindow = new GeneralControlsWindow();
            controlsWindow.Show();
        }

        private void xboxGamepadButton_Click(object sender, MouseButtonEventArgs e)
        {
            controlsWindow = new XboxGamepadWindow();
            controlsWindow.Show();
        }

        private void steeringWheelButton_Click(object sender, MouseButtonEventArgs e)
        {
            controlsWindow = new SteeringWheelWindow();
            controlsWindow.Show();
        }
    }
}
