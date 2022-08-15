using HtmlAgilityPack;

namespace WechatServerApiGenerator.Utilities;

static class HtmlDocNodeFinder
{
    public static HtmlNode? FindPrevious(HtmlNode current, string nodeName)
        => FindPrevious(current, x => x.Name == nodeName);

    public static HtmlNode? FindPrevious(HtmlNode current, Func<HtmlNode, bool> func)
    {
        if (current == null) return null;

        var temp = current;
        while (temp != null)
        {
            if (func(temp)) return temp;
            else temp = temp.PreviousSibling;
        }

        return temp;
    }

    public static HtmlNode? FindNext(HtmlNode current, string nodeName)
        => FindNext(current, x => x.Name == nodeName);

    public static HtmlNode? FindNext(HtmlNode current, Func<HtmlNode, bool> func)
    {
        if (current == null) return null;

        var temp = current;
        while (temp != null)
        {
            if (func(temp)) return temp;
            else temp = temp.NextSibling;
        }

        return temp;
    }
}
