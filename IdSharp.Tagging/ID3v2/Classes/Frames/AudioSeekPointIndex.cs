using System;
using System.ComponentModel;
using System.IO;

namespace IdSharp.Tagging.ID3v2.Frames;

internal sealed class AudioSeekPointIndex : Frame, IAudioSeekPointIndex
{
    private int _indexedDataStart;
    private int _indexedDataLength;
    private byte _bitsPerIndexPoint;

    public AudioSeekPointIndex()
    {
        FractionAtIndex = new BindingList<short>();
    }

    public int IndexedDataStart
    {
        get => _indexedDataStart;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value cannot be less than 0");
            }

            _indexedDataStart = value;
            RaisePropertyChanged(nameof(IndexedDataStart));
        }
    }

    public int IndexedDataLength
    {
        get => _indexedDataLength;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value cannot be less than 0");
            }

            _indexedDataLength = value;
            RaisePropertyChanged(nameof(IndexedDataLength));
        }
    }

    public byte BitsPerIndexPoint
    {
        get => _bitsPerIndexPoint;
        set
        {
            _bitsPerIndexPoint = value;
            RaisePropertyChanged(nameof(BitsPerIndexPoint));
        }
    }

    public BindingList<short> FractionAtIndex {
        // TODO: type must change to fire change events
        get; }

    public override string GetFrameID(ID3v2TagVersion tagVersion)
    {
        switch (tagVersion)
        {
            case ID3v2TagVersion.ID3v24:
            case ID3v2TagVersion.ID3v23:
                return "ASPI";
            case ID3v2TagVersion.ID3v22:
                return null;
            default:
                throw new ArgumentException("Unknown tag version");
        }
    }

    public override void Read(TagReadingInfo tagReadingInfo, Stream stream)
    {
        FractionAtIndex.Clear();
        throw new NotImplementedException();
    }

    public override byte[] GetBytes(ID3v2TagVersion tagVersion)
    {
        if (IndexedDataLength == 0 ||
            BitsPerIndexPoint == 0 ||
            FractionAtIndex.Count == 0)
        {
            return Array.Empty<byte>();
        }

        throw new NotImplementedException();
    }
}
