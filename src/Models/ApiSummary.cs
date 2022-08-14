
using System.Text.Json.Serialization;

namespace WechatServerApiGenerator.Models;

public class ApiSummary
{
    public string EnglishName { get; set; } = string.Empty;
    public string ChineseName { get; set; } = string.Empty;
    public string DocUrl { get; set; } = string.Empty;

    [JsonIgnore]
    public string JsonFileName => Path.GetFileNameWithoutExtension(DocUrl) + ".json";
}
