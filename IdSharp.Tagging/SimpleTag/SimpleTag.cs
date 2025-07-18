using System;
using System.ComponentModel;
using System.IO;
using IdSharp.Common.Utils;
using IdSharp.Tagging.ID3v1;
using IdSharp.Tagging.ID3v2;

namespace IdSharp.Tagging.SimpleTag;

/// <summary>
/// A simple wrapper for working with tags.  This class does not require knowing which tag format was used, however
/// it only offers basic functionality.
/// </summary>
public class SimpleTag : ISimpleTag
{
    private string _fileName;
    private string _title;
    private string _artist;
    private string _album;
    private string _year;
    private string _comment;
    private string _trackNumber;
    private string _genre;
    private string _tagVersion;

    /// <summary>Occurs when a property value changes.</summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleTag"/> class.
    /// </summary>
    /// <param name="path">The path.</param>
    public SimpleTag(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        Read(path);
    }

    /// <summary>Gets or sets the title.</summary>
    /// <value>The title.</value>
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            RaisePropertyChanged(nameof(Title));
        }
    }

    /// <summary>Gets or sets the artist.</summary>
    /// <value>The artist.</value>
    public string Artist
    {
        get => _artist;
        set
        {
            _artist = value;
            RaisePropertyChanged(nameof(Artist));
        }
    }

    /// <summary>Gets or sets the album.</summary>
    /// <value>The album.</value>
    public string Album
    {
        get => _album;
        set
        {
            _album = value;
            RaisePropertyChanged(nameof(Album));
        }
    }

    /// <summary>Gets or sets the year.</summary>
    /// <value>The year.</value>
    public string Year
    {
        get => _year;
        set
        {
            _year = value;
            RaisePropertyChanged(nameof(Year));
        }
    }

    /// <summary>Gets or sets the comment.</summary>
    /// <value>The comment.</value>
    public string Comment
    {
        get => _comment;
        set
        {
            _comment = value;
            RaisePropertyChanged(nameof(Comment));
        }
    }

    /// <summary>Gets or sets the track number.</summary>
    /// <value>The track number.</value>
    public string TrackNumber
    {
        get => _trackNumber;
        set
        {
            _trackNumber = value;
            RaisePropertyChanged(nameof(TrackNumber));
        }
    }

    /// <summary>Gets or sets the genre.</summary>
    /// <value>The genre.</value>
    public string Genre
    {
        get => _genre;
        set
        {
            _genre = value;
            RaisePropertyChanged(nameof(Genre));
        }
    }

    /// <summary>Gets the tag type and version.</summary>
    /// <value>The tag type and version.</value>
    public string TagVersion
    {
        get => _tagVersion;
        private set
        {
            _tagVersion = value;
            RaisePropertyChanged(nameof(TagVersion));
        }
    }

    /// <summary>Saves the tag.</summary>
    public void Save()
    {
        Save(_fileName);
    }

    /// <summary>Reads the tag from the specified stream.</summary>
    /// <param name="stream">The stream.</param>
    private void Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var id3v2Found = false;
        var tagVersion = string.Empty;

        // ID3v2
        if (ID3v2Tag.DoesTagExist(stream))
        {
            var id3v2 = new ID3v2Tag(stream);
            Title = id3v2.Title;
            Artist = id3v2.Artist;
            Album = id3v2.Album;
            Year = id3v2.Year;
            Genre = id3v2.Genre;
            TrackNumber = id3v2.TrackNumber;

            if (id3v2.CommentsList.Count > 0)
            {
                Comment = id3v2.CommentsList[0].Description;
            }
            else
            {
                Comment = null;
            }

            tagVersion = EnumUtils.GetDescription(id3v2.Header.TagVersion);
            
            id3v2Found = true;
        }

        // ID3v1
        if (ID3v1Tag.DoesTagExist(stream))
        {
            var id3v1 = new ID3v1Tag(stream);

            // Use ID3v1 fields if ID3v2 doesn't exist or field is blank
            if (!id3v2Found || string.IsNullOrEmpty(Title))
            {
                Title = id3v1.Title;
            }

            if (!id3v2Found || string.IsNullOrEmpty(Artist))
            {
                Artist = id3v1.Artist;
            }

            if (!id3v2Found || string.IsNullOrEmpty(Album))
            {
                Album = id3v1.Album;
            }

            if (!id3v2Found || string.IsNullOrEmpty(Year))
            {
                Year = id3v1.Year;
            }

            if (!id3v2Found || string.IsNullOrEmpty(Genre))
            {
                Genre = GenreHelper.GenreByIndex[id3v1.GenreIndex];
            }

            if (!id3v2Found || string.IsNullOrEmpty(TrackNumber))
            {
                TrackNumber = id3v1.TrackNumber.ToString();
            }

            if (!id3v2Found || string.IsNullOrEmpty(Comment))
            {
                Comment = id3v1.Comment;
            }

            tagVersion += (!string.IsNullOrEmpty(tagVersion) ? ", " : "") + EnumUtils.GetDescription(id3v1.TagVersion);
        }

        // Vorbis Comment
        if (VorbisComment.VorbisComment.IsFlac(stream))
        {
            var vc = new VorbisComment.VorbisComment(stream);
            Title = vc.Title;
            Artist = vc.Artist;
            Album = vc.Album;
            Year = vc.Year;
            Genre = vc.Genre;
            TrackNumber = vc.TrackNumber;
            Comment = vc.Comment;
            tagVersion = (!string.IsNullOrEmpty(tagVersion) ? ", " : "") + "Vorbis Comment";
        }

        // No tag found
        if (string.IsNullOrEmpty(tagVersion))
        {
            Title = null;
            Artist = null;
            Album = null;
            Year = null;
            Genre = null;
            TrackNumber = null;
            Comment = null;
            tagVersion = "None";
        }

        // Set TagVersion
        TagVersion = tagVersion;
    }

    /// <summary>Saves the tag to the specified path.</summary>
    /// <param name="path">The full path of the file.</param>
    private void Save(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (VorbisComment.VorbisComment.IsFlac(path))
        {
            var vc = new VorbisComment.VorbisComment(path);
            vc.Title = Title;
            vc.Artist = Artist;
            vc.Album = Album;
            vc.Year = Year;
            vc.Genre = Genre;
            vc.TrackNumber = TrackNumber;
            vc.Comment = Comment;
            vc.Save(path);

            ID3v2Tag.RemoveTag(path);
            ID3v1Tag.RemoveTag(path);
        }
        else
        {
            var id3v2 = new ID3v2Tag(path);
            id3v2.Title = Title;
            id3v2.Artist = Artist;
            id3v2.Album = Album;
            id3v2.Year = Year;
            id3v2.Genre = Genre;
            id3v2.TrackNumber = TrackNumber;

            if (id3v2.CommentsList.Count > 0)
            {
                id3v2.CommentsList[0].Description = Comment;
            }
            else
            {
                if (!string.IsNullOrEmpty(Comment))
                {
                    id3v2.CommentsList.AddNew().Description = Comment;
                }
            }

            var id3v1 = new ID3v1Tag(path);
            id3v1.Title = Title;
            id3v1.Artist = Artist;
            id3v1.Album = Album;
            id3v1.Year = Year;
            id3v1.GenreIndex = GenreHelper.GetGenreIndex(Genre);
            id3v1.Comment = Comment;
            int trackNumber;
            if (int.TryParse(TrackNumber, out trackNumber))
            {
                id3v1.TrackNumber = trackNumber;
            }
            else
            {
                // Handle ##/## format
                if (TrackNumber.Contains("/"))
                {
                    if (int.TryParse(TrackNumber.Split('/')[0], out trackNumber))
                    {
                        id3v1.TrackNumber = trackNumber;
                    }
                    else
                    {
                        id3v1.TrackNumber = 0;
                    }
                }
                else
                {
                    id3v1.TrackNumber = 0;
                }
            }

            id3v2.Save(path);
            id3v1.Save(path);
        }

        Read(path);
    }

    /// <summary>Reads the tag from the specified path.</summary>
    /// <param name="path">The full path of the file.</param>
    private void Read(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Read(fileStream);
        }

        _fileName = path;
    }

    private void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
