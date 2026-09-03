using System;
using System.IO;
using IdSharp.Common.Utils;

namespace IdSharp.Tagging.ID3v2.Frames;

internal sealed class LinkedInformation : Frame, ILinkedInformation
{
    private string _frameIdentifier;
    private string _url;
    private byte[] _additionalData;

    public string FrameIdentifier
    {
        get => _frameIdentifier;
        set
        {
            _frameIdentifier = value;
            RaisePropertyChanged(nameof(FrameIdentifier));
        }
    }

    public string Url
    {
        get => _url;
        set
        {
            _url = value;
            RaisePropertyChanged(nameof(Url));
        }
    }

    public byte[] AdditionalData
    {
        get => ByteUtils.Clone(_additionalData);
        set
        {
            _additionalData = ByteUtils.Clone(value);
            RaisePropertyChanged(nameof(AdditionalData));
        }
    }

    public override string GetFrameID(ID3v2TagVersion tagVersion)
    {
        switch (tagVersion)
        {
            case ID3v2TagVersion.ID3v24:
            case ID3v2TagVersion.ID3v23:
                return "LINK";
            case ID3v2TagVersion.ID3v22:
                return "LNK";
            default:
                throw new ArgumentException("Unknown tag version");
        }
    }

    public override void Read(TagReadingInfo tagReadingInfo, Stream stream)
    {
        _frameHeader.Read(tagReadingInfo, ref stream);

        var bytesLeft = _frameHeader.FrameSizeExcludingAdditions;
        var frameIDSize = (tagReadingInfo.TagVersion == ID3v2TagVersion.ID3v22 ? 3 : 4);
        if (bytesLeft > frameIDSize)
        {
            FrameIdentifier = ID3v2Utils.ReadString(EncodingType.ISO88591, stream, frameIDSize);
            bytesLeft -= frameIDSize;
            Url = ID3v2Utils.ReadString(EncodingType.ISO88591, stream, ref bytesLeft);
            AdditionalData = stream.Read(bytesLeft);
        }
        else
        {
            stream.Seek(bytesLeft, SeekOrigin.Current);
        }
    }

    public override byte[] GetBytes(ID3v2TagVersion tagVersion)
    {
        if (_additionalData == null || _additionalData.Length == 0)
        {
            return Array.Empty<byte>();
        }

        if (tagVersion == ID3v2TagVersion.ID3v22)
        {
            if (_frameIdentifier == null || _frameIdentifier.Length != 3)
            {
                return Array.Empty<byte>();
            }
        }
        else
        {
            if (_frameIdentifier == null || _frameIdentifier.Length != 4)
            {
                return Array.Empty<byte>();
            }
        }

        using (var frameData = new MemoryStream())
        {
            frameData.Write(ByteUtils.ISO88591GetBytes(_frameIdentifier));
            frameData.Write(ID3v2Utils.GetStringBytes(tagVersion, EncodingType.ISO88591, _url, true));
            frameData.Write(_additionalData);

            return _frameHeader.GetBytes(frameData, tagVersion, GetFrameID(tagVersion));
        }
    }
}
