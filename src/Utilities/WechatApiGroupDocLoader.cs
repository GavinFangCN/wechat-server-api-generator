using WechatServerApiGenerator.Models;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.Json;

namespace WechatServerApiGenerator.Utilities;

class WechatApiGroupDocLoader
{
    private const string LocalApiGroupJsonFileName = "api-groups.json";

    public static async Task<IEnumerable<ApiGroup>> GetApiGroupsAsync()
    {
        var apiGroups = await GetLocalApiGroupsAsync();
        if (apiGroups.Any()) return apiGroups;

        apiGroups = await GetRemoteApiGroupsAsync();
        if(apiGroups.Any())
        {
            await LocalJsonDataManager.SetJsonAsync(LocalApiGroupJsonFileName, apiGroups);
        }

        return apiGroups;
    }

    private static async Task<IEnumerable<ApiGroup>> GetLocalApiGroupsAsync()
    {
        var json = await LocalJsonDataManager.GetJsonAsync(LocalApiGroupJsonFileName);
        return string.IsNullOrWhiteSpace(json) ? Array.Empty<ApiGroup>() : JsonSerializer.Deserialize<ApiGroup[]>(json) ?? Array.Empty<ApiGroup>();
    }

    public static async Task<IEnumerable<ApiGroup>> GetRemoteApiGroupsAsync()
    {
        var html = await WechatApiDocLoaderUtility.GetStringAsync(WechatApiDocLoaderUtility.OpenApiRootUrl);
        if (string.IsNullOrWhiteSpace(html)) return Array.Empty<ApiGroup>();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var rootNode = doc.DocumentNode.SelectSingleNode("//*[@id=\"docContent\"]/div[2]");

        var apiGroups = new List<ApiGroup>();
        foreach (var groupNode in rootNode.ChildNodes.Skip(1))
        {
            if (!IsApiGroupStartNode(groupNode)) continue;

            var nextSlibling = groupNode.NextSibling;
            var tableContainer = nextSlibling.NextSibling;
            Debug.Assert(tableContainer.Name == "div");

            var groupTitle = groupNode.InnerText;
            if (groupTitle.StartsWith("# "))
            {
                groupTitle = groupTitle[2..];
            }

            var table = tableContainer.FirstChild;
            var apiGroup = new ApiGroup()
            {
                Title = groupTitle,
                Apis = ExtractApiSummariesViaTableNode(table).ToArray()
            };

            apiGroups.Add(apiGroup);
        }

        return apiGroups;
    }

    private static string GetDocUrlByEnglishTd(HtmlNode englishTd)
    {
        var relativeUrl = englishTd.FirstChild.GetAttributeValue("href", string.Empty);
        return WechatApiDocLoaderUtility.ToFullUrl(relativeUrl);
    }

    private static bool IsApiGroupStartNode(HtmlNode node)
    {
        if (node.NodeType != HtmlNodeType.Element)
        {
            return false;
        }

        var nodeName = node.Name;
        if (nodeName != "h2" && nodeName != "h3")
        {
            return false;
        }

        var nextSlibling = node.NextSibling;
        if (nextSlibling.NextSibling.Name == "h3")
        {
            return false;
        }

        return true;
    }

    private static IEnumerable<ApiSummary> ExtractApiSummariesViaTableNode(HtmlNode table)
    {
        var tbody = table.LastChild;
        if (tbody == null || tbody.Name != "tbody")
        {
            yield break;
        }

        foreach (var trNode in tbody.ChildNodes)
        {
            if (trNode.NodeType != HtmlNodeType.Element)
            {
                continue;
            }

            Debug.Assert(trNode.Name == "tr");

            var englishTd = trNode.FirstChild;
            var chineseTd = trNode.LastChild;

            var apiSummary = new ApiSummary
            {
                EnglishName = englishTd.InnerText,
                ChineseName = chineseTd.InnerText,
                DocUrl = GetDocUrlByEnglishTd(englishTd)
            };

            yield return apiSummary;
        }
    }
}