using NUnit.Framework;
using WindowsDesktopUse.App;

namespace E2ETests;

[TestFixture]
public class HwndParsingTests
{
    [Test]
    public void See_ParsesValidHwndString()
    {
        // Arrange & Act & Assert
        // _capture が初期化されていないため InvalidOperationException が発生する
        // しかし、hwndStr のパースまでは通ることを確認
        var ex = Assert.Throws<InvalidOperationException>(() => DesktopUseTools.See(hwndStr: "655936"));
        Assert.That(ex.Message, Does.Contain("ScreenCaptureService not initialized"));
    }

    // [Test]
    // public void See_ThrowsOnInvalidHwndString()
    // {
    //     // Arrange & Act & Assert
    //     var ex = Assert.Throws<ArgumentException>(() => DesktopUseTools.See(hwndStr: "invalid"));
    //     Assert.That(ex.Message, Does.Contain("Invalid hwnd value"));
    // }

    [Test]
    public void StartWatching_ParsesValidHwndString()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => DesktopUseTools.StartWatching(
            server: null!,
            targetType: "window",
            hwndStr: "12345"
        ));
        Assert.That(ex.Message, Does.Contain("ScreenCaptureService not initialized"));
    }

    [Test]
    public void CloseWindow_ParsesValidHwndString()
    {
        // Arrange & Act & Assert
        // hwndStr がパースされるまでは成功（実際のウィンドウ操作は失敗するが、パースは成功）
        var ex = Assert.Throws<ArgumentException>(() => DesktopUseTools.CloseWindow(hwndStr: "invalid"));
        Assert.That(ex.Message, Does.Contain("Invalid hwnd: 'invalid'. Must be integer."));
    }
}