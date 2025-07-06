using IdSharp.Common.Events;

using NUnit.Framework;

namespace IdSharp.Common.Tests.Events;

[TestFixture]
public class CancelDataEventArgsTest
{
    private static CancelDataEventArgs<int> Get(int value)
    {
        return new CancelDataEventArgs<int>(value);
    }

    [Test]
    public void TestDataProperty()
    {
        const int value = 5;
        var x = Get(value);
        Assert.That(value, Is.EqualTo(x.Data));
    }

    [Test]
    public void TestCancelProperty_InitialFalse()
    {
        const int value = 5;
        var x = Get(value);
        Assert.That(false, Is.EqualTo(x.Cancel));
    }

    [Test]
    public void TestCancelProperty_SetTrue()
    {
        const int value = 5;
        var x = Get(value);
        x.Cancel = true;
        Assert.That(true, Is.EqualTo(x.Cancel));
    }

    [Test]
    public void TestCancelReasonProperty_InitialNull()
    {
        const int value = 5;
        var x = Get(value);
        Assert.That(null, Is.EqualTo(x.CancelReason));
    }

    [Test]
    public void TestCancelReasonProperty_SetNonNull()
    {
        const int value = 5;
        const string msg = "hello";
        var x = Get(value);
        x.CancelReason = msg;
        Assert.That(msg, Is.EqualTo(x.CancelReason));
    }
}
