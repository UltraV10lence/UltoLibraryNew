using System.Text;
using UltoLibraryNew.Network.Apps.Tcp;

namespace UltoLibraryNew.Network.Apps.Packets;

public class PacketReceiver(NetConnection boundTo) {
    public readonly NetConnection BoundTo = boundTo;
    private byte[] packetBuffer = Array.Empty<byte>();

    public void AddData(byte[] data) {
        if (packetBuffer.Length + data.Length > 2048) {
            BoundTo.Disconnect(DisconnectReason.IllegalPacketData);
            return;
        }

        packetBuffer = UltoBytes.AppendArrays(packetBuffer, data);
        using var reader = new MemoryStream(packetBuffer);
        reader.Seek(0, SeekOrigin.Begin);

        skip:
        while (packetBuffer.Length - reader.Position > 0) {
            var channelNameLength = (byte) reader.ReadByte();

            if (packetBuffer.Length - reader.Position < channelNameLength) {
                reader.Seek(-1, SeekOrigin.Current);
                break;
            }
            var channelNameData = new byte[channelNameLength];
            reader.ReadExactly(channelNameData);

            if (packetBuffer.Length - reader.Position < 1) {
                reader.Seek(-1 - channelNameLength, SeekOrigin.Current);
                break;
            }
            var packetSliceLength = (byte)reader.ReadByte();

            if (packetBuffer.Length - reader.Position < packetSliceLength) {
                reader.Seek(-2 - channelNameLength, SeekOrigin.Current);
                break;
            }
            var packetSlice = new byte[packetSliceLength];
            reader.ReadExactly(packetSlice);
            
            var channelName = Encoding.UTF8.GetString(channelNameData);

            switch (BoundTo.GotDataToUnknownChannel) {
                case UnknownChannelPolitics.CreateNew:
                    break;
                case UnknownChannelPolitics.Skip:
                    if (!BoundTo.HasChannel(channelName)) goto skip;
                    break;
                case UnknownChannelPolitics.Disconnect:
                    if (!BoundTo.HasChannel(channelName)) {
                        BoundTo.Disconnect(DisconnectReason.ChannelInactive);
                        return;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var channel = BoundTo.OpenChannel(channelName);
            channel.AddUnnamedData(packetSlice);
        }
        
        packetBuffer = UltoBytes.SubArray(packetBuffer, (int) reader.Position, (int) (packetBuffer.Length - reader.Position));
    }
}