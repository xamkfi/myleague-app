using System.Text.RegularExpressions;

namespace Application.Features.Common.RulesSection;

internal static class RulesHtmlHelper
{
    private static string GetRulePattern(string ruleId)
    {
        return $@"<div\s+class=""rules-item""\s+data-rule-id=""{Regex.Escape(ruleId)}""\s*[^>]*>(.*?)</div>\s*";
    }

    public static bool ContainsRule(string contentHtml, string ruleId)
    {
        return Regex.IsMatch(contentHtml, GetRulePattern(ruleId), RegexOptions.Singleline);
    }

    public static string AppendRule(string contentHtml, string ruleHtml)
    {
        string trimmedRule = ruleHtml.Trim();
        if (string.IsNullOrWhiteSpace(trimmedRule))
        {
            return contentHtml;
        }

        if (string.IsNullOrWhiteSpace(contentHtml))
        {
            return trimmedRule;
        }

        return $"{contentHtml.Trim()}{trimmedRule}";
    }

    public static string UpdateRule(string contentHtml, string ruleId, string ruleHtml)
    {
        string pattern = GetRulePattern(ruleId);

        if (!Regex.IsMatch(contentHtml, pattern, RegexOptions.Singleline))
        {
            throw new InvalidOperationException($"Rule with ID '{ruleId}' not found.");
        }

        return Regex.Replace(contentHtml, pattern, ruleHtml.Trim(), RegexOptions.Singleline).Trim();
    }

    public static string DeleteRule(string contentHtml, string ruleId)
    {
        string pattern = GetRulePattern(ruleId);

        if (!Regex.IsMatch(contentHtml, pattern, RegexOptions.Singleline))
        {
            throw new InvalidOperationException($"Rule with ID '{ruleId}' not found.");
        }

        return Regex.Replace(contentHtml, pattern, string.Empty, RegexOptions.Singleline).Trim();
    }
}
