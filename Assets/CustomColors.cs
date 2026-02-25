using Piexe.Utilities;
using System.Windows.Media;

namespace Piexe.Assets;

public static class CustomColors
{
    public static Brush Primary
    {
        get
        {
            if (IsDarkModeEnabled())
            {
                return "#202020".ToBrush();
            }
            else
            {
                return "#FAFAFA".ToBrush();
            }
        }
    }

    public static Brush PrimaryVariant
    {
        get
        {
            if (IsDarkModeEnabled())
            {
                return "#FAFAFA".ToBrush();
            }
            else
            {
                return "#202020".ToBrush();
            }
        }
    }

    public static Brush ItemBackground
    {
        get
        {
            if (IsDarkModeEnabled())
            {
                return "#333333".ToBrush();
            }
            else
            {
                return "#c7c5c5".ToBrush();
            }
        }
    }

    public static Brush CustomBlue
    {
        get
        {
            if (IsDarkModeEnabled())
            {
                return "#60a5fa".ToBrush();
            }
            else
            {
                return "#60c4fa".ToBrush();
            }
        }
    }

    public static Brush CustomGreen
    {
        get
        {
            if (IsDarkModeEnabled())
            {
                return "#25a173".ToBrush();
            }
            else
            {
                return "#2cbf89".ToBrush();
            }
        }
    }

    public static bool IsDarkModeEnabled()
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT ||
            Environment.OSVersion.Version.Major < 10)
        {
            return false;
        }

        try
        {
            return Natives.ShouldAppsUseDarkMode();
        }
        catch
        {
            return false;
        }
    }
}