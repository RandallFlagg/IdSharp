using System;
using System.IO;
using IdSharp.Common.Utils;

namespace IdSharp.Tagging.APEv2;

public partial class APEv2Tag
{
    /// <summary>
    /// Gets the APEv2 tag size from a specified stream. Returns 0 if no tag exists.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public static int GetTagSize(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        var currentPosition = stream.Position;
        try
        {
            var apev2 = new APEv2Tag();
            apev2.Read(stream, readElements: false);
            return apev2.TagSize;
        }
        finally
        {
            stream.Position = currentPosition;
        }
    }

    /// <summary>
    /// Gets the APEv2 tag size from a specified path. Returns 0 if no tag exists.
    /// </summary>
    /// <param name="path">The full path of the file.</param>
    public static int GetTagSize(string path)
    {
        using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            return GetTagSize(fileStream);
        }
    }

    /// <summary>
    /// Returns <c>true</c> if an APEv2 tag exists in the specified stream; otherwise, <c>false</c>.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public static bool DoesTagExist(Stream stream)
    {
        return (GetTagSize(stream) != 0);
    }

    /// <summary>
    /// Returns <c>true</c> if an APEv2 tag exists in the specified path; otherwise, <c>false</c>.
    /// </summary>
    /// <param name="path">The full path of the file.</param>
    public static bool DoesTagExist(string path)
    {
        return (GetTagSize(path) != 0);
    }

    /// <summary>
    /// Removes an APEv2 tag from a specified path.
    /// </summary>
    /// <param name="path">The full path of the file.</param>
    /// <returns><c>true</c> if an APEv2 tag was removed; otherwise, <c>false</c>.</returns>
    public static bool RemoveTag(string path)
    {
        var apev2 = new APEv2Tag();
        using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            apev2.Read(fileStream, readElements: false);
        }

        var tagSize = apev2.TagSize;
        var tagOffset = apev2.TagOffset;
        if (tagSize > 0)
        {
            ByteUtils.ReplaceBytes(path, tagSize, Array.Empty<byte>(), tagOffset);
            return true;
        }
        else
        {
            return false;
        }
    }
}
