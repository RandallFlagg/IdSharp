using System;
using IdSharp.AudioInfo.Mpeg.Inspection;
using NUnit.Framework;

namespace IdSharp.Mpeg
{
    [TestFixture]
    internal static class PresetGuesserTests
    {
        [Test]
        public static void GuessPreset_ReturnsUnknown_ForVersionBelow3_90()
        {
            // VersionString must be at least 5 characters due to Substring(0, 5) in the code.
            var result = PresetGuesser.GuessPreset("3.89.0", 128, 78, 3, 2, 3, 2, 190);
            Assert.That(result.Preset, Is.EqualTo(LamePreset.Unknown));
        }

        [Test]
        public static void GuessPreset_ThrowsArgumentOutOfRangeException_ForEmptyVersionString()
        {
            // Substring(0, 4) on empty string throws ArgumentOutOfRangeException
            _ = Assert.Throws<ArgumentOutOfRangeException>(
                static () => _ = PresetGuesser.GuessPreset("", 0, 0, 0, 0, 0, 0, 0));
        }

        [Test]
        public static void GuessPreset_ThrowsArgumentOutOfRangeException_ForShortVersionString()
        {
            // Substring(0, 5) throws for strings shorter than 5 characters
            _ = Assert.Throws<ArgumentOutOfRangeException>(
                static () => _ = PresetGuesser.GuessPreset("3.90", 0, 0, 0, 0, 0, 0, 0));
        }

        [Test]
        public static void GuessPreset_Insane_ForKnownLame392Row()
        {
            // Row: (255, 58, 1, 1, 3, 2, 205, Insane, lvg390_3901_392)
            // Use "3.92a" to route to lvg390_3901_392 (VersionString4 == "3.92" branch)
            var result = PresetGuesser.GuessPreset("3.92a", 255, 58, 1, 1, 3, 2, 205);
            Assert.That(result.Preset, Is.EqualTo(LamePreset.Insane));
            Assert.That(result.NonBitrate, Is.False);
        }

        [Test]
        public static void GuessPreset_Extreme_ForKnownLame392Row()
        {
            // Row: (0, 78, 3, 2, 3, 2, 195, Extreme, lvg390_3901_392) — bitrate is 0 (VBR)
            // Use non-matching bitrate to trigger NonBitrate path
            byte nonMatchingBitrate = unchecked((byte)200);
            var result = PresetGuesser.GuessPreset("3.92a", nonMatchingBitrate, 78, 3, 2, 3, 2, 195);
            Assert.That(result.Preset, Is.EqualTo(LamePreset.Extreme));
            Assert.That(result.NonBitrate, Is.True);
        }

        [Test]
        public static void GuessPreset_Standard_ForKnownLame392Row()
        {
            // Row: (0, 78, 3, 2, 3, 4, 190, Standard, lvg390_3901_392, lvg3902_391) — bitrate is 0 (VBR)
            byte nonMatchingBitrate = unchecked((byte)200);
            var result = PresetGuesser.GuessPreset("3.92a", nonMatchingBitrate, 78, 3, 2, 3, 4, 190);
            Assert.That(result.Preset, Is.EqualTo(LamePreset.Standard));
            Assert.That(result.NonBitrate, Is.True);
        }

        [Test]
        public static void GuessPreset_Medium_ForKnownLame393Row()
        {
            // Row: (0, 68, 3, 2, 3, 4, 180, Medium, lvg3931_3903up)
            // 3.93 → BestGuessTwoVersions(lvg3931_3903up, lvg393)
            var result = PresetGuesser.GuessPreset("3.93.1", 0, 68, 3, 2, 3, 4, 180);
            Assert.That(result.Preset, Is.EqualTo(LamePreset.Medium));
        }

        [Test]
        public static void GuessPreset_R3mix_ForKnownLame392Row()
        {
            // Row: (0, 88, 4, 1, 3, 3, 195, R3mix, lvg390_3901_392)
            var result = PresetGuesser.GuessPreset("3.92a", 0, 88, 4, 1, 3, 3, 195);
            Assert.That(result.Preset, Is.EqualTo(LamePreset.R3mix));
        }

        [Test]
        public static void GuessPreset_Phone_ForKnownLame393Row()
        {
            // Row: (16, 58, 2, 2, 0, 2, 38, Phone, lvg3931_3903up)
            var result = PresetGuesser.GuessPreset("3.93.1", 16, 58, 2, 2, 0, 2, 38);
            Assert.That(result.Preset, Is.EqualTo(LamePreset.Phone));
        }

        [Test]
        public static void GuessPreset_Version394_UsesLvg394upGroup()
        {
            // Row: (255, 57, 1, 1, 3, 4, 205, Insane, lvg394up)
            var result = PresetGuesser.GuessPreset("3.94.0", 255, 57, 1, 1, 3, 4, 205);
            Assert.That(result.Preset, Is.EqualTo(LamePreset.Insane));
        }

        [Test]
        public static void GuessPreset_Version391_UsesLvg3902_391Group()
        {
            // Row: (0, 78, 3, 2, 3, 2, 196, Extreme, lvg3902_391)
            var result = PresetGuesser.GuessPreset("3.91.0", 0, 78, 3, 2, 3, 2, 196);
            Assert.That(result.Preset, Is.EqualTo(LamePreset.Extreme));
        }

        [Test]
        public static void GuessPreset_Version390Dot_UsesBestGuessTwoVersions()
        {
            // 3.90. → BestGuessTwoVersions(lvg3902_391, lvg3931_3903up)
            var result = PresetGuesser.GuessPreset("3.90.", 0, 78, 3, 2, 3, 2, 196);
            Assert.That(result.Preset, Is.EqualTo(LamePreset.Extreme));
        }

        [Test]
        public static void GuessPreset_NonBitrateFalse_WhenBitrateMatches()
        {
            // Row: (255, 58, 1, 1, 3, 2, 205, Insane, lvg390_3901_392) — bitrate 255 matches exactly
            var result = PresetGuesser.GuessPreset("3.92a", 255, 58, 1, 1, 3, 2, 205);
            Assert.That(result.NonBitrate, Is.False);
        }

        [Test]
        public static void GuessPreset_ReturnsUnknown_ForUnrecognizedTagValues()
        {
            // Valid version but no row matches these tag values
            var result = PresetGuesser.GuessPreset("3.92a", 200, 200, 9, 9, 9, 9, 0);
            Assert.That(result.Preset, Is.EqualTo(LamePreset.Unknown));
            Assert.That(result.NonBitrate, Is.False);
        }

        [Test]
        public static void GuessPreset_Version95_UsesLvg394upGroup()
        {
            // 3.95 and 3.95.1 map to lvg394up
            var result = PresetGuesser.GuessPreset("3.95.1", 255, 57, 2, 1, 3, 4, 205);
            Assert.That(result.Preset, Is.EqualTo(LamePreset.Studio));
        }

        [Test]
        public static void GuessPreset_NonBitrateTrue_WhenBitrateDoesNotMatchButEncodingMethodIsVbr()
        {
            // Row: (0, 78, 3, 2, 3, 2, 195, Extreme, lvg390_3901_392) — bitrate is 0 (VBR)
            byte nonMatchingBitrate = unchecked((byte)200);
            var result = PresetGuesser.GuessPreset("3.92a", nonMatchingBitrate, 78, 3, 2, 3, 2, 195);
            Assert.That(result.NonBitrate, Is.True);
            Assert.That(result.Preset, Is.EqualTo(LamePreset.Extreme));
        }

        [Test]
        public static void GuessPreset_NonBitrateFalse_WhenBitrateDoesNotMatchAndEncodingMethodIsNotVbr()
        {
            // encoding method 1 (CBR) — NonBitrate path only fires for encoding method 3 or 4
            byte nonMatchingBitrate = unchecked((byte)200);
            var result = PresetGuesser.GuessPreset("3.92a", nonMatchingBitrate, 78, 1, 2, 3, 2, 195);
            Assert.That(result.Preset, Is.EqualTo(LamePreset.Unknown));
            Assert.That(result.NonBitrate, Is.False);
        }
    }
}
