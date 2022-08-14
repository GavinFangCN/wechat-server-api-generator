using HtmlAgilityPack;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Net;
using WechatServerApiGenerator.Models;

namespace WechatServerApiGenerator.Utilities;

class WechatApiDetailsDocLoader
{
    public static async Task<ApiDetails?> GetApiDetailsAsync(string docUrl)
    {
        var fileName = Path.GetFileNameWithoutExtension(docUrl) + ".json";
        var details = await LocalJsonDataManager.GetAsJsonAsync<ApiDetails>(fileName);

        if (details == null)
        {
            details = await GetRemoteApiDetailsAsync(docUrl);
            if (details != null)
            {
                await LocalJsonDataManager.SetAsJsonAsync(fileName, details);
            }
        }

        return details;
    }

    private static async Task<ApiDetails> GetRemoteApiDetailsAsync(string docUrl)
    {
        var html = await WechatApiDocLoaderUtility.GetStringAsync(docUrl);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var root = htmlDoc.DocumentNode;

        var (httpMethod, url) = GetHttpInfo(root);
        var details = new ApiDetails
        {
            Description = GetDescription(root),
            HttpMethod = httpMethod,
            Url = url,
            RequestParameters = GetApiRequestParameters(root).ToArray(),
            RequestSample = GetRequestSample(root),
            ResponseParameters = GetApiResponseParameters(root).ToArray(),
            ResponseSample = GetResponseSample(root)
        };

        if (!string.IsNullOrWhiteSpace(details.RequestSample))
        {
            details.RequestSample = WebUtility.HtmlDecode(details.RequestSample);
        }

        if (!string.IsNullOrWhiteSpace(details.ResponseSample))
        {
            details.ResponseSample = WebUtility.HtmlDecode(details.ResponseSample);
        }

        return details;
    }

    private static string GetDescription(HtmlNode root)
        => root.SelectSingleNode("//*[@id=\"功能描述\"]").NextSibling.NextSibling.InnerText;

    private static (string httpMethod, string url) GetHttpInfo(HtmlNode root)
    {
        var node = root.SelectSingleNode("//*[@id=\"docContent\"]/div[2]/div[1]/pre/code");
        var raw = node.InnerText.Trim();

        var segments = raw.Split(' ');
        if (segments.Length != 2) return (string.Empty, string.Empty);

        return (segments[0], segments[1]);
    }

    private static IEnumerable<ApiRequestParameter> GetApiRequestParameters(HtmlNode root)
    {
        var tableNode = root.SelectSingleNode("//*[@id=\"请求参数\"]")?.NextSibling?.NextSibling?.FirstChild;
        if(tableNode == null)
        {
            yield break;
        }

        if(tableNode.Name == "thead")
        {
            tableNode = tableNode.ParentNode;
        }

        Debug.Assert(tableNode.Name == "table");

        var tbodyNode = tableNode.ChildNodes.FirstOrDefault(x => x.Name == "tbody");
        var trNodes = tbodyNode != null ? tbodyNode.ChildNodes : tableNode.ChildNodes.Where(x => x.Name == "tr");

        foreach (var trNode in trNodes)
        {
            IList<HtmlNode> tds = trNode.ChildNodes.Where(x => x.Name == "td").ToArray();
            if (tds.Count != 4)
            {
                //error
                continue;
            }

            var parameter = new ApiRequestParameter
            {
                Name = tds[0].InnerText,
                Type = tds[1].InnerText,
                IsRequired = tds[2].InnerText.Contains('是'),
                Description = tds[3].InnerText
            };

            yield return parameter;
        }
    }

    private static string GetRequestSample(HtmlNode root)
        => root.SelectSingleNode("//*[@id=\"请求数据示例\"]").NextSibling?.InnerText?.Trim() ?? string.Empty;

    private static IEnumerable<ApiResponseParameter> GetApiResponseParameters(HtmlNode root)
    {
        var tableNode = root.SelectSingleNode("//*[@id=\"返回参数\"]")?.NextSibling?.NextSibling;
        if(tableNode == null)
        {
            yield break;
        }

        if (tableNode.Name == "div")
        {
            tableNode = tableNode.FirstChild;
        }

        Debug.Assert(tableNode.Name == "table");

        var tbodyNode = tableNode.ChildNodes.FirstOrDefault(x => x.Name == "tbody");
        var trNodes = tbodyNode != null ? tbodyNode.ChildNodes : tableNode.ChildNodes.Where(x => x.Name == "tr");

        foreach (var trNode in trNodes)
        {
            IList<HtmlNode> tds = trNode.ChildNodes.Where(x => x.Name == "td").ToArray();
            if (tds.Count != 4)
            {
                //error
                continue;
            }

            var parameter = new ApiResponseParameter
            {
                Name = tds[1].InnerText,
                Type = tds[2].InnerText,
                Description = tds[3].InnerText
            };

            yield return parameter;
        }
    }

    private static string GetResponseSample(HtmlNode root)
        => root.SelectSingleNode("//*[@id=\"返回数据示例\"]")?.NextSibling?.InnerText?.Trim() ?? string.Empty;
}
