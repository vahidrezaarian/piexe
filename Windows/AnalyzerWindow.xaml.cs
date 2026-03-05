using Microsoft.Win32;
using Piexe.Assets;
using Piexe.Utilities;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Piexe;

public partial class MainWindow : Window
{
    private readonly Mutex _closeButtonAnimationMutex = new(false);
    private System.Drawing.Bitmap? _analyzingImageBitmap;
    private List<AnalyzedItem> _analyzedItems = [];
    private Tesseract.PageIteratorLevel _textScanningPageIteratorLevel = Tesseract.PageIteratorLevel.Block;

    public MainWindow(object image, Tesseract.PageIteratorLevel textScanningPageIteratorLevel)
    {
        InitializeTheWindow();
        _textScanningPageIteratorLevel = textScanningPageIteratorLevel;
        Task.Run(() =>
        {
            try
            {
                var items = PictureScanner.Scan(image, _textScanningPageIteratorLevel, out _analyzingImageBitmap);
                Dispatcher.Invoke(() =>
                {
                    InitializeItemsList(items);
                });
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    ShowError("Something went wrong. Please try again.");
                });
            }
        });
    }

    private void InitializeTheWindow()
    {
        InitializeComponent();
        SetColors();
        ShowLoading();

        SystemEvents.UserPreferenceChanged += (s, e) =>
        {
            SetColors();
            CreateAndInsertAnalyzedItems();
        };
    }

    private void InitializeItemsList(List<AnalyzedItem> analyzedItems)
    {
        Dispatcher.Invoke(() =>
        {
            MainContainer?.Children?.Clear();
            _analyzedItems = analyzedItems;
            HideLoading();
            CreateAndInsertAnalyzedItems();
            PutWindowInCenter();
            CheckScrollBarVisibility();
        });
    }

    private void PutWindowInCenter()
    {
        Task.Run(() =>
        {
            Task.Delay(10).Wait();
            Dispatcher.Invoke(() =>
            {
                var px = Natives.GetMonitorBoundsAtCursor(useWorkArea: false);
                var m = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice
                    ?? Matrix.Identity;

                var screenRight = m.Transform(new Point(px.Right, px.Bottom)).X;
                var screenButtom = m.Transform(new Point(px.Right, px.Bottom)).Y;

                Left = (screenRight - Width) / 2;
                Top = (screenButtom - Height) / 2;
            });
        });
    }

    private void CheckScrollBarVisibility()
    {
        Task.Run(() =>
        {
            Task.Delay(50).Wait();
            Dispatcher.Invoke(() =>
            {
                if (WindowContentScrollView.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                {
                    MainContainer.Margin = new Thickness(0, 0, 10, 0);
                }
                else
                {
                    MainContainer.Margin = new Thickness(0);
                }
            });
        });
    }

    private void SetColors()
    {
        WindowBorder.Background = CustomColors.Primary;
        WindowBorder.BorderBrush = CustomColors.PrimaryVariant;
        WindowTitleText.Foreground = CustomColors.PrimaryVariant;
        CloseWindowButtonText.Foreground = CustomColors.PrimaryVariant;
        WindowMessagingTextBlock.Foreground = CustomColors.Primary;
        CloseWindowButton.Background = CustomColors.PrimaryVariant;
        MessagingBackground.Background = CustomColors.PrimaryVariant;
        PageIteratorLevelSelectionBackground.Background = CustomColors.PrimaryVariant;
        PageIteratorLevelSelectionText.Foreground = CustomColors.Primary;
    }

    private void ShowLoading()
    {
        var blur = new BlurEffect
        {
            Radius = 12,
            RenderingBias = RenderingBias.Performance
        };
        WindowContentGrid.Effect = blur;
        ScanningGif.Visibility = Visibility.Visible;
    }

    private void HideLoading()
    {
        WindowContentGrid.Effect = null;
        ScanningGif.Visibility = Visibility.Collapsed;
    }

    private void CreateAndInsertAnalyzedItems()
    {
        if (_analyzedItems.Count == 0)
        {
            ShowError("No QR codes, barcodes or text detected.");
            return;
        }

        MainContainer.Children.Clear();

        bool textItemFound = false;
        foreach (var item in _analyzedItems)
        {
            if (!textItemFound && item.Type == AnalyzedItemType.Text)
            {
                textItemFound = true;
            }
            MainContainer.Children.Add(CreateDetectedElement(item.Type, item.Value.Trim()));
        }

        if (textItemFound)
        {
            PageIteratorLevelSelectionBackground.Visibility = Visibility.Visible;
            CreatePageIteratorSelectionWidget();
        }
        else
        {
            PageIteratorLevelSelectionBackground.Visibility = Visibility.Collapsed;
        }
    }

    private static Tesseract.PageIteratorLevel GetTextScanningPageIteratorLevel(string? levelName)
    {
        return levelName switch
        {
            "Block" => Tesseract.PageIteratorLevel.Block,
            "Word" => Tesseract.PageIteratorLevel.Word,
            "Symbol" => Tesseract.PageIteratorLevel.Symbol,
            "Line" => Tesseract.PageIteratorLevel.TextLine,
            "Paragraph" => Tesseract.PageIteratorLevel.Para,
            _ => throw new NotSupportedException("Invalid level name!"),
        };
    }

    private static string GetTextScanningPageIteratorLevel(Tesseract.PageIteratorLevel levelName)
    {
        return levelName switch
        {
            Tesseract.PageIteratorLevel.Block => "By Block",
            Tesseract.PageIteratorLevel.TextLine => "By Line",
            Tesseract.PageIteratorLevel.Symbol => "By Symbol",
            Tesseract.PageIteratorLevel.Para => "By Paragraph",
            Tesseract.PageIteratorLevel.Word => "By Word",
            _ => throw new NotSupportedException("Invalid level name!"),
        };
    }

    private void CreatePageIteratorSelectionWidget()
    {
        PageIteratorLevelSelectionStack.Children.Clear();

        var supportedLevels = new List<Tesseract.PageIteratorLevel>
        {
            Tesseract.PageIteratorLevel.Word,
            Tesseract.PageIteratorLevel.Para,
            Tesseract.PageIteratorLevel.Block,
            Tesseract.PageIteratorLevel.Symbol,
            Tesseract.PageIteratorLevel.TextLine
        };

        var primaryButton = CreateButton(GetTextScanningPageIteratorLevel(_textScanningPageIteratorLevel), hoverBrush: CustomColors.CustomBlue, foreground: CustomColors.PrimaryVariant, background: CustomColors.Primary, width: 150);
        PageIteratorLevelSelectionStack.Children.Add(primaryButton);
        supportedLevels.Remove(_textScanningPageIteratorLevel);

        for (int i = 0; i < supportedLevels.Count; i++)
        {
            var cornerRadius = i == 0 ? new CornerRadius(15, 0, 0, 15) : i == supportedLevels.Count - 1 ? new CornerRadius(0, 15, 15, 0) : new CornerRadius(0, 0, 0, 0);
            var button = CreateButton(GetTextScanningPageIteratorLevel(supportedLevels[i]), hoverBrush: CustomColors.CustomBlue, foreground: CustomColors.PrimaryVariant, background: CustomColors.Primary, cornerRadius: cornerRadius);
            button.Visibility = Visibility.Collapsed;
            var level = supportedLevels[i];

            button.Click += (s, e) =>
            {
                _textScanningPageIteratorLevel = level;
                SetButtonText(PageIteratorLevelSelectionStack.Children[0] as Button, GetButtonText(button));
                for (int j = 1; j < PageIteratorLevelSelectionStack.Children.Count; j++)
                {
                    var oldButton = PageIteratorLevelSelectionStack.Children[j] as Button;
                    if (oldButton != null)
                    {
                        oldButton.Visibility = Visibility.Collapsed;
                    }
                }
                ShowLoading();
                Task.Run(() =>
                {
                    InitializeItemsList(PictureScanner.Scan(_analyzingImageBitmap, _textScanningPageIteratorLevel));
                });
            };

            PageIteratorLevelSelectionStack.Children.Add(button);
        }

        PageIteratorLevelSelectionBackground.MouseLeave += (s, e) =>
        {
            SetIteratorLevelButtonsVisibility(true);
        };

        PageIteratorLevelSelectionBackground.MouseLeftButtonDown += (s, e) =>
        {
            SetIteratorLevelButtonsVisibility(true);
        };

        primaryButton.Click += (s, e) =>
        {
            SetIteratorLevelButtonsVisibility(false);
        };
    }

    private void SetIteratorLevelButtonsVisibility(bool showMain)
    {
        var button = PageIteratorLevelSelectionStack.Children[0];
        if (button != null)
        {
            button.Visibility = showMain ? Visibility.Visible : Visibility.Collapsed;
        }
        for (int j = 1; j < PageIteratorLevelSelectionStack.Children.Count; j++)
        {
            button = PageIteratorLevelSelectionStack.Children[j] as Button;
            if (button != null)
            {
                button.Visibility = showMain ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }

    private static Border CreateElementHeader(AnalyzedItemType type)
    {
        var stack = new StackPanel
        {
            Name = "ElementHeaderStack",
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = System.Windows.VerticalAlignment.Center
        };

        var border = new Border()
        {
            CornerRadius = new CornerRadius(15, 15, 0, 0),
            Child = stack,
            BorderThickness = new Thickness(0),
            Background = CustomColors.ItemBackground,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            ClipToBounds = false,
            Margin = new Thickness(0, 0, 0, 3),
            Padding = new Thickness(8, 5, 8, 5)
        };

        var iconType = CustomIcons.Document;
        if (type == AnalyzedItemType.QrCode)
        {
            iconType = CustomIcons.QRCode;
        }
        else if (type == AnalyzedItemType.Barcode)
        {
            iconType = CustomIcons.Barcode;
        }

        stack.Children.Add(new Image()
        {
            Name = "ItemIcon",
            Source = iconType,
            Height = 25,
            Margin = new Thickness(5, 0, 5, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        });

        return border;
    }

    private Border CreateElementContent(string text, StackPanel headerStack)
    {
        var textBoxStyle = new Style(typeof(Border));
        textBoxStyle.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(15)));
        var textBox = new TextBox()
        {
            Name = "ItemTextBox",
            Text = text,
            Foreground = CustomColors.PrimaryVariant,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
            Cursor = Cursors.IBeam,
            Background = new SolidColorBrush(Colors.Transparent),
            CaretBrush = CustomColors.PrimaryVariant,
            BorderThickness = new Thickness(1.5),
            FontWeight = FontWeights.SemiBold,
            BorderBrush = CustomColors.PrimaryVariant,
            TextAlignment = TextAlignment.Left,
            Padding = new Thickness(10),
            Resources =
                {
                    { typeof(Border), textBoxStyle }
                }
        };
        textBox.TextChanged += (s, e) =>
        {
            Dispatcher.Invoke(() =>
            {
                if (textBox.Text.IsDirectory())
                {
                    SetupButtonsForDirectory(headerStack);
                }
                else if (textBox.Text.IsLink())
                {
                    SetupButtonsForLink(headerStack);
                }
                else
                {
                    SetupButtonsDefaultButtons(headerStack);
                }
            });
        };

        headerStack.Children.Add(CreateCopyButton(textBox));

        headerStack.Children.Add(CreateOpenFolderButton(textBox));
        headerStack.Children.Add(CreateOpenInPowerShellButton(textBox));
        headerStack.Children.Add(CreateOpenCommandPromptButton(textBox));
        headerStack.Children.Add(CreateOpenLinkButton(textBox));

        if (text.IsDirectory())
        {
            SetupButtonsForDirectory(headerStack);
        }
        else if (text.Replace(" ", "").IsDirectory())
        {
            text = text.Replace(" ", "");
            SetupButtonsForDirectory(headerStack);
        }
        else if (text.IsLink())
        {
            SetupButtonsForLink(headerStack);
        }
        else if (text.Replace(" ", "").IsLink())
        {
            text = text.Replace(" ", "");
            SetupButtonsForLink(headerStack);
        }

        var border = new Border()
        {
            CornerRadius = new CornerRadius(0, 0, 15, 15),
            Child = textBox,
            BorderThickness = new Thickness(0),
            Background = CustomColors.ItemBackground,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ClipToBounds = false,
            Padding = new Thickness(10)
        };

        return border;
    }

    private StackPanel CreateDetectedElement(AnalyzedItemType type, string text)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            ClipToBounds = false,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment= HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 20)
        };
        stack.MouseLeftButtonDown += WindowMouseDown;

        var elementHeader = CreateElementHeader(type);
        var headerStack = elementHeader.Child as StackPanel ?? throw new NullReferenceException($"headerStack can't be null!");

        var elementContent = CreateElementContent(text, headerStack);

        stack.Children.Add(elementHeader);
        stack.Children.Add(elementContent);

        return stack;
    }

    private static Button CreateOpenLinkButton(TextBox textBox)
    {
        var button = CreateButton("Open Link", CustomIcons.Link, new CornerRadius(15));
        button.Name = "OpenLinkButton";
        button.Visibility = Visibility.Collapsed;
        button.Click += (s, e) =>
        {
            using Process process = new();
            process.StartInfo.FileName = textBox.Text;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        };
        return button;
    }

    private static Button CreateOpenFolderButton(TextBox textBox)
    {
        var button = CreateButton("Open Folder", CustomIcons.OpenFolder, new CornerRadius(15));
        button.Name = "OpenFolderButton";
        button.Visibility = Visibility.Collapsed;
        button.Click += (s, e) =>
        {
            using Process process = new();
            process.StartInfo.FileName = "explorer";
            process.StartInfo.Arguments = textBox.Text;
            process.StartInfo.UseShellExecute = false;
            process.Start();
        };
        return button;
    }

    private static Button CreateCopyButton(TextBox textBox)
    {
        var button = CreateButton("Copy", CustomIcons.Copy, new CornerRadius(15));
        button.Click += (s, e) =>
        {
            Clipboard.SetDataObject(textBox.Text);
            foreach (var item in ((StackPanel)button.Content).Children)
            {
                if (item is TextBlock buttonText)
                {
                    buttonText.Text = "Copied to clipboard!";
                }
                else if (item is Image buttonIcon)
                {
                    buttonIcon.Source = CustomIcons.Copied;
                }
            }
            button.Background = CustomColors.CustomGreen;
            Task.Run(() =>
            {
                Task.Delay(2000).Wait();
                App.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var item in ((StackPanel)button.Content).Children)
                    {
                        if (item is TextBlock buttonText)
                        {
                            buttonText.Text = "Copy";
                        }
                        else if (item is Image buttonIcon)
                        {
                            buttonIcon.Source = CustomIcons.Copy;
                        }
                    }
                    button.Background = CustomColors.CustomBlue;
                });
            });
        };
        return button;
    }

    private static Border CreateOpenInPowerShellButton(TextBox textBox)
    {
        var border = new Border()
        {
            Name = "OpenInPowerShellButton",
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0)
        };
        var buttonMainContent = CreateButton("Open in PowerShell", CustomIcons.PowerShell, new CornerRadius(15));
        border.Child = buttonMainContent;
        border.Visibility = Visibility.Collapsed;

        var buttonHoverContent = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var runAsAdminButton = CreateButton("Run as admin", CustomIcons.PowerShell, new CornerRadius(15, 0, 0, 15));
        runAsAdminButton.Click += (s, e) =>
        {
            using Process process = new();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.WorkingDirectory = textBox.Text;
            process.StartInfo.Arguments = $"-NoExit -Command \"Set-Location '{textBox.Text}'\"";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas";
            process.Start();
        };
        var runNormallyButton = CreateButton("Run normally", CustomIcons.PowerShell, new CornerRadius(0, 15, 15, 0));
        runNormallyButton.Click += (s, e) =>
        {
            using Process process = new();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.WorkingDirectory = textBox.Text;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        };
        buttonHoverContent.Children.Add(runAsAdminButton);
        buttonHoverContent.Children.Add(runNormallyButton);


        border.MouseEnter += (s, e) =>
        {
            border.Child = buttonHoverContent;
        };
        border.MouseLeave += (s, e) =>
        {
            border.Child = buttonMainContent;
        };
        return border;
    }

    private static Border CreateOpenCommandPromptButton(TextBox textBox)
    {
        var border = new Border()
        {
            Name = "OpenInCommandPromptButton",
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0)
        };
        var buttonMainContent = CreateButton("Open in Command Prompt", CustomIcons.PowerShell, new CornerRadius(15));
        border.Child = buttonMainContent;
        border.Visibility = Visibility.Collapsed;

        var buttonHoverContent = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var runAsAdminButton = CreateButton("Run as admin", CustomIcons.PowerShell, new CornerRadius(15, 0, 0, 15));
        runAsAdminButton.Click += (s, e) =>
        {
            using Process process = new();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WorkingDirectory = textBox.Text;
            process.StartInfo.Arguments = $"/k \"cd /d \"{textBox.Text}\"\"";
            process.StartInfo.Verb = "runas";
            process.StartInfo.UseShellExecute = true;
            process.Start();
        };
        var runNormallyButton = CreateButton("Run normally", CustomIcons.PowerShell, new CornerRadius(0, 15, 15, 0));
        runNormallyButton.Click += (s, e) =>
        {
            using Process process = new();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.WorkingDirectory = textBox.Text;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        };
        buttonHoverContent.Children.Add(runAsAdminButton);
        buttonHoverContent.Children.Add(runNormallyButton);

        border.MouseEnter += (s, e) =>
        {
            border.Child = buttonHoverContent;
        };
        border.MouseLeave += (s, e) =>
        {
            border.Child = buttonMainContent;
        };
        return border;
    }

    private static Button CreateButton(string text, ImageSource? icon = null, CornerRadius? cornerRadius = null, Brush? hoverBrush = null, Brush? foreground = null, Brush? background = null, double? width = null)
    {
        var contentPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            Margin = new Thickness(15, 0, 15, 0)
        };
        var buttonText = new TextBlock()
        {
            Text = text,
            Margin = new Thickness(icon is null ? 0 : 10, 0, 0, 0),
            HorizontalAlignment = icon is null ? HorizontalAlignment.Center : HorizontalAlignment.Right,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = System.Windows.VerticalAlignment.Center
        };

        if (icon != null)
        {
            var buttonIcon = new Image()
            {
                Source = icon,
                Width = 15,
                Height = 15,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            contentPanel.Children.Add(buttonIcon);
        }
        
        contentPanel.Children.Add(buttonText);

        var button = new Button()
        {
            Content = contentPanel,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            Margin = new Thickness(5, 0, 0, 0),
            Foreground = foreground ?? CustomColors.PrimaryVariant,
            Background = background ?? CustomColors.CustomBlue,
            Padding = new Thickness(15, 0, 15, 0),
            BorderThickness = new Thickness(1.5),
            BorderBrush = Brushes.Transparent,
            Height = 32,
            Style = CreateButtonStyle(hoverBrush ?? CustomColors.PrimaryVariant, cornerRadius ?? new CornerRadius(15)),
        };

        if (width != null)
        {
            button.Width = width.Value;
        }

        return button;
    }

    private static Style CreateButtonStyle(Brush hoverBorderBrush, CornerRadius cornerRadius)
    {
        var template = new ControlTemplate(typeof(Button));

        var border = new FrameworkElementFactory(typeof(Border))
        {
            Name = "Border"
        };
        border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
        border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Control.BorderBrushProperty));
        border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Control.BorderThicknessProperty));
        border.SetValue(FrameworkElement.SnapsToDevicePixelsProperty, true);

        var cornerRadiusStyle = new Style(typeof(Border));
        cornerRadiusStyle.Setters.Add(new Setter(Border.CornerRadiusProperty, cornerRadius));
        border.SetValue(FrameworkElement.StyleProperty, cornerRadiusStyle);

        var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
        presenter.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(ContentControl.ContentProperty));
        presenter.SetValue(ContentPresenter.ContentTemplateProperty, new TemplateBindingExtension(ContentControl.ContentTemplateProperty));
        presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

        border.AppendChild(presenter);
        template.VisualTree = border;

        var hoverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        hoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, hoverBorderBrush, "Border"));
        template.Triggers.Add(hoverTrigger);

        var style = new Style(typeof(Button));
        style.Setters.Add(new Setter(Control.TemplateProperty, template));

        return style;
    }

    private static void SetButtonVisibility(StackPanel buttonsStackPanel, string name, Visibility visibility)
    {
        var index = 0;
        foreach (var item in buttonsStackPanel.Children)
        {
            if (item is Button button && button.Name == name)
            {
                button.Visibility = visibility;
                return;
            }
            else if (item is Border border && border.Name == name)
            {
                border.Visibility = visibility;
                return;
            }
            index++;
        }
    }

    private static void SetButtonText(Button? button, string? text)
    {
        if (button is null)
        {
            return;
        }

        var buttonstack = button.Content as StackPanel;
        var textBlock = buttonstack?.Children[0] as TextBlock;
        if (textBlock != null)
        {
            textBlock.Text = text;
        }
    }

    private static string? GetButtonText(Button button)
    {
        var buttonstack = button.Content as StackPanel;
        var textBlock = buttonstack?.Children[0] as TextBlock;
        return textBlock?.Text;
    }

    private static void SetupButtonsForDirectory(StackPanel buttonsStackPanel)
    {
        SetButtonVisibility(buttonsStackPanel, "OpenFolderButton", Visibility.Visible);
        SetButtonVisibility(buttonsStackPanel, "OpenInPowerShellButton", Visibility.Visible);
        SetButtonVisibility(buttonsStackPanel, "OpenInCommandPromptButton", Visibility.Visible);
        SetButtonVisibility(buttonsStackPanel, "OpenLinkButton", Visibility.Collapsed);
    }

    private static void SetupButtonsForLink(StackPanel buttonsStackPanel)
    {
        SetButtonVisibility(buttonsStackPanel, "OpenFolderButton", Visibility.Collapsed);
        SetButtonVisibility(buttonsStackPanel, "OpenInPowerShellButton", Visibility.Collapsed);
        SetButtonVisibility(buttonsStackPanel, "OpenInCommandPromptButton", Visibility.Collapsed);
        SetButtonVisibility(buttonsStackPanel, "OpenLinkButton", Visibility.Visible);
    }

    private static void SetupButtonsDefaultButtons(StackPanel buttonsStackPanel)
    {
        SetButtonVisibility(buttonsStackPanel, "OpenFolderButton", Visibility.Collapsed);
        SetButtonVisibility(buttonsStackPanel, "OpenInPowerShellButton", Visibility.Collapsed);
        SetButtonVisibility(buttonsStackPanel, "OpenInCommandPromptButton", Visibility.Collapsed);
        SetButtonVisibility(buttonsStackPanel, "OpenLinkButton", Visibility.Collapsed);
    }

    private void CloseButtonMouseEnter(object sender, MouseEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            _closeButtonAnimationMutex.WaitOne();
            if (CloseWindowButton.Width != 20)
            {
                _closeButtonAnimationMutex.ReleaseMutex();
                return;
            }

            var buttonSizeChangeAnimation = new DoubleAnimation()
            {
                From = 20,
                To = 50,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            var buttonTextSizeChangeAnimation = new DoubleAnimation()
            {
                From = 0,
                To = 60,
                Duration = TimeSpan.FromSeconds(0.5)
            };

            buttonSizeChangeAnimation.Completed += (s, ee) =>
            {
                _closeButtonAnimationMutex.ReleaseMutex();
                if (!CloseWindowButton.IsMouseOver)
                {
                    CloseButtonMouseLeave(this, e);
                }
            };
            CloseWindowButton.BeginAnimation(WidthProperty, buttonSizeChangeAnimation);
            CloseWindowButtonText.BeginAnimation(WidthProperty, buttonTextSizeChangeAnimation);
        });
    }

    private void CloseButtonMouseLeave(object sender, MouseEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            _closeButtonAnimationMutex.WaitOne();
            if (CloseWindowButton.Width != 50)
            {
                _closeButtonAnimationMutex.ReleaseMutex();
                return;
            }

            var buttonSizeChangeAnimation = new DoubleAnimation()
            {
                From = 50,
                To = 20,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            var buttonTextSizeChangeAnimation = new DoubleAnimation()
            {
                From = 60,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.1)
            };

            buttonSizeChangeAnimation.Completed += (s, ee) =>
            {
                _closeButtonAnimationMutex.ReleaseMutex();
                if (CloseWindowButton.IsMouseOver)
                {
                    CloseButtonMouseEnter(this, e);
                }
            };
            CloseWindowButton.BeginAnimation(WidthProperty, buttonSizeChangeAnimation);
            CloseWindowButtonText.BeginAnimation(WidthProperty, buttonTextSizeChangeAnimation);
        });
    }

    private void CloseWindowButtonClick(object sender, RoutedEventArgs e)
    {
        App.Current.Shutdown();
    }

    private void WindowMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && !CloseWindowButton.IsMouseOver)
        {
            this.DragMove();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            App.Current.Shutdown();
        }
        base.OnKeyDown(e);
    }

    private void ShowDropMessage()
    {
        var blur = new BlurEffect
        {
            Radius = 12,
            RenderingBias = RenderingBias.Performance
        };
        WindowContentGrid.Effect = blur;
        DropMessage.Visibility = Visibility.Visible;
        WindowContentGrid.IsEnabled = false;
    }

    private void HideDropMessage()
    {
        WindowContentGrid.Effect = null;
        DropMessage.Visibility = Visibility.Collapsed;
        WindowContentGrid.IsEnabled = true;
    }

    private void Window_PreviewDragEnter(object sender, DragEventArgs e)
    {
        bool canDrop = false;

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                foreach (var file in files)
                {
                    if (file.IsImageFile())
                    {
                        canDrop = true;
                        break;
                    }
                }
            }
        }

        e.Effects = canDrop ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;

        if (canDrop)
        {
            ShowDropMessage();
        }
    }

    private void Window_PreviewDragLeave(object sender, DragEventArgs e)
    {
        e.Handled = true;
        HideDropMessage();
    }

    private void ShowError(string text, bool permenant = false)
    {
        WindowMessagingTextBlock.Text = text;
        WindowMessagingTextBlock.Foreground = CustomColors.PrimaryVariant;
        MessagingBackground.Background = Brushes.Red;
        if (!permenant)
        {
            Task.Run(() =>
            {
                Task.Delay(5000).Wait();
                Dispatcher.Invoke(() =>
                {
                    WindowMessagingTextBlock.Text = "Drag and drop images to scan";
                    WindowMessagingTextBlock.Foreground = CustomColors.Primary;
                    MessagingBackground.Background = CustomColors.PrimaryVariant;
                });
            });
        }
    }

    private void Window_PreviewDrop(object sender, DragEventArgs e)
    {
        HideDropMessage();
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            
            ShowLoading();

            Task.Run(() =>
            {
                try
                {
                    var scannedItems = new List<AnalyzedItem>();
                    foreach (var file in files)
                    {
                        if (file.IsImageFile())
                        {
                            scannedItems.AddRange(PictureScanner.Scan(file, _textScanningPageIteratorLevel, out _analyzingImageBitmap));
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        if (scannedItems.Count > 0)
                        {
                            InitializeItemsList(scannedItems);
                        }
                        else
                        {
                            HideLoading();
                            ShowError("No QR codes, barcodes or text detected in the dropped image(s).");
                        }
                    });
                }
                catch
                {
                    ShowError("Something went wrong. Please try again.");
                }
            });
        }
        e.Handled = true;
    }
}
