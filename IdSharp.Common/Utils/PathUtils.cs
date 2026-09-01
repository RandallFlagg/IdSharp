using System.Text;
using System.Security.Cryptography;

namespace IdSharp.Common.Utils;

/// <summary>
/// PathUtils.
/// </summary>
public static class PathUtils
{
    private static readonly List<char> _invalidFileNameChars = new (Path.GetInvalidFileNameChars());

    /// <summary>
    /// Gets a unique file name which does not exist based on the specified path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>Unique file name which does not exist.</returns>
    public static string GetUniqueFileName(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (!File.Exists(path))
        {
            return path;
        }

            var basePath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
            var ext = Path.GetExtension(path);

            for (var i = 1; i == 1 || File.Exists(path); i++)
        {
            path = $"{basePath} ({i}){ext}";
        }

        return path;
    }

    /// <summary>
    /// Returns a unique file name in the temporary path with the specified extension.
    /// </summary>
    /// <param name="extension">The extension.</param>
    /// <returns>A unique file name in the temporary path with the specified extension.</returns>
    public static string GetTempFileName(string extension)
    {
        if (!string.IsNullOrEmpty(extension))
        {
            if (extension[0] != '.')
            {
                extension = "." + extension;
            }

                if (extension.EndsWith(".", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Parameter 'extension' cannot end with a '.'", nameof(extension));
            }

            foreach (var c in extension)
            {
                if (_invalidFileNameChars.Contains(c))
                {
                    throw new ArgumentException($"Parameter 'extension' cannot contain '{c}'", nameof(extension));
                }
            }

            if (extension.IndexOfAny(new[] { '?', '\\', ':' }) >= 0)
            {
                throw new ArgumentException("Parameter 'extension' contains a character that is invalid in a file name on at least one supported platform", nameof(extension));
            }
        }

        var tempPath = Path.GetTempPath();
        var guid = Guid.NewGuid().ToString();

        var fileName = Path.Combine(tempPath, guid) + extension;
        return GetUniqueFileName(fileName);
    }

    /// <summary>
    /// Gets a new temporary file name using the <paramref name="baseFileName"/> as a base.
    /// </summary>
    /// <param name="baseFileName">The base file name.</param>
    /// <returns>A new temporary file name with the same prefix as <paramref name="baseFileName"/>.</returns>
    public static string GetTemporaryFileNameBasedOnFileName(string baseFileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseFileName, nameof(baseFileName));

        string tempFile;
        var rnd = new Random();
        var randomBytes = new byte[8];
        do
        {
            for (var i = 0; i < randomBytes.Length; i++)
            {
                randomBytes[i] = (byte)RandomNumberGenerator.GetInt32(65, 91);
            }

            var randomString = Encoding.ASCII.GetString(randomBytes);
            tempFile = $"{baseFileName}.{randomString}.tmp";
        } while (File.Exists(tempFile));

        return tempFile;
    }
}
