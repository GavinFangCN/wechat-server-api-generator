using System.Reflection.Metadata;
using System.Text;

namespace WechatServerApiGenerator;

abstract class TypeGeneratorBase
{
    private readonly string _namespace;

    public string NormalClassName { get; }

    private readonly StringBuilder _builder;

    protected int CurrentIndentLength { get; private set; }

    protected string CurrentIndent => CurrentIndentLength <= 0 ? string.Empty : new(' ', CurrentIndentLength);

    protected int NamespaceStartIndex { get; private set; }
    protected int ClassStartIndex { get; }

    public TypeGeneratorBase(string @namespace, string className, int initialIndentLength = 0)
    {
        _namespace = @namespace;
        NormalClassName = UpperFirstLetter(className);
        CurrentIndentLength = initialIndentLength;

        _builder = new StringBuilder();

        AddUsings();
        NamespaceStartIndex = Length;

        if (!string.IsNullOrWhiteSpace(@namespace))
        {
            AppendLine();
            AppendLine($"namespace {@namespace}");
            AppendStartObject();

            ClassStartIndex = Length;
        }

        AppendLine($"public partial class {NormalClassName}");
        AppendStartObject();

        AppendClassInfra(NormalClassName);
    }

    protected void AppendUsingFor<T>() => AppendUsing(typeof(T).Namespace!);

    protected virtual void AddUsings()
    {
        
    }

    protected virtual void AppendClassInfra(string className)
    {

    }

    protected void AppendUsing(string ns)
    {
        var text = $"using {ns};" + Environment.NewLine;
        Insert(NamespaceStartIndex, text);

        NamespaceStartIndex += text.Length;
    }

    public string Complete()
    {
        AppendEndObject();

        if (!string.IsNullOrWhiteSpace(_namespace))
        {
            AppendEndObject();
        }

        return _builder.ToString();
    }

    protected void AppendStartObject()
    {
        AppendLine("{");
        IncIndent();
    }

    protected void AppendEndObject()
    {
        DecIndent();

        AppendLine("}");
        AppendLine();
    }

    protected void IncIndent() => CurrentIndentLength += 4;
    protected void DecIndent() => CurrentIndentLength -= 4;

    protected void AppendLine(string text) => _builder.AppendLine(CurrentIndent + text);
    protected void AppendLine() => _builder.AppendLine();

    protected void Insert(int index, string text) => _builder.Insert(index, text);

    protected int Length => _builder.Length;

    protected static string UpperFirstLetter(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        if (char.IsLower(name[0]))
        {
            var chars = name.ToCharArray();
            chars[0] = char.ToUpper(chars[0]);

            name = new string(chars);
        }

        return name;
    }
}
