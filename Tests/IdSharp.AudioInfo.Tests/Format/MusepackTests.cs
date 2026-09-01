using System;
using System.IO;
using IdSharp.AudioInfo;
using NUnit.Framework;

namespace IdSharp.AudioInfo.Tests.Format
{
    [TestFixture]
    internal static class MusepackTests
    {
        private static readonly string MpcTestFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.mpc");

        [Test]
        public static void FileType_IsMusepack_WhenFileIsValid()
        {
            // Musepack fixture is not committed (ffmpeg doesn't support encoding MPC).
            // This test will be Inconclusive if the fixture is missing.
            Assume.That(File.Exists(MpcTestFilePath), Is.True,
                "sample.mpc fixture not found — ffmpeg cannot encode Musepack. " +
                "To run this test, generate a .mpc file manually and commit it.");
            var mpc = new Musepack(MpcTestFilePath);
            Assert.That(mpc.FileType, Is.EqualTo(AudioFileType.Musepack));
        }
    }
}
