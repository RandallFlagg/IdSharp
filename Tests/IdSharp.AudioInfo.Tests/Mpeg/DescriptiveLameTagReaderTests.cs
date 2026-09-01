using System;
using System.IO;
using IdSharp.AudioInfo.Mpeg.Inspection;
using NUnit.Framework;

namespace IdSharp.Mpeg
{
    [TestFixture]
    internal static class DescriptiveLameTagReaderTests
    {
        private static readonly string MP3TestFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "file_example_MP3_700KB.mp3");
        private static readonly string VbrMp3TestFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample_vbr.mp3");

        [Test]
        public static void Construct_ThrowsArgumentNullException_ForNullPath()
        {
            _ = Assert.Throws<ArgumentNullException>(static () => new DescriptiveLameTagReader(null!));
        }

        [Test]
        public static void Construct_ThrowsArgumentNullException_ForEmptyPath()
        {
            // The constructor uses string.IsNullOrEmpty, so empty string throws ArgumentNullException
            _ = Assert.Throws<ArgumentNullException>(static () => new DescriptiveLameTagReader(string.Empty));
        }

        [Test]
        public static void Construct_ThrowsFileNotFoundException_ForWhitespacePath()
        {
            // Whitespace is not caught by IsNullOrEmpty, so it reaches File.Open which throws FileNotFoundException
            _ = Assert.Throws<FileNotFoundException>(static () => new DescriptiveLameTagReader("   "));
        }

        [Test]
        public static void Construct_ThrowsDirectoryNotFoundException_ForNonExistentDirectory()
        {
            // On Linux, File.Open throws DirectoryNotFoundException when the parent directory doesn't exist.
            _ = Assert.Throws<DirectoryNotFoundException>(static () => new DescriptiveLameTagReader("/path/that/does/not/exist.mp3"));
        }

        [Test]
        public static void IsLameTagFound_ReturnsTrue_ForLameEncodedMp3()
        {
            var reader = new DescriptiveLameTagReader(MP3TestFilePath);
            Assert.That(reader.IsLameTagFound, Is.True);
        }

        [Test]
        public static void VersionString_ReturnsExpectedValue_ForKnownFixture()
        {
            var reader = new DescriptiveLameTagReader(MP3TestFilePath);
            Assert.That(reader.VersionString, Is.EqualTo("3.10"));
        }

        [Test]
        public static void VersionStringNonLameTag_ReturnsExpectedValue_ForKnownFixture()
        {
            var reader = new DescriptiveLameTagReader(MP3TestFilePath);
            Assert.That(reader.VersionStringNonLameTag, Does.StartWith("ME3.10"));
        }

        [Test]
        public static void LameTagInfoVersion_ContainsVersionAndLayer()
        {
            var reader = new DescriptiveLameTagReader(MP3TestFilePath);
            var info = reader.LameTagInfoVersion;
            Assert.That(info, Is.Not.Null.And.Not.Empty);
            // Should be "<Version> <Layer>" e.g. "MPEG 1 Layer III"
            Assert.That(info, Does.Contain(" "));
        }

        [Test]
        public static void LameTagInfoEncoder_ContainsLameForLameEncodedFile()
        {
            var reader = new DescriptiveLameTagReader(MP3TestFilePath);
            var encoder = reader.LameTagInfoEncoder;
            Assert.That(encoder, Does.Contain("LAME"));
        }

        [Test]
        public static void LameTagInfoEncoder_DoesNotContainVersion_WhenVersionStringIsPre3_90()
        {
            // The fixture's VersionString is "3.10" which is < "3.90" (FirstLameWithTag),
            // so the encoder string is just "LAME" (no version appended).
            var reader = new DescriptiveLameTagReader(MP3TestFilePath);
            var encoder = reader.LameTagInfoEncoder;
            Assert.That(encoder, Is.EqualTo("LAME"));
        }

        [Test]
        public static void LameTagInfoEncoder_StartsWithLame()
        {
            var reader = new DescriptiveLameTagReader(MP3TestFilePath);
            Assert.That(reader.LameTagInfoEncoder, Does.StartWith("LAME"));
        }

        [Test]
        public static void LameTagInfoPreset_IsEmpty_WhenVersionStringIsPre3_90()
        {
            // The fixture's VersionString is "3.10" which is < "3.90" (FirstLameWithTag),
            // so LameTagInfoPreset returns empty string (preset info only shown for 3.90+).
            var reader = new DescriptiveLameTagReader(MP3TestFilePath);
            Assert.That(reader.LameTagInfoPreset, Is.EqualTo(string.Empty));
        }

        [Test]
        public static void LameTagInfoPreset_ContainsV2ForPreset480()
        {
            var reader = new DescriptiveLameTagReader(MP3TestFilePath);
            // Even though LameTagInfoPreset is empty for pre-3.90, the raw Preset should contain V2
            Assert.That(reader.Preset, Does.Contain("V2"));
        }

        [Test]
        public static void Preset_ReturnsExpectedValue_ForKnownFixture()
        {
            // EncodingMethod == 4 and preset == 480 → "V2: preset standard (fast mode)"
            var reader = new DescriptiveLameTagReader(MP3TestFilePath);
            Assert.That(reader.Preset, Is.EqualTo("V2: preset standard (fast mode)"));
        }

        [Test]
        public static void PresetGuess_IsEmpty_WhenPresetIsKnown()
        {
            // When the preset is directly readable (not guessed), PresetGuess should be empty
            var reader = new DescriptiveLameTagReader(MP3TestFilePath);
            Assert.That(reader.PresetGuess, Is.EqualTo(string.Empty));
        }

        [Test]
        public static void UsePresetGuess_IsNotNeeded_WhenPresetIsKnown()
        {
            var reader = new DescriptiveLameTagReader(MP3TestFilePath);
            Assert.That(reader.UsePresetGuess, Is.EqualTo(UsePresetGuess.NotNeeded));
        }

        [Test]
        public static void IsPresetGuessNonBitrate_ReturnsFalse_ForKnownFixture()
        {
            var reader = new DescriptiveLameTagReader(MP3TestFilePath);
            Assert.That(reader.IsPresetGuessNonBitrate, Is.False);
        }

        [Test]
        public static void VbrMp3_DoesNotThrow()
        {
            Assume.That(File.Exists(VbrMp3TestFilePath), Is.True,
                "sample_vbr.mp3 fixture not found");
            Assert.DoesNotThrow(() => _ = new DescriptiveLameTagReader(VbrMp3TestFilePath));
        }

        [Test]
        public static void VbrMp3_LameTagInfoVersion_IsNotNull()
        {
            Assume.That(File.Exists(VbrMp3TestFilePath), Is.True,
                "sample_vbr.mp3 fixture not found");
            var reader = new DescriptiveLameTagReader(VbrMp3TestFilePath);
            Assert.That(reader.LameTagInfoVersion, Is.Not.Null.And.Not.Empty);
        }
    }
}
