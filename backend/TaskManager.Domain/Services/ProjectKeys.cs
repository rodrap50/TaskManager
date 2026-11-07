using System.Text.RegularExpressions;

namespace TaskManager.Domain.Services;

/// <summary>
/// Project keys (the "TSKM" in "TSKM-001") and task display IDs. A key is 2–6 uppercase
/// letters or digits, unique across projects. A display ID is the key plus the task's
/// per-project number, zero-padded to 3 digits.
/// </summary>
/// <remarks>
/// One place for the rules, so project creation and the startup backfill of existing
/// projects generate keys the same way.
/// </remarks>
public static partial class ProjectKeys
{
    public const int MinLength = 2;
    public const int MaxLength = 6;
    private const int GeneratedLength = 4;
    private const string Fallback = "PRJ";

    public static bool IsValid(string? key) => key is not null && ValidKey().IsMatch(key);

    /// <summary>
    /// Builds a key from a project name: the first word's first letter and consonants, then
    /// the initials of the remaining words, 4 characters at most. "Task Manager" → TSKM,
    /// "Home To-Dos" → HMTD, "FinancialBlazorApp" → FNBA.
    /// </summary>
    public static string Generate(string name)
    {
        var words = Word().Matches(name).Select(m => m.Value.ToUpperInvariant()).ToList();
        if (words.Count == 0) return Fallback;

        var initials = string.Concat(words.Skip(1).Select(w => w[0]));
        var first = words[0];
        var compressed = first[0] + string.Concat(first.Skip(1).Where(c => !"AEIOU".Contains(c)));
        if (compressed.Length < MinLength) compressed = first;

        var budget = Math.Max(MinLength, GeneratedLength - initials.Length);
        var key = (compressed[..Math.Min(budget, compressed.Length)] + initials);
        key = key[..Math.Min(GeneratedLength, key.Length)];

        return key.Length >= MinLength ? key : (key + "X")[..MinLength];
    }

    /// <summary>
    /// <see cref="Generate"/>, made unique against <paramref name="taken"/> by appending
    /// 2, 3, … (trimming the base so the key stays within <see cref="MaxLength"/>).
    /// </summary>
    public static string PickUnique(string name, IReadOnlySet<string> taken)
    {
        var key = Generate(name);
        if (!taken.Contains(key)) return key;

        for (var n = 2; ; n++)
        {
            var suffix = n.ToString();
            var candidate = key[..Math.Min(key.Length, MaxLength - suffix.Length)] + suffix;
            if (!taken.Contains(candidate)) return candidate;
        }
    }

    public static string FormatDisplayId(string key, int number) => $"{key}-{number:D3}";

    /// <summary>Parses "TSKM-001" (any case) into its key and number.</summary>
    public static bool TryParseDisplayId(string? displayId, out string key, out int number)
    {
        key = string.Empty;
        number = 0;

        var match = displayId is null ? Match.Empty : DisplayId().Match(displayId.Trim());
        if (!match.Success || !int.TryParse(match.Groups[2].Value, out number) || number < 1)
            return false;

        key = match.Groups[1].Value.ToUpperInvariant();
        return true;
    }

    [GeneratedRegex("^[A-Z0-9]{2,6}$")]
    private static partial Regex ValidKey();

    // A capitalised word, an all-caps run (acronym), a lowercase word, or a number:
    // splits "FinancialBlazorApp" and "MCPServer" as well as "Home To-Dos".
    [GeneratedRegex("[A-Z]?[a-z]+|[A-Z]+(?![a-z])|[0-9]+")]
    private static partial Regex Word();

    [GeneratedRegex("^([A-Za-z0-9]{2,6})-([0-9]{1,9})$")]
    private static partial Regex DisplayId();
}
