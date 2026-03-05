using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Piexe.Utilities;

namespace Piexe;

public partial class ScreenshotTaker : Window
{
    private bool _mouseOrTouchDown = false;
    private bool _selectionAborted = false;
    private Point _startPoint;
    private bool _analyze = true;
    private Rect? _currentSelectingRectangle;
    private bool _isToolbarVisible;
    private readonly Mutex _toolbarAnimationMutex = new(false);
    private Tesseract.PageIteratorLevel _textScanningPageIteratorLevel = Tesseract.PageIteratorLevel.Block;

    public ScreenshotTaker()
    {
        var px = Natives.GetMonitorBoundsAtCursor(useWorkArea: false);

        InitializeComponent();
        InitializeToolbar();

        Task.Run(() =>
        {
            Task.Delay(2000).Wait();
            Dispatcher.Invoke(() =>
            {
                if (!ToolBarBorder.IsMouseOver)
                {
                    BeginToolbarHidingAnimation();
                }
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

    private void InitializeToolbar()
    {
        ToolBarBorder.MouseEnter += (s, e) =>
        {
            BeginToolbarShowingAnimation();
        };
        ToolBarBorder.MouseLeave += (s, e) =>
        {
            BeginToolbarHidingAnimation();
        };
        ToolBarBorder.TouchUp += (s, e) =>
        {
            if (CaptureButton.AreAnyTouchesOver || AnalyzeToggleButton.AreAnyTouchesOver || ScreenshotToggleButton.AreAnyTouchesOver)
            {
                return;
            }
            if (_isToolbarVisible)
            {
                BeginToolbarHidingAnimation();
            }
            else
            {
                BeginToolbarShowingAnimation();
            }
        };
    }

    private void BeginToolbarShowingAnimation(bool checkHoverAtComplete = true)
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
                if (checkHoverAtComplete && !ToolBarBorder.IsMouseOver)
                {
                    BeginToolbarHidingAnimation();
                }
                _isToolbarVisible = true;
            };
            ScreenshotToggleButton.BeginAnimation(HeightProperty, animation);

            ToolBarStack.BeginAnimation(MarginProperty, new ThicknessAnimation()
            {
                From = new Thickness(0),
                To = new Thickness(0, 10, 0, 0),
                Duration = TimeSpan.FromSeconds(0.3)
            });

            if (_analyze)
            {
                IteratorLevelSelectionStack.BeginAnimation(HeightProperty, new DoubleAnimation()
                {
                    From = 0,
                    To = 30,
                    Duration = TimeSpan.FromSeconds(0.3)
                });
            }
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
                _isToolbarVisible = false;
            };

            ScreenshotToggleButton.BeginAnimation(HeightProperty, animation);

            ToolBarStack.BeginAnimation(MarginProperty, new ThicknessAnimation()
            {
                From = new Thickness(0, 10, 0, 0),
                To = new Thickness(0),
                Duration = TimeSpan.FromSeconds(0.3)
            });

            if (_analyze)
            {
                IteratorLevelSelectionStack.BeginAnimation(HeightProperty, new DoubleAnimation()
                {
                    From = 30,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.3)
                });
            }
        });
    }

    private void DoTheJob(System.Drawing.Bitmap image)
    {
        if (_analyze)
        {
            var analyzeWindow = new MainWindow(image, _textScanningPageIteratorLevel);
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
        IteratorLevelSelectionStack.BeginAnimation(HeightProperty, new DoubleAnimation()
        {
            From = 30,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.3)
        });
    }

    private void AnalyzeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _analyze = true;
        AnalyzeToggleButton.Background = Brushes.DeepSkyBlue;
        ScreenshotToggleButton.Background = Brushes.DarkGray;
        IteratorLevelSelectionStack.BeginAnimation(HeightProperty, new DoubleAnimation()
        {
            From = 0,
            To = 30,
            Duration = TimeSpan.FromSeconds(0.3)
        });
    }

    private void ResetSelection()
    {
        _selectionAborted = true;
        SelectingRectangle.Clip = new RectangleGeometry(new Rect(0, 0, Width, Height));
        ToolBarBorder.Visibility = Visibility.Visible;
        _currentSelectingRectangle = null;
        SelectingRectangleStroke.Visibility = Visibility.Collapsed;
    }

    private void SetCapturingStartParameters(Point point)
    {
        _mouseOrTouchDown = true;
        _selectionAborted = false;
        _startPoint = point;
        SelectingRectangle.Visibility = Visibility.Visible;
        ToolBarBorder.Visibility = Visibility.Collapsed;
    }

    private void SetCapturingEndParameters(Point point)
    {
        _mouseOrTouchDown = false;
        if (!_selectionAborted)
        {
            Hide();
            DoTheJob(ScreenShot.Take(new Rect(_startPoint, point)));
        }
    }

    private void UpdateCapturingParameters(Point point)
    {
        if (_mouseOrTouchDown && !_selectionAborted)
        {
            _currentSelectingRectangle = new Rect(_startPoint, point);

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
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (CaptureButton.IsMouseOver || AnalyzeToggleButton.IsMouseOver || ScreenshotToggleButton.IsMouseOver)
        {
            return;
        }

        SetCapturingStartParameters(e.GetPosition(this));
        base.OnMouseDown(e);
    }

    protected override void OnTouchDown(TouchEventArgs e)
    {
        if (ToolBarBorder.AreAnyTouchesOver || CaptureButton.AreAnyTouchesOver || AnalyzeToggleButton.AreAnyTouchesOver || ScreenshotToggleButton.AreAnyTouchesOver)
        {
            return;
        }

        SetCapturingStartParameters(e.GetTouchPoint(this).Position);
        base.OnTouchDown(e);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (CaptureButton.IsMouseOver || AnalyzeToggleButton.IsMouseOver || ScreenshotToggleButton.IsMouseOver)
        {
            return;
        }

        SetCapturingEndParameters(e.GetPosition(this));
        base.OnMouseUp(e);
    }

    protected override void OnTouchUp(TouchEventArgs e)
    {
        if (ToolBarBorder.AreAnyTouchesOver || CaptureButton.AreAnyTouchesOver || AnalyzeToggleButton.AreAnyTouchesOver || ScreenshotToggleButton.AreAnyTouchesOver)
        {
            return;
        }

        SetCapturingEndParameters(e.GetTouchPoint(this).Position);
        base.OnTouchUp(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        UpdateCapturingParameters(e.GetPosition(this));
        base.OnMouseMove(e);
    }

    protected override void OnTouchMove(TouchEventArgs e)
    {
        UpdateCapturingParameters(e.GetTouchPoint(this).Position);
        base.OnTouchMove(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (_mouseOrTouchDown)
            {
                ResetSelection();
            }
            else if (CaptureButton.Height != 0)
            {
                BeginToolbarHidingAnimation();
            }
            else
            {
                App.Current.Shutdown();
            }
        }
        else if (CaptureButton.Height == 0)
        {
            BeginToolbarShowingAnimation(false);
        }
        base.OnKeyUp(e);
    }

    protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
    {
        if (_mouseOrTouchDown)
        {
            ResetSelection();
        }
        base.OnMouseRightButtonDown(e);
    }

    private void BlockToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _textScanningPageIteratorLevel = Tesseract.PageIteratorLevel.Block;
        BlockToggleButton.Background = Brushes.DeepSkyBlue;
        LineToggleButton.Background = Brushes.DarkGray;
        WordToggleButton.Background = Brushes.DarkGray;
        SymbolToggleButton.Background = Brushes.DarkGray;
        ParagraphToggleButton.Background = Brushes.DarkGray;
    }

    private void LineToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _textScanningPageIteratorLevel = Tesseract.PageIteratorLevel.TextLine;
        BlockToggleButton.Background = Brushes.DarkGray;
        LineToggleButton.Background = Brushes.DeepSkyBlue;
        WordToggleButton.Background = Brushes.DarkGray;
        SymbolToggleButton.Background = Brushes.DarkGray;
        ParagraphToggleButton.Background = Brushes.DarkGray;
    }

    private void WordToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _textScanningPageIteratorLevel = Tesseract.PageIteratorLevel.Word;
        BlockToggleButton.Background = Brushes.DarkGray;
        LineToggleButton.Background = Brushes.DarkGray;
        WordToggleButton.Background = Brushes.DeepSkyBlue;
        SymbolToggleButton.Background = Brushes.DarkGray;
        ParagraphToggleButton.Background = Brushes.DarkGray;
    }

    private void SymbolToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _textScanningPageIteratorLevel = Tesseract.PageIteratorLevel.Symbol;
        BlockToggleButton.Background = Brushes.DarkGray;
        LineToggleButton.Background = Brushes.DarkGray;
        WordToggleButton.Background = Brushes.DarkGray;
        SymbolToggleButton.Background = Brushes.DeepSkyBlue;
        ParagraphToggleButton.Background = Brushes.DarkGray;
    }

    private void ParagraphToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _textScanningPageIteratorLevel = Tesseract.PageIteratorLevel.Para;
        BlockToggleButton.Background = Brushes.DarkGray;
        LineToggleButton.Background = Brushes.DarkGray;
        WordToggleButton.Background = Brushes.DarkGray;
        SymbolToggleButton.Background = Brushes.DarkGray;
        ParagraphToggleButton.Background = Brushes.DeepSkyBlue;
    }
}