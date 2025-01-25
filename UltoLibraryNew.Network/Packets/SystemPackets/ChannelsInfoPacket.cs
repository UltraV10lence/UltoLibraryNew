namespace UltoLibraryNew.Network.Packets.SystemPackets;

internal record ChannelsInfoPacket(Dictionary<byte, string> Channels);