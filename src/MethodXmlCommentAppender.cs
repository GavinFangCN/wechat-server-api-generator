namespace WechatServerApiGenerator;

class MethodXmlCommentAppender
{
    private readonly TypeGeneratorBase _textBuilder;
    private readonly string[] _descriptions;

    public MethodXmlCommentAppender(TypeGeneratorBase textBuilder, params string[] descriptions)
    {
        _textBuilder = textBuilder;
        _descriptions = descriptions;

        Init();
    }

    private void Init()
    {
        AppendLine("/// <summary>");

        foreach (var description in _descriptions.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            AppendLine("/// " + description.Replace("\n", " "));
        }

        AppendLine("/// </summary>");
    }

    public void AddParameter(string name, string description)
    {
        AppendLine($"/// <param name=\"{name}\">{description.Replace("\n", " ")}</param>");
    }

    public void Complete(string returnConent)
    {
        AppendLine($"/// <returns>{returnConent.Replace("\n", " ")}</returns>");
    }

    private void AppendLine(string text) => _textBuilder.AppendLine(text);
}
