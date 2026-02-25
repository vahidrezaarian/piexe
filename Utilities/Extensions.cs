using System.IO;
using System.Windows.Media;

namespace Piexe.Utilities;

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

    public static Brush ToBrush(this string data)
    {
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(data));
    }
}
