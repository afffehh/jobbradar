using System.Text.Json;

namespace JobbRadar.Services;

public class SeenStore(string path)
{
    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    public HashSet<string> Load()
    {
        if (!File.Exists(path)) return [];
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<HashSet<string>>(json) ?? [];
    }

    public void Save(IEnumerable<string> ids)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (directory is not null) Directory.CreateDirectory(directory);

        var sorted = ids.Distinct().Order().ToList();
        File.WriteAllText(path, JsonSerializer.Serialize(sorted, WriteOptions));
    }
}