using System;
using IdSharp.Common.Utils;

namespace IdSharp.Tagging.ID3v2.Frames;

internal sealed class EncryptedMetaFrame : Frame, IEncryptedMetaFrame
{
    private string _ownerIdentifier;
    private string _contentExplanation;
    private byte[] _encryptedData;

    public string OwnerIdentifier
    {
        get => _ownerIdentifier;
        set
        {
            _ownerIdentifier = value;
            RaisePropertyChanged(nameof(OwnerIdentifier));
        }
    }

    public string ContentExplanation
    {
        get => _contentExplanation;
        set
        {
            _contentExplanation = value;
            RaisePropertyChanged(nameof(ContentExplanation));
        }
    }

    public byte[] EncryptedData
    {
        get => ByteUtils.Clone(_encryptedData);
        set
        {
            _encryptedData = ByteUtils.Clone(value);
            RaisePropertyChanged(nameof(EncryptedData));
        }
    }

    public override string GetFrameID(ID3v2TagVersion tagVersion)
    {
        switch (tagVersion)
        {
            case ID3v2TagVersion.ID3v24:
            case ID3v2TagVersion.ID3v23:
                return null;
            case ID3v2TagVersion.ID3v22:
                return "CRM";
            default:
                throw new ArgumentException("Unknown tag version");
        }
    }

    public override void Read(TagReadingInfo tagReadingInfo, System.IO.Stream stream)
    {
        throw new NotImplementedException();
    }

    public override byte[] GetBytes(ID3v2TagVersion tagVersion)
    {
        throw new NotImplementedException();
    }
}
