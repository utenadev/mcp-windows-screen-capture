using NUnit.Framework;
using WindowsDesktopUse.Core;

namespace UnitTests;

[TestFixture]
public class MonitorSensitivityTests
{
    [Test]
    public void MonitorSensitivity_High_ShouldBe1Percent()
    {
        Assert.That((int)MonitorSensitivity.High, Is.EqualTo(1));
    }

    [Test]
    public void MonitorSensitivity_Medium_ShouldBe5Percent()
    {
        Assert.That((int)MonitorSensitivity.Medium, Is.EqualTo(5));
    }

    [Test]
    public void MonitorSensitivity_Low_ShouldBe15Percent()
    {
        Assert.That((int)MonitorSensitivity.Low, Is.EqualTo(15));
    }

    [Test]
    public void MonitorSession_DefaultValues()
    {
        var session = new MonitorSession();
        
        Assert.That(session.Sensitivity, Is.EqualTo(MonitorSensitivity.Medium));
        Assert.That(session.IntervalMs, Is.EqualTo(500));
    }

    [Test]
    public void MonitorSession_Dispose_ShouldCancelCts()
    {
        var session = new MonitorSession();
        
        session.Dispose();
        
        Assert.That(session.Cts.IsCancellationRequested, Is.True);
    }
}
