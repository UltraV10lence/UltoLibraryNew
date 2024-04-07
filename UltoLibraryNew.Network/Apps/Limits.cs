namespace UltoLibraryNew.Network.Apps;

internal static class Limits {
    public const int MaxPacketBufferSize = short.MaxValue; // 32KB
    public const int ToReceivePing = 1000 * 15; // 15s
    public const int ToSendPing = ToReceivePing / 4;
    public const int ToAuthorize = 1000 * 15; // 15s
}