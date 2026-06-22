using System.Text.RegularExpressions;
using LeadGen.Core.Domain;
using LeadGen.Core.Providers;

namespace LeadGen.Infrastructure.Providers;

public sealed partial class PublicContactEnrichmentClient : IContactEnrichmentClient
{
    public Task<IReadOnlyList<ContactCandidate>> FindContactsAsync(string companyName, string? domain, string? websiteUrl, IEnumerable<ExtractedPageDto> pages, CancellationToken ct)
    {
        var contacts = new List<ContactCandidate>();
        var normalizedDomain = NormalizeDomain(domain, websiteUrl);
        foreach (var page in pages)
        {
            foreach (Match match in EmailRegex().Matches(page.Text))
            {
                var email = CleanEmail(match.Value);
                if (string.IsNullOrWhiteSpace(email) || IsLowValueEmail(email))
                {
                    continue;
                }

                var confidence = EmailMatchesDomain(email, normalizedDomain) ? 90 : 78;
                contacts.Add(new ContactCandidate(LeadContactType.Email, email, page.Url, confidence, false));
            }

            foreach (var phone in ExtractPhones(page.Text))
            {
                contacts.Add(new ContactCandidate(LeadContactType.Phone, phone, page.Url, LooksLikeContactPage(page.Url) ? 76 : 66, false));
            }

            contacts.AddRange(ExtractPeople(companyName, page));

            var isContactPage = LooksLikeContactPage(page.Url) || page.Text.Contains("contact", StringComparison.OrdinalIgnoreCase) || page.Text.Contains("kontakt", StringComparison.OrdinalIgnoreCase);
            if (isContactPage && !string.IsNullOrWhiteSpace(websiteUrl))
            {
                contacts.Add(new ContactCandidate(LeadContactType.ContactPage, page.Url, page.Url, LooksLikeContactPage(page.Url) ? 78 : 65, false));
            }
        }

        return Task.FromResult<IReadOnlyList<ContactCandidate>>(
            contacts
                .GroupBy(contact => $"{contact.Type}:{contact.Value}", StringComparer.OrdinalIgnoreCase)
                .Select(group => group.OrderByDescending(contact => contact.ConfidenceScore).First())
                .OrderByDescending(ContactRank)
                .ThenByDescending(contact => contact.ConfidenceScore)
                .Take(8)
                .ToList());
    }

    private static int ContactRank(ContactCandidate contact)
    {
        return contact.Type switch
        {
            LeadContactType.Email => 50,
            LeadContactType.ContactPage => 40,
            LeadContactType.Phone => 30,
            LeadContactType.Social => 20,
            _ => 10
        };
    }

    private static string CleanEmail(string value)
    {
        return value.Trim().TrimEnd('.', ',', ';', ':', ')', ']', '}').ToLowerInvariant();
    }

    private static bool IsLowValueEmail(string email)
    {
        var localPart = email.Split('@')[0];
        return email.EndsWith("@example.com", StringComparison.OrdinalIgnoreCase)
            || email.EndsWith("@domain.com", StringComparison.OrdinalIgnoreCase)
            || localPart is "noreply" or "no-reply" or "donotreply" or "do-not-reply"
            || localPart.Contains("privacy", StringComparison.OrdinalIgnoreCase)
            || localPart.Contains("gdpr", StringComparison.OrdinalIgnoreCase);
    }

    private static bool EmailMatchesDomain(string email, string? domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return false;
        }

        var emailDomain = email.Split('@').LastOrDefault();
        return string.Equals(emailDomain, domain, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(emailDomain) && emailDomain.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase));
    }

    private static string? NormalizeDomain(string? domain, string? websiteUrl)
    {
        if (!string.IsNullOrWhiteSpace(domain))
        {
            return domain.Trim().Replace("www.", "", StringComparison.OrdinalIgnoreCase).ToLowerInvariant();
        }

        return Uri.TryCreate(websiteUrl, UriKind.Absolute, out var uri)
            ? uri.Host.Replace("www.", "", StringComparison.OrdinalIgnoreCase).ToLowerInvariant()
            : null;
    }

    private static IEnumerable<string> ExtractPhones(string text)
    {
        return PhoneRegex().Matches(text)
            .Select(match => CleanPhone(match.Value))
            .Where(phone => !string.IsNullOrWhiteSpace(phone))
            .Select(phone => phone!)
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static string? CleanPhone(string value)
    {
        var cleaned = Regex.Replace(value, @"[^\d+]", "");
        var digitCount = cleaned.Count(char.IsDigit);
        if (digitCount is < 7 or > 16)
        {
            return null;
        }

        if (!cleaned.StartsWith("+", StringComparison.Ordinal) && digitCount > 12)
        {
            return null;
        }

        return value.Trim().TrimEnd('.', ',', ';', ':', ')', ']', '}');
    }

    private static IEnumerable<ContactCandidate> ExtractPeople(string companyName, ExtractedPageDto page)
    {
        var text = page.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        foreach (Match match in PersonRegex().Matches(text).Take(30))
        {
            var name = Regex.Replace(match.Value, @"\s+", " ").Trim();
            if (!IsLikelyPersonName(name, companyName))
            {
                continue;
            }

            var context = SurroundingText(text, match.Index, 140);
            if (!HasPersonContext(context) && !name.StartsWith("dr", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var role = DetectRole(context);
            var value = string.IsNullOrWhiteSpace(role) ? $"Person: {name}" : $"Person: {name} ({role})";
            yield return new ContactCandidate(LeadContactType.Other, value, page.Url, 58, false);
        }
    }

    private static bool IsLikelyPersonName(string name, string companyName)
    {
        var lowered = name.ToLowerInvariant();
        if (lowered.Contains("clinic", StringComparison.Ordinal)
            || lowered.Contains("poliklinika", StringComparison.Ordinal)
            || lowered.Contains("ordinacija", StringComparison.Ordinal)
            || lowered.Contains("kontakt", StringComparison.Ordinal)
            || lowered.Contains("privacy", StringComparison.Ordinal)
            || lowered.Contains("facebook", StringComparison.Ordinal)
            || lowered.Contains("instagram", StringComparison.Ordinal))
        {
            return false;
        }

        return !companyName.Contains(name, StringComparison.OrdinalIgnoreCase)
            && name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length is >= 2 and <= 4;
    }

    private static bool HasPersonContext(string context)
    {
        var terms = new[]
        {
            "owner", "founder", "director", "manager", "doctor", "dentist", "team", "staff",
            "kontakt osoba", "direktor", "voditelj", "doktor", "stomatolog", "ordinacija", "dr."
        };

        return terms.Any(term => context.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static string? DetectRole(string context)
    {
        var roleMap = new (string Term, string Role)[]
        {
            ("owner", "owner"),
            ("founder", "founder"),
            ("director", "director"),
            ("manager", "manager"),
            ("dentist", "dentist"),
            ("doctor", "doctor"),
            ("direktor", "director"),
            ("voditelj", "manager"),
            ("doktor", "doctor"),
            ("stomatolog", "dentist")
        };

        return roleMap.FirstOrDefault(item => context.Contains(item.Term, StringComparison.OrdinalIgnoreCase)).Role;
    }

    private static string SurroundingText(string text, int index, int radius)
    {
        var start = Math.Max(0, index - radius);
        var length = Math.Min(text.Length - start, radius * 2);
        return text.Substring(start, length);
    }

    private static bool LooksLikeContactPage(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.AbsolutePath.Contains("contact", StringComparison.OrdinalIgnoreCase)
                || uri.AbsolutePath.Contains("kontakt", StringComparison.OrdinalIgnoreCase));
    }

    [GeneratedRegex(@"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"\+?\d[\d\s()./-]{6,}\d", RegexOptions.Compiled)]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"\b(?:dr\.?\s+|mr\.?\s+|mrs\.?\s+|ms\.?\s+|prof\.?\s+)?[A-Z][a-z]{2,}(?:\s+[A-Z][a-z]{2,}){1,2}\b", RegexOptions.Compiled)]
    private static partial Regex PersonRegex();
}
