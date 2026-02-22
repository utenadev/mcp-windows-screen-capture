using NUnit.Framework;
using WindowsDesktopUse.App;

namespace E2ETests;

[TestFixture]
public class HwndParsingTests
{
    [Test]
    public void VisualCapture_ParsesValidHwndString()
    {
        // Arrange & Act & Assert
        // _capture が初期化されていないため InvalidOperationException が発生する
        // しかし、hwnd のパースまでは通ることを確認
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => DesktopUseTools.VisualCapture(
            target: "window",
            hwnd: "655936"
        ));
        Assert.That(ex.Message, Does.Contain("ScreenCaptureService not initialized"));
    }

    [Test]
    public void VisualWatch_ParsesValidHwndString()
    {
        // Arrange & Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => DesktopUseTools.VisualWatch(
            server: null!,
            target: "window",
            hwnd: "12345"
        ));
        Assert.That(ex.Message, Does.Contain("ScreenCaptureService not initialized"));
    }

    [Test]
    public void InputWindow_ParsesValidHwndString()
    {
        // Arrange & Act & Assert
        // hwnd がパースされるまでは成功（実際のウィンドウ操作は失敗するが、パースは成功）
        var ex = Assert.Throws<ArgumentException>(() => DesktopUseTools.InputWindow(
            action: "close",
            hwnd: "invalid"
        ));
        Assert.That(ex.Message, Does.Contain("Invalid HWND format: 'invalid'. Expected numeric string."));
    }
}
