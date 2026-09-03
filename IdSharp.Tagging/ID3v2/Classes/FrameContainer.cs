using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using IdSharp.Common.Events;
using IdSharp.Common.Utils;
using IdSharp.Tagging.ID3v1;
using IdSharp.Tagging.ID3v2.Frames;

namespace IdSharp.Tagging.ID3v2;

/// <summary>
/// ID3v2 Frame Container
/// </summary>
public abstract partial class FrameContainer : IFrameContainer
{
    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Occurs when invalid data is assigned to a property.
    /// </summary>
    public event EventHandler<InvalidDataEventArgs> InvalidData;

    internal void Read(Stream stream, ID3v2TagVersion tagVersion, TagReadingInfo tagReadingInfo, int readUntil, int frameIDSize)
    {
        var multipleOccurrenceFrames = GetMultipleOccurrenceFrames(tagVersion);
        var singleOccurrenceFrames = GetSingleOccurrenceFrames(tagVersion);

        var bytesRead = 0;
        while (bytesRead < readUntil)
        {
            var frameIDBytes = stream.Read(frameIDSize);

            // If character is not a letter or number, padding reached, audio began,
            // or otherwise the frame is not readable
            if (frameIDSize == 4)
            {
                if (frameIDBytes[0] < 0x30 || frameIDBytes[0] > 0x5A ||
                    frameIDBytes[1] < 0x30 || frameIDBytes[1] > 0x5A ||
                    frameIDBytes[2] < 0x30 || frameIDBytes[2] > 0x5A ||
                    frameIDBytes[3] < 0x30 || frameIDBytes[3] > 0x5A)
                {
                    // TODO: Try to keep reading and look for a valid frame
                    if (frameIDBytes[0] != 0 && frameIDBytes[0] != 0xFF)
                    {
                        var msg = $"Out of range FrameID - 0x{frameIDBytes[0]:X}|0x{frameIDBytes[1]:X}|0x{frameIDBytes[2]:X}|0x{frameIDBytes[3]:X}";
                        if (ByteUtils.ISO88591GetString(frameIDBytes) != "MP3e")
                        {
                            var tmpBadFrameID = ByteUtils.ISO88591GetString(frameIDBytes).TrimEnd('\0');
                            Trace.WriteLine(msg + " - " + tmpBadFrameID);
                        }
                    }

                    break;
                }
            }
            else if (frameIDSize == 3)
            {
                if (frameIDBytes[0] < 0x30 || frameIDBytes[0] > 0x5A ||
                    frameIDBytes[1] < 0x30 || frameIDBytes[1] > 0x5A ||
                    frameIDBytes[2] < 0x30 || frameIDBytes[2] > 0x5A)
                {
                    // TODO: Try to keep reading and look for a valid frame
                    if (frameIDBytes[0] != 0 && frameIDBytes[0] != 0xFF)
                    {
                        var msg = $"Out of range FrameID - 0x{frameIDBytes[0]:X}|0x{frameIDBytes[1]:X}|0x{frameIDBytes[2]:X}";
                        Trace.WriteLine(msg);
                        Trace.WriteLine(ByteUtils.ISO88591GetString(frameIDBytes));
                    }

                    break;
                }
            }

            var frameID = ByteUtils.ISO88591GetString(frameIDBytes);

            // TODO: Take out
            //Console.WriteLine(tmpFrameID); // TODO: take out
            /*
                COMM Frames:
                SoundJam_CDDB_TrackNumber
                SoundJam_CDDB_1
                iTunNORM
                iTunSMPB
                iTunes_CDDB_IDs
                iTunes_CDDB_1
                iTunes_CDDB_TrackNumber
             */

            IFrame frame;
            do
            {
                IBindingList bindingList;
                if (singleOccurrenceFrames.TryGetValue(frameID, out frame))
                {
                    frame.Read(tagReadingInfo, stream);
                    bytesRead += frame.FrameHeader.FrameSizeTotal;
                    //m_ReadFrames.Add(tmpFrame);
                }
                else if (multipleOccurrenceFrames.TryGetValue(frameID, out bindingList))
                {
                    frame = (IFrame)bindingList.AddNew();
                    frame.Read(tagReadingInfo, stream);
                    //m_ReadFrames.Add(tmpFrame);
                    bytesRead += frame.FrameHeader.FrameSizeTotal;
                }
                else
                {
                    if (tagVersion == ID3v2TagVersion.ID3v24)
                    {
                        string newFrameID;
                        if (_id3v24FrameAliases.TryGetValue(frameID, out newFrameID))
                        {
                            frameID = newFrameID;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (tagVersion == ID3v2TagVersion.ID3v23)
                    {
                        string newFrameID;
                        if (_id3v23FrameAliases.TryGetValue(frameID, out newFrameID))
                        {
                            frameID = newFrameID;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            } while (frame == null);

            // Frame is unknown
            if (frame == null)
            {
                if (frameID != "NCON" &&  // non standard (old music match)
                    frameID != "MJMD" && // Non standard frame (Music Match XML)
                    frameID != "TT22" && // 000.00 - maybe meant to be 3 letter TT2 frame, and value be 2000.00? still makes no sense
                    frameID != "PCST" && // null data
                    frameID != "TCAT" && // category (ie, comedy) (distorted view)
                    frameID != "TKWD" && // looks like blog tags "comedy funny weird", etc (distorted view)
                    frameID != "TDES" && // xml file - used by distortedview.com
                    frameID != "TGID" && // url (from distortedview)
                    frameID != "WFED" && // url (thanks distortedview)
                    frameID != "CM1" && // some kind of comment, seen in ID3v2.2
                    frameID != "TMB" && // ripped by something other, not in spec
                    frameID != "RTNG" &&
                    frameID != "XDOR" && // year
                    frameID != "XSOP" && // looks like artist, "Allman Brothers Band, The"
                    frameID != "TENK") // itunes encoder (todo: add alias?)
                {
                    /*String msg = String.Format("Unrecognized FrameID '{0}' (not critical)", tmpFrameID);
                    Trace.WriteLine(msg);*/
                }

                var unknownFrame = new UnknownFrame(frameID, tagReadingInfo, stream);
                _unknownFrames.Add(unknownFrame);
                //m_ReadFrames.Add(tmpUNKN);
                bytesRead += unknownFrame.FrameHeader.FrameSizeTotal;
            }
        }

        // Process iTunes comments
        var iTunesCommentsInsertIndex = m_iTunesCommentsList.Count;
        for (var i = m_CommentsList.Count - 1; i >= 0; i--)
        {
            var comment = m_CommentsList[i];
            if (comment.Description?.StartsWith("iTun", StringComparison.Ordinal) == true)
            {
                m_CommentsList.RemoveAt(i);
                m_iTunesCommentsList.Insert(iTunesCommentsInsertIndex, comment);
            }
        }

        // Process ReplayGain frames
        foreach (var frame in new List<ITXXXFrame>(m_UserDefinedTextList))
        {
            if (frame.Description != null && frame.Description.ToUpper(CultureInfo.InvariantCulture).StartsWith("REPLAYGAIN_"))
            {
                m_UserDefinedTextList.Remove(frame);
                m_ReplayGainList.Add(frame);
            }
        }

        // Process genre // TODO: may need cleanup
        if (!string.IsNullOrEmpty(m_Genre.Value))
        {
            if (m_Genre.Value.StartsWith("("))
            {
                var closeIndex = m_Genre.Value.IndexOf(')', StringComparison.Ordinal);
                if (closeIndex != -1)
                {
                    if (closeIndex != m_Genre.Value.Length - 1)
                    {
                        // Take text description
                        m_Genre.Value = m_Genre.Value.Substring(closeIndex + 1, m_Genre.Value.Length - (closeIndex + 1));
                    }
                    else
                    {
                        // Lookup genre value
                        var innerValue = m_Genre.Value.Substring(1, closeIndex - 1);
                        int innerValueResult;
                        if (int.TryParse(innerValue, out innerValueResult))
                        {
                            if (GenreHelper.GenreByIndex.Length > innerValueResult && innerValueResult >= 0)
                            {
                                m_Genre.Value = GenreHelper.GenreByIndex[innerValueResult];
                            }
                            else
                            {
                                Trace.WriteLine("Unrecognized genre");
                            }
                        }
                        else
                        {
                            Trace.WriteLine("Unrecognized genre");
                        }
                    }
                }
            }
        }
    }

    internal List<IFrame> GetAllFrames(ID3v2TagVersion tagVersion)
    {
        var multipleOccurrenceFrames = GetMultipleOccurrenceFrames(tagVersion);
        var singleOccurenceFrames = GetSingleOccurrenceFrames(tagVersion);

        var allFrames = new List<IFrame>();
        allFrames.AddRange(singleOccurenceFrames.Select(p => p.Value));
        foreach (var kvp in multipleOccurrenceFrames)
        {
            var bindingList = kvp.Value;
            allFrames.AddRange(bindingList.Cast<IFrame>());

            // Special handling for iTunes comment frames
            if (kvp.Key == "COMM" || kvp.Key == "COM")
            {
                allFrames.AddRange(m_iTunesCommentsList);
            }

            // Special handling for ReplayGain frames
            if (kvp.Key == "TXXX" || kvp.Key == "TXX")
            {
                allFrames.AddRange(m_ReplayGainList);
            }
            
        }

        allFrames.AddRange(_unknownFrames);

        foreach (var frame in new List<IFrame>(allFrames))
        {
            if (frame.GetBytes(tagVersion).Length == 0)
            {
                allFrames.Remove(frame);
            }
        }

        return allFrames;
    }

    internal List<IFrame> GetAllFrames(ID3v2TagVersion tagVersion, string frameID)
    {
        if (string.IsNullOrEmpty(frameID))
        {
            throw new ArgumentNullException(nameof(frameID));
        }

        return GetAllFrames(tagVersion, new List<string> { frameID });
    }

    internal List<IFrame> GetAllFrames(ID3v2TagVersion tagVersion, IEnumerable<string> frameIDs)
    {
        ArgumentNullException.ThrowIfNull(frameIDs);

        if (frameIDs.Any() == false)
        {
            return new List<IFrame>();
        }

        var allFrames = GetAllFrames(tagVersion);
        for (var i = allFrames.Count - 1; i >= 0; i--)
        {
            var frame = allFrames[i];

            // Cached frame IDs.
            var iD3v22ID = frame.GetFrameID(ID3v2TagVersion.ID3v22);
            var iD3v23ID = frame.GetFrameID(ID3v2TagVersion.ID3v23);
            var iD3v24ID = frame.GetFrameID(ID3v2TagVersion.ID3v24);

            var found = false;
            foreach (var frameID in frameIDs)
            {
                if (iD3v22ID.Equals(frameID, StringComparison.Ordinal) ||
                    iD3v23ID.Equals(frameID, StringComparison.Ordinal) ||
                    iD3v24ID.Equals(frameID, StringComparison.Ordinal))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                allFrames.RemoveAt(i);
            }
        }

        return allFrames;
    }

    internal byte[] GetBytes(ID3v2TagVersion tagVersion)
    {
        using (var frameData = new MemoryStream())
        {
            // Note: this doesn't use GetAllFrames() because it would cause multiple calls to IFrame.GetBytes

            var multipleOccurrenceFrames = GetMultipleOccurrenceFrames(tagVersion);
            var singleOccurrenceFrames = GetSingleOccurrenceFrames(tagVersion);

            foreach (var frame in singleOccurrenceFrames.Values)
            {
                var rawData = frame.GetBytes(tagVersion);
                frameData.Write(rawData);
            }

            foreach (var kvp in multipleOccurrenceFrames)
            {
                var bindingList = kvp.Value;

                foreach (var frame in bindingList.Cast<IFrame>())
                {
                    var rawData = frame.GetBytes(tagVersion);
                    frameData.Write(rawData);
                }

                // Special handling for iTunes comment frames
                if (kvp.Key == "COMM" || kvp.Key == "COM")
                {
                    foreach (var frame in m_iTunesCommentsList)
                    {
                        // iTunes will only respect comments that end with a terminating null character
                        var rawData = ((Comments) frame).GetBytes(tagVersion, true);
                        frameData.Write(rawData);
                    }
                }

                // Special handling for ReplayGain frames
                if (kvp.Key == "TXXX" || kvp.Key == "TXX")
                {
                    foreach (var frame in m_ReplayGainList)
                    {   
                        var rawData = frame.GetBytes(tagVersion);
                        frameData.Write(rawData);
                    }
                }
            }

            foreach (var unknownFrame in _unknownFrames)
            {
                //if (m_ReadFrames.Contains(tmpUNKN))
                //    continue;

                var rawData = unknownFrame.GetBytes(tagVersion);
                frameData.Write(rawData);
            }

            return frameData.ToArray();
        }
    }

    /// <summary>
    /// Forces the <see cref="INotifyPropertyChanged.PropertyChanged"/> event to fire.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    public void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
