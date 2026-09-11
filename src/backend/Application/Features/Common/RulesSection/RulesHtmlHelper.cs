using System.Text.RegularExpressions;

namespace Application.Features.Common.RulesSection;

/// <summary>
/// Helper methods for manipulating rule HTML blocks within a rules section
/// </summary>
internal static class RulesHtmlHelper
{
    private static string GetRulePattern(string ruleId)
    {
        return $@"<div\s+class=""rules-item""\s+data-rule-id=""{Regex.Escape(ruleId)}""\s*[^>]*>(.*?)</div>\s*";
    }

    /// <summary>
    /// Checks whether the content HTML contains a rule with the given ID
    /// </summary>
    /// <param name="contentHtml">The section HTML content</param>
    /// <param name="ruleId">The rule identifier</param>
    /// <returns>True if the rule exists in the content</returns>
    public static bool ContainsRule(string contentHtml, string ruleId)
    {
        return Regex.IsMatch(contentHtml, GetRulePattern(ruleId), RegexOptions.Singleline);
    }

    /// <summary>
    /// Appends a rule HTML block to the section content
    /// </summary>
    /// <param name="contentHtml">The existing section HTML content</param>
    /// <param name="ruleHtml">The rule HTML to append</param>
    /// <returns>The updated HTML content</returns>
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

    /// <summary>
    /// Updates an existing rule HTML block within the section content
    /// </summary>
    /// <param name="contentHtml">The existing section HTML content</param>
    /// <param name="ruleId">The rule identifier</param>
    /// <param name="ruleHtml">The updated rule HTML</param>
    /// <returns>The updated HTML content</returns>
    public static string UpdateRule(string contentHtml, string ruleId, string ruleHtml)
    {
        string pattern = GetRulePattern(ruleId);

        if (!Regex.IsMatch(contentHtml, pattern, RegexOptions.Singleline))
        {
            throw new InvalidOperationException($"Rule with ID '{ruleId}' not found.");
        }

        return Regex.Replace(contentHtml, pattern, ruleHtml.Trim(), RegexOptions.Singleline).Trim();
    }

    /// <summary>
    /// Deletes a rule HTML block from the section content
    /// </summary>
    /// <param name="contentHtml">The existing section HTML content</param>
    /// <param name="ruleId">The rule identifier</param>
    /// <returns>The updated HTML content</returns>
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
