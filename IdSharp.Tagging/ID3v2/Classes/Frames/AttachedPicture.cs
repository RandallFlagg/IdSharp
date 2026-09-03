using System.Diagnostics;
using System.Text;

using IdSharp.Common.Utils;
using IdSharp.Tagging.ID3v2.Extensions;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.PixelFormats;

namespace IdSharp.Tagging.ID3v2.Frames;

internal sealed class AttachedPicture : Frame, IAttachedPicture
{
    private EncodingType _textEncoding;
    private string _mimeType;
    private PictureType _pictureType;
    private string _description;
    private byte[] _pictureData;
    private Image _picture;

    private bool _loadingPicture;
    private bool _readingTag;
    private bool _pictureCached;

    public AttachedPicture()
    {
        _pictureType = PictureType.CoverFront;
    }

    public EncodingType TextEncoding
    {
        get => _textEncoding;
        set
        {
            if (_textEncoding != value)
            {
                // TODO: From what I can tell, it looks like iTunes can't handle a picture with a Unicode encoded description. 
                // You add an ID3PictureFrame with the TextEncoding set to Unicode and iTunes doesn't recognize the picture. 
                // Interestingly, Windows Media Player does recognize it. If the ID3PictureFrame has a TextEncoding of ISO-8859-1, 
                // iTunes will recognize it.

                _textEncoding = value;
                RaisePropertyChanged(nameof(TextEncoding));
            }
        }
    }

    public string MimeType
    {
        get => _mimeType;
        set
        {
            if (_mimeType != value)
            {
                _mimeType = value;
                RaisePropertyChanged(nameof(MimeType));
            }
        }
    }

    public PictureType PictureType
    {
        get => _pictureType;
        set
        {
            // TODO: Validate enum
            // TODO: Validate uniqueness for 0x01 and 0x02
            if (_pictureType != value)
            {
                _pictureType = value;
                RaisePropertyChanged(nameof(PictureType));
            }
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                RaisePropertyChanged(nameof(Description));
            }
        }
    }

    public byte[] PictureData
    {
        get => ByteUtils.Clone(_pictureData);
        set
        {
            if (!ByteUtils.Compare(_pictureData, value))
            {
                _pictureData = ByteUtils.Clone(value);

                if (value != null && _readingTag == false)
                {
                    LoadPicture();
                }

                RaisePropertyChanged(nameof(PictureData));
            }
        }
    }

    public string PictureExtension
    {
        get
        {
            //TODO: see if this code can be merged with the SetMime method.
            throw new NotImplementedException("PictureExtension is not implemented yet.");
            LoadPicture();

            if (_picture == null)
            {
                return null;
            }


            var width = Picture.Width;
            var height = Picture.Height;
            var image = Image.LoadPixelData<Rgba32>(_pictureData, width, height);
            //image.Metadata.DecodedImageFormat
            //if (_picture.RawFormat.Equals(ImageFormat.Bmp))
            //    return "bmp";
            //else if (_picture.RawFormat.Equals(ImageFormat.Emf))
            //    return "emf";
            //else if (_picture.RawFormat.Equals(ImageFormat.Exif))
            //    return null;  // TODO - Unsure of MIME type?
            //else if (_picture.RawFormat.Equals(ImageFormat.Gif))
            //    return "gif";
            //else if (_picture.RawFormat.Equals(ImageFormat.Icon))
            //    return "ico";
            //else if (_picture.RawFormat.Equals(ImageFormat.Jpeg))
            //    return "jpg";
            //else if (_picture.RawFormat.Equals(ImageFormat.MemoryBmp))
            //    return "bmp";
            //else if (_picture.RawFormat.Equals(ImageFormat.Png))
            //    return "png";
            //else if (_picture.RawFormat.Equals(ImageFormat.Tiff))
            //    return "tif";
            //else if (_picture.RawFormat.Equals(ImageFormat.Wmf))
            //    return "wmf";
            //else
            //    return "";
        }
    }

    public Image Picture
    {
        get
        {
            if (!_pictureCached)
            {
                LoadPicture();
            }

            //return (_picture == null ? null : (Image)_picture.Clone());
            //TODO: Test. If it is good simplify the condition below.
            if (_picture == null)
            {
                return null;
            }
            else if (_picture is Image<Rgba32> rgbaImage)
            {
                return rgbaImage.Clone();//TODO: Why do we need to clone the image?
            }
            else
            {
                throw new InvalidOperationException("Image is not Rgba32 and cannot be cloned.");
            }
        }
        set
        {
            if (_picture != value)
            {
                _picture?.Dispose();
                _picture = value;

                if (value == null)
                {
                    _pictureData = null;
                }
                else
                {
                    if (!_loadingPicture)
                    {
                        using var ms = new MemoryStream();
                        var format = value.Metadata.DecodedImageFormat;
                        value.Save(ms, format);
                        _pictureData = ms.ToArray();

                        SetMimeType();
                    }
                }
            }

            RaisePropertyChanged(nameof(Picture));
        }
    }

    private void SetMimeType()
    {
        //TODO: See if this code can be merged with the PictureExtension property.
        //LoadPicture();//TODO: Why do we need to load picture at this point? If SetMime is called the image should be loaded already?

        //TODO: Add more formats as needed.
        if (_picture != null)
        {
            var format = _picture.Metadata.DecodedImageFormat;
            //var format = _picture.RawFormat;

            if (format.Equals(BmpFormat.Instance))
            {
                MimeType = "image/bmp";
            }
            //else if (format.Equals(ImageFormat.Emf))
            //{
            //    MimeType = "image/x-emf";
            //}
            //else if (format.Equals(ImageFormat.Exif))
            //{
            //    // TODO - Unsure of MIME type?
            //}
            else if (format.Equals(GifFormat.Instance))
            {
                MimeType = "image/gif";
            }
            //else if (format.Equals(ImageFormat.Icon))
            //{
            //    // TODO - How to handle this?
            //}
            else if (format.Equals(JpegFormat.Instance))
            {
                MimeType = "image/jpeg";
            }
            //else if (format.Equals(ImageFormat.MemoryBmp))
            //{
            //    MimeType = "image/bmp";
            //}
            else if (format.Equals(PngFormat.Instance))
            {
                MimeType = "image/png";
            }
            else if (format.Equals(TiffFormat.Instance))
            {
                MimeType = "image/tiff";
            }
            //else if (format.Equals(ImageFormat.Wmf))
            //{
            //    MimeType = "image/x-wmf";
            //}
            else
            {
                // TODO
                //MimeType = "image/";
                throw new NotSupportedException($"Unsupported image format: {format.Name}"); //TODO: Is this correct? Should we throw an exception here?
            }
        }
    }

    public override string GetFrameID(ID3v2TagVersion tagVersion)
    {
        switch (tagVersion)
        {
            case ID3v2TagVersion.ID3v24:
            case ID3v2TagVersion.ID3v23:
                return "APIC";
            case ID3v2TagVersion.ID3v22:
                return "PIC";
            default:
                throw new ArgumentException("Unknown tag version");
        }
    }

    public override void Read(TagReadingInfo tagReadingInfo, Stream stream)
    {
        // Read header
        _frameHeader.Read(tagReadingInfo, ref stream);

        // Read frame data
        var bytesLeft = _frameHeader.FrameSizeExcludingAdditions;
        if (bytesLeft >= 6) // note: 6 was chosen arbitrarily
        {
            // Read text encoding
            TextEncoding = (EncodingType)stream.Read1(ref bytesLeft);

            if (tagReadingInfo.TagVersion == ID3v2TagVersion.ID3v22)
            {
                // TODO: Do something with this?
                var imageFormat = ID3v2Utils.ReadString(EncodingType.ISO88591, stream, 3);
                bytesLeft -= 3;
            }
            else
            {
                // Read MIME type
                MimeType = ID3v2Utils.ReadString(EncodingType.ISO88591, stream, ref bytesLeft);
            }

            // Read picture type
            PictureType = (PictureType)stream.Read1(ref bytesLeft);

            // Short description
            Description = ID3v2Utils.ReadString(TextEncoding, stream, ref bytesLeft);

            // Picture data
            if (bytesLeft > 0)
            {
                var pictureData = stream.Read(bytesLeft);
                bytesLeft = 0;
                _readingTag = true;
                try
                {
                    _pictureCached = false;
                    PictureData = pictureData;
                }
                finally
                {
                    _readingTag = false;
                }
            }
            else
            {
                // Incomplete frame
                PictureData = null;
            }
        }
        else
        {
            // Incomplete frame
            TextEncoding = EncodingType.ISO88591;
            Description = null;
            MimeType = null;
            PictureType = PictureType.CoverFront;
            PictureData = null;
        }

        // Seek to end of frame
        if (bytesLeft > 0)
        {
            stream.Seek(bytesLeft, SeekOrigin.Current);
        }
    }

    public override byte[] GetBytes(ID3v2TagVersion tagVersion)
    {
        if (_pictureData == null || _pictureData.Length == 0)
        {
            return new byte[0];
        }

        // iTunes doesn't like Unicode in APIC descriptions - fixed in iTunes 7.1.0.59
        //TextEncoding = EncodingType.ISO88591;

        using (var stream = new MemoryStream())
        {
            byte[] descriptionData;

            do
            {
                descriptionData = ID3v2Utils.GetStringBytes(tagVersion, _textEncoding, _description, true);
            } while (
                this.RequiresFix(tagVersion, _description, descriptionData)
            );

            stream.WriteByte((byte)_textEncoding);
            if (tagVersion == ID3v2TagVersion.ID3v22)
            {
                var format = PictureExtension;
                if (string.IsNullOrEmpty(format) || format.Length < 3)
                {
                    format = "   ";
                }
                else if (format.Length > 3)
                {
                    format = format[..3]; //TODO: This condition is correct? Why only 3 characters?
                }

                stream.Write(Encoding.ASCII.GetBytes(format));
            }
            else
            {
                SetMimeType(); // iTunes needs this set properly
                stream.Write(ByteUtils.ISO88591GetBytes(_mimeType));
                stream.WriteByte(0); // terminator
            }

            stream.WriteByte((byte)_pictureType);
            stream.Write(descriptionData);
            stream.Write(_pictureData);
            return _frameHeader.GetBytes(stream, tagVersion, GetFrameID(tagVersion));
        }
    }

    private void LoadPicture()
    {
        _pictureCached = true;

        if (_pictureData == null)
        {
            Picture = null;
            return;
        }

        using var memoryStream = new MemoryStream(_pictureData);
        var isInvalidImage = false;

        try
        {
            _loadingPicture = true;
            try
            {
                Picture = Image.Load(memoryStream); //TODO: Changed. Needs to be tested.
            }
            catch
            {
                _pictureCached = false;
                throw;
            }
            finally
            {
                _loadingPicture = false;
            }
        }
        catch (OutOfMemoryException)
        {
            var msg = "OutOfMemoryException caught in APIC's PictureData setter";
            Trace.WriteLine(msg);

            isInvalidImage = true;
        }
        catch (ArgumentException)
        {
            var msg = "ArgumentException caught in APIC's PictureData setter";
            Trace.WriteLine(msg);

            isInvalidImage = true;
        }

        if (isInvalidImage)
        {
            // Invalid image
            _picture?.Dispose();
            _picture = null;

            try
            {
                var url = ByteUtils.ISO88591GetString(_pictureData);
                if (url.Contains("://", StringComparison.Ordinal))
                {
                    MimeType = "-->";
                }
            }
            catch (Exception ex)
            {
                // don't throw an exception
                Trace.WriteLine(ex);
            }
        }
    }
}
