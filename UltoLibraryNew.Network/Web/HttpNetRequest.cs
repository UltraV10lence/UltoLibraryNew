using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace UltoLibraryNew.Network.Web; 

public class HttpNetRequest(
    NameValueCollection headers,
    string fullRequestUrl,
    string localUrl,
    string[]? accept,
    IPEndPoint remoteIp,
    string requestMethod,
    HttpListenerRequest raw) {
    public readonly HttpListenerRequest Raw = raw;
    public readonly NameValueCollection Headers = headers;
    public readonly string FullRequestUrl = fullRequestUrl;
    public readonly string LocalUrl = localUrl;
    public readonly string[]? Accept = accept;
    public readonly IPEndPoint RemoteIp = remoteIp;
    public readonly string RequestMethod = requestMethod;

    /// <summary>
    /// Works once!
    /// </summary>
    public string ReadString() {
        using var stream = Raw.InputStream;

        using (var reader = new StreamReader(stream, Encoding.UTF8)) {
            return reader.ReadToEnd();
        }
    }
}