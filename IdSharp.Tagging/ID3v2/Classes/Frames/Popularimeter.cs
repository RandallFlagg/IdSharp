using System;
using System.IO;
using IdSharp.Common.Utils;

namespace IdSharp.Tagging.ID3v2.Frames;

internal sealed class Popularimeter : Frame, IPopularimeter
{
    private string _userEmail;
    private byte _rating;
    private long _playCount;

    public string UserEmail
    {
        get => _userEmail;
        set
        {
            _userEmail = value;
            RaisePropertyChanged(nameof(UserEmail));
        }
    }

    public byte Rating
    {
        get => _rating;
        set
        {
            _rating = value;
            RaisePropertyChanged(nameof(Rating));
        }
    }

    public long PlayCount
    {
        get => _playCount;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value cannot be less than 0");
            }

            _playCount = value;
            RaisePropertyChanged(nameof(PlayCount));
        }
    }

    public override string GetFrameID(ID3v2TagVersion tagVersion)
    {
        switch (tagVersion)
        {
            case ID3v2TagVersion.ID3v24:
            case ID3v2TagVersion.ID3v23:
                return "POPM";
            case ID3v2TagVersion.ID3v22:
                return "POP";
            default:
                throw new ArgumentException("Unknown tag version");
        }
    }

    public override void Read(TagReadingInfo tagReadingInfo, Stream stream)
    {
        _frameHeader.Read(tagReadingInfo, ref stream);

        var bytesLeft = _frameHeader.FrameSizeExcludingAdditions;
        if (bytesLeft > 0)
        {
            UserEmail = ID3v2Utils.ReadString(EncodingType.ISO88591, stream, ref bytesLeft);
            if (bytesLeft > 0)
            {
                Rating = stream.Read1(ref bytesLeft);
                if (bytesLeft > 0)
                {
                    var playCount = stream.Read(bytesLeft);
                    PlayCount = ByteUtils.ConvertToInt64(playCount);
                }
                else
                {
                    PlayCount = 0;
                }
            }
            else
            {
                Rating = 0;
                PlayCount = 0;
            }
        }
        else
        {
            UserEmail = null;
            Rating = 0;
            PlayCount = 0;
        }
    }

    public override byte[] GetBytes(ID3v2TagVersion tagVersion)
    {
        if (_rating == 0 && _playCount == 0)
        {
            return Array.Empty<byte>();
        }

        using (var frameData = new MemoryStream())
        {
            frameData.Write(ID3v2Utils.GetStringBytes(tagVersion, EncodingType.ISO88591, _userEmail, true));
            frameData.WriteByte(_rating);
            frameData.Write(ByteUtils.GetBytesMinimal(_playCount));
            return _frameHeader.GetBytes(frameData, tagVersion, GetFrameID(tagVersion));
        }
    }
}
