
namespace WechatServerApiGenerator.Models;

public class ApiDetails
{
    public string Description { get; set; } = string.Empty;

    public string HttpMethod { get; set; } = "GET";

    public string Url { get; set; } = string.Empty;

    public IList<ApiRequestParameter> RequestParameters { get; set; } = Array.Empty<ApiRequestParameter>();
    public string RequestSample { get; set; } = string.Empty;

    public IList<ApiResponseParameter> ResponseParameters { get; set; } = Array.Empty<ApiResponseParameter>();
    public string ResponseSample { get; set; } = string.Empty;
}

public abstract class ApiParameter
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = "string";

    public string Description { get; set; } = string.Empty;


    public string CSharpType
    {
        get
        {
            if(string.IsNullOrWhiteSpace(Type))
            {
                if (Name.Contains("code")) Type = "number";
                else Type = "string";
            }

            return Type switch
            {
                "number" => "int",
                "string" => "string",
                "boolean" => "bool",
                "Date" => "DateTime",
                "array&lt;object&gt;" => "IList<object>",
                "array&lt;string&gt;" => "IList<string>",
                "object" => "object",
                "FormData" => "object",
                "array&lt;number&gt;" => "IList<int>",
                "Buffer" => "object",
                "Number" => "int",
                "int32" => "int",
                " " => "string",
                _ => Type
            };
        }
    }
}

public class ApiRequestParameter : ApiParameter
{
    public bool IsRequired { get; set; } = false;
}

public class ApiResponseParameter : ApiParameter
{

}
