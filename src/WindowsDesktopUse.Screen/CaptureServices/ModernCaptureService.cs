using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WindowsDesktopUse.Screen;

/// <summary>
/// Capture API preference options
/// </summary>
public enum CaptureApiPreference
{
    Auto,
    Modern,
    Legacy,
    Hybrid
}

/// <summary>
/// Interface for capture services
/// </summary>
public interface ICaptureService
{
    bool IsAvailable { get; }
    string ApiName { get; }
    Task<Bitmap?> CaptureWindowAsync(IntPtr hwnd, CancellationToken ct = default);
    Task<Bitmap?> CaptureMonitorAsync(uint monitorIndex, CancellationToken ct = default);
}

/// <summary>
/// Windows Graphics Capture API implementation
/// </summary>
public sealed class ModernCaptureService : ICaptureService, IDisposable
{
    private bool _disposed;

    public string ApiName => "Windows.Graphics.Capture";

    public bool IsAvailable => Environment.OSVersion.Version.Build >= 17763; // Windows 10 1809+

    public ModernCaptureService()
    {
    }

    public async Task<Bitmap?> CaptureWindowAsync(IntPtr hwnd, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            // Get window bounds
            GetWindowRect(hwnd, out var rect);
            var width = rect.Right - rect.Left;
            var height = rect.Bottom - rect.Top;

            if (width <= 0 || height <= 0)
            {
                Console.Error.WriteLine($"[ModernCapture] Invalid window dimensions: {width}x{height}");
                return null;
            }

            // Use PrintWindow with PW_RENDERFULLCONTENT for GPU-accelerated content
            return await Task.Run(() => CaptureWithPrintWindow(hwnd, width, height), ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ModernCapture] Capture failed: {ex.Message}");
            return null;
        }
    }

    private Bitmap? CaptureWithPrintWindow(IntPtr hwnd, int width, int height)
    {
        try
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

                var hdcDest = g.GetHdc();
                try
                {
                    // PW_RENDERFULLCONTENT = 0x00000002 - captures GPU-accelerated content
                    const uint PW_RENDERFULLCONTENT = 0x00000002;
                    var success = PrintWindow(hwnd, hdcDest, PW_RENDERFULLCONTENT);

                    if (!success)
                    {
                        Console.Error.WriteLine($"[ModernCapture] PrintWindow failed: {Marshal.GetLastWin32Error()}");
                        return null;
                    }
                }
                finally
                {
                    g.ReleaseHdc(hdcDest);
                }
            }

            return bmp;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ModernCapture] CaptureWithPrintWindow error: {ex.Message}");
            return null;
        }
    }

    public async Task<Bitmap?> CaptureMonitorAsync(uint monitorIndex, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return await Task.FromResult<Bitmap?>(null).ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    #region P/Invoke

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion
}

/// <summary>
/// Hybrid capture service that tries modern API first, falls back to legacy
/// </summary>
public sealed class HybridCaptureService : ICaptureService, IDisposable
{
    private readonly ModernCaptureService? _modern;
    private readonly ScreenCaptureService _legacy;
    private readonly CaptureApiPreference _preference;

    public string ApiName => _preference switch
    {
        CaptureApiPreference.Modern => _modern?.ApiName ?? "Legacy (Modern unavailable)",
        CaptureApiPreference.Legacy => _legacy.GetType().Name,
        CaptureApiPreference.Hybrid => "Hybrid (Modern + Legacy)",
        _ => "Auto"
    };

    public bool IsAvailable => _preference switch
    {
        CaptureApiPreference.Modern => _modern?.IsAvailable ?? false,
        CaptureApiPreference.Legacy => true,
        CaptureApiPreference.Hybrid => _modern?.IsAvailable ?? true,
        _ => _modern?.IsAvailable ?? true
    };

    public HybridCaptureService(ScreenCaptureService legacy, CaptureApiPreference preference = CaptureApiPreference.Auto)
    {
        _legacy = legacy ?? throw new ArgumentNullException(nameof(legacy));
        _preference = preference;

        try
        {
            _modern = new ModernCaptureService();
        }
        catch
        {
            _modern = null;
        }
    }

    public async Task<Bitmap?> CaptureWindowAsync(IntPtr hwnd, CancellationToken ct = default)
    {
        if (ShouldTryModern() && _modern != null)
        {
            try
            {
                var result = await _modern.CaptureWindowAsync(hwnd, ct).ConfigureAwait(false);
                if (result != null)
                {
                    Console.Error.WriteLine("[HybridCapture] Using Modern capture");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[HybridCapture] Modern capture failed: {ex.Message}");
            }
        }

        Console.Error.WriteLine("[HybridCapture] Falling back to Legacy capture");
        return CaptureWindowLegacy(hwnd);
    }

    public async Task<Bitmap?> CaptureMonitorAsync(uint monitorIndex, CancellationToken ct = default)
    {
        if (ShouldTryModern() && _modern != null)
        {
            try
            {
                var result = await _modern.CaptureMonitorAsync(monitorIndex, ct).ConfigureAwait(false);
                if (result != null)
                    return result;
            }
            catch
            {
            }
        }

        return CaptureMonitorLegacy(monitorIndex);
    }

    private bool ShouldTryModern()
    {
        if (_modern == null || !_modern.IsAvailable)
            return false;

        return _preference switch
        {
            CaptureApiPreference.Modern => true,
            CaptureApiPreference.Hybrid => true,
            CaptureApiPreference.Auto => true,
            _ => false
        };
    }

    private Bitmap CaptureWindowLegacy(IntPtr hwnd)
    {
        var hwndLong = hwnd.ToInt64();
        var imageData = ScreenCaptureService.CaptureWindow(hwndLong, 1920, 80);

        var base64Data = imageData.Contains(";base64,", StringComparison.Ordinal)
            ? imageData.Split(',')[1]
            : imageData;

        var bytes = Convert.FromBase64String(base64Data);
        using var ms = new MemoryStream(bytes);
        return new Bitmap(ms);
    }

    private Bitmap CaptureMonitorLegacy(uint monitorIndex)
    {
        var imageData = _legacy.CaptureSingle(monitorIndex, 1920, 80);

        var base64Data = imageData.Contains(";base64,", StringComparison.Ordinal)
            ? imageData.Split(',')[1]
            : imageData;

        var bytes = Convert.FromBase64String(base64Data);
        using var ms = new MemoryStream(bytes);
        return new Bitmap(ms);
    }

    public void Dispose()
    {
        _modern?.Dispose();
        GC.SuppressFinalize(this);
    }
}
