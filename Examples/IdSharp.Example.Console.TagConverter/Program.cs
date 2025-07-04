using System.Globalization;

using IdSharp.Example.Console.TagConverter;
using IdSharp.Tagging.ID3v2;

bool _isRecursive = false;
string _fileName = null;
string _directory = null;
Verbosity _verbosity = Verbosity.Default;
bool _upgradeOnly = false;
ID3v2TagVersion? _newTagVersion = ID3v2TagVersion.ID3v24;
bool _hasWritten = false;
bool _isTest = false;

var success = ParseArguments(args);
if (!success)
{
    return;
}

var start = DateTime.Now;

if (_isTest && _verbosity >= Verbosity.Default)
{
    WriteLine("TEST - NO ACTUAL FILES WILL BE CHANGED");
    WriteLine();
}

if (_verbosity >= Verbosity.Default)
{
    if (_directory != null)
    {
        WriteLine(string.Format("Directory: \"{0}\"", _directory));
    }
    else
    {
        WriteLine(string.Format("File: \"{0}\"", _fileName));
    }


    string options = "Options: Convert to " + _newTagVersion;
    if (_isTest)
        options += ", test";
    if (_isRecursive)
        options += ", recurisve";
    if (_upgradeOnly)
        options += ", upgrade only";

    WriteLine(options);
}

string?[] files;
if (_directory != null)
{
    WriteLine($"Getting '{_directory}' files...", Verbosity.Full);

    files = Directory.GetFiles(_directory, "*.mp3", _isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
}
else
{
    files = [_fileName];
}

int updated = 0, skipped = 0, failed = 0;
int id3v22Count = 0, id3v23Count = 0, id3v24Count = 0, noid3v2Count = 0;
for (var i = 0; i < files.Length; i++)
{
    var file = files[i];

    try
    {
        WriteLine($"File {(i + 1):#,0}/{files.Length:#,0}: '{file}'", Verbosity.Full);

        var id3v2 = new ID3v2Tag(file);
        if (id3v2.Header != null)
        {
            switch (id3v2.Header.TagVersion)
            {
                case ID3v2TagVersion.ID3v22:
                    id3v22Count++;
                    break;
                case ID3v2TagVersion.ID3v23:
                    id3v23Count++;
                    break;
                case ID3v2TagVersion.ID3v24:
                    id3v24Count++;
                    break;
                default:
                    throw new NotSupportedException($"Unknonw option: {id3v2.Header.TagVersion}");
                    break;
            }

            if (id3v2.Header.TagVersion != _newTagVersion.Value)
            {
                if (!_upgradeOnly || _newTagVersion.Value > id3v2.Header.TagVersion)
                {
                    var oldTagVersion = id3v2.Header.TagVersion;

                    if (!_isTest)
                    {
                        id3v2.Header.TagVersion = _newTagVersion.Value;
                        id3v2.Save(file);
                    }

                    WriteLine($"- Converted {oldTagVersion} to {_newTagVersion.Value}", Verbosity.Full);

                    updated++;
                }
                else
                {
                    WriteLine($"- Skipped, existing tag {id3v2.Header.TagVersion} > {_newTagVersion.Value}", Verbosity.Full);

                    skipped++;
                }
            }
            else
            {
                WriteLine("- Skipped, ID3v2 tag version equal to requested version", Verbosity.Full);

                skipped++;
            }
        }
        else
        {
            WriteLine("- Skipped, ID3v2 tag does not exist", Verbosity.Full);

            skipped++;
            noid3v2Count++;
        }
    }
    catch (Exception ex)
    {
        if (_verbosity >= Verbosity.Default)
        {
            string header = string.Format("File: {0}", file);
            WriteLine(header);
            WriteLine(new string('=', header.Length));
            WriteLine(ex.ToString());
            WriteLine();
        }

        failed++;
    }
}

if (_verbosity >= Verbosity.Default)
{
    DateTime end = DateTime.Now;
    TimeSpan duration = end - start;

    if (_hasWritten)
    {
        WriteLine();
    }

    WriteLine(string.Format("ID3v2.2:   {0:#,0}", id3v22Count));
    WriteLine(string.Format("ID3v2.3:   {0:#,0}", id3v23Count));
    WriteLine(string.Format("ID3v2.4:   {0:#,0}", id3v24Count));
    WriteLine(string.Format("No ID3v2:  {0:#,0}", noid3v2Count));
    WriteLine();
    WriteLine(string.Format("Start:     {0:HH:mm:ss}", start));
    WriteLine(string.Format("End:       {0:HH:mm:ss}", end));
    WriteLine(string.Format("Duration:  {0:hh}:{0:mm}:{0:ss}.{0:fff}", duration));
    WriteLine();
    WriteLine(string.Format("Updated:   {0:#,0}", updated));
    WriteLine(string.Format("Skipped:   {0:#,0}", skipped));
    WriteLine(string.Format("Failed:    {0:#,0}", failed));
}

bool ParseArguments(string[] args)
{
    for (var i = 0; i < args.Length; i++)
    {
        string arg = args[i];

        if (arg.StartsWith("--"))
            arg = arg.Substring(1);

        if (arg == "-2.3" || arg == "-23")
        {
            _newTagVersion = ID3v2TagVersion.ID3v23;
        }
        else if (arg == "-2.4" || arg == "-24")
        {
            _newTagVersion = ID3v2TagVersion.ID3v24;
        }
        else if (arg == "-r" || arg == "-recursive")
        {
            _isRecursive = true;
        }
        else if (arg == "-v" || arg == "-verbose" || arg == "-verbosity")
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException("Missing verbosity level after -v.");

            string level = args[++i];

            switch (level)
            {
                case "0":
                    _verbosity = Verbosity.Silent;
                    break;
                case "1":
                    _verbosity = Verbosity.Default;
                    break;
                case "2":
                    _verbosity = Verbosity.Full;
                    break;
                default:
                    throw new NotSupportedException($"'-v {level}' not supported.");
            }
        }
        else if (arg == "-t" || arg == "-test")
        {
            _isTest = true;
        }
        else if (arg == "-u" || arg == "-up" || arg == "-upgrade" || arg == "-upgradeonly")
        {
            _upgradeOnly = true;
        }
        else
        {
            if (File.Exists(arg))
            {
                _fileName = arg;
            }
            else if (Directory.Exists(arg))
            {
                _directory = arg;
            }
            else
            {
                string message = arg.StartsWith("-")
                    ? $"switch '{arg}' not recognized."
                    : $"'{arg}' not found.";

                WriteLine($"Error: {message}");
                WriteLine();

                PrintUsage();
                return false;
            }
        }
    }

    if (_newTagVersion == null)
    {
        PrintUsage();
        return false;
    }

    if (_fileName == null && _directory == null)
    {
        _directory = Environment.CurrentDirectory;
    }

    return true;
}

void PrintUsage()
{
    WriteLine("tagconvert 0.1 - http://cdtag.com/tagconvert");
    WriteLine();
    WriteLine("Usage: tagconvert -[2.3|2.4] [OPTIONS] [FILE|DIRECTORY]");
    WriteLine();
    WriteLine("Options:");
    WriteLine("  -[2.3|2.4]                 Convert to ID3v2.3 or ID3v2.4 (required)");
    WriteLine("  -recursive, -r             Recursive (include subdirectories)");
    WriteLine("  -verbosity #, -v #         Verbosity:");
    WriteLine("                               0 - Silent");
    WriteLine("                               1 - Options + final report + show errors for failed files (default)");
    WriteLine("                               2 - Full report for each file");
    WriteLine("  -test, -t                  Test, only display what would be performed.");
    WriteLine("  -upgrade, -u               Upgrade only. Useful for upgrading 2.2 to 2.3 without changing 2.4 tags.");
    WriteLine();
    WriteLine("Examples:");
    WriteLine("  tagconvert -2.3 -r \"C:\\Music\"");
    WriteLine("  tagconvert -2.3 -r -upgrade \"C:\\Music\"");
    WriteLine("  tagconvert -2.4 -r -test \"C:\\Music\"");
    WriteLine("  tagconvert -2.3 -r -upgrade -test -verbosity 2 \"C:\\Music\"");
    WriteLine("  tagconvert -2.3 \"C:\\Music\\JustThisDirectory\"");
    WriteLine("  tagconvert -2.3 \"C:\\Music\\JustThisDirectory\\JustThisFile.mp3\"");
}

void WriteLine(string value = null, Verbosity? minimumVerbosity = null)
{
    if (minimumVerbosity == null || _verbosity >= minimumVerbosity)
    {
        _hasWritten = true;
        System.Console.WriteLine(value);
    }
}
