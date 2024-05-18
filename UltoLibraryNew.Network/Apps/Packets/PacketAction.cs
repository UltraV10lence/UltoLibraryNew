namespace UltoLibraryNew.Network.Apps.Packets;

public enum PacketAction {
    /// <summary>
    /// Continue executing next task from current position in stream
    /// </summary>
    MoveNext,
    
    /// <summary>
    /// Continue executing next task from start of the stream
    /// </summary>
    Skip,
    
    /// <summary>
    /// Stop executing at current task
    /// </summary>
    Stop
}