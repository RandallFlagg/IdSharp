using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IdSharp.Common.Utils;

namespace IdSharp.Tagging.VorbisComment;

/// <summary>
/// Vorbis Comment
/// </summary>
public class VorbisComment : IVorbisComment
{
    private static readonly byte[] FLAC_MARKER = Encoding.ASCII.GetBytes("fLaC");
    private List<FlacMetaDataBlock> _metaDataBlockList;

    private class InternalInfo
    {
        public int OrigVorbisCommentSize = 0;
        public int OrigPaddingSize = 0;
        public FileType? FileType = null;
        public string Vendor = null;
        public IEnumerable<FlacMetaDataBlock> MetaDataBlockList = null;
    }

    private enum FileType
    {
        OggVorbis,
        Flac
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VorbisComment"/> class.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    public VorbisComment(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        Read(path);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VorbisComment"/> class.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public VorbisComment(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        Read(stream);
    }

    private VorbisComment()
    {
    }

    /// <summary>
    /// Gets the vendor specified in the Vorbis Comment header.
    /// </summary>
    /// <value>The vendor specified in the Vorbis Comment header..</value>
    public string Vendor { get; private set; } = string.Empty;

    /// <summary>
    /// Gets or sets the artist.
    /// </summary>
    /// <value>The artist.</value>
    public string Artist
    { get => Items.GetValue("ARTIST"); set => Items.SetValue("ARTIST", value);
    }

    /// <summary>
    /// Gets or sets the album.
    /// </summary>
    /// <value>The album.</value>
    public string Album
    { get => Items.GetValue("ALBUM"); set => Items.SetValue("ALBUM", value);
    }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <value>The title.</value>
    public string Title
    { get => Items.GetValue("TITLE"); set => Items.SetValue("TITLE", value);
    }

    /// <summary>
    /// Gets or sets the year.
    /// </summary>
    /// <value>The year.</value>
    public string Year
    {
        get
        {
            var value = Items.GetValue("DATE");
            if (string.IsNullOrEmpty(value))
            {
                value = Items.GetValue("YEAR");
            }

            return value;
        }

        set => Items.SetValue("DATE", value);
    }

    /// <summary>
    /// Gets or sets the genre.
    /// </summary>
    /// <value>The genre.</value>
    public string Genre
    { get => Items.GetValue("GENRE"); set => Items.SetValue("GENRE", value);
    }

    /// <summary>
    /// Gets or sets the track number.
    /// </summary>
    /// <value>The track number.</value>
    public string TrackNumber
    { get => Items.GetValue("TRACKNUMBER"); set => Items.SetValue("TRACKNUMBER", value);
    }

    /// <summary>
    /// Gets or sets the comment.
    /// </summary>
    /// <value>The comment.</value>
    public string Comment
    { get => Items.GetValue("COMMENT"); set => Items.SetValue("COMMENT", value);
    }

    /// <summary>
    /// Gets the Name/Value item list.
    /// </summary>
    /// <value>The Name/Value item list.</value>
    public NameValueList Items { get; } = new NameValueList();

    /// <summary>
    /// Writes the tag.
    /// </summary>
    /// <param name="path">The path.</param>
    public void Save(string path)
    {
        var vorbisComment = new VorbisComment();
        InternalInfo info;

        using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            info = vorbisComment.ReadTagInternal(fs);
        }

        Vendor = info.Vendor; // keep the vendor of the original file, don't copy it from another source
        if (string.IsNullOrEmpty(Vendor))
        {
            Vendor = "idsharp library";
        }

        if (info.FileType == FileType.Flac)
        {
            WriteTagFlac(path, info);
        }
        /*else if (info.FileType == FileType.OggVorbis)
        {
        }*/
        else
        {
            throw new InvalidDataException($"File '{path}' is not a valid FLAC or Ogg-Vorbis file");
        }
    }

    /// <summary>
    /// Reads the tag.
    /// </summary>
    /// <param name="path">The path.</param>
    public void Read(string path)
    {
        try
        {
            using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Read(fs);
            }
        }
        catch (InvalidDataException ex)
        {
            throw new InvalidDataException($"Cannot read '{path}'", ex);
        }
    }

    /// <summary>
    /// Reads the tag.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public void Read(Stream stream)
    {
        ReadTagInternal(stream);
    }

    private void WriteTagFlac(string path, InternalInfo targetFile)
    {
        // This will store the metadata blocks we're actually going to write
        var myMetaDataBlocks = new List<FlacMetaDataBlock>();

        // Get byte array of new vorbis comment block
        byte[] newTagArray;
        using (var newTag = new MemoryStream())
        {
            // Write vendor
            var vendorBytes = Encoding.UTF8.GetBytes(Vendor);
            newTag.WriteInt32LittleEndian(vendorBytes.Length);
            newTag.Write(vendorBytes);

            // Remove dead items and replace commonly misnamed items
            foreach (var item in new List<NameValueItem>(Items))
            {
                if (string.IsNullOrEmpty(item.Value))
                {
                    Items.Remove(item);
                }
                else if (string.Compare(item.Name, "YEAR", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (string.IsNullOrEmpty(Items.GetValue("DATE")))
                    {
                        Items.SetValue("DATE", item.Value);
                    }

                    Items.Remove(item);
                }
            }

            // Write item count
            newTag.WriteInt32LittleEndian(Items.Count);

            // Write items
            foreach (var item in Items)
            {
                if (string.IsNullOrEmpty(item.Value))
                {
                    continue;
                }

                var keyBytes = Encoding.ASCII.GetBytes(item.Name);
                var valueBytes = Encoding.UTF8.GetBytes(item.Value);

                newTag.WriteInt32LittleEndian(keyBytes.Length + 1 + valueBytes.Length);
                newTag.Write(keyBytes);
                newTag.WriteByte((byte)'=');
                newTag.Write(valueBytes);
            }

            newTagArray = newTag.ToArray();
        }

        // 1. Get old size of metadata blocks.
        // 2. Find StreamInfo, SeekTable, and Padding blocks.
        //    These blocks should occur only once.  If not, an exception is thrown.  The padding
        //    block we don't really care about so no exception is thrown if it's duplicated.
        FlacMetaDataBlock paddingBlock = null;
        FlacMetaDataBlock streamInfoBlock = null;
        FlacMetaDataBlock seekTableBlock = null;
        long origMetaDataSize = 0;
        foreach (var metaDataBlock in targetFile.MetaDataBlockList)
        {
            origMetaDataSize += 4; // Identifier + Size
            origMetaDataSize += metaDataBlock.Size;

            if (metaDataBlock.BlockType == FlacMetaDataBlockType.Padding)
            {
                paddingBlock = metaDataBlock;
            }
            else if (metaDataBlock.BlockType == FlacMetaDataBlockType.StreamInfo)
            {
                if (streamInfoBlock != null)
                {
                    throw new InvalidDataException("Multiple stream info blocks");
                }

                streamInfoBlock = metaDataBlock;
            }
            else if (metaDataBlock.BlockType == FlacMetaDataBlockType.SeekTable)
            {
                if (seekTableBlock != null)
                {
                    throw new InvalidDataException("Multiple seek tables");
                }

                seekTableBlock = metaDataBlock;
            }
        }

        // No Padding block found, create one
        if (paddingBlock == null)
        {
            paddingBlock = new FlacMetaDataBlock(FlacMetaDataBlockType.Padding);
            paddingBlock.SetBlockDataZeroed(2000);
        }
        // Padding block found, adjust size
        else
        {
            // TODO: This is not entirely accurate, since we may be reading from one file
            // and writing to another.  The other blocks need to be accounted for, however for
            // same file read/write this works.  Not high priority.
            var adjustPadding = targetFile.OrigVorbisCommentSize - newTagArray.Length;
            var newSize = paddingBlock.Size + adjustPadding;
            if (newSize < 10)
            {
                paddingBlock.SetBlockDataZeroed(2000);
            }
            else
            {
                paddingBlock.SetBlockDataZeroed(newSize);
            }
        }

        // Set Vorbis-Comment block data
        var vorbisCommentBlock = new FlacMetaDataBlock(FlacMetaDataBlockType.VorbisComment);
        vorbisCommentBlock.SetBlockData(newTagArray);

        // Create list of blocks to write
        myMetaDataBlocks.Add(streamInfoBlock); // StreamInfo MUST be first
        if (seekTableBlock != null)
        {
            myMetaDataBlocks.Add(seekTableBlock);
        }

        // Add other blocks we read from the original file.
        foreach (var metaDataBlock in _metaDataBlockList)
        {
            if (metaDataBlock.BlockType == FlacMetaDataBlockType.Application ||
                metaDataBlock.BlockType == FlacMetaDataBlockType.CueSheet ||
                metaDataBlock.BlockType == FlacMetaDataBlockType.Picture)
            {
                myMetaDataBlocks.Add(metaDataBlock);
            }
        }

        // Add our new vorbis comment and padding blocks
        myMetaDataBlocks.Add(vorbisCommentBlock);
        myMetaDataBlocks.Add(paddingBlock);

        // Get new size of metadata blocks
        long newMetaDataSize = 0;
        foreach (var metaDataBlock in myMetaDataBlocks)
        {
            newMetaDataSize += 4; // Identifier + Size
            newMetaDataSize += metaDataBlock.Size;
        }

        // If the new metadata size is less than the original, increase the padding
        if (newMetaDataSize != origMetaDataSize)
        {
            var newPaddingSize = paddingBlock.Size + (int)(origMetaDataSize - newMetaDataSize);
            if (newPaddingSize > 0)
            {
                paddingBlock.SetBlockDataZeroed(newPaddingSize);

                // Get new size of metadata blocks
                newMetaDataSize = 0;
                foreach (var metaDataBlock in myMetaDataBlocks)
                {
                    newMetaDataSize += 4; // Identifier + Size
                    newMetaDataSize += metaDataBlock.Size;
                }
            }
        }

        string tempFilename = null;

        // no rewrite necessary
        if (newMetaDataSize == origMetaDataSize)
        {
            //
        }
        // rewrite necessary - grab a snickers.
        else if (newMetaDataSize > origMetaDataSize)
        {
            // rename

            tempFilename = PathUtils.GetTemporaryFileNameBasedOnFileName(path);
            File.Move(path, tempFilename);

            // open for read, open for write
            using (var fsRead = File.Open(tempFilename, FileMode.Open, FileAccess.Read, FileShare.None))
            using (var fsWrite = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                // copy ID3v2 tag.. technically there shouldn't be one, but we don't want to destroy data
                var tmpID3v2TagSize = ID3v2.ID3v2Tag.GetTagSize(fsRead);
                if (tmpID3v2TagSize != 0)
                {
                    var id3v2 = fsRead.Read(tmpID3v2TagSize);
                    fsWrite.Write(id3v2);
                }

                fsWrite.Write(FLAC_MARKER);
                // create blankspace
                var blankSpace = new Byte[newMetaDataSize];
                fsWrite.Write(blankSpace);

                fsRead.Seek(4 + origMetaDataSize, SeekOrigin.Current);

                var buf = new byte[32768];
                var bytesRead = fsRead.Read(buf, 0, 32768);
                while (bytesRead != 0)
                {
                    fsWrite.Write(buf, 0, bytesRead);
                    bytesRead = fsRead.Read(buf, 0, 32768);
                }
            }
        }
        // newMetaDataSize < origMetaDataSize is an error
        else
        {
            throw new Exception($"Internal Error: newMetaDataSize ({newMetaDataSize}) < origMetaDataSize ({origMetaDataSize})");
        }

        using (var fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            // skip fLaC marker and ID3v2 tag size
            var tmpID3v2TagSize = ID3v2.ID3v2Tag.GetTagSize(fs);
            fs.Position = tmpID3v2TagSize + 4;

            byte blockType;

            foreach (var metaDataBlock in myMetaDataBlocks)
            {
                // always write padding last
                if (metaDataBlock == paddingBlock)
                {
                    continue;
                }

                blockType = (byte)metaDataBlock.BlockType;
                fs.WriteByte(blockType);
                fs.WriteInt24(metaDataBlock.Size);
                fs.Write(metaDataBlock.BlockData);
            }

            // write padding, add stop bit to block type
            blockType = (byte)(paddingBlock.BlockType + 0x80);
            fs.WriteByte(blockType);
            fs.WriteInt24(paddingBlock.Size);
            fs.Write(paddingBlock.BlockData);
        }

        if (!string.IsNullOrEmpty(tempFilename))
        {
            File.Delete(tempFilename);
        }
    }

    /*else if (mAudioFileTrack == AUDIO_OGG)
    {
        unsigned char *vc = new unsigned char[TagSize+10];

        // Vendor size, Vendor
        sprintf(vc, "%c%c%c%c%s%c%c%c%c", mVendor.Length() & 0xFF, (mVendor.Length() >> 8) & 0xFF, (mVendor.Length() >> 16) & 0xFF, (mVendor.Length() >> 24) & 0xFF, mVendor.c_str(), \
                                          elements & 0xFF, (elements >> 8) & 0xFF, (elements >> 16) & 0xFF, (elements >> 24) & 0xFF);

        unsigned long offset = 4 + mVendor.Length() + 4;
        if (Field != NULL)
        {
            Rewind();
            do
            {
                String Vector = Field->ItemKey + "=" + Field->ItemValue;

                // Vector size, Vector
                vc[offset++] =  Vector.Length() & 0xFF;
                vc[offset++] = (Vector.Length() >> 8) & 0xFF;
                vc[offset++] = (Vector.Length() >> 16) & 0xFF;
                vc[offset++] = (Vector.Length() >> 24) & 0xFF;

                for (i=0; i<Vector.Length(); i++)
                {
                    vc[offset++] = Vector.c_str()[i];
                }

            } while (this->NextField());
        }

        if (NewPaddingSize > 3)
        {
            NewPaddingSize -= 1;

            fseek(fp, mvcoffset, SEEK_SET);
            fwrite(vc, offset, 1, fp);
            fputc(0x01, fp);
            for (i=0; i<NewPaddingSize; i++) fputc(0x00, fp);
        }
        else
        {
            fclose(fp);

            sprintf(buf, "%s__tmpaudio%lu", DirTrunc(Filename), releasetotalcount+16);

            if (rename(Filename.c_str(), buf) != 0)
            {
                String msg = "";
                fp = fopen(buf, "rb");
                if (fp != NULL)
                {
                    fclose(fp);
                    msg = String((char *)buf) + " exists; ";
                    throw Exception(String(msg + "rename(\"" + FilenameTrunc(Filename) + "\", \"" + String(FilenameTrunc((char *)buf)) + "\"); failed").c_str());
                }
                else
                {
                    msg = String((char *)buf) + " does not exist; ";
                    fp = fopen(buf, "wb");
                    if (fp == NULL)
                    {
                        msg = "cant open " + String((char *)buf) + "; ";
                        throw Exception(String(msg + "rename(\"" + FilenameTrunc(Filename) + "\", \"" + String(FilenameTrunc((char *)buf)) + "\"); failed").c_str());
                    }
                    FILE *ren = fopen(Filename.c_str(), "rb");
                    if (ren == NULL)
                    {
                        msg = Filename + " does not exist; ";
                        throw Exception(String(msg + "rename(\"" + FilenameTrunc(Filename) + "\", \"" + String(FilenameTrunc((char *)buf)) + "\"); failed").c_str());
                    }

                    iRead = fread(buf, 1, cuiReadBufSize, ren);
                    while (iRead)
                    {
                        fwrite(buf, iRead, 1, fp);
                        iRead = fread(buf, 1, cuiReadBufSize, ren);
                    }

                    fclose(fp);
                    fclose(ren);
                }

                sprintf(buf, "%s__tmpaudio%lu", DirTrunc(Filename), releasetotalcount+16);
            }

            fp = fopen(buf, "rb");
            out = fopen(Filename.c_str(), "wb+");

            unsigned long curpos = 0;
            step = "initial read";
            iRead = fread(buf, 1, cuiReadBufSize, fp);
            while (iRead)
            {
                curpos += iRead;
                if (curpos > lSPageStart)
                {
                    iRead -= (curpos - lSPageStart);
                    fwrite(buf, iRead, 1, out);
                    break;
                }
                else
                {
                    fwrite(buf, iRead, 1, out);
                    iRead = fread(buf, 1, cuiReadBufSize, fp);
                }
            }

            step = "read second page header";
            fseek(fp, lSPageStart, SEEK_SET);
            fread(buf, 1, 27, fp);

            buf[22] = 0; // clear CRC
            buf[23] = 0;
            buf[24] = 0;
            buf[25] = 0;

            step = "set up new lace vals";
            int origlace = ((mOrigVorbisCommentSize + mOrigPaddingSize + 7) / 255) + 1;
            int newlace = ((offset + 1000 + 8) / 255) + 1;
            int oldlace = buf[26];

            buf[26] += (newlace - origlace);

            step = "output header";
            fwrite(buf, 27, 1, out);

            step = "output new lacing vals";
            int toffset = offset + 1000 + 8; // 0x03 + 'vorbis' + 0x01
            while (toffset >= 0)
            {
                fputc((toffset >= 255 ? 255 : toffset) & 0xFF, out);
                toffset -= 255;
            }
            step = "skip old lacing vals";
            fread(buf, 1, origlace, fp);
            step = "output last lacing vals";
            for (i=origlace; i<oldlace; i++)
            {
                fputc(fgetc(fp), out);
            }

            sprintf(buf, "%cvorbis", 0x03);
            fwrite(buf, 7, 1, out);

            step = "write vorbis comment";
            fwrite(vc, offset, 1, out);

            int a = FileSize/cuiReadBufSize / 21;
            int b = a, x, y;

            if (Progress != NULL)
            {
                Progress->SetVisible();
            }

            // write stop bit
            fputc(1, out);
            for (i=0; i<1000; i++) fputc(0x00, out);

            // seek past vorbis comment
            fseek(fp, mvcoffset + mOrigVorbisCommentSize + mOrigPaddingSize, SEEK_SET);
            // copy rest of file
            iRead = fread(buf, 1, cuiReadBufSize, fp);
            while (iRead)
            {
                fwrite(buf, iRead, 1, out);
                iRead = fread(buf, 1, cuiReadBufSize, fp);
            }

            fclose(fp);
            fclose(out);

            sprintf(buf, "%s__tmpaudio%lu", DirTrunc(Filename), releasetotalcount+16);
            unlink(buf);

            fp = fopen(Filename.c_str(), "rb+");
        }

        delete[] vc;

        // crc: clear current crc
        fseek(fp, lSPageStart+22, SEEK_SET);
        fputc(0x00, fp);
        fputc(0x00, fp);
        fputc(0x00, fp);
        fputc(0x00, fp);

        // crc: reseek
        fseek(fp, lSPageStart+26, SEEK_SET);
        int lacings = fgetc(fp);
        unsigned char LacingSize[255];
        step = "crc: get lacings (" + String(lacings) + ")";
        for (i=0; i<lacings; i++)
        {
            LacingSize[i] = fgetc(fp);
        }

        // Go to start of 2nd page

        unsigned long iCRC = 0x00;
        unsigned long iRead;
        unsigned long count;
        unsigned char uiOffset;

        // crc: go to 2nd page start
        fseek(fp, lSPageStart, SEEK_SET);

        // crc: fread begin
        iRead = count = fread(buf, 1, 27+lacings, fp);
        do
        {
            uiOffset = ((iCRC >> 24) & 0xFF) ^ buf[iRead - count];
            iCRC = (iCRC << 8) ^ VCCRCTAB[uiOffset];
        } while (--count);

        for (i=0; i<lacings; i++)
        {
            if (LacingSize[i])
            {
                step = "crc: fread(" + String(LacingSize[i]) + ")";
                iRead = count = fread(buf, 1, LacingSize[i], fp);
                do
                {
                    uiOffset = ((iCRC >> 24) & 0xFF) ^ buf[iRead - count];
                    iCRC = (iCRC << 8) ^ VCCRCTAB[uiOffset];
                } while (--count);
            }
        }

        // crc: parse crc
        unsigned char a = (iCRC >> 24) & 0xFF;
        unsigned char b = (iCRC >> 16) & 0xFF;
        unsigned char c = (iCRC >> 8) & 0xFF;
        unsigned char d = iCRC & 0xFF;

        // crc: seek to write pos
        fseek(fp, lSPageStart+22, SEEK_SET);
        fputc(d, fp);
        fputc(c, fp);
        fputc(b, fp);
        fputc(a, fp);

        // crc: close file
        fclose(fp);
    }
}*/

    private InternalInfo ReadTagInternal(Stream stream)
    {
        var info = new InternalInfo();
        info.OrigPaddingSize = 0;
        info.OrigVorbisCommentSize = 0;

        // Skip ID3v2 tag
        var id3v2TagSize = ID3v2.ID3v2Tag.GetTagSize(stream);
        stream.Seek(id3v2TagSize, SeekOrigin.Begin);

        if (IsFlac(stream))
        {
            ReadTag_FLAC(stream, info);
        }
        /*else if (mAudioFileTrack == AUDIO_OGG)
        {
            return ReadTag_OGG(fp);
        }*/
        else
        {
            throw new InvalidDataException("FLAC marker not found");
            //throw new InvalidDataException(String.Format("File '{0}' is not a valid FLAC or Ogg-Vorbis file", path));
        }

        info.Vendor = Vendor;

        return info;
    }

    private void ReadTag_VorbisComment(Stream stream)
    {
        Items.Clear();

        var size = stream.ReadInt32LittleEndian();
        Vendor = stream.ReadUTF8(size);

        var elements = stream.ReadInt32LittleEndian();

        for (; elements > 0; elements--)
        {
            size = stream.ReadInt32LittleEndian();

            var text = stream.ReadUTF8(size);
            var nameValue = text.Split("=".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);

            if (nameValue.Length == 2)
            {
                var name = nameValue[0];
                var value = nameValue[1];

                Items.Add(new NameValueItem(name, value));
            }
        }
    }

    private void ReadTag_FLAC(Stream stream, InternalInfo info)
    {
        info.FileType = FileType.Flac;

        // Skip "fLaC" marker    
        stream.Seek(4, SeekOrigin.Current);

        bool isLastMetaDataBlock;

        var metaDataBlockList = new List<FlacMetaDataBlock>();

        do
        {
            var c = stream.ReadByte();
            isLastMetaDataBlock = (((c >> 7) & 0x01) == 1);

            var blocksize = stream.ReadInt24();

            var blockType = (FlacMetaDataBlockType)(c & 0x07);

            if (blockType == FlacMetaDataBlockType.VorbisComment) // Vorbis comment
            {
                info.OrigVorbisCommentSize = blocksize;
                var mvcoffset = stream.Position - 4;
                ReadTag_VorbisComment(stream);
                stream.Seek(mvcoffset + 4, SeekOrigin.Begin);
            }

            var metaDataBlock = new FlacMetaDataBlock(blockType);
            metaDataBlock.SetBlockData(stream, blocksize);
            metaDataBlockList.Add(metaDataBlock);
        } while (isLastMetaDataBlock == false);

        info.MetaDataBlockList = metaDataBlockList;
        _metaDataBlockList = metaDataBlockList;
    }

    /*private void ReadTag_OGG(FILE *fp)
    {
        info.FileType = FileType.OggVorbis;
        unsigned char buf[30];
        unsigned long curpos;

        curpos = ftell(fp);

        fread(buf, 1, 27, fp);

        buf[4] = 0;
        if (strcmp(buf, "OggS"))
        {
            throw Exception("No OggS marker found");
        }

        // Add segments + skip parameters vorbis-header (0x01 + 'vorbis')
        curpos += 27 + buf[26] + 30;
        fseek(fp, curpos, SEEK_SET);

        lSPageStart = ftell(fp);

        fread(buf, 1, 27, fp);
        buf[4] = 0;
        if (strcmp(buf, "OggS"))
        {
            throw Exception("No second OggS marker found");
        }

        // Seek to 0x03 + 'vorbis'
        curpos += 27 + buf[26];
        fseek(fp, curpos, SEEK_SET);

        // Read 0x03 + 'vorbis'
        fread(buf, 1, 7, fp);
        mvcoffset = ftell(fp);

        ReadTag_VorbisComment(fp);

        mOrigVorbisCommentSize = ftell(fp) - mvcoffset + 1;

        if (fgetc(fp) == 0x01) // check stop bit
        {
            mOrigPaddingSize = 0;
            while (fgetc(fp) == 0x00) mOrigPaddingSize++;
        }
    }*/

    internal static bool IsFlac(string path)
    {
        using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            return IsFlac(fs);
        }
    }

    internal static bool IsFlac(Stream stream)
    {
        // Read flac marker
        var flacMarker = new byte[4];
        stream.Read(flacMarker, 0, 4);
        stream.Seek(-4, SeekOrigin.Current);
        return ByteUtils.Compare(flacMarker, FLAC_MARKER);
    }
}
