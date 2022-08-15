namespace WechatServerApiGenerator;

class RequestDtoTypeGenerator : TypeGeneratorBase
{
    public RequestDtoTypeGenerator(string className, int initialIndentLength)
        : base(string.Empty, className, initialIndentLength)
    {
    }

    public void AppendJsonProperty(string jsonPropertyName, string propretyType, string description)
    {
        if (jsonPropertyName.Contains("access_token", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        AppendLine("/// <summary>");
        AppendLine("/// " + description.Replace("\n", " "));
        AppendLine("/// </summary>");

        AppendLine($"[JsonPropertyName(\"{jsonPropertyName}\")]");
        var propertyName = NormalizePropertyName(jsonPropertyName);
        AppendLine($"public {propretyType} {propertyName} {{ get; set; }}");
        AppendLine();
    }

    private static string NormalizePropertyName(string name)
    {
        name = UpperFirstLetter(name);

        if (name.Contains('_'))
        {
            var parts = name.Split('_', StringSplitOptions.RemoveEmptyEntries);
            for (var i = 1; i < parts.Length; i++)
            {
                parts[i] = UpperFirstLetter(parts[i]);
            }

            name = string.Concat(parts);
        }

        return name;
    }
}
