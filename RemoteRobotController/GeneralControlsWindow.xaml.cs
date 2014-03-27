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

namespace RemoteRobotController
{
    /// <summary>
    /// Interaction logic for GeneralControlsWindow.xaml
    /// </summary>
    public partial class GeneralControlsWindow : Window
    {
        private Dispatcher _dispatcher;

        System.Threading.Timer time;

        char lastDirectionSet;
        String lastDirectionSetter;

        public char direction;

        CommunicationsWindow communicationsWindow = new CommunicationsWindow();

        SolidColorBrush transparentBrush = new SolidColorBrush(Colors.Transparent);
        SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);

        public GeneralControlsWindow()
        {
            InitializeComponent();

            _dispatcher = this.Dispatcher;

            TimerCallback tcb = this.Update;
            AutoResetEvent ar = new AutoResetEvent(true);
            time = new System.Threading.Timer(tcb, ar, 250, 250);

            SetDirection('p', "init");

            communicationsWindow.Show();
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

                if (direction == 'a')
                {
                    buttonForward.Opacity = 0.5;
                    buttonBackward.Opacity = 0.5;
                    buttonLeft.Opacity = 1;
                    buttonRight.Opacity = 0.5;
                    buttonStop.Opacity = 0.5;
                }
                if (direction == 'd')
                {
                    buttonForward.Opacity = 0.5;
                    buttonBackward.Opacity = 0.5;
                    buttonLeft.Opacity = 0.5;
                    buttonRight.Opacity = 1;
                    buttonStop.Opacity = 0.5;
                }
                if (direction == 'w')
                {
                    buttonForward.Opacity = 1;
                    buttonBackward.Opacity = 0.5;
                    buttonLeft.Opacity = 0.5;
                    buttonRight.Opacity = 0.5;
                    buttonStop.Opacity = 0.5;
                }
                if (direction == 's')
                {
                    buttonForward.Opacity = 0.5;
                    buttonBackward.Opacity = 1;
                    buttonLeft.Opacity = 0.5;
                    buttonRight.Opacity = 0.5;
                    buttonStop.Opacity = 0.5;
                }
                if (direction == 'p')
                {
                    buttonForward.Opacity = 0.5;
                    buttonBackward.Opacity = 0.5;
                    buttonLeft.Opacity = 0.5;
                    buttonRight.Opacity = 0.5;
                    buttonStop.Opacity = 1;
                }

                if (msg != "" && direction != lastDirectionSet)
                    communicationsWindow.SendMessage(msg);

                lastDirectionSet = direction;
                lastDirectionSetter = setter;

            }));
        }

        #region Keyboard
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                SetDirection('a', "keyboard");
            }
            if (e.Key == Key.Right)
            {
                SetDirection('d', "keyboard");
            }
            if (e.Key == Key.Up)
            {
                SetDirection('w', "keyboard");
            }
            if (e.Key == Key.Down)
            {
                SetDirection('s', "keyboard");
            }
            if (e.Key == Key.Space)
            {
                SetDirection('p', "keyboard");
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SetDirection('p', "keyboard");
        }
        #endregion

        #region Buttons
        private void buttonForward_Click(object sender, RoutedEventArgs e)
        {
            SetDirection('w', "button");
        }

        private void buttonBackward_Click(object sender, RoutedEventArgs e)
        {
            SetDirection('s', "button");
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            SetDirection('p', "button");
        }

        private void buttonLeft_Click(object sender, RoutedEventArgs e)
        {
            SetDirection('a', "button");
        }

        private void buttonRight_Click(object sender, RoutedEventArgs e)
        {
            SetDirection('d', "button");
        }
#endregion

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
                BottomCanvas.Background = transparentBrush;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                MainCanvas.Background = whiteBrush;
                BottomCanvas.Background = whiteBrush;
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
