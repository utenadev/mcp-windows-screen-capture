using System.Diagnostics;
using System.Runtime.InteropServices;
using WindowsDesktopUse.Core;

namespace WindowsDesktopUse.Screen;

/// <summary>
/// UI Automation-based video target finder with dynamic tracking
/// </summary>
public class VideoTargetFinder : IDisposable
{
    private readonly Dictionary<string, VideoTargetInfo> _trackedTargets = new();
    private readonly System.Timers.Timer _trackingTimer;
    private readonly object _lock = new();
    private bool _disposed;

    public VideoTargetFinder()
    {
        _trackingTimer = new System.Timers.Timer(1000); // 1秒間隔で追跡
        _trackingTimer.Elapsed += OnTrackingTimerElapsed;
        _trackingTimer.AutoReset = true;
    }

    /// <summary>
    /// Find video element by target name (YouTube, ActiveWindow, etc.)
    /// </summary>
    public VideoTargetInfo? FindVideoTarget(string targetName)
    {
        try
        {
            Console.Error.WriteLine($"[VideoTargetFinder] Searching for target: {targetName}");

            if (targetName.Equals("ActiveWindow", StringComparison.OrdinalIgnoreCase))
            {
                return FindActiveWindowVideo();
            }

            // Search for specific video player types
            var videoTargets = SearchVideoElements();
            
            foreach (var target in videoTargets)
            {
                if (target.WindowTitle.Contains(targetName, StringComparison.OrdinalIgnoreCase) ||
                    target.ElementName.Contains(targetName, StringComparison.OrdinalIgnoreCase))
                {
                    lock (_lock)
                    {
                        _trackedTargets[targetName] = target;
                    }
                    Console.Error.WriteLine($"[VideoTargetFinder] Found target: {target.WindowTitle} at ({target.X}, {target.Y}) {target.Width}x{target.Height}");
                    return target;
                }
            }

            Console.Error.WriteLine($"[VideoTargetFinder] Target not found: {targetName}");
            return null;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[VideoTargetFinder] Error finding target: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Start tracking registered targets for position/size changes
    /// </summary>
    public void StartTracking()
    {
        _trackingTimer.Start();
        Console.Error.WriteLine("[VideoTargetFinder] Tracking started");
    }

    /// <summary>
    /// Stop tracking
    /// </summary>
    public void StopTracking()
    {
        _trackingTimer.Stop();
        Console.Error.WriteLine("[VideoTargetFinder] Tracking stopped");
    }

    /// <summary>
    /// Get updated position for a tracked target
    /// </summary>
    public VideoTargetInfo? GetUpdatedPosition(string targetName)
    {
        lock (_lock)
        {
            if (!_trackedTargets.TryGetValue(targetName, out var target))
                return null;

            try
            {
                // Check if window still exists
                if (!IsWindow(target.WindowHandle))
                {
                    _trackedTargets.Remove(targetName);
                    return null;
                }

                // Get current window position
                GetWindowRect(target.WindowHandle, out var rect);
                var currentX = rect.Left;
                var currentY = rect.Top;
                var currentW = rect.Right - rect.Left;
                var currentH = rect.Bottom - rect.Top;

                // Update if changed
                if (currentX != target.X || currentY != target.Y || 
                    currentW != target.Width || currentH != target.Height)
                {
                    var updated = target with { X = currentX, Y = currentY, Width = currentW, Height = currentH };
                    _trackedTargets[targetName] = updated;
                    Console.Error.WriteLine($"[VideoTargetFinder] Position updated for {targetName}: ({currentX}, {currentY}) {currentW}x{currentH}");
                    return updated;
                }

                return target;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[VideoTargetFinder] Error updating position: {ex.Message}");
                return target;
            }
        }
    }

    private void OnTrackingTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        lock (_lock)
        {
            foreach (var targetName in _trackedTargets.Keys.ToList())
            {
                GetUpdatedPosition(targetName);
            }
        }
    }

    private List<VideoTargetInfo> SearchVideoElements()
    {
        var results = new List<VideoTargetInfo>();
        
        try
        {
            // Method 1: Search by window class names and titles
            EnumWindows((hwnd, param) =>
            {
                if (!IsWindowVisible(hwnd)) return true;

                var title = GetWindowTitle(hwnd);
                if (string.IsNullOrWhiteSpace(title)) return true;

                // Check for video player indicators in title
                if (IsVideoPlayerWindow(title))
                {
                    GetWindowRect(hwnd, out var rect);
                    var w = rect.Right - rect.Left;
                    var h = rect.Bottom - rect.Top;
                    
                    if (w > 0 && h > 0)
                    {
                        results.Add(new VideoTargetInfo(
                            hwnd,
                            title,
                            ExtractVideoTitle(title),
                            rect.Left,
                            rect.Top,
                            w,
                            h
                        ));
                    }
                }

                return true;
            }, IntPtr.Zero);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[VideoTargetFinder] Error in window enumeration: {ex.Message}");
        }

        return results;
    }

    private VideoTargetInfo? FindActiveWindowVideo()
    {
        try
        {
            var hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return null;

            var title = GetWindowTitle(hwnd);
            GetWindowRect(hwnd, out var rect);
            var w = rect.Right - rect.Left;
            var h = rect.Bottom - rect.Top;

            if (w <= 0 || h <= 0) return null;

            return new VideoTargetInfo(
                hwnd,
                title,
                ExtractVideoTitle(title),
                rect.Left,
                rect.Top,
                w,
                h
            );
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[VideoTargetFinder] Error finding active window: {ex.Message}");
            return null;
        }
    }

    private string GetWindowTitle(IntPtr hwnd)
    {
        var sb = new System.Text.StringBuilder(256);
        GetWindowText(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private static bool IsVideoPlayerWindow(string title)
    {
        var videoIndicators = new[]
        {
            "YouTube",
            "Video",
            "Player",
            "Netflix",
            "Vimeo",
            "Twitch",
            "Hulu",
            "Prime Video",
            "Disney+",
            "HBO",
            "MP4",
            "AVI",
            "MKV",
            "VLC",
            "Media Player",
            "PotPlayer",
            " MPC",
            "映画",
            "動画"
        };

        return videoIndicators.Any(indicator => 
            title.Contains(indicator, StringComparison.OrdinalIgnoreCase));
    }

    private static string ExtractVideoTitle(string windowTitle)
    {
        // Try to extract video title from window title
        // Common patterns: "Video Title - YouTube", "Video Title | Netflix", etc.
        
        var separators = new[] { " - ", " | ", " — ", " • " };
        foreach (var sep in separators)
        {
            var index = windowTitle.LastIndexOf(sep, StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                return windowTitle.Substring(0, index).Trim();
            }
        }

        return windowTitle.Trim();
    }

    #region P/Invoke

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion

    public void Dispose()
    {
        if (!_disposed)
        {
            StopTracking();
            _trackingTimer.Dispose();
            _disposed = true;
        }
    }
}
