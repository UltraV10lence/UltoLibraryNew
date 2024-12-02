using UltoLibraryNew.Network.Apps.Tcp;

namespace UltoLibraryNew.Network.Apps;

public class PacketReader(TcpNetConnection conn) {
    private const int PacketLengthBytes = 8;
    private readonly MemoryStream currentData = new();
    private long currentPacketLength = -1;
    private bool currentPacketPing;
    
    public void AddData(byte[] data) {
        conn.PacketTick();
        using var sData = new MemoryStream(data, false);

        while (true) {
            if (!CheckHasLength(sData)) {
                sData.CopyTo(currentData);
                return;
            }

        hasEnough:
            if (currentData.Position - PacketLengthBytes >= currentPacketLength) {
                var buffer = new MemoryStream();
                currentData.Seek(-currentPacketLength, SeekOrigin.Current);
                CopyToExactly(currentData, buffer, currentPacketLength);
                
                if (currentPacketPing) {
                    buffer.Dispose();
                } else {
                    var packet = new ByteBuf(buffer);
                    packet.EnterReadOnlyMode();
                    conn.Packet(packet);
                }
                
                currentData.Seek(-(currentPacketLength + PacketLengthBytes), SeekOrigin.Current);
                currentPacketLength = -1;
                currentPacketPing = false;
                continue;
            }

            if (currentData.Position - PacketLengthBytes + sData.Length - sData.Position >= currentPacketLength) {
                var toSwap = currentPacketLength - (currentData.Position - PacketLengthBytes);
                CopyToExactly(sData, currentData, toSwap);
                goto hasEnough;
            }
            
            break;
        }
        
        sData.CopyTo(currentData);
    }

    private void CopyToExactly(Stream from, Stream to, long toCopy) {
        while (toCopy > 0) {
            var buf = new byte[Math.Min(toCopy, 1024)];
            from.ReadExactly(buf);
            to.Write(buf);
            toCopy -= buf.Length;
        }
    }

    private bool CheckHasLength(MemoryStream data) {
        if (currentPacketLength != -1) return true;

        var l = GetLength(data);
        currentPacketLength = l.length;
        currentPacketPing = l.isPing;
        return currentPacketLength != -1;
    }

    private (long length, bool isPing) GetLength(MemoryStream data) {
        while (true) {
            if (currentData.Position >= PacketLengthBytes) {
                currentData.Seek(-PacketLengthBytes, SeekOrigin.Current);
                var buf = new byte[PacketLengthBytes];
                currentData.ReadExactly(buf);
                var isPing = (buf[PacketLengthBytes - 1] & 0b10000000) != 0;
                if (isPing) buf[PacketLengthBytes - 1] &= 0b01111111;
                return (BitConverter.ToInt64(buf), isPing);
            }

            if (currentData.Position + data.Length - data.Position < PacketLengthBytes) return (-1, false);

            var toSwap = PacketLengthBytes - currentData.Position;
            CopyToExactly(data, currentData, toSwap);
        }
    }
}