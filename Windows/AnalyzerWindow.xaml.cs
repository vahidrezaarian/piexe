using Piexe.Assets;
using Piexe.Utilities;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Piexe;

public partial class MainWindow : Window
{
    private System.Drawing.Bitmap? _analyzingImageBitmap;
    private List<AnalyzedItem> _analyzedItems = [];
    private Tesseract.PageIteratorLevel _textScanningPageIteratorLevel = Tesseract.PageIteratorLevel.Block;

    public MainWindow(object image, Tesseract.PageIteratorLevel textScanningPageIteratorLevel)
    {
        _textScanningPageIteratorLevel = textScanningPageIteratorLevel;
        InitializeTheWindow();
        SetWindowLocation();
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
        SetProgressRingColors();
        ShowLoading();
        InitializePageIteratorCombobox();
    }

    private void InitializeItemsList(List<AnalyzedItem> analyzedItems)
    {
        Dispatcher.Invoke(() =>
        {
            MainContainer?.Children?.Clear();
            _analyzedItems = analyzedItems;
            HideLoading();
            CreateAndInsertAnalyzedItems();
            SetWindowLocation();
            CheckScrollBarVisibility();
        });
    }

    private void InitializePageIteratorCombobox()
    {
        var supportedLevels = new List<string>
        {
            "Block", "Paragraph", "Word", "Line", "Symbol"
        };

        PageIteratorSelectionComboBox.ItemsSource = supportedLevels;
        PageIteratorSelectionComboBox.SelectedIndex = supportedLevels.IndexOf(GetIterationLevelName(_textScanningPageIteratorLevel));
    }

    private void SetWindowLocation()
    {
        Task.Run(() =>
        {
            Task.Delay(10).Wait();
            Dispatcher.Invoke(() =>
            {
                var px = Natives.GetMonitorBoundsAtCursor(useWorkArea: false);
                var m = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice
                    ?? Matrix.Identity;

                var screenWidth = m.Transform(new Point(px.Right, px.Bottom)).X;
                var screenHeight = m.Transform(new Point(px.Right, px.Bottom)).Y;

                if (Height > screenHeight)
                {
                    Height = screenHeight;
                }

                if (Width > screenWidth)
                {
                    Width = screenWidth;
                }

                Left = (screenWidth - Width) / 2;
                Top = (screenHeight - Height) / 2;
            });
        });
    }

    private void SetProgressRingColors()
    {
        ScanningProgressRing.Foreground = SystemColors.AccentColorBrush;
        ScanningProgressRingBackground.Background = SystemColors.DesktopBrush;
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
                    MainContainer.Margin = new Thickness(0, 0, 20, 0);
                }
                else
                {
                    MainContainer.Margin = new Thickness(0);
                }
            });
        });
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

    private void ShowError(string text, bool permenant = false)
    {
        ErrorTextBlock.Text = text;
        MessageTextBlock.Visibility = Visibility.Collapsed;
        ErrorTextBlock.Visibility = Visibility.Visible;
        if (!permenant)
        {
            Task.Run(() =>
            {
                Task.Delay(5000).Wait();
                Dispatcher.Invoke(() =>
                {
                    ErrorTextBlock.Visibility = Visibility.Collapsed;
                    MessageTextBlock.Visibility = Visibility.Visible;
                });
            });
        }
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
            PageIteratorSelectionWidget.Visibility = Visibility.Visible;
        }
        else
        {
            PageIteratorSelectionWidget.Visibility = Visibility.Collapsed;
        }
    }

    #region StaticHelpers
    private static Grid CreateElementHeader(AnalyzedItemType type)
    {
        var stack = new StackPanel
        {
            Name = "ElementHeaderStack",
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8)
        };

        var grid = new Grid()
        {
            Margin = new Thickness(0, 0, 0, 3)
        };

        var border = new Button()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            IsEnabled = false
        };

        var iconType = CustomIcons.Document(SystemColors.AccentColor);
        if (type == AnalyzedItemType.QrCode)
        {
            iconType = CustomIcons.QRCode(SystemColors.AccentColor);
        }
        else if (type == AnalyzedItemType.Barcode)
        {
            iconType = CustomIcons.Barcode(SystemColors.AccentColor);
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

        grid.Children.Add(border);
        grid.Children.Add(stack);

        return grid;
    }

    private static Grid CreateElementContent(string text, StackPanel headerStack)
    {
        var textBox = new TextBox()
        {
            Name = "ItemTextBox",
            Text = text,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Cursor = Cursors.IBeam,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            FontWeight = FontWeights.SemiBold,
            AcceptsReturn = true,
            AcceptsTab = true,
            TextAlignment = TextAlignment.Left,
            Padding = new Thickness(8),
            Margin = new Thickness(10)
        };
        textBox.TextChanged += (s, e) =>
        {
            App.Current.Dispatcher.Invoke(() =>
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

        var grid = new Grid();

        var border = new Button()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            IsEnabled = false
        };

        grid.Children.Add(border);
        grid.Children.Add(textBox);

        return grid;
    }

    private static StackPanel CreateDetectedElement(AnalyzedItemType type, string text)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            ClipToBounds = false,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment= HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var elementHeader = CreateElementHeader(type);
        var headerStack = elementHeader.Children[1] as StackPanel ?? throw new NullReferenceException($"HeaderStack can't be null!");

        var elementContent = CreateElementContent(text, headerStack);

        stack.Children.Add(elementHeader);
        stack.Children.Add(elementContent);

        return stack;
    }

    private static Button CreateOpenLinkButton(TextBox textBox)
    {
        var button = CreateButton("Open Link", CustomIcons.Link(SystemColors.AccentColor));
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
        var button = CreateButton("Open Folder", CustomIcons.OpenFolder(SystemColors.AccentColor));
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
        var button = CreateButton("Copy", CustomIcons.Copy(SystemColors.AccentColor));
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
                    buttonIcon.Source = CustomIcons.Copied(SystemColors.AccentColor);
                }
            }
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
                            buttonIcon.Source = CustomIcons.Copy(SystemColors.AccentColor);
                        }
                    }
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
        var buttonMainContent = CreateButton("Open in PowerShell", CustomIcons.PowerShell(SystemColors.AccentColor));
        border.Child = buttonMainContent;
        border.Visibility = Visibility.Collapsed;

        var buttonHoverContent = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var runAsAdminButton = CreateButton("Run as admin", CustomIcons.PowerShell(SystemColors.AccentColor));
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
        var runNormallyButton = CreateButton("Run normally", CustomIcons.PowerShell(SystemColors.AccentColor));
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
        var buttonMainContent = CreateButton("Open in Command Prompt", CustomIcons.PowerShell(SystemColors.AccentColor));
        border.Child = buttonMainContent;
        border.Visibility = Visibility.Collapsed;

        var buttonHoverContent = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var runAsAdminButton = CreateButton("Run as admin", CustomIcons.PowerShell(SystemColors.AccentColor));
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
        var runNormallyButton = CreateButton("Run normally", CustomIcons.PowerShell(SystemColors.AccentColor));
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

    private static Button CreateButton(string text, ImageSource? icon = null)
    {
        var contentPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(15, 0, 15, 0)
        };
        var buttonText = new TextBlock()
        {
            Text = text,
            Margin = new Thickness(icon is null ? 0 : 10, 0, 0, 0),
            HorizontalAlignment = icon is null ? HorizontalAlignment.Center : HorizontalAlignment.Right,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
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
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(5, 0, 0, 0),
            Padding = new Thickness(5)
        };

        return button;
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

    private static string GetIterationLevelName(Tesseract.PageIteratorLevel level)
    {
        return level switch
        {
            Tesseract.PageIteratorLevel.Para => "Paragraph",
            Tesseract.PageIteratorLevel.Word => "Word",
            Tesseract.PageIteratorLevel.TextLine => "Line",
            Tesseract.PageIteratorLevel.Symbol => "Symbol",
            _ => "Block",
        };
    }

    private static Tesseract.PageIteratorLevel GetIterationLevel(string? levelName)
    {
        return levelName switch
        {
            "Paragraph" => Tesseract.PageIteratorLevel.Para,
            "Word" => Tesseract.PageIteratorLevel.Word,
            "Line" => Tesseract.PageIteratorLevel.TextLine,
            "Symbol" => Tesseract.PageIteratorLevel.Symbol,
            _ => Tesseract.PageIteratorLevel.Block,
        };
    }
    #endregion

    #region DragAndDropControls
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
    #endregion

    private void PageIteratorSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _textScanningPageIteratorLevel = GetIterationLevel(PageIteratorSelectionComboBox.SelectedItem.ToString());

        ShowLoading();
        Task.Run(() =>
        {
            InitializeItemsList(PictureScanner.Scan(_analyzingImageBitmap, _textScanningPageIteratorLevel));
        });
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        DragMove();
        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            App.Current.Shutdown();
        }
        base.OnKeyDown(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        App.Current.Shutdown();
        base.OnClosed(e);
    }
}
