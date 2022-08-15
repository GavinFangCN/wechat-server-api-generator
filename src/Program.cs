using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using WechatServerApiGenerator;
using WechatServerApiGenerator.Utilities;

var apiGroups = await WechatApiGroupDocLoader.GetApiGroupsAsync();

var rootNamespace = "Wechat.OpenApi";
var outputDirectory = "output";
if (!Directory.Exists(outputDirectory))
{
    Directory.CreateDirectory(outputDirectory);
}

var infraGenerator = new ApiInfraCodeGenerator(rootNamespace);
infraGenerator.WriteAccessLoaderInterface(Path.Combine(outputDirectory, ApiGeneratorShared.AccessTokenLoaderInterfaceName + ".cs"));
infraGenerator.WriteWechatClientInterface(Path.Combine(outputDirectory, ApiGeneratorShared.WechatApiClientInterfaceName + ".cs"));

var namespaceMappingJson = await File.ReadAllTextAsync("ApiNamespaceMapping.json");
var namespaceMappings = JsonUtility.Deserialize<Dictionary<string, string>>(namespaceMappingJson)!;

var apiGroupNameMappingJson = await File.ReadAllTextAsync("ApiClassNameMapping.json");
var apiGroupNameMappings = JsonUtility.Deserialize<Dictionary<string, string>>(apiGroupNameMappingJson)!;
foreach (var groupedByParent in apiGroups.GroupBy(x => x.Parent))
{
    var currentNamespace = rootNamespace;
    var namespaceName = groupedByParent.Key;
    var currentDir = outputDirectory;

    if (!string.IsNullOrEmpty(namespaceName))
    {
        namespaceName = namespaceMappings[namespaceName];
        currentNamespace = rootNamespace + "." + namespaceName;

        var dir = Path.Combine(outputDirectory, namespaceName);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        currentDir = dir;
    }

    foreach (var apiGroup in groupedByParent)
    {
        if (!apiGroupNameMappings.TryGetValue(apiGroup.Title, out var className))
        {
            className = apiGroup.Title;
        }

        var classGgenerator = new ApiGroupTypeGenerator(currentNamespace, className);
        foreach (var apiSummary in apiGroup.Apis)
        {
            var apiDetails = await WechatApiDetailsDocLoader.GetApiDetailsAsync(apiSummary.DocUrl);
            classGgenerator.AppendApi(apiSummary, apiDetails!);
        }

        var classSource = classGgenerator.Complete();

        var filePath = Path.Combine(currentDir, classGgenerator.NormalClassName + ".cs");
        File.WriteAllText(filePath, classSource);

        Console.WriteLine(filePath);
    }
}