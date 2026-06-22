namespace LeadGen.Core.Domain;

public static class LeadIdentity
{
    public static string BuildDedupeKey(string? domain, string? companyName, string? location, string? primaryContact = null)
    {
        var normalizedDomain = NormalizeDomain(domain);
        if (!string.IsNullOrWhiteSpace(normalizedDomain))
        {
            return $"domain:{normalizedDomain}";
        }

        var normalizedContact = NormalizeContact(primaryContact);
        if (!string.IsNullOrWhiteSpace(normalizedContact))
        {
            return $"contact:{normalizedContact}";
        }

        var name = NormalizeToken(companyName);
        var place = NormalizeToken(location);
        return string.IsNullOrWhiteSpace(place)
            ? $"name:{name}"
            : $"name:{name}:location:{place}";
    }

    public static string NormalizeDomain(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var candidate = value.Trim();
        if (Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            candidate = uri.Host;
        }

        return candidate
            .Replace("www.", "", StringComparison.OrdinalIgnoreCase)
            .Trim()
            .TrimEnd('/')
            .ToLowerInvariant();
    }

    public static string NormalizeToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : ' ')
            .ToArray();

        return string.Join(' ', new string(chars).Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string NormalizeContact(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim().ToLowerInvariant();
        return trimmed.Contains('@', StringComparison.Ordinal)
            ? trimmed
            : NormalizeToken(trimmed);
    }
}
