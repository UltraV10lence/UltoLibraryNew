namespace UltoLibraryNew.Network.Apps.Packets;

public enum PacketAction {
    MoveNext,
    
    // MoveNext, but pointer in stream is set to 0
    Skip,
    
    // Stop executing
    Stop
}