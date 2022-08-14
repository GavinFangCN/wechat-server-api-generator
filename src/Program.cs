using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using WechatServerApiGenerator;
using WechatServerApiGenerator.Utilities;

var apiGroups = await WechatApiGroupDocLoader.GetApiGroupsAsync();

//foreach(var api in apiGroups.SelectMany(g => g.Apis))
//{
//    var details = await WechatApiDetailsDocLoader.GetApiDetailsAsync(api.DocUrl);
//    Console.WriteLine(JsonUtility.Serialize(details));
//}

var outputDirectory = "output";
if (!Directory.Exists(outputDirectory))
{
    Directory.CreateDirectory(outputDirectory);
}

var ns = "Wechat.OpenApi";
new ApiInfraCodeGenerator(ns).WriteWechatClientInterface(Path.Combine(outputDirectory, ApiGeneratorShared.WechatApiClientInterfaceName + ".cs"));
new ApiInfraCodeGenerator(ns).WriteAccessLoaderInterface(Path.Combine(outputDirectory, ApiGeneratorShared.AccessTokenLoaderInterfaceName + ".cs"));

var generator = new ApiGroupTypeGenerator(ns, "accessTokenManager");

var apiSummary = apiGroups.First().Apis.First();
var apiDetails = await WechatApiDetailsDocLoader.GetApiDetailsAsync(apiSummary.DocUrl);
generator.AppendApi(apiSummary, apiDetails!);

var source = generator.Complete();



var filePath = Path.Combine(outputDirectory, generator.NormalClassName + ".cs");

File.WriteAllText(filePath, source);