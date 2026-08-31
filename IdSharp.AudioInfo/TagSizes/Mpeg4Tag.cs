using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IdSharp.Common.Utils;

namespace IdSharp.AudioInfo;

/// <summary>
/// MPEG-4
/// </summary>
internal class Mpeg4Tag
{
    private static readonly string[] ATOM_TYPES = new[]{"moov", "trak", "udta", "tref", "imap", "mdia",
		"minf", "stbl", "edts", "mdra", "rmra", "imag", "vnrp", "dinf" };

    private readonly List<Atom> _atoms = new List<Atom>();

    private int _freesize;

    internal Mpeg4Tag(string path)
    {
        Read(path);
    }

    private class Atom
    {
        public string name;
        public int level;
    }

    #region <<< Internal Properties >>>

    internal int MdatAtomSize { get; private set; }

    internal int Samples { get; private set; }

    internal int Channels { get; private set; }

    internal int Frequency { get; private set; }

    internal String Codec { get; private set; }

    #endregion <<< Internal Properties >>>

    private static int ReadInt32(byte[] data, int offset)
    {
        var n = (data[offset] & 0xff) << 24;
        n += (data[offset + 1] & 0xff) << 16;
        n += (data[offset + 2] & 0xff) << 8;
        n += (data[offset + 3] & 0xff);
        return n;
    }

    private void ParseStsdAtom(byte[] atomdata)
    {
        var data_format = new byte[4];
        var encoder_vendor = new byte[4];

        var num = ReadInt32(atomdata, 4);
        var stsdOff = 8;
        for (var i = 0; i < num; i++)
        {
            var size = ReadInt32(atomdata, stsdOff);
            stsdOff += 4;
            Buffer.BlockCopy(atomdata, stsdOff, data_format, 0, 4);

            stsdOff += 12;
            var data = new byte[size - 4 - 12];
            Buffer.BlockCopy(atomdata, stsdOff, data, 0, (size - 4 - 12));
            stsdOff += (size - 4 - 12);

            Buffer.BlockCopy(data, 4, encoder_vendor, 0, 4);
            if (encoder_vendor[0] == 0)
            {
                Frequency = ((data[16] & 0xff) << 8) + (data[17] & 0xff);
                Channels = (data[8] << 8) + data[9];
                if (ByteUtils.Compare(data_format, Encoding.ASCII.GetBytes("mp4a")))
                {
                    Codec = "AAC";
                }
                else
                {
                    Codec = Encoding.UTF8.GetString(data_format);
                }
            }
        }
    }

    private void ParseAtom(Stream stream, long firstOffset, long stopAt, int level)
    {
        ++level;

        var end = stream.Length;
        var offset = firstOffset;
        while (offset < stopAt)
        {
            stream.Seek(offset, SeekOrigin.Begin);

            var atomsize = stream.ReadInt32();
            var atomname = ByteUtils.ISO88591.GetString(stream.Read(4));

            if ((offset + atomsize) > end)
            {
                throw new InvalidDataException($"atom at {offset} claims {atomsize} bytes, end = {end}");
            }

            var atom = new Atom();
            atom.name = atomname;
            //atom.size = atomsize;
            //atom.pos = stream.Position - 8;
            atom.level = level;

            _atoms.Add(atom);

            if (string.Equals(atomname, "mdat", StringComparison.OrdinalIgnoreCase))
            {
                MdatAtomSize = atomsize;
            }
            /*else if (string.Equals(atomname, "moov", StringComparison.OrdinalIgnoreCase))
            {
                _moovpos = stream.Position - 8;
            }*/
            /*else if (string.Equals(atomname, "udta", StringComparison.OrdinalIgnoreCase))
            {
                _udtapos = stream.Position - 8;
                _udtasize = atomsize;
            }*/
            else if (string.Equals(atomname, "free", StringComparison.OrdinalIgnoreCase))
            {
                // we'd like a free under a moov
                // so check for level = 1
                if (atom.level == 2)
                {
                    // go backwards through the entries
                    for (var i = _atoms.Count - 1; i > 0; i--)
                    {
                        // until we hit a level 1
                        if (_atoms[i].level == 1)
                        {
                            if (string.Equals(_atoms[i].name, "moov", StringComparison.OrdinalIgnoreCase))
                            {
                                if (atomsize > _freesize)
                                {
                                    // is this free bigger than others?
                                    //_freepos = stream.Position - 8;
                                    _freesize = atomsize;
                                }
                                else
                                {
                                    break;
                                }
                            } // if moov
                        } // if level = 1
                    } // for
                } // if level = 2
            } // if free

            // if it's a container atom, parse the contents of the atom
            foreach (var atomType in ATOM_TYPES)
            {
                if (string.Equals(atomname, atomType, StringComparison.OrdinalIgnoreCase))
                {
                    ParseAtom(stream, stream.Position, offset + atomsize, level);
                    break;
                }
            }

            // meta atom contains tags and some other data
            if (string.Equals(atomname, "meta", StringComparison.OrdinalIgnoreCase))
            {
                // read in meta atom
                var atomdata = stream.Read(atomsize - 8);
                var nextTagPosition = 0;
                for (var i = 0; i < atomdata.Length - 4; i++)
                {
                    // ilst
                    if (atomdata[i] == 'i' && atomdata[i + 1] == 'l' && atomdata[i + 2] == 's' && atomdata[i + 3] == 't')
                    {
                        nextTagPosition = i + 8;
                        break;
                    }
                }

                while (nextTagPosition < (atomsize - 4) && nextTagPosition > 8)
                {
                    var size = ReadInt32(atomdata, (nextTagPosition - 4)) - 4;
                    nextTagPosition += size + 4;
                }
            } // if meta

            // mdhd has data for calculating playtime
            if (string.Equals(atomname, "mdhd", StringComparison.OrdinalIgnoreCase))
            {
                stream.Seek(12, SeekOrigin.Current);
                Frequency = stream.ReadInt32();
                Samples = stream.ReadInt32();
            }

            // stsd has data for sample, channels and codec
            if (string.Equals(atomname, "stsd", StringComparison.OrdinalIgnoreCase))
            {
                var atomdata = stream.Read(atomsize - 8);
                ParseStsdAtom(atomdata);
            }

            // bump offest and continue
            if (atomsize == 0)
            {
                // a 0 byte atom is the end
                offset = stopAt;
            }
            else
            {
                offset += atomsize;
            }
        } // while

        --level;
    }

    /// <summary>
    /// Reads the tag from the specified path.
    /// </summary>
    /// <param name="path">The path.</param>
    public void Read(string path)
    {
        using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            ReadStream(fs);
        }
    }

    /// <summary>
    /// Reads the tag from the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public void ReadStream(Stream stream)
    {
        _atoms.Clear();
        _freesize = 0;
        MdatAtomSize = 0;

        var end = stream.Length;
        ParseAtom(stream, 0, end, 0);
    }
}
