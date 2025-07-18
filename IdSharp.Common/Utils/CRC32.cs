using System;
using System.IO;

namespace IdSharp.Common.Utils;

/// <summary>
/// Calculates a 32-bit Cyclic Redundancy Checksum (CRC) using the
/// same polynomial used by Zip.
/// </summary>
public static class CRC32
{
    private static readonly uint[] crc32Table = new uint[256];
    private const int BUFFER_SIZE = 1024;

    static CRC32()
    {
        unchecked
        {
            // This is the official polynomial used by CRC32 in PKZip.
            // Often the polynomial is shown reversed as 0x04C11DB7.
            const uint dwPolynomial = 0xEDB88320;

            for (uint i = 0; i < 256; i++)
            {
                var dwCrc = i;
                for (uint j = 8; j > 0; j--)
                {
                    if ((dwCrc & 1) == 1)
                    {
                        dwCrc = (dwCrc >> 1) ^ dwPolynomial;
                    }
                    else
                    {
                        dwCrc >>= 1;
                    }
                }

                crc32Table[i] = dwCrc;
            }
        }
    }

    /// <summary>
    /// Returns the CRC32 Checksum of a specified file as a string.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>CRC32 Checksum as a string.</returns>
    public static string Calculate(FileInfo file)
    {
        return file == null ? throw new ArgumentNullException(nameof(file)) : $"{CalculateInt32(file):X8}";
    }

    /// <summary>
    /// Returns the CRC32 Checksum of an input stream as a string.
    /// </summary>
    /// <param name="stream">Input stream.</param>
    /// <returns>CRC32 Checksum as a string.</returns>
    public static string Calculate(Stream stream)
    {
        return stream == null ? throw new ArgumentNullException(nameof(stream)) : $"{CalculateInt32(stream):X8}";
    }

    /// <summary>
    /// Returns the CRC32 Checksum of a byte array as a string.
    /// </summary>
    /// <param name="data">The byte array.</param>
    /// <returns>CRC32 Checksum as a string.</returns>
    public static string Calculate(byte[] data)
    {
        return data == null ? throw new ArgumentNullException(nameof(data)) : $"{CalculateInt32(data):X8}";
    }

    /// <summary>
    /// Returns the CRC32 Checksum of a specified file as a four byte signed integer (Int32).
    /// </summary>
    /// <param name="file">The file.</param>
    /// <returns>CRC32 Checksum as a four byte signed integer (Int32).</returns>
    public static uint CalculateInt32(FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);

        using var fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        return CalculateInt32(fileStream);
    }

    /// <summary>
    /// Returns the CRC32 Checksum of an input stream as a four byte signed integer (Int32).
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>CRC32 Checksum as a four byte signed integer (Int32).</returns>
    public static uint CalculateInt32(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        unchecked
        {
            stream.Position = 0;
            var crc32Result = 0xFFFFFFFF;
            var buffer = new byte[BUFFER_SIZE];

            var count = stream.Read(buffer, 0, BUFFER_SIZE);
            while (count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    crc32Result = ((crc32Result) >> 8) ^ crc32Table[(buffer[i]) ^ ((crc32Result) & 0x000000FF)];
                }

                count = stream.Read(buffer, 0, BUFFER_SIZE);
            }

            return ~crc32Result;
        }
    }

    /// <summary>
    /// Returns the CRC32 Checksum of a byte array as a four byte signed integer (Int32).
    /// </summary>
    /// <param name="data">The byte array.</param>
    /// <returns>CRC32 Checksum as a four byte signed integer (Int32).</returns>
    public static uint CalculateInt32(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        using var memoryStream = new MemoryStream(data);
        return CalculateInt32(memoryStream);
    }
}