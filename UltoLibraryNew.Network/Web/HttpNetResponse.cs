using System.Net;
using System.Text;

namespace UltoLibraryNew.Network.Web; 

public class HttpNetResponse(HttpListenerResponse raw) {
    public readonly HttpListenerResponse Raw = raw;

    public void WriteString(string data) {
        var bytes = Encoding.UTF8.GetBytes(data);
        Raw.ContentLength64 += bytes.Length;
        Raw.OutputStream.Write(bytes);
    }

    public void WriteFile(string absolutePath) {
        var data = File.ReadAllBytes(absolutePath);
        Raw.ContentLength64 += data.Length;
        Raw.OutputStream.Write(data);
    }

    public void WriteBytes(byte[] data) {
        Raw.ContentLength64 += data.Length;
        Raw.OutputStream.Write(data);
    }

    public void Redirect(string url) {
        Raw.Redirect(url);
    }

    public void SetCode(HttpStatusCode code) {
        Raw.StatusCode = (int) code;
    }
}