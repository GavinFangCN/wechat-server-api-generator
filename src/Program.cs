using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using WechatServerApiGenerator.Utilities;

var apiGroups = await WechatApiGroupDocLoader.GetApiGroupsAsync();

Console.WriteLine(apiGroups.Count());