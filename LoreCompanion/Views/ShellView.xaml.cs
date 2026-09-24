using System.Windows;
using System.Windows.Input;

namespace LoreCompanion.Views
{
    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class ShellView
    {
        public const double BaseWidth = 1360;
        public const double BaseHeight = 768;
        private const double BaseMinWidth = 1024;
        private const double BaseMinHeight = 576;

        private Point _dragStart;

        public ShellView()
        {
            InitializeComponent();
            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            var scale = screenHeight switch
            {
                >= 2160 => 1.75,
                >= 1440 => 1.35,
                var _ => 1.0,
            };

            //scale = 1.75;

            // Apply LayoutTransform to inner content
            RootScaleTransform.ScaleX = scale;
            RootScaleTransform.ScaleY = scale;

            // Adjust Window Min dimensions
            MinWidth = BaseMinWidth * scale;
            MinHeight = BaseMinHeight * scale;

            // Adjust Window dimensions clamped to WorkArea
            var workArea = SystemParameters.WorkArea;
            Width = Math.Min(BaseWidth * scale, workArea.Width);
            Height = Math.Min(BaseHeight * scale, workArea.Height);

            // Center on screen
            Left = workArea.Left + ((workArea.Width - Width) / 2);
            Top = workArea.Top + ((workArea.Height - Height) / 2);
        }

        private void OnWindowMinimize(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow!.WindowState = WindowState.Minimized;
        }

        private void OnWindowMaximize(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow!.WindowState =
                Application.Current.MainWindow!.WindowState == WindowState.Normal
                    ? WindowState.Maximized
                    : WindowState.Normal;
        }

        private void OnWindowClose(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow!.Close();
        }

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

                return;
            }

            _dragStart = e.GetPosition(this);
            ((UIElement)sender).CaptureMouse();
        }

        private void OnTitleBarMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            var currentPosition = e.GetPosition(this);

            var deltaX = currentPosition.X - _dragStart.X;
            var deltaY = currentPosition.Y - _dragStart.Y;

            if ((Math.Abs(deltaX) < SystemParameters.MinimumHorizontalDragDistance) &&
                (Math.Abs(deltaY) < SystemParameters.MinimumVerticalDragDistance))
            {
                return;
            }

            if (WindowState == WindowState.Maximized)
            {
                var screenPosition = PointToScreen(currentPosition);

                WindowState = WindowState.Normal;

                Left = screenPosition.X - (RestoreBounds.Width / 2);
                Top = screenPosition.Y - 10;
            }

            try
            {
                DragMove();
            }
            catch (InvalidOperationException)
            {
                // Mouse was released before DragMove could take over.
            }
        }

        private void OnTitleBarMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).ReleaseMouseCapture();
        }
    }
}