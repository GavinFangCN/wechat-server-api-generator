using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WechatServerApiGenerator.Models;

namespace WechatServerApiGenerator;

internal class ApiGroupTypeGenerator : TypeGeneratorBase
{
    public ApiGroupTypeGenerator(string @namespace, string className) : base(@namespace, className)
    {
    }

    public void AppendApi(ApiSummary summary, ApiDetails details, bool needAccessToken = true)
    {
        var name = UpperFirstLetter(summary.EnglishName);

        AppendXmlComments(details);

        foreach (var requestParameter in details.RequestParameters)
        {
            var parameterName = requestParameter.Name;
            if (parameterName.Contains('/'))
            {
                requestParameter.Name = parameterName.Substring(0, parameterName.IndexOf('/')).Trim();
            }
        }

        var resultTypeName = name + "Result";
        var responseTypeGenerator = new ApiResponseTypeGenerator(resultTypeName, 4);
        foreach (var responseParameter in details.ResponseParameters)
        {
            responseTypeGenerator.AppendJsonProperty(responseParameter.Name, responseParameter.CSharpType, responseParameter.Description);
        }

        Insert(ClassStartIndex, responseTypeGenerator.Complete());

        AppendLine($"public async ValueTask<{resultTypeName}> {name}({GenerateMethodArguments(details.RequestParameters)})");
        AppendStartObject();

        if (needAccessToken)
        {
            AppendLine("var accessToken = await _accessTokenLoader.GetAccessTokenAsync();");
        }
        AppendStringVarDeclaration("reqUrl", GenerateFullUrl(details.Url, details.RequestParameters, needAccessToken ? "accessToken" : string.Empty));

        AppendLine();
        AppendLine("var json = await _apiClient.GetJsonAsync(reqUrl);");
        AppendLine($"return JsonSerializer.Deserialize<{resultTypeName}>(json);");

        AppendEndObject();
    }

    protected override void AddUsings()
    {
        AppendUsing("System.Text.Json");
        AppendUsingFor<JsonPropertyNameAttribute>();
    }

    protected override void AppendClassInfra(string className)
    {
        AppendLine($"private readonly {ApiGeneratorShared.WechatApiClientInterfaceName} _apiClient;");
        AppendLine($"private readonly {ApiGeneratorShared.AccessTokenLoaderInterfaceName} _accessTokenLoader;");

        AppendLine($"public {className}({ApiGeneratorShared.WechatApiClientInterfaceName} apiClient, {ApiGeneratorShared.AccessTokenLoaderInterfaceName} accessTokenLoader)");
        AppendStartObject();
        AppendLine("_apiClient = apiClient;");
        AppendLine("_accessTokenLoader = accessTokenLoader;");
        AppendEndObject();
    }

    protected void AppendStringVarDeclaration(string varName, string value) => AppendLine($"var {varName} = $\"{value}\";");

    private void AppendXmlComments(ApiDetails details)
    {
        AppendLine("/// <summary>");
        AppendLine("/// " + details.Description);

        if (!string.IsNullOrWhiteSpace(details.RequestSample))
        {
            AppendLine("/// " + details.RequestSample.Replace("\n", " "));
        }

        AppendLine("/// </summary>");

        foreach (var requestParameter in details.RequestParameters)
        {
            AppendLine($"/// <param name=\"{requestParameter.Name}\">{requestParameter.Description}, IsRequired: {requestParameter.IsRequired}</param>");
        }

        AppendLine($"/// <returns>{details.ResponseSample.Replace("\n", " ")}</returns>");
    }

    private static string GenerateFullUrl(string url, IList<ApiRequestParameter> parameters, string accessTokenVariableName)
    {
        if (url.Contains('?'))
        {
            url = url.Substring(0, url.IndexOf('?'));
        }

        var query = string.Join('&', parameters.Select(p => $"{p.Name}={{{p.Name}}}"));
        var result = url + "?" + query;

        if (!string.IsNullOrWhiteSpace(accessTokenVariableName))
        {
            if (result.Contains("ACCESS_TOKEN"))
            {
                result = result.Replace("ACCESS_TOKEN", $"{{{accessTokenVariableName}}}");
            }
            else
            {
                result += $"&access_token={{{accessTokenVariableName}}}";
            }
        }

        return result;
    }

    private static string GenerateMethodArguments(IList<ApiRequestParameter> parameters)
    {
        if (parameters.Count == 0) return string.Empty;

        return string.Join(", ", parameters.Select(p => $"{p.CSharpType} {p.Name}"));
    }
}
