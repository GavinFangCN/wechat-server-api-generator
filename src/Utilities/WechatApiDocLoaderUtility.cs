using System.Diagnostics.CodeAnalysis;

namespace WechatServerApiGenerator.Utilities;

static class WechatApiDocLoaderUtility
{
    public static readonly HttpClient DocClient = new();

    public const string OpenApiRootUrl = "https://developers.weixin.qq.com/miniprogram/dev/OpenApiDoc/";

    public static string ToFullUrl(string relativeOrFullUrl)
    {
        return relativeOrFullUrl.StartsWith("./") ? OpenApiRootUrl + relativeOrFullUrl[2..] : relativeOrFullUrl;
    }

    public static Task<string> GetStringAsync(string? requestUri) => DocClient.GetStringAsync(requestUri);
}
