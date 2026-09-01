using IdSharp.Tagging.ID3v2;
using NUnit.Framework;

namespace IdSharp.Tagging.Tests;

[TestFixture]
public class ID3v22_FrameTest : FrameTest
{
    public ID3v22_FrameTest()
        : base(ID3v2TagVersion.ID3v22)
    {
    }

    // Skipped: ID3v2.2 Commercial and Encryption frame types are not implemented in IdSharp.
    // Implementing these frame types is a feature, not a test, and is deferred to phase2.md.
    [Ignore("ID3v2.2 Commercial/Encryption frame types not implemented; see phase2.md")]
    public override void CommercialFrameWithLogo()
    {
        base.CommercialFrameWithLogo();
    }

    [Ignore("ID3v2.2 Commercial/Encryption frame types not implemented; see phase2.md")]
    public override void CommercialFrameWithoutLogo()
    {
        base.CommercialFrameWithLogo();
    }

    [Ignore("ID3v2.2 Commercial/Encryption frame types not implemented; see phase2.md")]
    public override void EncryptionMethodWithoutData()
    {
        base.EncryptionMethodWithoutData();
    }

    [Ignore("ID3v2.2 Commercial/Encryption frame types not implemented; see phase2.md")]
    public override void EncryptionMethodWithData()
    {
        base.EncryptionMethodWithData();
    }
}
