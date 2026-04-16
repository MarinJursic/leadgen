using Leadgen.Lab1Runner.Seed;
using Leadgen.Model.Entities;

namespace leadgen.Services;

public sealed class LeadgenMockRepository : ILeadgenReadRepository
{
    private readonly LeadgenLabDataset _dataset;

    public LeadgenMockRepository()
    {
        _dataset = LeadgenSeedFactory.Create();
    }

    public LeadgenLabDataset GetDataset()
    {
        return _dataset;
    }

    public IReadOnlyList<BusinessDnaMission> GetMissions()
    {
        return _dataset.Missions;
    }

    public BusinessDnaMission? GetMission(Guid id)
    {
        return _dataset.Missions.FirstOrDefault(mission => mission.Id == id);
    }

    public IReadOnlyList<ClarificationQuestion> GetClarificationQuestions()
    {
        return _dataset.Missions.SelectMany(mission => mission.ClarificationQuestions).ToList();
    }

    public ClarificationQuestion? GetClarificationQuestion(Guid id)
    {
        return GetClarificationQuestions().FirstOrDefault(question => question.Id == id);
    }

    public IReadOnlyList<MissionRun> GetMissionRuns()
    {
        return _dataset.Missions.SelectMany(mission => mission.Runs).ToList();
    }

    public MissionRun? GetMissionRun(Guid id)
    {
        return GetMissionRuns().FirstOrDefault(run => run.Id == id);
    }

    public IReadOnlyList<MissionAgentAssignment> GetMissionAgentAssignments()
    {
        return GetMissionRuns().SelectMany(run => run.AgentAssignments).ToList();
    }

    public MissionAgentAssignment? GetMissionAgentAssignment(Guid id)
    {
        return GetMissionAgentAssignments().FirstOrDefault(assignment => assignment.Id == id);
    }

    public IReadOnlyList<SwarmAgent> GetSwarmAgents()
    {
        return _dataset.Agents;
    }

    public SwarmAgent? GetSwarmAgent(Guid id)
    {
        return _dataset.Agents.FirstOrDefault(agent => agent.Id == id);
    }

    public IReadOnlyList<TargetCompany> GetTargetCompanies()
    {
        return GetMissionRuns().SelectMany(run => run.TargetCompanies).ToList();
    }

    public TargetCompany? GetTargetCompany(Guid id)
    {
        return GetTargetCompanies().FirstOrDefault(company => company.Id == id);
    }

    public IReadOnlyList<TargetContact> GetTargetContacts()
    {
        return GetTargetCompanies().SelectMany(company => company.Contacts).ToList();
    }

    public TargetContact? GetTargetContact(Guid id)
    {
        return GetTargetContacts().FirstOrDefault(contact => contact.Id == id);
    }

    public IReadOnlyList<ContactChannel> GetContactChannels()
    {
        return GetTargetContacts().SelectMany(contact => contact.ContactChannels).ToList();
    }

    public ContactChannel? GetContactChannel(Guid id)
    {
        return GetContactChannels().FirstOrDefault(channel => channel.Id == id);
    }

    public IReadOnlyList<EvidencePoint> GetEvidencePoints()
    {
        return GetTargetContacts().SelectMany(contact => contact.EvidencePoints).ToList();
    }

    public EvidencePoint? GetEvidencePoint(Guid id)
    {
        return GetEvidencePoints().FirstOrDefault(evidence => evidence.Id == id);
    }

    public IReadOnlyList<LeadDossier> GetLeadDossiers()
    {
        return GetMissionRuns().SelectMany(run => run.LeadDossiers).ToList();
    }

    public LeadDossier? GetLeadDossier(Guid id)
    {
        return GetLeadDossiers().FirstOrDefault(dossier => dossier.Id == id);
    }
}
