using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

using IdSharp.Common.Utils;

namespace IdSharp.Utils;

/// <summary>
/// Methods for HTTP
/// </summary>
internal static class Http
{
    private const string m_UserAgent = @"Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.0.9) Gecko/2009040821 Firefox/3.0.9 (.NET CLR 3.5.30729)";

    private class RequestAndData
    {
        public HttpWebRequest Request { get; set; }
        public byte[] Data { get; set; }
    }

    /// <summary>
    /// GETs a byte array of data from the specified request URI string.
    /// </summary>
    /// <param name="requestUriString">The request URI string.</param>
    /// <param name="postData">The post data.</param>
    /// <returns>A byte array of data from the specified request URI string.</returns>
    public static byte[] Get(string requestUriString, IEnumerable<PostData> postData)
    {
        if (string.IsNullOrWhiteSpace(requestUriString))
        {
            throw new ArgumentNullException(nameof(requestUriString));
        }

        ArgumentNullException.ThrowIfNull(postData);

        return Get(GetQueryString(requestUriString, postData));
    }

    /// <summary>
    /// GETs a byte array of data from the specified request URI string.
    /// </summary>
    /// <param name="requestUriString">The request URI string.</param>
    /// <returns>A byte array of data from the specified request URI string.</returns>
    public static byte[] Get(string requestUriString)
    {
        return Get(requestUriString, cookies: null, credentials: null);
    }

    /// <summary>
    /// GETs a byte array of data from the specified request URI string.
    /// </summary>
    /// <param name="requestUriString">The request URI string.</param>
    /// <param name="credentials">The Basic Authentication credentials.</param>
    /// <returns>A byte array of data from the specified request URI string.</returns>
    public static byte[] Get(string requestUriString, BasicAuthenticationCredentials credentials)
    {
        return Get(requestUriString, cookies: null, credentials: credentials);
    }

    /// <summary>
    /// GETs a byte array of data from the specified request URI string.
    /// </summary>
    /// <param name="requestUriString">The request URI string.</param>
    /// <param name="cookies">The cookies.</param>
    /// <param name="credentials">The Basic Authentication credentials.</param>
    /// <returns>A byte array of data from the specified request URI string.</returns>
    public static byte[] Get(string requestUriString, CookieContainer cookies, BasicAuthenticationCredentials credentials)
    {
        if (string.IsNullOrWhiteSpace(requestUriString))
        {
            throw new ArgumentNullException(nameof(requestUriString));
        }

        HttpWebResponse webResponse;
        return Get(requestUriString, cookies, credentials, out webResponse);
    }

    /// <summary>
    /// GETs a byte array of data from the specified request URI string.
    /// </summary>
    /// <param name="requestUriString">The request URI string.</param>
    /// <param name="cookies">The cookies.</param>
    /// <param name="credentials">The Basic Authentication credentials.</param>
    /// <param name="webResponse">The web response.</param>
    /// <returns>A byte array of data from the specified request URI string.</returns>
    public static byte[] Get(string requestUriString, CookieContainer cookies, BasicAuthenticationCredentials credentials, out HttpWebResponse webResponse)
    {
        if (string.IsNullOrWhiteSpace(requestUriString))
        {
            throw new ArgumentNullException(nameof(requestUriString));
        }

        var webRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
        webRequest.Proxy = WebRequest.DefaultWebProxy;
        webRequest.CookieContainer = cookies;
        webRequest.UserAgent = m_UserAgent;
        if (credentials != null)
        {
            webRequest.Headers.Add($"Authorization: Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{credentials.UserName}:{credentials.Password}"))}");
        }

        webResponse = (HttpWebResponse)webRequest.GetResponse();

        var data = new byte[256];
        using (var responseStream = webResponse.GetResponseStream())
        {
            using (var memoryStream = new MemoryStream())
            {
                int read;
                do
                {
                    read = responseStream.Read(data, 0, 256);
                    memoryStream.Write(data, 0, read);
                } while (read > 0);

                data = memoryStream.ToArray();
            }
        }

        return data;
    }

    /// <summary>
    /// POSTs to the specified request URI string asynchronously.
    /// </summary>
    /// <param name="requestUriString">The request URI string.</param>
    /// <param name="postData">The post data.</param>
    /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request.</returns>
    public static IAsyncResult PostAsync(string requestUriString, IEnumerable<PostData> postData)
    {
        if (string.IsNullOrWhiteSpace(requestUriString))
        {
            throw new ArgumentNullException(nameof(requestUriString));
        }

        ArgumentNullException.ThrowIfNull(postData);

        byte[] data;
        var webRequest = GetPostRequest(requestUriString, postData, out data);

        var requestAndData = new RequestAndData { Request = webRequest, Data = data };

        return webRequest.BeginGetRequestStream(GetRequestStreamCallback, requestAndData);
    }

    /// <summary>
    /// POSTs to the specified request URI string.
    /// </summary>
    /// <param name="requestUriString">The request URI string.</param>
    /// <param name="postData">The post data.</param>
    /// <param name="cookies">The cookies.</param>
    public static byte[] Post(string requestUriString, IEnumerable<PostData> postData, out CookieContainer cookies)
    {
        if (string.IsNullOrWhiteSpace(requestUriString))
        {
            throw new ArgumentNullException(nameof(requestUriString));
        }

        ArgumentNullException.ThrowIfNull(postData);

        var uri = new Uri(requestUriString);
        return Post(uri, postData, out cookies);
    }

    /// <summary>
    /// POSTs to the specified request URI string.
    /// </summary>
    /// <param name="requestUriString">The request URI string.</param>
    /// <param name="postData">The post data.</param>
    /// <param name="cookies">The cookies.</param>
    public static byte[] PostCookies(string requestUriString, IEnumerable<PostData> postData, CookieContainer cookies)
    {
        if (string.IsNullOrWhiteSpace(requestUriString))
        {
            throw new ArgumentNullException(nameof(requestUriString));
        }

        ArgumentNullException.ThrowIfNull(postData);

        var uri = new Uri(requestUriString);
        return PostCookies(uri, postData, cookies);
    }

    /// <summary>
    /// POSTs to the specified request URI string.
    /// </summary>
    /// <param name="requestUriString">The request URI string.</param>
    /// <param name="postData">The post data.</param>
    public static byte[] Post(string requestUriString, IEnumerable<PostData> postData)
    {
        if (string.IsNullOrWhiteSpace(requestUriString))
        {
            throw new ArgumentNullException(nameof(requestUriString));
        }

        ArgumentNullException.ThrowIfNull(postData);

        var uri = new Uri(requestUriString);
        CookieContainer cookies;
        return Post(uri, postData, out cookies);
    }

    /// <summary>
    /// POSTs to the specified request URI.
    /// </summary>
    /// <param name="requestUri">The request URI.</param>
    public static byte[] Post(Uri requestUri)
    {
        ArgumentNullException.ThrowIfNull(requestUri);

        CookieContainer cookies;
        return Post(requestUri, null, out cookies);
    }

    /// <summary>
    /// POSTs to the specified request URI.
    /// </summary>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="postData">The post data.</param>
    /// <param name="cookies">The cookies.</param>
    public static byte[] Post(Uri requestUri, IEnumerable<PostData> postData, out CookieContainer cookies)
    {
        ArgumentNullException.ThrowIfNull(requestUri, nameof(requestUri));
        ArgumentNullException.ThrowIfNull(postData, nameof(postData));


        var webRequest = GetPostRequest(requestUri, postData, out var data);

        using (var postStream = webRequest.GetRequestStream())
        {
            postStream.Write(data, 0, data.Length);
        }

        var webResponse = webRequest.GetResponse();//TODO: was HttpWebResponse

        using (var responseStream = webResponse.GetResponseStream())
        {
            data = new byte[256];
            using var memoryStream = new MemoryStream();
            int read;
            do
            {
                read = responseStream.Read(data, 0, 256);
                memoryStream.Write(data, 0, read);
            } while (read > 0);

            data = memoryStream.ToArray();
        }

        cookies = webRequest.CookieContainer;

        return data;
    }

    /// <summary>
    /// POSTs to the specified request URI.
    /// </summary>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="postData">The post data.</param>
    /// <param name="cookies">The cookies.</param>
    //TODO: Check if this can be comined or used together with Post
    public static byte[] PostCookies(Uri requestUri, IEnumerable<PostData> postData, CookieContainer cookies)
    {
        ArgumentNullException.ThrowIfNull(requestUri, nameof(requestUri));
        ArgumentNullException.ThrowIfNull(postData, nameof(postData));

        var webRequest = GetPostRequest(requestUri, postData, out var data);
        webRequest.CookieContainer = cookies;

        using (var postStream = webRequest.GetRequestStream())
        {
            postStream.Write(data, 0, data.Length);
        }

        var webResponse = webRequest.GetResponse();//TODO: was HttpWebResponse

        using (var responseStream = webResponse.GetResponseStream())
        {
            data = new byte[256];
            using var memoryStream = new MemoryStream();
            int read;
            do
            {
                read = responseStream.Read(data, 0, 256);
                memoryStream.Write(data, 0, read);
            } while (read > 0);

            data = memoryStream.ToArray();
        }

        return data;
    }

    /// <summary>
    /// Gets the full URI including GET/POST arguments.
    /// </summary>
    /// <param name="requestUriString">The request URI string.</param>
    /// <param name="postData">The post data.</param>
    public static string GetQueryString(string requestUriString, IEnumerable<PostData> postData)
    {
        if (string.IsNullOrWhiteSpace(requestUriString))
        {
            throw new ArgumentNullException(nameof(requestUriString));
        }

        ArgumentNullException.ThrowIfNull(postData);

        var getString = new StringBuilder();
        foreach (var item in postData)
        {
            AddField(getString, item);
        }

        if (!requestUriString.Contains("?", StringComparison.Ordinal))
        {
            getString.Insert(0, '?');
        }
        else
        {
            getString.Insert(0, '&');
        }

        return $"{requestUriString}{getString}";
    }

    private static void AddField(StringBuilder postString, PostData postData)
    {
        ArgumentNullException.ThrowIfNull(postString);

        ArgumentNullException.ThrowIfNull(postData);

        if (postData.Value == null)
        {
            throw new ArgumentNullException("postData.Item");
        }

        var value = new StringBuilder();
        foreach (var c in postData.Value)
        {
            if (c <= 255)
            {
                if (c < 128 && c != '&' && c != ' ')
                {
                    value.Append(c);
                }
                else
                {
                    value.Append("%" + ((int)c).ToString("X2", CultureInfo.InvariantCulture));
                }
            }
        }


        if (postString.Length == 0)
        {
            postString.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}", postData.Field, value);
        }
        else
        {
            postString.AppendFormat(CultureInfo.InvariantCulture, "&{0}={1}", postData.Field, value);
        }
    }

    private static HttpWebRequest GetPostRequest(string requestUriString, IEnumerable<PostData> postData, out byte[] data)
    {
        if (string.IsNullOrWhiteSpace(requestUriString))
        {
            throw new ArgumentNullException(nameof(requestUriString));
        }

        ArgumentNullException.ThrowIfNull(postData);

        var uri = new Uri(requestUriString);
        return GetPostRequest(uri, postData, out data);
    }

    private static HttpWebRequest GetPostRequest(Uri requestUri, IEnumerable<PostData> postData, out byte[] data)
    {
        ArgumentNullException.ThrowIfNull(requestUri);

        var getString = new StringBuilder();
        foreach (var item in postData)
        {
            AddField(getString, item);
        }

        data = Encoding.ASCII.GetBytes(getString.ToString());

        // Prepare web request...
        var webRequest = (HttpWebRequest)WebRequest.Create(requestUri);
        webRequest.AllowAutoRedirect = true;
        webRequest.Proxy = WebRequest.DefaultWebProxy;
        webRequest.Method = "POST";
        webRequest.ContentType = "application/x-www-form-urlencoded";
        webRequest.ContentLength = data.Length;
        webRequest.CookieContainer = new CookieContainer();
        webRequest.UserAgent = m_UserAgent;

        return webRequest;
    }

    private static void GetRequestStreamCallback(IAsyncResult asyncResult)
    {
        var requestAndData = (RequestAndData)asyncResult.AsyncState;
        var webRequest = requestAndData.Request;
        var data = requestAndData.Data;

        using (var postStream = webRequest.EndGetRequestStream(asyncResult))
        {
            postStream.Write(data, 0, data.Length);
        }
    }

    /// <summary>
    /// Gets the file name from headers.
    /// </summary>
    /// <param name="headers">The headers.</param>
    /// <returns>The file name.</returns>
    public static string GetFileNameFromHeaders(WebHeaderCollection headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        var keys = headers.AllKeys;
        if (keys == null || keys.Length == 0)
        {
            throw new Exception("No keys found.");
        }

        var keyList = new List<string>(keys);
        if (keyList.Contains("Content-disposition"))
        {
            var contentDisposition = headers["Content-disposition"];

            const string filename = "filename=\"";
            var idx = contentDisposition.IndexOf(filename);
            idx = idx + filename.Length;
            var fn = contentDisposition.Substring(idx, contentDisposition.Length - idx - 1);

            return fn;
        }

        throw new Exception("'Content-disposition' header not found.");
    }
}

/// <summary>
/// HTTP Basic Authentication credentials.
/// </summary>
public class BasicAuthenticationCredentials
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BasicAuthenticationCredentials"/> class.
    /// </summary>
    public BasicAuthenticationCredentials()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicAuthenticationCredentials"/> class.
    /// </summary>
    /// <param name="userName">The user name.</param>
    /// <param name="password">The password.</param>
    public BasicAuthenticationCredentials(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    /// <value>The user name.</value>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    /// <value>The password.</value>
    public string Password { get; set; }
}