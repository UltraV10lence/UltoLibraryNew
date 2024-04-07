using System.Net;

namespace UltoLibraryNew.Network.Web; 

public class HttpNetServer(string[] prefixes, bool async = true, bool collapseDot = true) {
    public event Action<Exception> OnException = null!;
    public event Action<HttpNetRequest, HttpNetResponse> OnRequest = null!;
    
    public readonly string[] Prefixes = prefixes;
    public readonly bool Async = async;
    public readonly bool CollapseDot = collapseDot;

    protected TaskCompletionSource CloseSource = new();
    public Task CloseTask => CloseSource.Task;
    public readonly HttpAccessControl AccessControl = new();

    public HttpNetServer(string prefix, bool async = true, bool collapseDot = true) : this([ prefix ], async, collapseDot) {
    }

    public void Bind() {
        CloseSource = new TaskCompletionSource();

        Task.Run(() => {
            try {
                using var listener = new HttpListener();
                foreach (var p in Prefixes) listener.Prefixes.Add(p);
                listener.Start();

                while (!CloseTask.IsCompleted) {
                    try {
                        var ctx = listener.GetContext();

                        if (Async) Task.Run(() => ProcessRequest(ctx));
                        else ProcessRequest(ctx);
                    } catch (Exception e) {
                        OnException(e);
                    }
                }
            } catch (Exception e) {
                OnException(e);
                Close();
            }
        });
    }

    private void ProcessRequest(HttpListenerContext ctx) {
        var headers = ctx.Request.Headers;
        var fullRequestUrl = ctx.Request.Url!.ToString();
        var localUrl = ctx.Request.RawUrl!;
        var accept = ctx.Request.AcceptTypes;
        var remoteIp = ctx.Request.RemoteEndPoint;
        var requestMethod = ctx.Request.HttpMethod;

        if (CollapseDot) {
            while (fullRequestUrl.Length != (fullRequestUrl = fullRequestUrl.Replace("..", ".")).Length) { }
            while (localUrl.Length != (localUrl = localUrl.Replace("..", ".")).Length) { }
        }
        
        var request = new HttpNetRequest(headers, fullRequestUrl, localUrl, accept, remoteIp, requestMethod, ctx.Request);
        var response = new HttpNetResponse(ctx.Response);

        var access = AccessControl.Process(request, response);

        if (!access.process) {
            response.SetCode(access.responseCode);
        } else OnRequest(request, response);
        
        ctx.Response.Close();
    }

    public void Close() {
        CloseSource.SetResult();
    }
}