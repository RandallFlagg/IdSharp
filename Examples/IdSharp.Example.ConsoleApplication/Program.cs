using System;
using System.IO;

using IdSharp.AudioInfo;
using IdSharp.Common.Utils;
using IdSharp.Tagging.ID3v1;
using IdSharp.Tagging.ID3v2;
using IdSharp.Tagging.VorbisComment;

var fileName = GetFileName(args);
if (fileName == null)
{
    return;
}

Console.WriteLine();
Console.WriteLine($"File: {fileName}");
Console.WriteLine();

var audioFile = AudioFile.Create(fileName, true);

Console.WriteLine("Audio Info");
Console.WriteLine();
Console.WriteLine($"Type:      {EnumUtils.GetDescription(audioFile.FileType)}");
Console.WriteLine($"Length:    {(int)audioFile.TotalSeconds / 60}:{(int)audioFile.TotalSeconds % 60:00}");
Console.WriteLine($"Bitrate:   {(int)audioFile.Bitrate:#,0} kbps");
Console.WriteLine($"Frequency: {audioFile.Frequency:#,0} Hz");
Console.WriteLine($"Channels:  {audioFile.Channels}");
Console.WriteLine();

if (ID3v2Tag.DoesTagExist(fileName))
{
    Console.WriteLine(EnumUtils.GetDescription(new ID3v2Tag(fileName).Header.TagVersion));
    Console.WriteLine();

    Console.WriteLine($"Artist:    {new ID3v2Tag(fileName).Artist}");
    Console.WriteLine($"Title:     {new ID3v2Tag(fileName).Title}");
    Console.WriteLine($"Album:     {new ID3v2Tag(fileName).Album}");
    Console.WriteLine($"Year:      {new ID3v2Tag(fileName).Year}");
    Console.WriteLine($"Track:     {new ID3v2Tag(fileName).TrackNumber}");
    Console.WriteLine($"Genre:     {new ID3v2Tag(fileName).Genre}");
    Console.WriteLine($"Pictures:  {new ID3v2Tag(fileName).PictureList.Count}");
    Console.WriteLine($"Comments:  {new ID3v2Tag(fileName).CommentsList.Count}");
    Console.WriteLine();

    // Example of saving an ID3v2 tag
    //
    // id3v2.Title = "New song title";
    // id3v2.Save(fileName);
}

if (ID3v1Tag.DoesTagExist(fileName))
{
    Console.WriteLine(EnumUtils.GetDescription(new ID3v1Tag(fileName).TagVersion));
    Console.WriteLine();

    Console.WriteLine($"Artist:    {new ID3v1Tag(fileName).Artist}");
    Console.WriteLine($"Title:     {new ID3v1Tag(fileName).Title}");
    Console.WriteLine($"Album:     {new ID3v1Tag(fileName).Album}");
    Console.WriteLine($"Year:      {new ID3v1Tag(fileName).Year}");
    Console.WriteLine($"Comment:   {new ID3v1Tag(fileName).Comment}");
    Console.WriteLine($"Track:     {new ID3v1Tag(fileName).TrackNumber}");
    Console.WriteLine($"Genre:     {GenreHelper.GenreByIndex[new ID3v1Tag(fileName).GenreIndex]}");
    Console.WriteLine();

    // Example of saving an ID3v1 tag
    //
    // id3v1.Title = "New song title";
    // id3v1.Save(fileName);
}

if (audioFile.FileType == AudioFileType.Flac)
{
    var vorbis = new VorbisComment(fileName);

    Console.WriteLine("Vorbis Comment");
    Console.WriteLine();

    Console.WriteLine($"Artist:    {vorbis.Artist}");
    Console.WriteLine($"Title:     {vorbis.Title}");
    Console.WriteLine($"Album:     {vorbis.Album}");
    Console.WriteLine($"Year:      {vorbis.Year}");
    Console.WriteLine($"Comment:   {vorbis.Comment}");
    Console.WriteLine($"Track:     {vorbis.TrackNumber}");
    Console.WriteLine($"Genre:     {vorbis.Genre}");
    Console.WriteLine($"Vendor:    {vorbis.Vendor}");
    Console.WriteLine();

    // Example of saving a Vorbis Comment
    //
    // vorbis.Title = "New song title";
    // vorbis.Save(fileName);
}

static string GetFileName(string[] args)
{
    string fileName;

    if (args.Length == 1)
    {
        fileName = args[0];
    }
    else
    {
        Console.Write("File name: ");
        fileName = Console.ReadLine()?.Trim('"');
    }

    if (!File.Exists(fileName.Trim()))
    {
        var tryFileName = Path.Combine(Environment.CurrentDirectory, fileName);
        if (File.Exists(tryFileName))
        {
            fileName = tryFileName;
        }
        else
        {
            var baseDir = AppContext.BaseDirectory;
            while (baseDir != null)
            {
                tryFileName = Path.Combine(baseDir, fileName);
                if (File.Exists(tryFileName))
                {
                    fileName = tryFileName;
                    break;
                }

                var testFilesDir = Path.Combine(baseDir, "TestFiles");
                if (Directory.Exists(testFilesDir))
                {
                    fileName = Path.Combine(testFilesDir, Path.GetFileName(fileName));
                    break;
                }

                baseDir = Directory.GetParent(baseDir)?.FullName;
            }

            if (!File.Exists(fileName))
            {
                Console.WriteLine($"\"{fileName}\" not found.");
                return null;
            }
        }
    }

    return fileName;
}
