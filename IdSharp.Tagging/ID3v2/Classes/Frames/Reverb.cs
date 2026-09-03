using System;
using System.IO;

namespace IdSharp.Tagging.ID3v2.Frames;

internal sealed class Reverb : Frame, IReverb
{
    private short _reverbLeftMilliseconds;
    private short _reverbRightMilliseconds;
    private byte _reverbBouncesLeft;
    private byte _reverbBouncesRight;
    private byte _reverbFeedbackLeftToLeft;
    private byte _reverbFeedbackLeftToRight;
    private byte _reverbFeedbackRightToRight;
    private byte _reverbFeedbackRightToLeft;
    private byte _premixLeftToRight;
    private byte _premixRightToLeft;

    public short ReverbLeftMilliseconds
    {
        get => _reverbLeftMilliseconds;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value cannot be less than 0");
            }

            _reverbLeftMilliseconds = value;
            RaisePropertyChanged(nameof(ReverbLeftMilliseconds));
        }
    }

    public short ReverbRightMilliseconds
    {
        get => _reverbRightMilliseconds;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value cannot be less than 0");
            }

            _reverbRightMilliseconds = value;
            RaisePropertyChanged(nameof(ReverbRightMilliseconds));
        }
    }

    public byte ReverbBouncesLeft
    {
        get => _reverbBouncesLeft;
        set
        {
            _reverbBouncesLeft = value;
            RaisePropertyChanged(nameof(ReverbBouncesLeft));
        }
    }

    public byte ReverbBouncesRight
    {
        get => _reverbBouncesRight;
        set
        {
            _reverbBouncesRight = value;
            RaisePropertyChanged(nameof(ReverbBouncesRight));
        }
    }

    public byte ReverbFeedbackLeftToLeft
    {
        get => _reverbFeedbackLeftToLeft;
        set
        {
            _reverbFeedbackLeftToLeft = value;
            RaisePropertyChanged(nameof(ReverbFeedbackLeftToLeft));
        }
    }

    public byte ReverbFeedbackLeftToRight
    {
        get => _reverbFeedbackLeftToRight;
        set
        {
            _reverbFeedbackLeftToRight = value;
            RaisePropertyChanged(nameof(ReverbFeedbackLeftToRight));
        }
    }

    public byte ReverbFeedbackRightToRight
    {
        get => _reverbFeedbackRightToRight;
        set
        {
            _reverbFeedbackRightToRight = value;
            RaisePropertyChanged(nameof(ReverbFeedbackRightToRight));
        }
    }

    public byte ReverbFeedbackRightToLeft
    {
        get => _reverbFeedbackRightToLeft;
        set
        {
            _reverbFeedbackRightToLeft = value;
            RaisePropertyChanged(nameof(ReverbFeedbackRightToLeft));
        }
    }

    public byte PremixLeftToRight
    {
        get => _premixLeftToRight;
        set
        {
            _premixLeftToRight = value;
            RaisePropertyChanged(nameof(PremixLeftToRight));
        }
    }

    public byte PremixRightToLeft
    {
        get => _premixRightToLeft;
        set
        {
            _premixRightToLeft = value;
            RaisePropertyChanged(nameof(PremixRightToLeft));
        }
    }

    public override string GetFrameID(ID3v2TagVersion tagVersion)
    {
        switch (tagVersion)
        {
            case ID3v2TagVersion.ID3v24:
            case ID3v2TagVersion.ID3v23:
                return "RVRB";
            case ID3v2TagVersion.ID3v22:
                return "REV";
            default:
                throw new ArgumentException("Unknown tag version");
        }
    }

    public override void Read(TagReadingInfo tagReadingInfo, Stream stream)
    {
        throw new NotImplementedException();
    }

    public override byte[] GetBytes(ID3v2TagVersion tagVersion)
    {
        if (ReverbLeftMilliseconds == 0 &&
            ReverbRightMilliseconds == 0)
        {
            return Array.Empty<byte>();
        }

        throw new NotImplementedException();
    }
}
