using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace MicroStock.Common.Application.Extensions;

public static class EnumExtensions
{
    private static readonly Dictionary<Type, Dictionary<object, string>> _descriptionCache = new();
    private static readonly Lock _lock = new();

    public static string GetDescription(this Enum enumValue)
    {
        Type type = enumValue.GetType();
        Enum enumKey = enumValue;

        lock (_lock)
        {
            if (!_descriptionCache.TryGetValue(type, out Dictionary<object, string>? cache))
            {
                cache = BuildDescriptionCache(type);
                _descriptionCache[type] = cache;
            }

            return cache.TryGetValue(enumKey, out var description)
                ? description
                : Humanize(enumValue.ToString());
        }
    }

    private static Dictionary<object, string> BuildDescriptionCache(Type enumType)
    {
        var cache = new Dictionary<object, string>();

        foreach (FieldInfo field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            string description = field.Name;

            DescriptionAttribute? descAttr = field.GetCustomAttribute<DescriptionAttribute>();
            if (descAttr is not null)
            {
                description = descAttr.Description;
            }
            else
            {
                description = Humanize(field.Name);
            }

            cache[field.GetValue(null)!] = description;
        }

        return cache;
    }

    private static string Humanize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var result = new StringBuilder(input.Length + 10);

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (i > 0)
            {
                char prev = input[i - 1];

                // Insert space before capital letters (not first char)
                if (char.IsUpper(c) &&
                    (char.IsLower(prev) ||
                     (i > 1 && char.IsLower(input[i - 2]) && char.IsUpper(prev))))
                {
                    result.Append(' ');
                }
                // Insert space before digit runs
                else if (char.IsDigit(c) && !char.IsDigit(prev))
                {
                    result.Append(' ');
                }
                // Insert space after digit when letter follows
                else if (char.IsLetter(c) && char.IsDigit(prev))
                {
                    result.Append(' ');
                }
            }

            result.Append(c);
        }

        return result.ToString()
             .Replace('_', ' ')
             .Trim();
    }
}
