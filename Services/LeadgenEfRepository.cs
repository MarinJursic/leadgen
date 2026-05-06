using Leadgen.Lab1Runner.Seed;
using Leadgen.Model.Entities;
using leadgen.Data;
using Microsoft.EntityFrameworkCore;

namespace leadgen.Services;

public sealed class LeadgenEfRepository : ILeadgenReadRepository
{
    private readonly LeadgenDbContext _dbContext;
    private LeadgenLabDataset? _cachedDataset;

    public LeadgenEfRepository(LeadgenDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public LeadgenLabDataset GetDataset()
    {
        if (_cachedDataset is not null)
        {
            return _cachedDataset;
        }

        var agents = _dbContext.SwarmAgents
            .AsNoTracking()
            .Include(agent => agent.MissionAssignments)
            .OrderBy(agent => agent.CodeName)
            .ToList();

        var missions = _dbContext.BusinessDnaMissions
            .AsNoTracking()
            .AsSplitQuery()
            .Include(mission => mission.ClarificationQuestions)
            .Include(mission => mission.Runs)
                .ThenInclude(run => run.AgentAssignments)
                    .ThenInclude(assignment => assignment.SwarmAgent)
            .Include(mission => mission.Runs)
                .ThenInclude(run => run.TargetCompanies)
                    .ThenInclude(company => company.Contacts)
                        .ThenInclude(contact => contact.ContactChannels)
            .Include(mission => mission.Runs)
                .ThenInclude(run => run.TargetCompanies)
                    .ThenInclude(company => company.Contacts)
                        .ThenInclude(contact => contact.EvidencePoints)
            .Include(mission => mission.Runs)
                .ThenInclude(run => run.LeadDossiers)
            .OrderBy(mission => mission.CreatedAtUtc)
            .ToList();

        var dataset = new LeadgenLabDataset
        {
            Missions = missions,
            Agents = agents
        };

        _cachedDataset = dataset;
        return dataset;
    }

    public IReadOnlyList<BusinessDnaMission> GetMissions() => GetDataset().Missions;

    public BusinessDnaMission? GetMission(Guid id) => GetDataset().Missions.FirstOrDefault(mission => mission.Id == id);

    public IReadOnlyList<ClarificationQuestion> GetClarificationQuestions() =>
        GetDataset().Missions.SelectMany(mission => mission.ClarificationQuestions).OrderByDescending(question => question.CreatedAtUtc).ToList();

    public ClarificationQuestion? GetClarificationQuestion(Guid id) =>
        GetClarificationQuestions().FirstOrDefault(question => question.Id == id);

    public IReadOnlyList<MissionRun> GetMissionRuns() =>
        GetDataset().Missions.SelectMany(mission => mission.Runs).OrderByDescending(run => run.StartedAtUtc).ToList();

    public MissionRun? GetMissionRun(Guid id) => GetMissionRuns().FirstOrDefault(run => run.Id == id);

    public IReadOnlyList<MissionAgentAssignment> GetMissionAgentAssignments() =>
        GetMissionRuns().SelectMany(run => run.AgentAssignments).OrderByDescending(assignment => assignment.AssignedAtUtc).ToList();

    public MissionAgentAssignment? GetMissionAgentAssignment(Guid id) =>
        GetMissionAgentAssignments().FirstOrDefault(assignment => assignment.Id == id);

    public IReadOnlyList<SwarmAgent> GetSwarmAgents() => GetDataset().Agents;

    public SwarmAgent? GetSwarmAgent(Guid id) => GetDataset().Agents.FirstOrDefault(agent => agent.Id == id);

    public IReadOnlyList<TargetCompany> GetTargetCompanies() =>
        GetMissionRuns().SelectMany(run => run.TargetCompanies).OrderByDescending(company => company.MatchScore).ToList();

    public TargetCompany? GetTargetCompany(Guid id) => GetTargetCompanies().FirstOrDefault(company => company.Id == id);

    public IReadOnlyList<TargetContact> GetTargetContacts() =>
        GetTargetCompanies().SelectMany(company => company.Contacts).OrderBy(contact => contact.FullName).ToList();

    public TargetContact? GetTargetContact(Guid id) => GetTargetContacts().FirstOrDefault(contact => contact.Id == id);

    public IReadOnlyList<ContactChannel> GetContactChannels() =>
        GetTargetContacts().SelectMany(contact => contact.ContactChannels).ToList();

    public ContactChannel? GetContactChannel(Guid id) => GetContactChannels().FirstOrDefault(channel => channel.Id == id);

    public IReadOnlyList<EvidencePoint> GetEvidencePoints() =>
        GetTargetContacts().SelectMany(contact => contact.EvidencePoints).OrderByDescending(evidence => evidence.CapturedAtUtc).ToList();

    public EvidencePoint? GetEvidencePoint(Guid id) => GetEvidencePoints().FirstOrDefault(evidence => evidence.Id == id);

    public IReadOnlyList<LeadDossier> GetLeadDossiers() =>
        GetMissionRuns().SelectMany(run => run.LeadDossiers).OrderByDescending(dossier => dossier.LeadgenScore).ToList();

    public LeadDossier? GetLeadDossier(Guid id) => GetLeadDossiers().FirstOrDefault(dossier => dossier.Id == id);
}
