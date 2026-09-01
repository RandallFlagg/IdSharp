using System.Runtime.InteropServices;

using IdSharp.AudioInfo.Mpeg.Inspection;

namespace IdSharp.AudioInfo.Inspection;

// Xing/FhG VBR header data
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VBRData : System.IEquatable<VBRData>
{
    public bool Found;                  // True if VBR header found
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U1)]
    public byte[] ID;                      // Header ID: "Xing" or "VBRI"
    public int Frames;                   // Total number of frames
    public int Bytes;                    // Total number of bytes
    public byte Scale;                     // VBR scale (1..100)
    public string VendorID;                // Vendor ID (if present)

    public bool Equals(VBRData other) =>
        Found == other.Found
        && ReferenceEquals(ID, other.ID) || (ID != null && other.ID != null && ID.AsSpan().SequenceEqual(other.ID))
        && Frames == other.Frames
        && Bytes == other.Bytes
        && Scale == other.Scale
        && VendorID == other.VendorID;

    public override bool Equals(object obj) => obj is VBRData other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Found, Frames, Bytes, Scale, VendorID);

    public static bool operator ==(VBRData left, VBRData right) => left.Equals(right);
    public static bool operator !=(VBRData left, VBRData right) => !left.Equals(right);
}

// MPEG frame header data
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct FrameData : System.IEquatable<FrameData>
{
    public bool Found;                  // True if frame found
    public int Position;                   // Frame position in the file
    public ushort Size;                    // Frame size (bytes)
    public bool Xing;                   // True if Xing encoder
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U1)]
    public byte[] Data;                    // The whole frame header data
    public MpegVersion VersionID;          // MPEG version ID
    public MpegLayer LayerID;              // MPEG layer ID
    public bool ProtectionBit;          // True if protected by CRC
    public ushort BitRateID;               // Bit rate ID
    public SampleRateLevel SampleRateID;   // Sample rate ID
    public bool PaddingBit;             // True if frame padded
    public bool PrivateBit;             // Extra information
    public MpegChannel ModeID;             // Channel mode ID
    public JointStereoExtensionMode ModeExtensionID;           // Mode extension ID (for Joint Stereo)
    public bool CopyrightBit;           // True if audio copyrighted
    public bool OriginalBit;            // True if original media
    public Emphasis EmphasisID;            // Emphasis ID

    public bool Equals(FrameData other) =>
        Found == other.Found
        && Position == other.Position
        && Size == other.Size
        && Xing == other.Xing
        && ReferenceEquals(Data, other.Data) || (Data != null && other.Data != null && Data.AsSpan().SequenceEqual(other.Data))
        && VersionID == other.VersionID
        && LayerID == other.LayerID
        && ProtectionBit == other.ProtectionBit
        && BitRateID == other.BitRateID
        && SampleRateID == other.SampleRateID
        && PaddingBit == other.PaddingBit
        && PrivateBit == other.PrivateBit
        && ModeID == other.ModeID
        && ModeExtensionID == other.ModeExtensionID
        && CopyrightBit == other.CopyrightBit
        && OriginalBit == other.OriginalBit
        && EmphasisID == other.EmphasisID;

    public override bool Equals(object obj) => obj is FrameData other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Found, Position, Size, Xing, VersionID, LayerID, BitRateID);

    public static bool operator ==(FrameData left, FrameData right) => left.Equals(right);
    public static bool operator !=(FrameData left, FrameData right) => !left.Equals(right);
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct LameTag : System.IEquatable<LameTag>
{
    public byte Quality;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U1)]
    public byte[] Encoder;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5, ArraySubType = UnmanagedType.U1)]
    public byte[] VersionString;
    public byte TagRevision_EncodingMethod;
    public byte Lowpass;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.U1)]
    public byte[] ReplayGain;
    public byte EncodingFlags_ATHType;
    public byte Bitrate;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.U1)]
    public byte[] EncoderDelays;
    public byte MiscInfo;
    public byte MP3Gain;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U1)]
    public byte[] Surround_Preset;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U1)]
    public byte[] MusicLength;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U1)]
    public byte[] MusicCRC;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U1)]
    public byte[] InfoTagCRC;
    public byte NoiseShaping;
    public byte StereoMode;

    public static LameTag FromBinaryReader(BinaryReader br)
    {
        var tmpLameTag = new LameTag
        {
            Quality = br.ReadByte(),
            Encoder = br.ReadBytes(4),
            VersionString = br.ReadBytes(5),
            TagRevision_EncodingMethod = (byte)(br.ReadByte() & 0x0F),
            Lowpass = br.ReadByte(),
            ReplayGain = br.ReadBytes(8),
            EncodingFlags_ATHType = (byte)(br.ReadByte() & 0x0F),
            Bitrate = br.ReadByte(),
            EncoderDelays = br.ReadBytes(3),
            MiscInfo = br.ReadByte(),
            MP3Gain = br.ReadByte(),
            Surround_Preset = br.ReadBytes(2),
            MusicLength = br.ReadBytes(4),
            MusicCRC = br.ReadBytes(2),
            InfoTagCRC = br.ReadBytes(2)
        };

        tmpLameTag.NoiseShaping = (byte)(tmpLameTag.MiscInfo & 0x03);
        tmpLameTag.StereoMode = (byte)((tmpLameTag.MiscInfo & 0x1C) >> 2);

        return tmpLameTag;
    }

    public bool Equals(LameTag other) =>
        Quality == other.Quality
        && ByteArrayEquals(Encoder, other.Encoder)
        && ByteArrayEquals(VersionString, other.VersionString)
        && TagRevision_EncodingMethod == other.TagRevision_EncodingMethod
        && Lowpass == other.Lowpass
        && ByteArrayEquals(ReplayGain, other.ReplayGain)
        && EncodingFlags_ATHType == other.EncodingFlags_ATHType
        && Bitrate == other.Bitrate
        && ByteArrayEquals(EncoderDelays, other.EncoderDelays)
        && MiscInfo == other.MiscInfo
        && MP3Gain == other.MP3Gain
        && ByteArrayEquals(Surround_Preset, other.Surround_Preset)
        && ByteArrayEquals(MusicLength, other.MusicLength)
        && ByteArrayEquals(MusicCRC, other.MusicCRC)
        && ByteArrayEquals(InfoTagCRC, other.InfoTagCRC)
        && NoiseShaping == other.NoiseShaping
        && StereoMode == other.StereoMode;

    public override bool Equals(object obj) => obj is LameTag other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Quality, TagRevision_EncodingMethod, Lowpass, Bitrate, NoiseShaping, StereoMode);

    public static bool operator ==(LameTag left, LameTag right) => left.Equals(right);
    public static bool operator !=(LameTag left, LameTag right) => !left.Equals(right);

    private static bool ByteArrayEquals(byte[] a, byte[] b) =>
        ReferenceEquals(a, b) || (a != null && b != null && a.AsSpan().SequenceEqual(b));
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct OldLameHeader : System.IEquatable<OldLameHeader>
{
    public byte UnusedByte;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U1)]
    public byte[] Encoder;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U1)]
    public byte[] VersionString;

    public static OldLameHeader FromBinaryReader(BinaryReader br)
    {
        var tmpOldLameHeader = new OldLameHeader();
        tmpOldLameHeader.UnusedByte = br.ReadByte();
        tmpOldLameHeader.Encoder = br.ReadBytes(4);
        tmpOldLameHeader.VersionString = br.ReadBytes(16);
        return tmpOldLameHeader;
    }

    public bool Equals(OldLameHeader other) =>
        UnusedByte == other.UnusedByte
        && ByteArrayEquals(Encoder, other.Encoder)
        && ByteArrayEquals(VersionString, other.VersionString);

    public override bool Equals(object obj) => obj is OldLameHeader other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(UnusedByte, Encoder?.Length ?? 0, VersionString?.Length ?? 0);

    public static bool operator ==(OldLameHeader left, OldLameHeader right) => left.Equals(right);
    public static bool operator !=(OldLameHeader left, OldLameHeader right) => !left.Equals(right);

    private static bool ByteArrayEquals(byte[] a, byte[] b) =>
        ReferenceEquals(a, b) || (a != null && b != null && a.AsSpan().SequenceEqual(b));
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct StartOfFile : System.IEquatable<StartOfFile>
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13, ArraySubType = UnmanagedType.U1)]
    public byte[] Misc1;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U1)]
	public byte[] Info1;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U1)]
	public byte[] Misc2;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U1)]
	public byte[] Info2;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11, ArraySubType = UnmanagedType.U1)]
	public byte[] Misc3;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U1)]
	public byte[] Info3;

    public static StartOfFile FromBinaryReader(BinaryReader br)
    {
        var tmpStartOfFile = new StartOfFile();
        tmpStartOfFile.Misc1 = br.ReadBytes(13);
        tmpStartOfFile.Info1 = br.ReadBytes(4);
        tmpStartOfFile.Misc2 = br.ReadBytes(4);
        tmpStartOfFile.Info2 = br.ReadBytes(4);
        tmpStartOfFile.Misc3 = br.ReadBytes(11);
        tmpStartOfFile.Info3 = br.ReadBytes(4);
        return tmpStartOfFile;
    }

    public bool Equals(StartOfFile other) =>
        ByteArrayEquals(Misc1, other.Misc1)
        && ByteArrayEquals(Info1, other.Info1)
        && ByteArrayEquals(Misc2, other.Misc2)
        && ByteArrayEquals(Info2, other.Info2)
        && ByteArrayEquals(Misc3, other.Misc3)
        && ByteArrayEquals(Info3, other.Info3);

    public override bool Equals(object obj) => obj is StartOfFile other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Misc1?.Length ?? 0, Info1?.Length ?? 0, Misc2?.Length ?? 0);

    public static bool operator ==(StartOfFile left, StartOfFile right) => left.Equals(right);
    public static bool operator !=(StartOfFile left, StartOfFile right) => !left.Equals(right);

    private static bool ByteArrayEquals(byte[] a, byte[] b) =>
        ReferenceEquals(a, b) || (a != null && b != null && a.AsSpan().SequenceEqual(b));
};
