namespace UltoLibraryNew.Network.Apps.Packets; 

public class PacketNotRegisteredException : Exception {
    public PacketNotRegisteredException(object packet) : base(packet.GetType().ToString()) { }
    public PacketNotRegisteredException(short id) : base($"Cannot find decoder to packet with id: {id}") { }
}