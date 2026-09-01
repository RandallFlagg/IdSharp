using System.Runtime.InteropServices;
using System.Text;

using IdSharp.AudioInfo.Inspection;
using IdSharp.AudioInfo.Mpeg.Inspection;

namespace IdSharp.AudioInfo.Mpeg.Mpeg.Inspection;

/// <summary>
/// Basic LAME tag reader
/// </summary>
internal sealed class BasicLameTagReader
{
    private const byte Info1Offset = 0x0D;
    private const byte Info2Offset = 0x15;
    private const byte Info3Offset = 0x24;
    private const byte LAMETagOffset = 0x77;

    private readonly LameTag _tag;
    private readonly bool _isPresetGuessNonBitrate;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicLameTagReader"/> class.
    /// </summary>
    /// <param name="path">The path.</param>
    public BasicLameTagReader(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        // Initialize
        using var br = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));

        IsLameTagFound = true;//TODO: Delete?


        var startPos = ID3v2.GetTagSize(br.BaseStream);

        // Seek past ID3v2 tag
        br.BaseStream.Seek(startPos, SeekOrigin.Begin); //Find frame header after tag

        // Get StartOfFile structure
        var startOfFile = StartOfFile.FromBinaryReader(br);

        // Seek past ID3v2 tag
        br.BaseStream.Seek(startPos, SeekOrigin.Begin); //Go back to the begining of the frame header

        var info1 = Encoding.ASCII.GetString(startOfFile.Info1);
        var info2 = Encoding.ASCII.GetString(startOfFile.Info2);
        var info3 = Encoding.ASCII.GetString(startOfFile.Info3);

        if (info1 is "Xing" or "Info")
        {
            br.BaseStream.Seek(Info1Offset, SeekOrigin.Current);
        }
        else if (info2 is "Xing" or "Info")
        {
            br.BaseStream.Seek(Info2Offset, SeekOrigin.Current);
        }
        else if (info3 is "Xing" or "Info")
        {
            br.BaseStream.Seek(Info3Offset, SeekOrigin.Current);
        }
        else
        {
            IsLameTagFound = true;
        }

        // Read LAME tag structure
        br.BaseStream.Seek(LAMETagOffset, SeekOrigin.Current);
        _tag = LameTag.FromBinaryReader(br);

        // Read old LAME header
        br.BaseStream.Seek(0 - Marshal.SizeOf(typeof(LameTag)), SeekOrigin.Current);
        var oldLameHeader = OldLameHeader.FromBinaryReader(br);
        VersionStringNonLameTag = Encoding.ASCII.GetString(oldLameHeader.VersionString);

        // Set version string
        if (_tag.VersionString[1] == '.')
        {
            var versionString = new byte[6];
            int i;
            for (i = 0; i < 4 || i == 4 && _tag.VersionString[i] == 'b'; i++)
            {
                versionString[i] = _tag.VersionString[i];
            }

            Array.Resize(ref versionString, i);
            VersionString = Encoding.ASCII.GetString(versionString);
        }
        else
        {
            VersionString = "";
        }

        // If encoder is not LAME, set IsLameTagFound to false
        // TODO : How about other encoders that use the LAME tag?
        if (Encoding.ASCII.GetString(_tag.Encoder) != "LAME")
        {
            IsLameTagFound = false;
        }

        // Set preset WORD
        Preset = (ushort)((_tag.Surround_Preset[0] << 8) + _tag.Surround_Preset[1] & 0x07FF);

        // Guess preset
        var result = new PresetGuesser().GuessPreset(
            VersionStringNonLameTag, /*m_Tag.VersionString*/
            _tag.Bitrate,
            _tag.Quality,
            _tag.TagRevision_EncodingMethod,
            _tag.NoiseShaping,
            _tag.StereoMode,
            _tag.EncodingFlags_ATHType,
            _tag.Lowpass);
        PresetGuess = result.Preset;
        _isPresetGuessNonBitrate = result.NonBitrate;

    }

    /// <summary>
    /// Returns the version string from the LAME tag
    /// </summary>
    public string VersionString { get; }

    /// <summary>
    /// Returns the version string from the old LAME header (pre-3.90)
    /// </summary>
    public string VersionStringNonLameTag { get; }

    /// <summary>
    /// Returns Encoding Method Byte
    /// </summary>
    public byte EncodingMethod
    {
        get { return _tag.TagRevision_EncodingMethod; }
    }

    /// <summary>
    /// Returns Preset WORD
    /// </summary>
    public ushort Preset { get; }

    /// <summary>
    /// Returns guessed preset enum
    /// </summary>
    public LamePreset PresetGuess { get; }

    /// <summary>
    /// Returns bitrate from the LAME tag (not the actual bitrate for VBR files)
    /// </summary>
    public byte Bitrate
    {
        get { return _tag.Bitrate; }
    }

    /// <summary>
    /// Returns true if the preset is guessed to be a command-line modified version of a preset.
    /// Only applies to LAME encoded MP3's that do not have preset info stored in the LAME tag.
    /// </summary>
    public bool IsPresetGuessNonBitrate
    {
        get { return _isPresetGuessNonBitrate; }
    }

    /// <summary>
    /// Returns true if a LAME tag is present
    /// </summary>
    public bool IsLameTagFound { get; }
}
