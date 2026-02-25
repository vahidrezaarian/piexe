using System.Runtime.InteropServices;

namespace Piexe.Utilities;

internal class Natives
{
    public static RECT GetMonitorBoundsAtCursor(bool useWorkArea)
    {
        GetCursorPos(out POINT p);
        IntPtr hMon = MonitorFromPoint(p, MONITOR_DEFAULTTONEAREST);

        var mi = new MONITORINFO();
        mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
        GetMonitorInfo(hMon, ref mi);

        return useWorkArea ? mi.rcWork : mi.rcMonitor;
    }

    private const int MONITOR_DEFAULTTONEAREST = 2;

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(POINT pt, int dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [DllImport("uxtheme.dll", SetLastError = true, EntryPoint = "#132")]
    public static extern bool ShouldAppsUseDarkMode();
}
