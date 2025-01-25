namespace UltoLibraryNew;

public static class StreamUtils {
    public static void CopyToExactly(this Stream self, Stream other, long toCopy, int bufferSize = 256) {
        var buffer = new byte[bufferSize];
        while (toCopy > 0) {
            var copy = (int) Math.Min(bufferSize, toCopy);
            self.ReadExactly(buffer, 0, copy);
            other.Write(buffer, 0, copy);
            toCopy -= copy;
        }
    }

    public static long GetRemainingBytes(this Stream self) {
        return self.Length - self.Position;
    }
}