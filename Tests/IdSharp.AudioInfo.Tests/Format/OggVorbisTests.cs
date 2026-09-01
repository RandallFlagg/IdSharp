using System;
using System.IO;
using IdSharp.AudioInfo;
using NUnit.Framework;

namespace IdSharp.AudioInfo.Tests.Format
{
    [TestFixture]
    internal static class OggVorbisTests
    {
        private static readonly string OggTestFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample.ogg");
        private static readonly string NonExistentPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "does_not_exist.ogg");

        [Test]
        public static void Construct_ReadsOggFile_Successfully()
        {
            Assert.That(File.Exists(OggTestFilePath), Is.True, "sample.ogg fixture not found");
            var ogg = new OggVorbis(OggTestFilePath);
            Assert.Multiple(() =>
            {
                Assert.That(ogg.Frequency, Is.GreaterThan(0));
                Assert.That(ogg.Channels, Is.GreaterThan(0));
                Assert.That(ogg.Samples, Is.GreaterThan(0));
                Assert.That(ogg.TotalSeconds, Is.GreaterThan(0));
                Assert.That(ogg.Bitrate, Is.GreaterThan(0));
            });
        }

        [Test]
        public static void FileType_IsOggVorbis()
        {
            Assume.That(File.Exists(OggTestFilePath), Is.True);
            var ogg = new OggVorbis(OggTestFilePath);
            Assert.That(ogg.FileType, Is.EqualTo(AudioFileType.OggVorbis));
        }

        [Test]
        public static void Frequency_IsExpectedValue_ForKnownFixture()
        {
            Assume.That(File.Exists(OggTestFilePath), Is.True);
            var ogg = new OggVorbis(OggTestFilePath);
            Assert.That(ogg.Frequency, Is.EqualTo(44100));
        }

        [Test]
        public static void Construct_ThrowsFileNotFoundException_ForNonExistentFile()
        {
            Assert.That(File.Exists(NonExistentPath), Is.False);
            _ = Assert.Throws<FileNotFoundException>(() => _ = new OggVorbis(NonExistentPath));
        }
    }
}
