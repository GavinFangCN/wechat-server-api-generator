
namespace WechatServerApiGenerator.Models;

public class ApiGroup
{
    public string Title { get; set; } = string.Empty;

    public IList<ApiSummary> Apis { get; set; } = Array.Empty<ApiSummary>();
}