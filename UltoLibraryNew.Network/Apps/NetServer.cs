namespace UltoLibraryNew.Network.Apps;

public abstract class NetServer {
    public event Action<NetConnection> OnConnect = null!;
    public event Action<Exception> OnException = null!;

    public readonly string CurrentIp;
    public readonly int CurrentPort;

    protected TaskCompletionSource CloseSource = new();
    public Task CloseTask => CloseSource.Task;

    protected NetServer(string currentIp, int currentPort) {
        OnException += _ => { };
        OnConnect += _ => { };
        CurrentIp = currentIp;
        CurrentPort = currentPort;
    }
    
    internal void Connect(NetConnection connection) {
        try {
            OnConnect(connection);
        } catch (Exception e) {
            Exception(e);
        }
    }

    internal void Exception(Exception ex) {
        try {
            OnException(ex);
        } catch (Exception e1) {
            Console.WriteLine(10);
            Console.WriteLine(e1);
        }
    }

    public abstract void Bind();
    public abstract void Close();
}