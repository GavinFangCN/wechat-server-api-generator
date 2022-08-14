namespace WechatServerApiGenerator;

class ApiInfraCodeGenerator
{
    private readonly string _namespace;

    public ApiInfraCodeGenerator(string @namespace)
    {
        _namespace = @namespace;
    }

    public void WriteWechatClientInterface(string filePath)
    {
        var text = $"using {_namespace};" + Environment.NewLine + @"
public interface " + ApiGeneratorShared.WechatApiClientInterfaceName + @"
{
    ValueTask<string> GetJsonAsync(string url);
}";

        File.WriteAllText(filePath, text);
    }

    public void WriteAccessLoaderInterface(string filePath)
    {
        var text = $"using {_namespace};" + Environment.NewLine + @"
public interface " + ApiGeneratorShared.AccessTokenLoaderInterfaceName + @"
{
    ValueTask<string> GetAccessTokenAsync();
}";

        File.WriteAllText(filePath, text);
    }
}
