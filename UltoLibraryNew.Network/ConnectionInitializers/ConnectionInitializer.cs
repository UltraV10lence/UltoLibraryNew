using UltoLibraryNew.Network.Packets;

namespace UltoLibraryNew.Network.ConnectionInitializers;

public interface ConnectionInitializer {
    public ConnectionInitializer RegisterPackets(Action<PacketTypeIdentifier> identifier);
    //public ConnectionInitializer SetEncryption(asd encryptor, asd decryptor);
}