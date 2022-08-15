using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using WechatServerApiGenerator.Models;

namespace WechatServerApiGenerator;

internal class ApiGroupTypeGenerator : TypeGeneratorBase
{
    public ApiGroupTypeGenerator(string @namespace, string className) : base(@namespace, className)
    {
    }

    public void AppendApi(ApiSummary summary, ApiDetails details, bool needAccessToken = true)
    {
        if (details.HttpMethod == "GET")
        {
            AppendGetApi(summary, details, needAccessToken);
        }
        else if (details.HttpMethod == "POST")
        {
            AppendPostApi(summary, details, needAccessToken);
        }
        else
        {
            Console.Error.WriteLine($"Unhandled httpmethod: {details.HttpMethod} for api: {summary.EnglishName}");
        }
    }

    public void AppendGetApi(ApiSummary summary, ApiDetails details, bool needAccessToken = true)
    {
        var name = UpperFirstLetter(summary.EnglishName);

        AppendGetMethodXmlComments(details);
        NormalizeParameterNames(details.RequestParameters);

        var resultTypeName = CreateResultType(name, details.ResponseParameters);

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

    public void AppendPostApi(ApiSummary summary, ApiDetails details, bool needAccessToken = true)
    {
        var name = UpperFirstLetter(summary.EnglishName);

        var requestBodyTypeName = CreateRequestBodyType(name, details.RequestParameters);

        AppendPostMethodXmlComments(details, "requestBody", string.Empty);
        NormalizeParameterNames(details.RequestParameters);

        var resultTypeName = CreateResultType(name, details.ResponseParameters);

        AppendLine($"public async ValueTask<{resultTypeName}> {name}({requestBodyTypeName} requestBody)");
        AppendStartObject();

        if (needAccessToken)
        {
            AppendLine("var access_token = await _accessTokenLoader.GetAccessTokenAsync();");
        }
        AppendStringVarDeclaration("reqUrl", details.Url);

        AppendLine();
        AppendLine("var requestBodyJson = JsonSerializer.Serialize(requestBody);");
        AppendLine("var json = await _apiClient.PostAsJsonAsync(reqUrl, requestBodyJson);");
        AppendLine($"return JsonSerializer.Deserialize<{resultTypeName}>(json);");

        AppendEndObject();
    }

    private string CreateRequestBodyType(string apiName, IEnumerable<ApiRequestParameter> parameters)
    {
        var typeName = apiName + "Body";
        return CreateDtoType(typeName, parameters, (typeName, initialIndentLength) => new ApiRequestBodyTypeGenerator(typeName, initialIndentLength));
    }

    private string CreateResultType(string apiName, IEnumerable<ApiResponseParameter> parameters)
    {
        var resultTypeName = apiName + "Result";
        return CreateDtoType(resultTypeName, parameters, (typeName, initialIndentLength) => new ApiResponseTypeGenerator(typeName, initialIndentLength));
    }

    private string CreateDtoType<TParameter, TGenerator>(string typeName, IEnumerable<TParameter> parameters, Func<string, int, TGenerator> generatorCtor)
        where TParameter : ApiParameter
        where TGenerator: RequestDtoTypeGenerator
    {
        var typeGenerator = generatorCtor(typeName, 4);
        foreach (var parameter in parameters)
        {
            typeGenerator.AppendJsonProperty(parameter.Name, parameter.CSharpType, parameter.Description);
        }

        Insert(ClassStartIndex, typeGenerator.Complete());
        return typeName;
    }

    private static void NormalizeParameterNames<T>(IEnumerable<T> parameters) where T : ApiParameter
    {
        foreach (var parameter in parameters)
        {
            NormalizeParameterNames(parameter);
        }
    }

    private static void NormalizeParameterNames<T>(T parameter) where T : ApiParameter
    {
        var parameterName = parameter.Name;
        if (parameterName.Contains('/'))
        {
            parameter.Name = parameterName[..parameterName.IndexOf('/')].Trim();
        }
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

    private void AppendGetMethodXmlComments(ApiDetails details)
    {
        var xmlCommentAppender = new MethodXmlCommentAppender(this, details.Description, details.RequestSample);

        foreach (var requestParameter in details.RequestParameters)
        {
            xmlCommentAppender.AddParameter(requestParameter.Name, $"{requestParameter.Description}, IsRequired: {requestParameter.IsRequired}");
        }

        xmlCommentAppender.Complete(details.ResponseSample);
    }

    private void AppendPostMethodXmlComments(ApiDetails details, string bodyParameterName, string bodyDescription)
    {
        var xmlCommentAppender = new MethodXmlCommentAppender(this, details.Description, details.RequestSample);
        xmlCommentAppender.AddParameter(bodyParameterName, bodyDescription);

        xmlCommentAppender.Complete(details.ResponseSample);
    }

    private static string GenerateFullUrl(string url, IList<ApiRequestParameter> parameters, string accessTokenVariableName)
    {
        if (url.Contains('?'))
        {
            url = url.Substring(0, url.IndexOf('?'));
        }

        var query = string.Join('&', parameters.Select(p => $"{p.Name}={{{(p.Name == "params" ? "@params" : p.Name)}}}"));
        var result = url + "?" + query;

        //if (!string.IsNullOrWhiteSpace(accessTokenVariableName))
        //{
        //    if (result.Contains("ACCESS_TOKEN"))
        //    {
        //        result = result.Replace("ACCESS_TOKEN", $"{{{accessTokenVariableName}}}");
        //    }
        //    else
        //    {
        //        result += $"&access_token={{{accessTokenVariableName}}}";
        //    }
        //}

        return result;
    }

    private static string GenerateMethodArguments(IList<ApiRequestParameter> parameters)
    {
        if (parameters.Count == 0) return string.Empty;

        return string.Join(", ", parameters.Select(p => $"{p.CSharpType} {(p.Name == "params" ? "@params" : p.Name)}"));
    }
}
