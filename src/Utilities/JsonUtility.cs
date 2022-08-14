using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace WechatServerApiGenerator.Utilities;

static class JsonUtility
{
    private static readonly JsonSerializerOptions JsonSerializerOptions;

    static JsonUtility()
    {
        var encoderSettings = new TextEncoderSettings();
        encoderSettings.AllowRange(UnicodeRanges.BasicLatin);
        encoderSettings.AllowRange(UnicodeRanges.CjkUnifiedIdeographs);
        encoderSettings.AllowRange(UnicodeRanges.CjkCompatibility);

        JsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.Create(encoderSettings) };
    }

    public static string Serialize<T>(T data) => JsonSerializer.Serialize(data, JsonSerializerOptions);

    public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
}
