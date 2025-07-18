using System.Collections.Generic;
using IdSharp.Common.Utils;

namespace IdSharp.Tagging.ID3v2;

/// <summary>
/// Static helper class for <see cref="PictureType"/>.
/// </summary>
public static class PictureTypeHelper
{
    private static readonly SortedDictionary<string, PictureType> m_PictureTypeDictionary;

    /// <summary>
    /// Gets the picture type strings.
    /// </summary>
    /// <value>The picture type strings.</value>
    public static string[] PictureTypeStrings { get; private set; }

    /// <summary>
    /// Gets the picture types.
    /// </summary>
    /// <value>The picture types.</value>
    public static PictureType[] PictureTypes { get; private set; }

    /// <summary>
    /// Gets the picture type from a string.
    /// </summary>
    /// <param name="pictureTypeString">The picture type string.</param>
    /// <returns>The picture type.</returns>
    public static PictureType GetPictureTypeFromString(string pictureTypeString)
    {
        PictureType pictureType;
        if (m_PictureTypeDictionary.TryGetValue(pictureTypeString, out pictureType))
        {
            return pictureType;
        }
        else
        {
            return PictureType.Other;
        }
    }

    /// <summary>
    /// Gets a string from a picture picture.
    /// </summary>
    /// <param name="pictureType">Type of the picture.</param>
    public static string GetStringFromPictureType(PictureType pictureType)
    {
        foreach (var pictureTypeKVP in m_PictureTypeDictionary)
        {
            if (pictureTypeKVP.Value == pictureType)
            {
                return pictureTypeKVP.Key;
            }
        }

        return "Other";
    }

    static PictureTypeHelper()
    {
        m_PictureTypeDictionary = new SortedDictionary<string, PictureType>();
        
        AddToPictureTypeDictionary(PictureType.Other);
        AddToPictureTypeDictionary(PictureType.FileIcon32x32Png);
        AddToPictureTypeDictionary(PictureType.OtherFileIcon);
        AddToPictureTypeDictionary(PictureType.CoverFront);
        AddToPictureTypeDictionary(PictureType.CoverBack);
        AddToPictureTypeDictionary(PictureType.LeafletPage);
        AddToPictureTypeDictionary(PictureType.MediaLabelSideOfCD);
        AddToPictureTypeDictionary(PictureType.LeadArtistPerformer);
        AddToPictureTypeDictionary(PictureType.ArtistPerformer);
        AddToPictureTypeDictionary(PictureType.Conductor);
        AddToPictureTypeDictionary(PictureType.BandOrchestra);
        AddToPictureTypeDictionary(PictureType.Composer);
        AddToPictureTypeDictionary(PictureType.Lyricist);
        AddToPictureTypeDictionary(PictureType.RecordingLocation);
        AddToPictureTypeDictionary(PictureType.DuringRecording);
        AddToPictureTypeDictionary(PictureType.DuringPerformance);
        AddToPictureTypeDictionary(PictureType.MovieVideoScreenCapture);
        AddToPictureTypeDictionary(PictureType.Illustration);
        AddToPictureTypeDictionary(PictureType.BandArtistLogo);
        AddToPictureTypeDictionary(PictureType.PublisherStudioLogo);

        PictureTypeStrings = new string[m_PictureTypeDictionary.Count];
        PictureTypes = new PictureType[m_PictureTypeDictionary.Count];
        
        var i = 0;
        foreach (var kvp in m_PictureTypeDictionary)
        {
            PictureTypeStrings[i] = kvp.Key;
            PictureTypes[i] = kvp.Value;

            i++;
        }
    }

    private static void AddToPictureTypeDictionary(PictureType pictureType)
    {
        var enumString = EnumUtils.GetDescription(pictureType);

        m_PictureTypeDictionary.Add(enumString, pictureType);
    }
}
