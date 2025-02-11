using System.Text;

namespace UltoLibraryNew.Network.Packets;

public class PacketReader {
    private readonly MemoryStream currentPacket = new(1024);
    private PacketMetadata? currentMetadata;
    
    public void AppendData(byte[] buffer, int count, Action<ByteBuf, PacketMetadata> consumer) {
        var offset = 0;
        
        while (offset < count) {
            if (!GetPacketMetadata(buffer, ref offset, count)) {
                currentPacket.Write(buffer, offset, count - offset);
                return;
            }

            if (currentPacket.Position + (count - offset) < currentMetadata!.Value.PacketLength) {
                currentPacket.Write(buffer, offset, count - offset);
                return;
            }

            var toSwap = (int) (currentMetadata.Value.PacketLength - currentPacket.Position);
            if (toSwap > 0) {
                currentPacket.Write(buffer, offset, toSwap);
                offset += toSwap;
            }

            currentPacket.Seek(0, SeekOrigin.Begin);
            currentPacket.SetLength(currentMetadata.Value.PacketLength);
            
            using (var buf = new MemoryStream()) {
                currentPacket.CopyTo(buf);
                var packetBuf = new ByteBuf(buf);
                packetBuf.EnterReadOnlyMode();
                consumer.Invoke(packetBuf, currentMetadata.Value);
            }

            currentMetadata = null;
            currentPacket.Seek(0, SeekOrigin.Begin);
            currentPacket.SetLength(0);
        }
    }

    private bool GetPacketMetadata(byte[] buffer, ref int offset, int count) {
        if (currentMetadata.HasValue) return true;

        const int metadataSize = 6;
        if (currentPacket.Position + (count - offset) < metadataSize) return false;
        
        var toSwap = (int) (metadataSize - currentPacket.Position);
        if (toSwap > 0) {
            currentPacket.Write(buffer, offset, toSwap);
            offset += toSwap;
        }

        currentPacket.Seek(0, SeekOrigin.Begin);
        using (var reader = new BinaryReader(currentPacket, Encoding.UTF8, true)) {
            currentMetadata = new PacketMetadata {
                PacketLength = reader.ReadInt32(),
                DataType = reader.ReadInt16()
            };
                
            currentPacket.Seek(0, SeekOrigin.Begin);
            return true;
        }

    }
}