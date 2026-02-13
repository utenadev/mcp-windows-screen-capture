using NUnit.Framework;
using WindowsDesktopUse.Screen;

namespace UnitTests;

[TestFixture]
public class VideoTargetFinderTests
{
    private VideoTargetFinder _finder = null!;

    [SetUp]
    public void Setup()
    {
        _finder = new VideoTargetFinder();
    }

    [TearDown]
    public void TearDown()
    {
        _finder?.Dispose();
    }

    [Test]
    public void FindVideoTarget_ActiveWindow_ShouldReturnTarget()
    {
        var target = _finder.FindVideoTarget("ActiveWindow");
        
        Assert.That(target, Is.Not.Null);
        Assert.That(target!.WindowTitle, Is.Not.Null);
        Assert.That(target.Width, Is.GreaterThan(0));
        Assert.That(target.Height, Is.GreaterThan(0));
    }

    [Test]
    public void FindVideoTarget_InvalidTarget_ShouldReturnNull()
    {
        var target = _finder.FindVideoTarget("NonExistentVideoPlayerXYZ123");
        
        Assert.That(target, Is.Null);
    }

    [Test]
    public void StartTracking_ShouldNotThrow()
    {
        Assert.DoesNotThrow(() => _finder.StartTracking());
    }

    [Test]
    public void StopTracking_ShouldNotThrow()
    {
        _finder.StartTracking();
        Assert.DoesNotThrow(() => _finder.StopTracking());
    }

    [Test]
    public void GetUpdatedPosition_NotTracked_ShouldReturnNull()
    {
        var target = _finder.GetUpdatedPosition("NotTracked");
        
        Assert.That(target, Is.Null);
    }

    [Test]
    public void Dispose_ShouldNotThrow()
    {
        Assert.DoesNotThrow(() => _finder.Dispose());
    }
}
