using System.Windows;
using System.Windows.Media;

namespace Piexe.Utilities;

internal static class ScreenShot
{
    public static System.Drawing.Bitmap Take(Rect region)
    {
        double dpiScale = VisualTreeHelper.GetDpi(new DrawingVisual()).DpiScaleX;

        int left = (int)(region.Left * dpiScale);
        int top = (int)(region.Top * dpiScale);
        int width = (int)(region.Width * dpiScale);
        int height = (int)(region.Height * dpiScale);

        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(width, height);

        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(
                left,
                top,
                0, 0,
                new System.Drawing.Size(width, height),
                System.Drawing.CopyPixelOperation.SourceCopy);
        }

        return bmp;
    }
}
