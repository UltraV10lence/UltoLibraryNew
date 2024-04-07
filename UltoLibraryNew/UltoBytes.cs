using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace UltoLibraryNew; 

public static class UltoBytes {
    public static byte[] AppendArrays(byte[] first, params byte[][] other) {
        using var newArray = new MemoryStream(first.Length + other.Sum(a => a.Length));
        
        newArray.Write(first);
        foreach (var array in other) {
            newArray.Write(array);
        }
        return newArray.ToArray();
    }
    
    public static byte[] SubArray(byte[] array, int start, int length) =>
        new ArraySegment<byte>(array, start, length).ToArray();
    
    public static bool ContainsArray(byte[] array, byte[] value) {
        if (value.Length == 0) return true;
        if (value.Length > array.Length) return false;
        
        var i = 0;
        bool found;
        while (!(found = array.Skip(i).Take(value.Length).SequenceEqual(value)) && i < array.Length - value.Length) i++;
        
        return found;
    }
    
    public static string ToString(byte[] array) {
        return array.Length switch {
            0 => string.Empty,
            > int.MaxValue / 3 => throw new ArgumentOutOfRangeException(nameof(array.Length), "Length is too large."),
            _ => $"[{string.Join(", ", array)}]"
        };
    }
    
    public static byte[] XorKey(byte[] data, byte[] key) {
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

    public static byte[] RandomSecure(int length) {
        return RandomNumberGenerator.GetBytes(length);
    }
    
    public static string ToBase64Str(byte[] bytes) {
        return Convert.ToBase64String(bytes);
    }
    public static byte[] ToBase64Bytes(byte[] bytes) {
        return Encoding.UTF8.GetBytes(ToBase64Str(bytes));
    }
    
    public static byte[] FromBase64Str(string str) {
        return Convert.FromBase64String(str);
    }
    public static byte[] FromBase64Bytes(byte[] bytes) {
        return FromBase64Str(Encoding.UTF8.GetString(bytes));
    }
    
    public static string ToHexStr(byte[] bytes) {
        return BitConverter.ToString(bytes).Replace("-", "");
    }
    public static byte[] FromHexStr(string str) {
        var bytes = new byte[str.Length / 2];
        for (var i = 0; i < str.Length; i += 2) bytes[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
        return bytes;
    }
    
    public static byte[] HashSHA1(byte[] data) {
        return SHA1.HashData(data);
    }

    public static byte[] HashSHA256(byte[] data) {
        return SHA256.HashData(data);
    }

    public static byte[] HashSHA512(byte[] data) {
        return SHA512.HashData(data);
    }

    public static byte[] HashMD5(byte[] data) {
        return MD5.HashData(data);
    }

    public static byte[] GenerateAesKey() {
        using var aes = Aes.Create();
        aes.KeySize = 128;
        aes.GenerateKey();
        return aes.Key;
    }
    public static byte[] EncryptAes(byte[] data, byte[] key) {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }
    public static byte[] DecryptAes(byte[] data, byte[] key) {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(data, 0, data.Length);
    }
    public static void EncryptAesStream(Stream data, Stream to, byte[] key) {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        using (var cryptoStream = new CryptoStream(to, aes.CreateEncryptor(), CryptoStreamMode.Write, true)) {
            data.CopyTo(cryptoStream);
        }
    }
    public static Stream EncryptionAesStream(Stream to, byte[] key) {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        return new CryptoStream(to, aes.CreateEncryptor(), CryptoStreamMode.Write, true);
    }
    public static void DecryptAesStream(Stream data, Stream to, byte[] key) {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        using (var cryptoStream = new CryptoStream(data, aes.CreateDecryptor(), CryptoStreamMode.Read, true)) {
            cryptoStream.CopyTo(to);
        }
    }
    public static Stream DecryptionAesStream(Stream data, byte[] key) {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        return new CryptoStream(data, aes.CreateDecryptor(), CryptoStreamMode.Read, true);
    }

    public static byte[] Compress(byte[] data) {
        var output = new MemoryStream();
        using (var deflate = new DeflateStream(output, CompressionMode.Compress)) {
            deflate.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }
    public static byte[] Decompress(byte[] data) {
        var output = new MemoryStream();
        using (var deflate = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress)) {
            deflate.CopyTo(output);
        }
        return output.ToArray();
    }

    public static void CompressStream(Stream data, Stream output) {
        using (var deflate = new DeflateStream(output, CompressionMode.Compress, true)) {
            data.CopyTo(deflate);
        }
    }
    public static Stream CompressionStream(Stream output) {
        return new DeflateStream(output, CompressionMode.Compress, true);
    }
    public static void DecompressStream(Stream data, Stream output) {
        using (var deflate = new DeflateStream(data, CompressionMode.Decompress, true)) {
            deflate.CopyTo(output);
        }
    }
    public static Stream DecompressionStream(Stream data) {
        return new DeflateStream(data, CompressionMode.Decompress, true);
    }
    
    public static (byte[] publicKey, byte[] privateKey) GenerateRsaKeys(bool doubleKeys) {
        using var rsa = new RSACryptoServiceProvider(doubleKeys ? 2048 : 1024);
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