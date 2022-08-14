
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
}

public class ApiRequestParameter : ApiParameter
{
    public bool IsRequired { get; set; } = false;
}

public class ApiResponseParameter : ApiParameter
{

}
