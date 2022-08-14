namespace WechatServerApiGenerator.Utilities;

static class LocalJsonDataManager
{
    private const string DirectoryName = "json-files";

    static LocalJsonDataManager()
    {
        if (!Directory.Exists(DirectoryName))
        {
            Directory.CreateDirectory(DirectoryName);
        }
    }

    public static async ValueTask<T?> GetAsJsonAsync<T>(string fileName)
    {
        var json = await GetJsonAsync(fileName);

        return string.IsNullOrWhiteSpace(json) ? default : JsonUtility.Deserialize<T>(json);
    }

    public static async ValueTask<string> GetJsonAsync(string fileName)
    {
        var path = Path.Combine(DirectoryName, fileName);
        if (!File.Exists(path)) return string.Empty;

        return await File.ReadAllTextAsync(path);
    }

    public static Task SetAsJsonAsync<T>(string fileName, T data)
    {
        var json = JsonUtility.Serialize(data);

        return SetJsonAsync(fileName, json);
    }

    public static async Task SetJsonAsync(string fileName, string json)
    {
        var path = Path.Combine(DirectoryName, fileName);

        await File.WriteAllTextAsync(path, json);
    }
}
