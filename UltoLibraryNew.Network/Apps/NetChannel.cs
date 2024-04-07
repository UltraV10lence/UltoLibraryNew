using System.Text;
using UltoLibraryNew.Network.Apps.Packets;
using UltoLibraryNew.Network.Apps.Tcp;

namespace UltoLibraryNew.Network.Apps;

public class NetChannel(NetConnection boundTo, string channelName) {
    public readonly NetConnection BoundTo = boundTo;
    public readonly string ChannelName = channelName;
    
    private byte[] packetBuffer = Array.Empty<byte>();
    private byte[] sendBuffer = Array.Empty<byte>();
    internal readonly List<Func<NetChannel, ByteBuf, PacketAction>> OnPacket = [ ];

    public void Send(ByteBuf buf) {
        buf.EnterReadMode();
        if (buf.Stream.Length > Limits.MaxPacketBufferSize) throw new ArgumentException($"Buffer has too much data in it. ({buf.Stream.Length})");

        var data = new byte[buf.Stream.Length];
        var read = buf.Stream.Read(data);
        if (read < data.Length) data = UltoBytes.SubArray(data, 0, read);
        
        sendBuffer = UltoBytes.AppendArrays(sendBuffer, BitConverter.GetBytes((short) data.Length), data);
    }

    public void Close() {
        BoundTo.CloseChannel(this);
    }

    internal void AddUnnamedData(byte[] data) {
        if (packetBuffer.Length + data.Length > Limits.MaxPacketBufferSize) {
            BoundTo.Disconnect(DisconnectReason.PacketBufferOverflow);
            return;
        }

        packetBuffer = UltoBytes.AppendArrays(packetBuffer, data);
        var reader = new MemoryStream(packetBuffer);
        reader.Seek(0, SeekOrigin.Begin);

        while (packetBuffer.Length - reader.Position > 1) {
            var lengthData = new byte[2];
            reader.ReadExactly(lengthData);
            var packetLength = BitConverter.ToUInt16(lengthData);

            if (packetBuffer.Length - reader.Position < packetLength) {
                reader.Seek(-2, SeekOrigin.Current);
                break;
            }
            var packet = new byte[packetLength];
            reader.ReadExactly(packet);

            ProcessPacket(new ByteBuf(packet));
        }

        packetBuffer = UltoBytes.SubArray(packetBuffer, (int) reader.Position, (int) (packetBuffer.Length - reader.Position));
        reader.Close();
    }

    private void ProcessPacket(ByteBuf packet) {
        foreach (var f in OnPacket) {
            var process = f.Invoke(this, packet);

            switch (process) {
                case PacketAction.MoveNext:
                    break;
                case PacketAction.Skip:
                    packet.Stream.Seek(0, SeekOrigin.Begin);
                    break;
                case PacketAction.Stop:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private byte[]? cachedNameData;
    internal byte[]? GetNextNamedSlice() {
        if (sendBuffer.Length == 0) return null;
        
        using var ms = new MemoryStream();

        var nameData = cachedNameData ??= Encoding.UTF8.GetBytes(ChannelName);
        
        ms.WriteByte((byte) nameData.Length);
        ms.Write(nameData);
        
        var data = sendBuffer.Length > 255 ? UltoBytes.SubArray(sendBuffer, 0, 255) : sendBuffer;
        sendBuffer = UltoBytes.SubArray(sendBuffer, data.Length, sendBuffer.Length - data.Length);
        
        ms.WriteByte((byte) data.Length);
        ms.Write(data);

        return ms.ToArray();
    }
}