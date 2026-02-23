using Piexe.Assets;
using Piexe.Utilities;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Piexe
{
    internal static class Extensions
    {
        public static bool IsLink(this string value)
        {
            if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
            {
                return true;
            }
            return false;
        }

        public static bool IsDirectory(this string value)
        {
            return Directory.Exists(value);
        }

        public static bool IsImageFile(this string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp";
        }
    }

    public enum AnalyzedItemType
    {
        QrCode,
        Barcode,
        Text
    }

    public class AnalyzedItem(string value, AnalyzedItemType type)
    {
        public readonly AnalyzedItemType Type = type;
        public readonly string Value = value;
    }

    public partial class MainWindow : Window
    {
        private readonly Mutex _closeButtonAnimationMutex = new(false);

        public MainWindow(List<AnalyzedItem> items)
        {
            InitializeComponent();

            if (items.Count == 0)
            {
                App.Current.Shutdown();
                return;
            }

            foreach (var item in items)
            {
                MainContainer.Children.Add(CreateDetectedElement(item.Type, item.Value.Trim()));
            }
        }

        public MainWindow(System.Drawing.Bitmap imageBitmap)
        {
            InitializeComponent();

            Dispatcher.Invoke(() =>
            {
                var items = PictureScanner.Scan(imageBitmap);

                if (items.Count == 0)
                {
                    App.Current.Shutdown();
                    return;
                }

                foreach (var item in items)
                {
                    MainContainer.Children.Add(CreateDetectedElement(item.Type, item.Value.Trim()));
                }
            });
        }

        private Border CreateDetectedElement(AnalyzedItemType type, string text)
        {
            var itemsStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                ClipToBounds = false,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
            };

            var border = new Border()
            {
                CornerRadius = new CornerRadius(15),
                Child = itemsStack,
                BorderThickness = new Thickness(0),
                Background = new LinearGradientBrush(Color.FromRgb(Colors.DeepSkyBlue.R, Colors.DeepSkyBlue.G, Colors.DeepSkyBlue.B), Color.FromRgb(Colors.Black.R, Colors.Black.G, Colors.Black.B), 35),
                HorizontalAlignment = HorizontalAlignment.Left,
                ClipToBounds = false,
                Margin = new Thickness(0,10,10,10),
                Padding = new Thickness(10)
            };
            border.MouseDown += WindowMouseDown;

            var iconAndButtons = new StackPanel
            {
                Name = "ItemButtonsStack",
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5),
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

            iconAndButtons.Children.Add(new Image()
            {
                Name = "ItemIcon",
                Source = iconType,
                Height = 20,
                Margin = new Thickness(0,0,5,0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            });

            var textBoxStyle = new Style(typeof(Border));
            textBoxStyle.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(15)));
            var textBox = new TextBox()
            {
                Name = "ItemTextBox",
                Text = text,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Cursor = Cursors.IBeam,
                Background = new SolidColorBrush(Colors.Transparent),
                CaretBrush = new SolidColorBrush(Colors.AliceBlue),
                BorderThickness = new Thickness(1.5),
                FontWeight = FontWeights.SemiBold,
                BorderBrush = new SolidColorBrush(Colors.White),
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
                        SetupButtonsForDirectory(iconAndButtons);
                    }
                    else if (textBox.Text.IsLink())
                    {
                        SetupButtonsForLink(iconAndButtons);
                    }
                    else
                    {
                        SetupButtonsDefaultButtons(iconAndButtons);
                    }
                });
            };

            iconAndButtons.Children.Add(CreateCopyButton(textBox));

            iconAndButtons.Children.Add(CreateOpenFolderButton(textBox));
            iconAndButtons.Children.Add(CreateOpenInPowerShellButton(textBox));
            iconAndButtons.Children.Add(CreateOpenCommandPromptButton(textBox));
            iconAndButtons.Children.Add(CreateOpenLinkButton(textBox));

            if (text.IsDirectory())
            {
                SetupButtonsForDirectory(iconAndButtons);
            }
            else if (text.Replace(" ", "").IsDirectory())
            {
                text = text.Replace(" ", "");
                SetupButtonsForDirectory(iconAndButtons);
            }
            else if (text.IsLink())
            {
                SetupButtonsForLink(iconAndButtons);
            }
            else if (text.Replace(" ", "").IsLink())
            {
                text = text.Replace(" ", "");
                SetupButtonsForLink(iconAndButtons);
            }

            itemsStack.Children.Add(iconAndButtons);

            itemsStack.Children.Add(textBox);

            return border;
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
                button.Foreground = Brushes.SpringGreen;
                button.BorderBrush = Brushes.SpringGreen;
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
                        button.Foreground = Brushes.White;
                        button.BorderBrush = Brushes.White;
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

        private static Button CreateButton(string text, ImageSource icon, CornerRadius cornerRadius)
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
                Margin = new Thickness(10, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            var buttonIcon = new Image()
            {
                Source = icon,
                Width = 15,
                Height = 15,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            contentPanel.Children.Add(buttonIcon);
            contentPanel.Children.Add(buttonText);

            return new Button()
            {
                Content = contentPanel,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0),
                Foreground = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush(Colors.Transparent),
                Padding = new Thickness(15, 0, 15, 0),
                BorderThickness = new Thickness(1.5),
                BorderBrush = new SolidColorBrush(Colors.White),
                Height = 33,
                Style = CreateButtonStyle(Brushes.CornflowerBlue, cornerRadius),
            };
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

        private void ShowError(string text)
        {
            WindowMessagingTextBlock.Text = text;
            WindowMessagingTextBlock.Foreground = Brushes.Red;
            Task.Run(() =>
            {
                Task.Delay(5000).Wait();
                Dispatcher.Invoke(() =>
                {
                    WindowMessagingTextBlock.Text = "Drag and drop images to scan";
                    WindowMessagingTextBlock.Foreground = Brushes.White;
                });
            });
        }

        private void Window_PreviewDrop(object sender, DragEventArgs e)
        {
            HideDropMessage();
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var scannedItems = new List<AnalyzedItem>();

                foreach (var file in files)
                {
                    if (file.IsImageFile())
                    {
                        scannedItems.AddRange(PictureScanner.Scan(file));
                    }
                }

                if (scannedItems.Count() > 0)
                {
                    MainContainer.Children.Clear();
                    foreach (var item in scannedItems)
                    {
                        MainContainer.Children.Add(CreateDetectedElement(item.Type, item.Value.Trim()));
                    }
                }
                else
                {
                    ShowError("No QR codes, barcodes or text detected in the dropped image(s).");
                }
            }
            e.Handled = true;
        }
    }
}
