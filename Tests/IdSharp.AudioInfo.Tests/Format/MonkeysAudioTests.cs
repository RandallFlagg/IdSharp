using System;
using System.IO;
using IdSharp.AudioInfo;
using NUnit.Framework;

namespace IdSharp.AudioInfo.Tests.Format
{
    [TestFixture]
    internal static class MonkeysAudioTests
    {
        private static readonly string ApeTestFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.ape");

        [Test]
        public static void FileType_IsMonkeysAudio_WhenFileIsValid()
        {
            // Monkey's Audio fixture is not committed (ffmpeg doesn't support encoding APE).
            // This test will be Inconclusive if the fixture is missing.
            Assume.That(File.Exists(ApeTestFilePath), Is.True,
                "sample.ape fixture not found — ffmpeg cannot encode Monkey's Audio. " +
                "To run this test, generate a .ape file manually and commit it.");
            var ape = new MonkeysAudio(ApeTestFilePath);
            Assert.That(ape.FileType, Is.EqualTo(AudioFileType.MonkeysAudio));
        }
    }
}
