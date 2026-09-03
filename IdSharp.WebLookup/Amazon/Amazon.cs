using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

using IdSharp.Common.Utils;
using IdSharp.Utils;

namespace IdSharp.WebLookup.Amazon;

/// <summary>
/// Amazon
/// </summary>
public static class Amazon
{
    private static readonly Dictionary<AmazonServer, string> _amazonServers = new()        {
            { AmazonServer.UnitedStates, "amazonaws.com" },
            { AmazonServer.Germany, "amazonaws.de" },
            { AmazonServer.Japan, "amazonaws.jp" },
            { AmazonServer.UnitedKingdom, "amazonaws.co.uk" },
            { AmazonServer.France, "amazonaws.fr" },
            { AmazonServer.Canada, "amazonaws.ca" }
        };
    internal const string HttpRequestUri = "/onca/xml";

    private static void FixSearchString(ref string value)
    {
        if (value == null)
        {
            return;
        }

        // remove everything after parentheses
        var parenIndex = value.IndexOf('(', StringComparison.Ordinal);
        if (parenIndex > 0)
        {
            value = value.Substring(0, parenIndex);
        }

        // remove all characters that are not letters, digits, or spaces
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (!char.IsLetterOrDigit(c) && c != ' ')
            {
                value = value.Replace(c, ' ');
                i--;
            }
        }

        // remove dangling digits
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (char.IsDigit(c))
            {
                if (i > 0)
                {
                    if (value[i - 1] != ' ' || (i != value.Length - 1 && value[i + 1] != ' '))
                    {
                        continue;
                    }
                }
                else
                {
                    if (i != value.Length - 1 && value[i + 1] != ' ')
                    {
                        continue;
                    }
                }

                value = value.Remove(i, 1);
                i--;
            }
        }

        // remove common words
        value = value.ToUpperInvariant();

        value = value.Replace(" THE ", " ", StringComparison.Ordinal);
        value = value.Replace(" A ", " ", StringComparison.Ordinal);
        value = value.Replace(" OF ", " ", StringComparison.Ordinal);
        value = value.Replace(" AND ", " ", StringComparison.Ordinal);
        value = value.Replace(" AN ", " ", StringComparison.Ordinal);
        value = value.Replace(" OST ", " ", StringComparison.Ordinal);
        value = value.Replace(" IN ", " ", StringComparison.Ordinal);
        value = value.Replace(" DISC ", " ", StringComparison.Ordinal);
        value = value.Replace(" DISK ", " ", StringComparison.Ordinal);
        value = value.Replace(" CD ", " ", StringComparison.Ordinal);

        value = value.Trim();

        if (value == "VARIOUS")
        {
            value = string.Empty;
        }

        //value = value.Replace(' ', '+');
    }

    /// <summary>
    /// Search
    /// </summary>
    /// <param name="server">The server.</param>
    /// <param name="awsAccessKeyId">Required: Your AWSAccessKeyId.</param>
    /// <param name="secretAccessKey">Required: Your Secret Access Key.</param>
    /// <param name="artist">The artist.</param>
    /// <param name="album">The album.</param>
    /// <param name="keywords">The keywords.</param>
    /// <param name="page">The page.</param>
    public static AmazonSearchResponse Search(AmazonServer server, string awsAccessKeyId, string secretAccessKey, string artist, string album, string keywords, int page)
    {
        if (string.IsNullOrWhiteSpace(awsAccessKeyId))
        {
            throw new ArgumentNullException(nameof(awsAccessKeyId));
        }

        if (string.IsNullOrWhiteSpace(secretAccessKey))
        {
            throw new ArgumentNullException(nameof(secretAccessKey));
        }

        var amazonDomain = GetDomain(server);

        FixSearchString(ref artist);
        FixSearchString(ref album);
        FixSearchString(ref keywords);

        var sort = (String.IsNullOrEmpty(artist) ? "artistrank" : "titlerank");
        var postData = new List<PostData>();
        postData.Add(new PostData("Service", "AWSECommerceService"));
        postData.Add(new PostData("AWSAccessKeyId", awsAccessKeyId));
        postData.Add(new PostData("Operation", "ItemSearch"));
        postData.Add(new PostData("SearchIndex", "Music"));
        if (!string.IsNullOrEmpty(artist))
        {
            postData.Add(new PostData("Artist", artist));
        }

        if (!string.IsNullOrEmpty(album))
        {
            postData.Add(new PostData("Title", album));
        }

        if (!string.IsNullOrEmpty(keywords))
        {
            postData.Add(new PostData("Keywords", keywords));
        }

        postData.Add(new PostData("ItemPage", page.ToString(CultureInfo.InvariantCulture)));
        postData.Add(new PostData("Sort", sort));
        postData.Add(new PostData("Timestamp", $"{DateTime.UtcNow:yyyy-MM-dd}T{DateTime.UtcNow:HH:mm:ss}Z"));

        var hostHeader = $"ecs.{amazonDomain}";
        var signature = GetSignature(postData, hostHeader, secretAccessKey);
        postData.Add(new PostData("Signature", signature));

        var requestUri = $"http://{hostHeader}{HttpRequestUri}";
        requestUri = Http.GetQueryString(requestUri, postData);
        var byteResponse = Http.Get(requestUri);
        if (byteResponse == null)
        {
            throw new WebException($"Response from {amazonDomain} was null");
        }

        var response = Encoding.UTF8.GetString(byteResponse);

        var result = new AmazonSearchResponse();
        result.TotalPages = 0;
        result.TotalResults = 0;

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(response);
        foreach (XmlNode node in xmlDocument.ChildNodes)
        {
            if (node.Name == "ItemSearchResponse")
            {
                foreach (XmlNode responseNode in node.ChildNodes)
                {
                    if (responseNode.Name == "OperationRequest")
                    {
                        foreach (XmlNode opReqNode in responseNode.ChildNodes)
                        {
                            if (opReqNode.Name == "Errors")
                            {
                                var fullErrorMessage = "";
                                foreach (XmlNode errorNode in opReqNode.ChildNodes)
                                {
                                    if (errorNode.Name == "Error")
                                    {
                                        var errorMessage = "";
                                        var errorCode = "";
                                        foreach (XmlNode errorItemNode in errorNode.ChildNodes)
                                        {
                                            if (errorItemNode.Name == "Code")
                                            {
                                                errorCode = errorItemNode.InnerText;
                                            }
                                            else if (errorItemNode.Name == "Message")
                                            {
                                                errorMessage = errorItemNode.InnerText;
                                            }
                                        }

                                        if (errorMessage != "" || errorCode != "")
                                        {
                                            if (errorCode != "")
                                            {
                                                errorMessage = $"{errorMessage} ({errorCode})";
                                            }

                                            if (fullErrorMessage != "")
                                            {
                                                fullErrorMessage += "\n\n";
                                            }

                                            fullErrorMessage += errorMessage;
                                        }
                                    }
                                }

                                if (fullErrorMessage != "")
                                {
                                    throw new Exception(fullErrorMessage);
                                }
                            }
                        }
                    }
                    else if (responseNode.Name == "Items")
                    {
                        foreach (XmlNode itemNode in responseNode.ChildNodes)
                        {
                            if (itemNode.Name == "TotalResults")
                            {
                                int totalResults;
                                if (int.TryParse(itemNode.InnerText, out totalResults))
                                {
                                    result.TotalResults = totalResults;
                                }
                            }
                            else if (itemNode.Name == "TotalPages")
                            {
                                int totalPages;
                                if (int.TryParse(itemNode.InnerText, out totalPages))
                                {
                                    result.TotalPages = totalPages;
                                }
                            }
                            else if (itemNode.Name == "Item")
                            {
                                var albumItem = new AmazonAlbum(server, awsAccessKeyId, secretAccessKey);
                                result.Items.Add(albumItem);
                                foreach (XmlNode itemDetail in itemNode.ChildNodes)
                                {
                                    if (itemDetail.Name == "ASIN")
                                    {
                                        albumItem.Asin = itemDetail.InnerText;
                                    }
                                    else if (itemDetail.Name == "DetailPageURL")
                                    {
                                        albumItem.DetailPageUrl = itemDetail.InnerText;
                                    }
                                    else if (itemDetail.Name == "ItemAttributes")
                                    {
                                        foreach (XmlNode itemAttribute in itemDetail.ChildNodes)
                                        {
                                            if (itemAttribute.Name == "Artist")
                                            {
                                                albumItem.Artist = itemAttribute.InnerText;
                                            }
                                            else if (itemAttribute.Name == "Manufacturer")
                                            {
                                                albumItem.Manufacturer = itemAttribute.InnerText;
                                            }
                                            else if (itemAttribute.Name == "Title")
                                            {
                                                albumItem.Album = itemAttribute.InnerText;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    internal static string GetSignature(List<PostData> postData, string hostHeader, string secretAccessKey)
    {
        var ordered = GetOrderedPostData(postData);
        StringBuilder getString = new();
        foreach (var item in ordered)
        {
            if (getString.Length != 0)
            {
                getString.Append("&");
            }

            getString.Append(item.Field);
            getString.Append("=");
            foreach (var c in item.Value)
            {
                if (c <= 255)
                {
                    if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || (c == '-') || (c == '_') || (c == '.') || (c == '~'))
                    {
                        getString.Append(c);
                    }
                    else
                    {
                        getString.Append("%" + ((int)c).ToString("X2", CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        var stringToSign = $"GET{"\n"}{hostHeader}{"\n"}{HttpRequestUri}{"\n"}{getString}";
        var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(secretAccessKey));
        var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(stringToSign));
        var signature = Convert.ToBase64String(hash).Replace("+", "%2B", StringComparison.Ordinal).Replace("=", "%3D", StringComparison.Ordinal);
        return signature;
    }

    private static List<PostData> GetOrderedPostData(List<PostData> postData)
    {
        List<PostData> newList = [.. postData];
        newList.Sort(new PostDataComparer());
        return newList;
    }

    private class PostDataComparer : Comparer<PostData>
    {
        public override int Compare(PostData x, PostData y)
        {
            return string.CompareOrdinal(x.Field, y.Field);
        }
    }

    /// <summary>
    /// Gets the domain for a specified AmazonServer.
    /// </summary>
    /// <param name="server">The server.</param>
    public static string GetDomain(AmazonServer server)
    {
        return _amazonServers[server];
    }
}
