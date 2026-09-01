namespace IdSharp.AudioInfo.Mpeg.Inspection;

// MPEG version codes
// Duplicate values are intentional aliases (Mpeg1/Version1, Mpeg2/Version2, Mpeg25/Version25)
// that expose both naming conventions for the same MPEG spec concept.
#pragma warning disable CA1069
internal enum MpegVersion
{
    Unknown = 1,
    /// <summary>
    /// MPEG-1
    /// </summary>
    Mpeg1 = 3,
    Version1 = 3,
    /// <summary>
    /// MPEG-2
    /// </summary>
    Mpeg2 = 2,
    Version2 = 2,
    /// <summary>
    /// MPEG-2.5
    /// </summary>
    Mpeg25 = 0,
    Version25 = 0,
}
#pragma warning restore CA1069

// MPEG layer codes
// Duplicate values are intentional aliases (LayerI/Layer1, LayerII/Layer2, LayerIII/Layer3)
// that expose both naming conventions for the same MPEG layer concept.
#pragma warning disable CA1069
internal enum MpegLayer
{
    Unknown = 0,
        /// <summary>
        /// Layer I
        /// </summary>
    LayerI = 3,
    Layer1 = 3,
    /// <summary>
    /// Layer II
    /// </summary>
    LayerII = 2,
    Layer2 = 2,
    /// <summary>
    /// Layer III
    /// </summary>
    LayerIII = 1,
    Layer3 = 1
}
#pragma warning restore CA1069

// Channel mode codes
internal enum MpegChannel
{
    Stereo = 0,
    JointStereo = 1,
    DualChannel = 2,
    Mono = 3,
    Unknown = 4
}

// Extension mode codes (for Joint Stereo)
internal enum JointStereoExtensionMode
{
    Off = 0,
    IS = 1,
    MS = 2,
    On = 3,
    Unknown = 4
}

// Emphasis mode codes
internal enum Emphasis
{
    None = 0,
    _5015 = 1,
    Unknown = 2,
    CCIT = 3
}

// Sample rate codes
internal enum SampleRateLevel
{
    Level3 = 0,
    Level2 = 1,
    Level1 = 2,
    Unknown = 3
}

// Encoder codes
internal enum MpegEncoder
{
    Unknown,
    Xing,
    FhG,
    LAME,
    Blade,
    GoGo,
    Shine,
    QDesign
}

// VBR header ID for Xing/FhG
internal static class VBRHeaderID
{
    private const string m_Xing = "Xing";
    private const string m_FhG = "VBRI";

    public static string Xing { get { return m_Xing; } }
    public static string FhG { get { return m_FhG; } }
}

// VBR Vendor ID strings
internal static class VBRVendorID
{
    private const string m_LAME = "LAME";
    private const string m_GoGoNew = "GOGO";
    private const string m_GoGoOld = "MPGE";

    public static string LAME { get { return m_LAME; } }
    public static string GoGoNew { get { return m_GoGoNew; } }
    public static string GoGoOld { get { return m_GoGoOld; } }
}

/// <summary>
/// Preset guess status
/// </summary>
public enum UsePresetGuess
{
    /// <summary>
    /// Not needed
    /// </summary>
    NotNeeded,
    /// <summary>
    /// Use guess
    /// </summary>
    UseGuess,
    /// <summary>
    /// Unabled to guess
    /// </summary>
    UnableToGuess
}

/// <summary>
/// Lame Preset
/// </summary>
public enum LamePreset
{
    /// <summary>
    /// --[alt-]preset insane
    /// </summary>
    Insane,
    /// <summary>
    /// --[alt-]preset extreme
    /// </summary>
    Extreme,
    /// <summary>
    /// --[alt-]preset fast extreme
    /// </summary>
    FastExtreme,
    /// <summary>
    /// --[alt-]preset standard
    /// </summary>
    Standard,
    /// <summary>
    /// --[alt-]preset fast standard
    /// </summary>
    FastStandard,
    /// <summary>
    /// --preset medium
    /// </summary>
    Medium,
    /// <summary>
    /// --preset medium
    /// </summary>
    FastMedium,
    /// <summary>
    /// --r3mix
    /// </summary>
    R3mix,
    /// <summary>
    /// --preset studio
    /// </summary>
    Studio,
    /// <summary>
    /// --preset cd 
    /// </summary>
    CD,
    /// <summary>
    /// --preset hifi
    /// </summary>
    Hifi,
    /// <summary>
    /// --preset tape
    /// </summary>
    Tape,
    /// <summary>
    /// --preset radio
    /// </summary>
    Radio,
    /// <summary>
    /// --preset fm 
    /// </summary>
    FM,
    /// <summary>
    /// --preset tape OR --preset radio OR --preset fm
    /// the above three presets generate identical files for
    /// versions of lame from 3.90.3 and 3.93 up
    /// </summary>
    TapeRadioFM,
    /// <summary>
    /// --preset voice
    /// </summary>
    Voice,
    /// <summary>
    /// --preset mw-us
    /// </summary>
    MWUS,
    /// <summary>
    /// --preset phon+ OR --preset lw OR --preset mw-eu OR --preset sw
    /// </summary>
    MWEU,
    /// <summary>
    /// --preset phone
    /// </summary>
    Phone,
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown
};

internal enum LameVersionGroup
{
    lvg390_3901_392,
    lvg3902_391,
    lvg393,
    lvg3931_3903up,
    lvg394up,
    None
};
