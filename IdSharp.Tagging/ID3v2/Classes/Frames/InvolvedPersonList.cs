using System;
using System.ComponentModel;
using System.IO;
using IdSharp.Common.Utils;
using IdSharp.Tagging.ID3v2.Extensions;
using IdSharp.Tagging.ID3v2.Frames.Items;
using IdSharp.Tagging.ID3v2.Frames.Lists;

namespace IdSharp.Tagging.ID3v2.Frames;

internal sealed class InvolvedPersonList : Frame, IInvolvedPersonList
{
    private EncodingType _textEncoding;
    private readonly InvolvedPersonBindingList _involvedPersons;

    public InvolvedPersonList()
    {
        _involvedPersons = new InvolvedPersonBindingList();
    }

    public EncodingType TextEncoding
    {
        get => _textEncoding;
        set
        {
            _textEncoding = value;
            RaisePropertyChanged(nameof(TextEncoding));
        }
    }

    public BindingList<IInvolvedPerson> Items
    {
        get { return _involvedPersons; }
    }

    public override string GetFrameID(ID3v2TagVersion tagVersion)
    {
        switch (tagVersion)
        {
            case ID3v2TagVersion.ID3v24:
                return "TIPL";
            case ID3v2TagVersion.ID3v23:
                return "IPLS";
            case ID3v2TagVersion.ID3v22:
                return "IPL";
            default:
                throw new ArgumentException("Unknown tag version");
        }
    }

    public override void Read(TagReadingInfo tagReadingInfo, Stream stream)
    {
        _frameHeader.Read(tagReadingInfo, ref stream);
        _involvedPersons.Clear();

        var bytesLeft = _frameHeader.FrameSizeExcludingAdditions;
        if (bytesLeft > 0)
        {
            TextEncoding = (EncodingType)stream.Read1(ref bytesLeft);
            while (bytesLeft > 0)
            {
                var involvement = ID3v2Utils.ReadString(TextEncoding, stream, ref bytesLeft);
                var name = ID3v2Utils.ReadString(TextEncoding, stream, ref bytesLeft);

                if (!string.IsNullOrEmpty(involvement) || !string.IsNullOrEmpty(name))
                {
                    var involvedPerson = _involvedPersons.AddNew();
                    involvedPerson.Involvement = involvement;
                    involvedPerson.Name = name;
                }
            }
        }
    }

    public override byte[] GetBytes(ID3v2TagVersion tagVersion)
    {
        if (Items.Count == 0)
        {
            return Array.Empty<byte>();
        }

        // Sets appropriate TextEncoding if ISO-8859-1 is insufficient
        if (TextEncoding == EncodingType.ISO88591)
        {
            foreach (var involvedPerson in Items)
            {
                if (string.IsNullOrEmpty(involvedPerson.Involvement) && string.IsNullOrEmpty(involvedPerson.Name))
                {
                    continue;
                }

                var involvementData = ID3v2Utils.GetStringBytes(tagVersion, _textEncoding, involvedPerson.Involvement, true);
                var nameData = ID3v2Utils.GetStringBytes(tagVersion, _textEncoding, involvedPerson.Name, true);

                if (this.RequiresFix(tagVersion, involvedPerson.Involvement, involvementData))
                {
                    break;
                }

                if (this.RequiresFix(tagVersion, involvedPerson.Name, nameData))
                {
                    break;
                }
            }
        }

        using (var frameData = new MemoryStream())
        {
            frameData.WriteByte((byte)_textEncoding);

            var foundItem = false;
            foreach (var involvedPerson in Items)
            {
                if (string.IsNullOrEmpty(involvedPerson.Involvement) && string.IsNullOrEmpty(involvedPerson.Name))
                {
                    continue;
                }

                frameData.Write(ID3v2Utils.GetStringBytes(tagVersion, _textEncoding, involvedPerson.Involvement, true));
                frameData.Write(ID3v2Utils.GetStringBytes(tagVersion, _textEncoding, involvedPerson.Name, true));
                foundItem = true;
            }

            if (!foundItem)
            {
                return Array.Empty<byte>();
            }

            return _frameHeader.GetBytes(frameData, tagVersion, GetFrameID(tagVersion));
        }
    }
}
