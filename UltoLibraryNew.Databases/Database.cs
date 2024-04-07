using System.Text;

namespace UltoLibraryNew.Databases;

public class Database {
    public List<dynamic> Tables { get; private init; } = new(10);

    public DatabaseTable<T>? GetTable<T>(string name) {
        var type = typeof(T);
        var table = Tables.SingleOrDefault(e => e?.GetType().GenericTypeArguments[0] == type && e?.Name == name, null);
        return table;
    }
    
    public DatabaseTable<T> CreateTable<T>(string name) {
        var table = new DatabaseTable<T>(name);
        Tables.Add(table);
        return table;
    }

    public void Save(string file, byte[]? encryptionKey = null) {
        using var data = new MemoryStream();
        
        using var writer = new BinaryWriter(data, Encoding.UTF8, true);
        writer.Write(Tables.Count);
        foreach (var table in Tables) {
            table.SaveTable(writer);
        }
        
        data.Seek(0, SeekOrigin.Begin);

        using var fileStream = File.OpenWrite(file);
        using var stream = encryptionKey is not null ? UltoBytes.EncryptionAesStream(fileStream, encryptionKey) : fileStream;
        
        UltoBytes.CompressStream(data, stream);
    }
    
    public static Database Load(string file, byte[]? encryptionKey = null) {
        using var fileStream = File.OpenRead(file);
        using var stream = encryptionKey is not null ?
            UltoBytes.DecompressionStream(UltoBytes.DecryptionAesStream(fileStream, encryptionKey)) :
            UltoBytes.DecompressionStream(fileStream);
        var reader = new BinaryReader(stream);

        var tablesCount = reader.ReadInt32();
        var tables = new List<dynamic>();
        for (var i = 0; i < tablesCount; i++) {
            tables.Add(LoadTable(reader));
        }

        return new Database {
            Tables = tables
        };
    }

    private static dynamic LoadTable(BinaryReader reader) {
        var name = reader.ReadString();
        var structName = reader.ReadString();
        var entriesCount = reader.ReadInt32();
        var cl = TypeFromName(structName);

        var tableClass = typeof(DatabaseTable<>).MakeGenericType(cl!);
        var table = Activator.CreateInstance(tableClass, name);

        for (var j = 0; j < entriesCount; j++) {
            var obj = Activator.CreateInstance(cl!);
            var prop = cl!.GetFields();
        
            foreach (var t in prop) {
                if (t.CustomAttributes.All(a => a.AttributeType != typeof(IncludeInDatabase))) continue;
                
                var value = t.FieldType.IsArray ? LoadArray(reader, t.FieldType.GetElementType()!) : LoadObject(reader, t.FieldType);
                t.SetValue(obj, value);
            }

            tableClass.GetMethod("Add", [ cl ])!.Invoke(table, [ obj ]);
        }

        return table!;
    }

    private static object LoadArray(BinaryReader reader, Type type) {
        var length = reader.ReadInt32();
        var arr = Array.CreateInstance(type, length);
        for (var i = 0; i < length; i++) arr.SetValue(LoadObject(reader, type), i);
        return arr;
    }

    private static object LoadObject(BinaryReader reader, Type type) {
        if (type == typeof(byte)) return reader.ReadByte();
        if (type == typeof(char)) return reader.ReadChar();
        if (type == typeof(ushort)) return reader.ReadUInt16();
        if (type == typeof(short)) return reader.ReadInt16();
        if (type == typeof(uint)) return reader.ReadUInt32();
        if (type == typeof(int)) return reader.ReadInt32(); 
        if (type == typeof(ulong)) return reader.ReadUInt64();
        if (type == typeof(long)) return reader.ReadInt64();
        if (type == typeof(float)) return reader.ReadSingle();
        if (type == typeof(Half)) return reader.ReadHalf();
        if (type == typeof(double)) return reader.ReadDouble();
        if (type == typeof(bool)) return reader.ReadBoolean();
        if (type == typeof(string)) return reader.ReadString();
        if (type == typeof(DateTime)) return DateTime.FromBinary(reader.ReadInt64());
        if (type == typeof(TimeSpan)) return TimeSpan.FromTicks(reader.ReadInt64());
        throw new ArgumentException("Недопустимое значение: " + type.FullName);
    }

    private static Type? TypeFromName(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType(name)).OfType<Type>()
            .FirstOrDefault();
}