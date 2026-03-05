using Piexe.Utilities;
using System.Windows;

namespace Piexe;

public partial class App : Application
{
    [System.STAThreadAttribute()]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    static void Main(string[] args)
    {
        App app = new App();
        if (args != null && args.Length > 0)
        {
            app.MainWindow = new MainWindow(args[0], Tesseract.PageIteratorLevel.Block);
        }
        else
        {
            app.MainWindow = new ScreenshotTaker();
        }

        app.MainWindow.Activate();
        app.MainWindow.Focus();
        app.MainWindow.Show();
        app.Run();
    }
}