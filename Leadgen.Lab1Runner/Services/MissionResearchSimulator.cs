using Leadgen.Model.Entities;

namespace Leadgen.Lab1Runner.Services;

public class MissionResearchSimulator
{
    public async Task<List<TargetCompany>> RunScoutAsync(
        BusinessDnaMission mission,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(250, cancellationToken);

        var run = GetLatestRun(mission);
        return run?.TargetCompanies
            .Where(company => company.IsHeadquartersVerified)
            .OrderByDescending(company => company.MatchScore)
            .Take(2)
            .ToList() ?? new List<TargetCompany>();
    }

    public async Task<List<TargetCompany>> RunSentinelAsync(
        BusinessDnaMission mission,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(325, cancellationToken);

        var run = GetLatestRun(mission);
        return run?.TargetCompanies
            .Where(company => company.LastSignalAtUtc.HasValue)
            .OrderByDescending(company => company.LastSignalAtUtc)
            .Take(2)
            .ToList() ?? new List<TargetCompany>();
    }

    public async Task<List<TargetContact>> RunAnchorAsync(
        IEnumerable<TargetCompany> companies,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(220, cancellationToken);

        return companies
            .SelectMany(company => company.Contacts)
            .Where(contact => contact.IsDecisionMaker)
            .GroupBy(contact => contact.Id)
            .Select(group => group.First())
            .OrderBy(contact => contact.FullName)
            .ToList();
    }

    public async Task<List<EvidencePoint>> RunSoulAsync(
        IEnumerable<TargetContact> contacts,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(260, cancellationToken);

        return contacts
            .SelectMany(contact => contact.EvidencePoints)
            .Where(evidence => evidence.IsQualificationSignal)
            .OrderByDescending(evidence => evidence.CapturedAtUtc)
            .ToList();
    }

    private static MissionRun? GetLatestRun(BusinessDnaMission mission)
    {
        return mission.Runs
            .OrderByDescending(run => run.StartedAtUtc)
            .FirstOrDefault();
    }
}
