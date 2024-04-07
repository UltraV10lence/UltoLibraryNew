namespace UltoLibraryNew.Network.Apps.Tcp; 

public enum DisconnectReason {
    Disconnect,
    Timeout,
    AuthorizationTimeout,
    ValidationFailed,
    PacketBufferOverflow,
    ChannelInactive,
    IllegalPacketData,
    Exception
}