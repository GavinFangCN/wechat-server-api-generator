using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using WechatServerApiGenerator.Utilities;

var apiGroups = await WechatApiGroupDocLoader.GetApiGroupsAsync();

foreach(var api in apiGroups.SelectMany(g => g.Apis))
{
    var details = await WechatApiDetailsDocLoader.GetApiDetailsAsync(api.DocUrl);
    Console.WriteLine(JsonUtility.Serialize(details));
}