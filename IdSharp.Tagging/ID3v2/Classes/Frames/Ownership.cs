using System;
using System.IO;

namespace IdSharp.Tagging.ID3v2.Frames;

internal sealed class Ownership : Frame, IOwnership
{
    private EncodingType _textEncoding;
    private decimal _pricePaid;
    private string _currencyCode;
    private DateTime _dateOfPurchase;
    private string _seller;

    public EncodingType TextEncoding
    {
        get => _textEncoding;
        set
        {
            _textEncoding = value;
            RaisePropertyChanged(nameof(TextEncoding));
        }
    }

    public decimal PricePaid
    {
        get => _pricePaid;
        set
        {
            _pricePaid = value;
            RaisePropertyChanged(nameof(PricePaid));
        }
    }

    public string CurrencyCode
    {
        get => _currencyCode;
        set
        {
            _currencyCode = value; // todo: validation
            RaisePropertyChanged(nameof(CurrencyCode));
        }
    }

    public DateTime DateOfPurchase
    {
        get => _dateOfPurchase.Date;
        set
        {
            _dateOfPurchase = value.Date;
            RaisePropertyChanged(nameof(DateOfPurchase));
        }
    }

    public string Seller
    {
        get => _seller;
        set
        {
            _seller = value;
            RaisePropertyChanged(nameof(Seller));
        }
    }

    public override string GetFrameID(ID3v2TagVersion tagVersion)
    {
        switch (tagVersion)
        {
            case ID3v2TagVersion.ID3v24:
            case ID3v2TagVersion.ID3v23:
                return "OWNE";
            case ID3v2TagVersion.ID3v22:
                return null;
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
        if (PricePaid == 0 &&
            string.IsNullOrEmpty(CurrencyCode) &&
            DateOfPurchase == DateTime.MinValue &&
            string.IsNullOrEmpty(Seller))
        {
            return Array.Empty<byte>();
        }

        throw new NotImplementedException();
    }
}
