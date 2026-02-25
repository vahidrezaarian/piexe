using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Piexe.Utilities;

namespace Piexe;

public partial class ScreenshotTaker : Window
{
    private bool _mouseDown = false;
    private bool _selectionAborted = false;
    private Point _startPoint;
    private bool _analyze = true;
    private Rect? _currentSelectingRectangle;
    private readonly Mutex _toolbarAnimationMutex = new(false);

    public ScreenshotTaker()
    {
        var px = Natives.GetMonitorBoundsAtCursor(useWorkArea: false);

        InitializeComponent();
        InitializeToolbarAnimations();

        Task.Run(() =>
        {
            Task.Delay(2000).Wait();
            Dispatcher.Invoke(() =>
            {
                BeginToolbarHidingAnimation();
            });
        });

        var m = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice
            ?? Matrix.Identity;

        var topLeft = m.Transform(new Point(px.Left, px.Top));
        var bottomRight = m.Transform(new Point(px.Right, px.Bottom));

        Left = topLeft.X;
        Top = topLeft.Y;
        Width = bottomRight.X - topLeft.X;
        Height = bottomRight.Y - topLeft.Y;
    }

    private void InitializeToolbarAnimations()
    {
        ToolBarBorder.MouseEnter += (s, e) =>
        {
            BeginToolbarShowingAnimation();
        };
        ToolBarBorder.MouseLeave += (s, e) =>
        {
            BeginToolbarHidingAnimation();
        };
    }

    private void BeginToolbarShowingAnimation()
    {
        Dispatcher.Invoke(() =>
        {
            _toolbarAnimationMutex.WaitOne();
            if (ScreenshotToggleButton.Height != 0)
            {
                _toolbarAnimationMutex.ReleaseMutex();
                return;
            }

            CaptureButton.BeginAnimation(HeightProperty, new DoubleAnimation()
            {
                From = 0,
                To = 25,
                Duration = TimeSpan.FromSeconds(0.3)
            });
            AnalyzeToggleButton.BeginAnimation(HeightProperty, new DoubleAnimation()
            {
                From = 0,
                To = 25,
                Duration = TimeSpan.FromSeconds(0.3)
            });

            var animation = new DoubleAnimation()
            {
                From = 0,
                To = 25,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            animation.Completed += (s, e) =>
            {
                _toolbarAnimationMutex.ReleaseMutex();
                if (!ToolBarBorder.IsMouseOver)
                {
                    BeginToolbarHidingAnimation();
                }
            };
            ScreenshotToggleButton.BeginAnimation(HeightProperty, animation);

            ToolBarStack.BeginAnimation(MarginProperty, new ThicknessAnimation()
            {
                From = new Thickness(0),
                To = new Thickness(0, 10, 0, 0),
                Duration = TimeSpan.FromSeconds(0.3)
            });
        });
    }

    private void BeginToolbarHidingAnimation()
    {
        Dispatcher.Invoke(() =>
        {
            _toolbarAnimationMutex.WaitOne();
            if (ScreenshotToggleButton.Height != 25)
            {
                _toolbarAnimationMutex.ReleaseMutex();
                return;
            }

            CaptureButton.BeginAnimation(HeightProperty, new DoubleAnimation()
            {
                From = 25,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            });
            AnalyzeToggleButton.BeginAnimation(HeightProperty, new DoubleAnimation()
            {
                From = 25,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            });

            var animation = new DoubleAnimation()
            {
                From = 25,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            animation.Completed += (s, e) =>
            {
                _toolbarAnimationMutex.ReleaseMutex();
                if (ToolBarBorder.IsMouseOver)
                {
                    BeginToolbarShowingAnimation();
                }
            };

            ScreenshotToggleButton.BeginAnimation(HeightProperty, animation);

            ToolBarStack.BeginAnimation(MarginProperty, new ThicknessAnimation()
            {
                From = new Thickness(0, 10, 0, 0),
                To = new Thickness(0),
                Duration = TimeSpan.FromSeconds(0.3)
            });
        });
    }

    private void DoTheJob(System.Drawing.Bitmap image)
    {
        if (_analyze)
        {
            var analyzeWindow = new MainWindow(image);
            analyzeWindow.Show();
            analyzeWindow.Activate();
            analyzeWindow.Focus();
        }
        else
        {
            image.Save($"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}\\Screenshot {DateTime.Now:ddMMyyyy-hhmmssffff}.png");
            App.Current.Shutdown();
        }
    }

    private void CaptureButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
        DoTheJob(ScreenShot.Take(new Rect(Left, Top, Width, Height)));
    }

    private void ScreenshotToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _analyze = false;
        AnalyzeToggleButton.Background = Brushes.DarkGray;
        ScreenshotToggleButton.Background = Brushes.DeepSkyBlue;
    }

    private void AnalyzeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _analyze = true;
        AnalyzeToggleButton.Background = Brushes.DeepSkyBlue;
        ScreenshotToggleButton.Background = Brushes.DarkGray;
    }

    private void ResetSelection()
    {
        _selectionAborted = true;
        SelectingRectangle.Clip = new RectangleGeometry(new Rect(0, 0, Width, Height));
        ToolBarBorder.Visibility = Visibility.Visible;
        _currentSelectingRectangle = null;
        SelectingRectangleStroke.Visibility = Visibility.Collapsed;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (CaptureButton.IsMouseOver || AnalyzeToggleButton.IsMouseOver || ScreenshotToggleButton.IsMouseOver)
        {
            return;
        }

        _mouseDown = true;
        _selectionAborted = false;
        _startPoint = e.GetPosition(this);
        SelectingRectangle.Visibility = Visibility.Visible;
        ToolBarBorder.Visibility = Visibility.Collapsed;

        base.OnMouseDown(e);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (CaptureButton.IsMouseOver || AnalyzeToggleButton.IsMouseOver || ScreenshotToggleButton.IsMouseOver)
        {
            return;
        }

        _mouseDown = false;
        if (!_selectionAborted)
        {
            Hide();
            DoTheJob(ScreenShot.Take(new Rect(_startPoint, e.GetPosition(this))));
        }
        base.OnMouseUp(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_mouseDown && !_selectionAborted)
        {
            _currentSelectingRectangle = new Rect(_startPoint, e.GetPosition(this));

            var wholeScreenGeometry = new RectangleGeometry(new Rect(0, 0, Width, Height));
            var selectedGeometry = new RectangleGeometry(_currentSelectingRectangle.Value);

            SelectingRectangle.Clip = new CombinedGeometry(wholeScreenGeometry, selectedGeometry)
            {
                GeometryCombineMode = GeometryCombineMode.Xor
            };

            SelectingRectangleStroke.Data = new CombinedGeometry(wholeScreenGeometry, selectedGeometry)
            {
                GeometryCombineMode = GeometryCombineMode.Exclude
            };

            SelectingRectangleStroke.Visibility = Visibility.Visible;
        }
        base.OnMouseMove(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (_mouseDown)
            {
                ResetSelection();
            }
            else
            {
                App.Current.Shutdown();
            }
        }
        base.OnKeyUp(e);
    }

    protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
    {
        if (_mouseDown)
        {
            ResetSelection();
        }
        base.OnMouseRightButtonDown(e);
    }
}