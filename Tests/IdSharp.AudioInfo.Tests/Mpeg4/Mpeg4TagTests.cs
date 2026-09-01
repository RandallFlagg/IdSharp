using System;
using System.IO;
using IdSharp.AudioInfo;
using NUnit.Framework;

namespace IdSharp.AudioInfo.Tests.Mpeg4
{
    [TestFixture]
    internal static class Mpeg4TagTests
    {
        private static readonly string TestDataDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
        private static readonly string M4aTestFilePath = Path.Combine(TestDataDir, "sample.m4a");
        private static readonly string NonExistentPath = Path.Combine(TestDataDir, "does_not_exist.mp4");

        [Test]
        public static void Construct_ThrowsFileNotFoundException_ForNonExistentPath()
        {
            Assert.That(File.Exists(NonExistentPath), Is.False);
            var ex = Assert.Throws<FileNotFoundException>(
                static () => new Mpeg4Tag(NonExistentPath));
            Assert.That(ex.Message, Does.Contain("does_not_exist").IgnoreCase);
        }

        [Test]
        public static void ReadRealM4aFile_ParsesAudioInfo()
        {
            Assert.That(File.Exists(M4aTestFilePath), Is.True, "sample.m4a fixture not found");

            var tag = new Mpeg4Tag(M4aTestFilePath);

            Assert.Multiple(() =>
            {
                Assert.That(tag.Frequency, Is.GreaterThan(0), "Frequency should be greater than 0");
                Assert.That(tag.Samples, Is.GreaterThan(0), "Samples should be greater than 0");
                Assert.That(tag.Channels, Is.GreaterThan(0), "Channels should be greater than 0");
                Assert.That(tag.Codec, Is.Not.Null.And.Not.Empty, "Codec should be set");
                Assert.That(tag.MdatAtomSize, Is.GreaterThan(0), "MdatAtomSize should be greater than 0");
            });
        }

        [Test]
        public static void ReadRealM4aFile_FrequencyIsExpectedValue()
        {
            Assert.That(File.Exists(M4aTestFilePath), Is.True);

            var tag = new Mpeg4Tag(M4aTestFilePath);

            // The sample was generated at 44100 Hz (default ffmpeg AAC output)
            Assert.That(tag.Frequency, Is.EqualTo(44100));
        }

        [Test]
        public static void ReadRealM4aFile_StereoChannels()
        {
            Assert.That(File.Exists(M4aTestFilePath), Is.True);

            var tag = new Mpeg4Tag(M4aTestFilePath);

            Assert.That(tag.Channels, Is.EqualTo(2));
        }

        [Test]
        public static void ReadRealM4aFile_ChannelsIsPositive()
        {
            Assert.That(File.Exists(M4aTestFilePath), Is.True);

            var tag = new Mpeg4Tag(M4aTestFilePath);

            Assert.That(tag.Channels, Is.GreaterThan(0));
        }

        [Test]
        public static void ReadRealM4aFile_CodecIsAac()
        {
            Assert.That(File.Exists(M4aTestFilePath), Is.True);

            var tag = new Mpeg4Tag(M4aTestFilePath);

            Assert.That(tag.Codec, Is.EqualTo("AAC"));
        }

        [Test]
        public static void ReadRealM4aFile_TotalSecondsCalculatedCorrectly()
        {
            Assert.That(File.Exists(M4aTestFilePath), Is.True);

            var tag = new Mpeg4Tag(M4aTestFilePath);

            var mpeg4 = new IdSharp.AudioInfo.Mpeg4(M4aTestFilePath);
            Assert.That(mpeg4.TotalSeconds, Is.GreaterThan(0));
            Assert.That(mpeg4.Bitrate, Is.GreaterThan(0));
        }

        [Test]
        public static void ReadStream_EmptyStream_DoesNotThrowAndDefaultsAreZero()
        {
            var filePath = Path.Combine(TestDataDir, "_empty_m4a_test_tmp");
            File.WriteAllBytes(filePath, Array.Empty<byte>());
            try
            {
                var tag = new Mpeg4Tag(filePath);
                Assert.Multiple(() =>
                {
                    Assert.That(tag.MdatAtomSize, Is.EqualTo(0));
                    Assert.That(tag.Frequency, Is.EqualTo(0));
                    Assert.That(tag.Samples, Is.EqualTo(0));
                    Assert.That(tag.Channels, Is.EqualTo(0));
                    Assert.That(tag.Codec, Is.Null);
                });
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public static void ReadStream_MdatOnly_SetsMdatAtomSize()
        {
            var filePath = Path.Combine(TestDataDir, "_mdat_only_test_tmp");
            File.WriteAllBytes(filePath, BuildAtom("mdat", new byte[1000]));
            try
            {
                var tag = new Mpeg4Tag(filePath);
                Assert.That(tag.MdatAtomSize, Is.EqualTo(1008));
                Assert.That(tag.Frequency, Is.EqualTo(0));
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public static void ReadStream_MdatInsideMoov_SetsMdatAtomSize()
        {
            var mdat = BuildAtom("mdat", new byte[1000]);
            var moov = BuildContainer("moov", mdat);
            var filePath = Path.Combine(TestDataDir, "_mdat_in_moov_test_tmp");
            File.WriteAllBytes(filePath, moov);
            try
            {
                var tag = new Mpeg4Tag(filePath);
                Assert.That(tag.MdatAtomSize, Is.EqualTo(1008));
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public static void ReadStream_ParsesMdhdAtom()
        {
            var mdhd = BuildMdhd(44100, 176400);
            var moov = BuildContainer("moov", mdhd);
            var filePath = Path.Combine(TestDataDir, "_mdhd_test_tmp");
            File.WriteAllBytes(filePath, moov);
            try
            {
                var tag = new Mpeg4Tag(filePath);
                Assert.Multiple(() =>
                {
                    Assert.That(tag.Frequency, Is.EqualTo(44100));
                    Assert.That(tag.Samples, Is.EqualTo(176400));
                });
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public static void ReadStream_ParsesStsdAtom_Stereo()
        {
            var stsd = BuildStsd(2, 44100);
            var moov = BuildContainer("moov", stsd);
            var filePath = Path.Combine(TestDataDir, "_stsd_stereo_test_tmp");
            File.WriteAllBytes(filePath, moov);
            try
            {
                var tag = new Mpeg4Tag(filePath);
                Assert.Multiple(() =>
                {
                    Assert.That(tag.Channels, Is.EqualTo(2));
                    Assert.That(tag.Frequency, Is.EqualTo(44100));
                    Assert.That(tag.Codec, Is.EqualTo("AAC"));
                });
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public static void ReadStream_ParsesStsdAtom_Mono()
        {
            var stsd = BuildStsd(1, 22050);
            var moov = BuildContainer("moov", stsd);
            var filePath = Path.Combine(TestDataDir, "_stsd_mono_test_tmp");
            File.WriteAllBytes(filePath, moov);
            try
            {
                var tag = new Mpeg4Tag(filePath);
                Assert.Multiple(() =>
                {
                    Assert.That(tag.Channels, Is.EqualTo(1));
                    Assert.That(tag.Frequency, Is.EqualTo(22050));
                });
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public static void ReadStream_ParsesStsdAtom_NonMp4aCodec()
        {
            var stsd = BuildStsdNonMp4a(2, 44100, "abcd");
            var moov = BuildContainer("moov", stsd);
            var filePath = Path.Combine(TestDataDir, "_stsd_non_mp4a_test_tmp");
            File.WriteAllBytes(filePath, moov);
            try
            {
                var tag = new Mpeg4Tag(filePath);
                Assert.That(tag.Codec, Is.EqualTo("abcd"));
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public static void ReadStream_ParsesFullMp4()
        {
            var mdhd = BuildMdhd(48000, 96000);
            var stsd = BuildStsd(2, 48000);
            var mdat = BuildAtom("mdat", new byte[5000]);
            var moov = BuildContainer("moov", mdhd, stsd);
            var combined = CombineAtoms(moov, mdat);
            var filePath = Path.Combine(TestDataDir, "_full_mp4_test_tmp");
            File.WriteAllBytes(filePath, combined);
            try
            {
                var tag = new Mpeg4Tag(filePath);
                Assert.Multiple(() =>
                {
                    Assert.That(tag.Frequency, Is.EqualTo(48000));
                    Assert.That(tag.Samples, Is.EqualTo(96000));
                    Assert.That(tag.Channels, Is.EqualTo(2));
                    Assert.That(tag.Codec, Is.EqualTo("AAC"));
                    Assert.That(tag.MdatAtomSize, Is.EqualTo(5008));
                });
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public static void ReadStream_ParsesMultipleFreeAtomsWithoutThrowing()
        {
            var free1 = BuildAtom("free", new byte[100]);
            var free2 = BuildAtom("free", new byte[200]);
            var moov = BuildContainer("moov", free1, free2);
            var filePath = Path.Combine(TestDataDir, "_free_atoms_test_tmp");
            File.WriteAllBytes(filePath, moov);
            try
            {
                Assert.DoesNotThrow(() => _ = new Mpeg4Tag(filePath));
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public static void ReadStream_AtomSizeExceedsFile_ThrowsInvalidDataException()
        {
            // Build an atom whose size field claims more bytes than the file contains
            var badAtom = new byte[8];
            WriteBigEndianInt32(badAtom, 0, 9999);
            badAtom[4] = (byte)'m'; badAtom[5] = (byte)'d'; badAtom[6] = (byte)'a'; badAtom[7] = (byte)'t';
            var filePath = Path.Combine(TestDataDir, "_bad_atom_size_test_tmp");
            File.WriteAllBytes(filePath, badAtom);
            try
            {
                Assert.Throws<InvalidDataException>(() => _ = new Mpeg4Tag(filePath));
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public static void ReadStream_MdatOnlyAtTopLevel_SetsMdatAtomSize()
        {
            var mdat = BuildAtom("mdat", new byte[500]);
            var filePath = Path.Combine(TestDataDir, "_mdat_top_level_test_tmp");
            File.WriteAllBytes(filePath, mdat);
            try
            {
                var tag = new Mpeg4Tag(filePath);
                Assert.That(tag.MdatAtomSize, Is.EqualTo(508));
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        private static byte[] BuildAtom(string name, byte[] data = null)
        {
            var dataLen = data?.Length ?? 0;
            var atom = new byte[8 + dataLen];
            WriteBigEndianInt32(atom, 0, 8 + dataLen);
            for (int i = 0; i < name.Length; i++)
                atom[4 + i] = (byte)name[i];
            if (data != null)
                Buffer.BlockCopy(data, 0, atom, 8, dataLen);
            return atom;
        }

        private static byte[] BuildContainer(string name, params byte[][] children)
        {
            var total = 0;
            foreach (var c in children) total += c.Length;
            var data = new byte[total];
            var off = 0;
            foreach (var c in children)
            {
                Buffer.BlockCopy(c, 0, data, off, c.Length);
                off += c.Length;
            }
            return BuildAtom(name, data);
        }

        private static void WriteBigEndianInt32(byte[] buf, int offset, int value)
        {
            buf[offset]     = (byte)((value >> 24) & 0xFF);
            buf[offset + 1] = (byte)((value >> 16) & 0xFF);
            buf[offset + 2] = (byte)((value >> 8)  & 0xFF);
            buf[offset + 3] = (byte)( value        & 0xFF);
        }

        private static byte[] BuildMdhd(int timescale, int duration)
        {
            var data = new byte[20];
            data[0] = 0; // version
            WriteBigEndianInt32(data, 12, timescale);
            WriteBigEndianInt32(data, 16, duration);
            return BuildAtom("mdhd", data);
        }

        private static byte[] BuildStsd(int channels, int sampleRate)
        {
            return BuildStsdWithFormat(channels, sampleRate, 'm', 'p', '4', 'a');
        }

        private static byte[] BuildStsdNonMp4a(int channels, int sampleRate, string fourcc)
        {
            if (fourcc.Length != 4)
                throw new ArgumentException("FourCC must be 4 characters", nameof(fourcc));
            return BuildStsdWithFormat(channels, sampleRate, fourcc[0], fourcc[1], fourcc[2], fourcc[3]);
        }

        private static byte[] BuildStsdWithFormat(int channels, int sampleRate, char f0, char f1, char f2, char f3)
        {
            // stsd atom layout (from ParseStsdAtom code path):
            //   [0-3]  size of the entire stsd atom (set by BuildAtom)
            //   [4-7]  "stsd"
            //   [8-11] version/flags (= 0)
            //   [12-15] num_entries (= 1)
            //   [16-19] entry size
            //   [20-23] format (e.g. "mp4a")
            //   [24-27] reserved (12 bytes total) — 4 bytes of "pre_defined + reserved" before the data the code reads
            //   [28-35] data[0..7]   (encoder_vendor check lives at data[4] = atomdata[32], must be 0)
            //   [36-37] data[8..9]   = channelcount  (Channels = (data[8] << 8) + data[9])
            //   [38-43] data[10..15]
            //   [44-45] data[16..17] = samplerate high bytes (Frequency = (data[16] << 8) + data[17])
            //   [46-51] data[18..23] (zeroed)
            //
            // The code does:
            //   stsdOff = 8; size = ReadInt32(atomdata, 8); stsdOff = 12; read format; stsdOff += 12 -> 24;
            //   data = new byte[size - 4 - 12]; BlockCopy(atomdata, 24, data, 0, data.Length)
            // So data starts at atomdata offset 24.
            const int dataLen = 24;
            const int entrySize = 4 + 4 + 12 + dataLen; // size(4) + format(4) + reserved(12) + data(24) = 44

            var atomdata = new byte[4 + 4 + entrySize];   // 52 bytes total
            WriteBigEndianInt32(atomdata, 4, 1);            // num_entries = 1
            WriteBigEndianInt32(atomdata, 8, entrySize);    // entry size = 44
            atomdata[12] = (byte)f0; atomdata[13] = (byte)f1;
            atomdata[14] = (byte)f2; atomdata[15] = (byte)f3;
            // reserved bytes 16-27 stay zero (data[0..3] = atomdata[24..27] = zero,
            // and data[4] = atomdata[28] = zero so encoder_vendor[0] == 0)
            var d = 24;
            atomdata[d + 8]  = (byte)((channels   >> 8) & 0xFF);
            atomdata[d + 9]  = (byte)( channels      & 0xFF);
            atomdata[d + 16] = (byte)((sampleRate >> 8) & 0xFF);
            atomdata[d + 17] = (byte)( sampleRate    & 0xFF);
            return BuildAtom("stsd", atomdata);
        }

        private static byte[] CombineAtoms(params byte[][] atoms)
        {
            var total = 0;
            foreach (var a in atoms) total += a.Length;
            var result = new byte[total];
            var off = 0;
            foreach (var a in atoms)
            {
                Buffer.BlockCopy(a, 0, result, off, a.Length);
                off += a.Length;
            }
            return result;
        }
    }
}
