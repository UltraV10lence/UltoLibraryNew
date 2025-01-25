using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace UltoLibraryNew; 

public static class UltoBytes {
    public static byte[] Append(this byte[] self, params byte[][] other) {
        using var newArray = new MemoryStream(self.Length + other.Sum(a => a.Length));
        
        newArray.Write(self);
        foreach (var array in other) {
            newArray.Write(array);
        }
        return newArray.ToArray();
    }
    
    public static byte[] SubArray(this byte[] self, int start, int length) =>
        new ArraySegment<byte>(self, start, length).ToArray();
    
    public static bool ContainsArray(this byte[] self, byte[] value) {
        if (value.Length == 0) return true;
        if (value.Length > self.Length) return false;
        
        var i = 0;
        bool found;
        while (!(found = self.Skip(i).Take(value.Length).SequenceEqual(value)) && i < self.Length - value.Length) i++;
        
        return found;
    }
    
    public static string ToFormattedString(this byte[] array) => $"[{string.Join(", ", array)}]";
    
    public static byte[] XorKey(this byte[] data, byte[] key) {
        if (key.Length == 0 || data.Length == 0) return data;

        var dOut = new byte[data.Length];

        for (var i = 0; i < Math.Max(data.Length, key.Length); i++) {
            dOut[i % dOut.Length] = (byte)(data[i % data.Length] ^ key[i % key.Length]);
        }

        return dOut;
    }

    public static byte[] Random(int length) {
        var bytes = new byte[length];
        new Random().NextBytes(bytes);
        return bytes;
    }

    public static byte[] RandomSecure(int length) => RandomNumberGenerator.GetBytes(length);

    public static string ToBase64Str(this byte[] bytes) => Convert.ToBase64String(bytes);
    public static byte[] ToBase64Bytes(this byte[] bytes, Encoding? encoding = null) {
        encoding ??= Encoding.UTF8;
        return encoding.GetBytes(ToBase64Str(bytes));
    }
    
    public static byte[] FromBase64Str(string str) => Convert.FromBase64String(str);
    public static byte[] FromBase64Bytes(this byte[] bytes, Encoding? encoding = null) {
        encoding ??= Encoding.UTF8;
        return FromBase64Str(encoding.GetString(bytes));
    }
    
    public static string ToHexStr(this byte[] bytes) => BitConverter.ToString(bytes).Replace("-", "");
    public static byte[] FromHexStr(string str) {
        var bytes = new byte[str.Length / 2];
        for (var i = 0; i < str.Length; i += 2) bytes[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
        return bytes;
    }
    
    public static byte[] HashSHA1(this byte[] data) => SHA1.HashData(data);
    public static byte[] HashSHA256(this byte[] data) => SHA256.HashData(data);
    public static byte[] HashSHA512(this byte[] data) => SHA512.HashData(data);
    public static byte[] HashMD5(this byte[] data) => MD5.HashData(data);

    public static byte[] GenerateAesKey() {
        using var aes = Aes.Create();
        aes.KeySize = 128;
        aes.GenerateKey();
        return aes.Key;
    }
    
    public static byte[] EncryptAes(byte[] data, byte[] key, byte[]? iv) {
        using var aes = Aes.Create();
        aes.Key = key;
        if (iv != null) aes.IV = iv;
        else aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }
    public static void EncryptAesStream(Stream data, Stream to, byte[] key, byte[]? iv, bool leaveOpen = true) {
        using (var cryptoStream = EncryptionAesStream(to, key, iv, true)) {
            data.CopyTo(cryptoStream);
        }
        if (!leaveOpen) to.Close();
    }
    public static Stream EncryptionAesStream(Stream to, byte[] key, byte[]? iv, bool leaveOpen = false) {
        using var aes = Aes.Create();
        aes.Key = key;
        if (iv != null) aes.IV = iv;
        else aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        return new CryptoStream(to, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen);
    }
    
    public static byte[] DecryptAes(byte[] data, byte[] key, byte[]? iv) {
        using var aes = Aes.Create();
        aes.Key = key;
        if (iv != null) aes.IV = iv;
        else aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(data, 0, data.Length);
    }
    public static void DecryptAesStream(Stream data, Stream to, byte[] key, byte[]? iv, bool leaveOpen = true) {
        using (var cryptoStream = DecryptionAesStream(data, key, iv, leaveOpen)) {
            cryptoStream.CopyTo(to);
        }
        if (!leaveOpen) to.Close();
    }
    public static Stream DecryptionAesStream(Stream data, byte[] key, byte[]? iv, bool leaveOpen = false) {
        using var aes = Aes.Create();
        aes.Key = key;
        if (iv != null) aes.IV = iv;
        else aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        return new CryptoStream(data, aes.CreateDecryptor(), CryptoStreamMode.Read, leaveOpen);
    }

    public static byte[] Compress(byte[] data) {
        using (var output = new MemoryStream())
        using (var input = new MemoryStream(data)) {
            CompressStream(input, output);
            return output.ToArray();
        }
    }
    public static void CompressStream(Stream data, Stream output, bool leaveOpen = true) {
        using (var deflate = CompressionStream(output, leaveOpen)) {
            data.CopyTo(deflate);
        }
        if (!leaveOpen) data.Dispose();
    }
    public static Stream CompressionStream(Stream output, bool leaveOpen = false, CompressionLevel compressionLevel = CompressionLevel.Optimal) {
        return new BrotliStream(output, compressionLevel, leaveOpen);
    }
    
    public static byte[] Decompress(byte[] data) {
        using (var output = new MemoryStream())
        using (var input = new MemoryStream(data)) {
            DecompressStream(input, output);
            return output.ToArray();
        }
    }
    public static void DecompressStream(Stream data, Stream output, bool leaveOpen = true) {
        using (var deflate = DecompressionStream(data, leaveOpen)) {
            deflate.CopyTo(output);
        }
        if (!leaveOpen) output.Dispose();
    }
    public static Stream DecompressionStream(Stream data, bool leaveOpen = false) {
        return new BrotliStream(data, CompressionMode.Decompress, leaveOpen);
    }
    
    public static (byte[] publicKey, byte[] privateKey) GenerateRsaKeys(bool doubleKeyLength) {
        using var rsa = new RSACryptoServiceProvider(doubleKeyLength ? 2048 : 1024);
        var publicKey = rsa.ExportRSAPublicKey();
        var privateKey = rsa.ExportRSAPrivateKey();
        return (publicKey, privateKey);
    }
    public static byte[] EncryptRsa(byte[] data, byte[] publicKey) {
        using var rsa = new RSACryptoServiceProvider();
        rsa.ImportRSAPublicKey(publicKey, out _);
        return rsa.Encrypt(data, true);
    }
    public static byte[] DecryptRsa(byte[] data, byte[] privateKey) {
        using var rsa = new RSACryptoServiceProvider();
        rsa.ImportRSAPrivateKey(privateKey, out _);
        return rsa.Decrypt(data, true);
    }
}