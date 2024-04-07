using System.Reflection;

namespace UltoLibraryNew.Databases;

public class DatabaseTable<T> {
    public readonly string Name;
    public readonly List<T> Entries = new();
    private readonly IEnumerable<FieldInfo> databaseEntries;

    public static readonly Type[] LegalTypes = {
        typeof(byte), typeof(byte[]), typeof(char), typeof(char[]), typeof(ushort), typeof(ushort[]), typeof(short),
        typeof(short[]), typeof(uint), typeof(uint[]), typeof(int), typeof(int[]), typeof(ulong), typeof(ulong[]),
        typeof(long), typeof(long[]), typeof(float), typeof(float[]), typeof(Half), typeof(Half[]), typeof(double),
        typeof(double[]), typeof(bool), typeof(bool[]), typeof(string), typeof(string[]), typeof(DateTime),
        typeof(DateTime[]), typeof(DateTimeOffset), typeof(DateTimeOffset[]), typeof(TimeSpan), typeof(TimeSpan[])
    };

    public DatabaseTable(string name) {
        Name = name;
        databaseEntries = GetDatabaseFields(typeof(T));
        if (!IsEntryLegal(typeof(T)))
            throw new ArgumentException(
                "Запись недопустима. Вхождения значения должны быть следующими типами: "
                + string.Join(", ", LegalTypes.Where(e => !e.IsArray).Select(e => e.Name)) + " и их массивы");
    }
    
    public T? Find(Predicate<T> predicate) {
        return Entries.Find(predicate);
    }

    public T? FindByParam(string name, object value) {
        var property = typeof(T).GetProperty(name);
        if (property == null) return default;
        return Find(e => {
            var propertyValue = property.GetValue(e);
            return propertyValue != null && propertyValue.Equals(value);
        });
    }

    private bool IsEntryLegal(Type t) {
        if (t.IsPrimitive || t.IsPointer) return false;
        
        return databaseEntries.All(prop => LegalTypes.Contains(prop.FieldType));
    }

    private IEnumerable<FieldInfo> GetDatabaseFields(Type t) {
        return t.GetFields().Where(f => f.GetCustomAttribute<IncludeInDatabase>() != null);
    }
    
    public void Add(T entry) {
        Entries.Add(entry);
    }

    #region Save
    internal void SaveTable(BinaryWriter writer) {
        writer.Write(Name);
        writer.Write(typeof(T).FullName!);
        writer.Write(Entries.Count);
        foreach (var e in Entries) SaveEntry(writer, e!);
    }

    private void SaveEntry(BinaryWriter writer, object obj) {
        foreach (var field in typeof(T).GetFields()) {
            if (field.CustomAttributes.All(a => a.AttributeType != typeof(IncludeInDatabase))) continue;
            
            if (field.FieldType.IsArray) SaveArray(writer, field, obj);
            else SaveField(writer, field, obj);
        } 
    }

    private void SaveArray(BinaryWriter writer, FieldInfo field, object obj) {
        if (field.FieldType.IsNotPublic) return;

        var val = field.GetValue(obj);
        if (val == null)
            throw new NullReferenceException($"Значение не может быть null ({field.Name})");

        var arr = ((Array)val).Cast<object>().ToArray();
        writer.Write(arr.Length);
        foreach (var o in arr) SaveObject(writer, o, $"array {field.Name} entry");
    }
    
    private void SaveField(BinaryWriter writer, FieldInfo field, object obj) {
        if (field.FieldType.IsNotPublic) return;

        var val = field.GetValue(obj);
        SaveObject(writer, val, field.Name);
    }

    private void SaveObject(BinaryWriter writer, object? val, string? fieldName = null) {
        fieldName ??= "undefined field";
        switch (val) {
            case null:
                throw new NullReferenceException($"Значение не может быть null ({fieldName})");
            case byte b:
                writer.Write(b);
                break;
            case char c:
                writer.Write(c);
                break;
            case ushort us:
                writer.Write(us);
                break;
            case short s:
                writer.Write(s);
                break;
            case uint ui:
                writer.Write(ui);
                break;
            case int i:
                writer.Write(i);
                break;
            case ulong ul:
                writer.Write(ul);
                break;
            case long l:
                writer.Write(l);
                break;
            case float f:
                writer.Write(f);
                break;
            case Half h:
                writer.Write(h);
                break;
            case double d:
                writer.Write(d);
                break;
            case bool b:
                writer.Write(b);
                break;
            case string s:
                writer.Write(s);
                break;
            case DateTime d:
                writer.Write(d.ToBinary());
                break;
            case TimeSpan t:
                writer.Write(t.Ticks);
                break;
            default:
                throw new ArgumentException("Недопустимое значение: " + fieldName);
        }
    }
    #endregion
}