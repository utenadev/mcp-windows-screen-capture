using System.Drawing;
using NUnit.Framework;
using WindowsDesktopUse.Screen;

namespace UnitTests;

[TestFixture]
public class VisualChangeDetectorTests
{
    private VisualChangeDetector _detector = null!;

    [SetUp]
    public void Setup()
    {
        _detector = new VisualChangeDetector
        {
            GridSize = 8,
            ChangeThreshold = 0.08,
            PixelThreshold = 30,
            KeyFrameInterval = 10
        };
    }

    [TearDown]
    public void TearDown()
    {
        _detector?.Dispose();
    }

    [Test]
    public void AnalyzeFrame_FirstFrame_ShouldSend()
    {
        using var frame = CreateTestBitmap(100, 100, Color.Red);
        
        var result = _detector.AnalyzeFrame(frame);
        
        Assert.That(result.ShouldSend, Is.True);
        Assert.That(result.HasChange, Is.True);
        // First frame is treated as "Initial Frame" or "Key Frame" depending on implementation
        Assert.That(result.EventTag, Is.EqualTo("Initial Frame").Or.EqualTo("Key Frame"));
    }

    [Test]
    public void AnalyzeFrame_NoChange_ShouldNotSend()
    {
        using var frame1 = CreateTestBitmap(100, 100, Color.Red);
        using var frame2 = CreateTestBitmap(100, 100, Color.Red);
        
        _detector.AnalyzeFrame(frame1); // First frame
        var result = _detector.AnalyzeFrame(frame2); // Same frame
        
        Assert.That(result.ShouldSend, Is.False);
        Assert.That(result.HasChange, Is.False);
        Assert.That(result.ChangeRatio, Is.LessThan(0.08));
    }

    [Test]
    public void AnalyzeFrame_SignificantChange_ShouldSend()
    {
        using var frame1 = CreateTestBitmap(100, 100, Color.Red);
        using var frame2 = CreateTestBitmap(100, 100, Color.Blue);
        
        _detector.AnalyzeFrame(frame1); // First frame
        var result = _detector.AnalyzeFrame(frame2); // Different frame
        
        Assert.That(result.ShouldSend, Is.True);
        Assert.That(result.HasChange, Is.True);
        Assert.That(result.ChangeRatio, Is.GreaterThan(0.08));
    }

    [Test]
    public void AnalyzeFrame_DimensionChange_ShouldSend()
    {
        using var frame1 = CreateTestBitmap(100, 100, Color.Red);
        using var frame2 = CreateTestBitmap(200, 200, Color.Red);
        
        _detector.AnalyzeFrame(frame1); // First frame
        var result = _detector.AnalyzeFrame(frame2); // Different size
        
        Assert.That(result.ShouldSend, Is.True);
        Assert.That(result.HasChange, Is.True);
        Assert.That(result.EventTag, Is.EqualTo("Dimension Change"));
    }

    [Test]
    public void AnalyzeFrame_KeyFrameInterval_ShouldForceSend()
    {
        _detector.KeyFrameInterval = 0; // Force immediate key frame
        
        using var frame1 = CreateTestBitmap(100, 100, Color.Red);
        using var frame2 = CreateTestBitmap(100, 100, Color.Red);
        
        _detector.AnalyzeFrame(frame1); // First frame
        var result = _detector.AnalyzeFrame(frame2); // Key frame interval
        
        Assert.That(result.ShouldSend, Is.True);
        Assert.That(result.HasChange, Is.True);
        Assert.That(result.EventTag, Is.EqualTo("Key Frame"));
    }

    [Test]
    public void Reset_ShouldClearPreviousFrame()
    {
        using var frame1 = CreateTestBitmap(100, 100, Color.Red);
        using var frame2 = CreateTestBitmap(100, 100, Color.Red);
        
        _detector.AnalyzeFrame(frame1);
        _detector.Reset();
        var result = _detector.AnalyzeFrame(frame2);
        
        Assert.That(result.ShouldSend, Is.True);
        // After reset, first frame is treated as "Initial Frame" or "Key Frame"
        Assert.That(result.EventTag, Is.EqualTo("Initial Frame").Or.EqualTo("Key Frame"));
    }

    private static Bitmap CreateTestBitmap(int width, int height, Color color)
    {
        var bmp = new Bitmap(width, height);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(color);
        }
        return bmp;
    }
}
