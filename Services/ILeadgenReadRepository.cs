using Leadgen.Lab1Runner.Seed;
using Leadgen.Model.Entities;

namespace leadgen.Services;

public interface ILeadgenReadRepository
{
    LeadgenLabDataset GetDataset();

    IReadOnlyList<BusinessDnaMission> GetMissions();

    BusinessDnaMission? GetMission(Guid id);

    IReadOnlyList<ClarificationQuestion> GetClarificationQuestions();

    ClarificationQuestion? GetClarificationQuestion(Guid id);

    IReadOnlyList<MissionRun> GetMissionRuns();

    MissionRun? GetMissionRun(Guid id);

    IReadOnlyList<MissionAgentAssignment> GetMissionAgentAssignments();

    MissionAgentAssignment? GetMissionAgentAssignment(Guid id);

    IReadOnlyList<SwarmAgent> GetSwarmAgents();

    SwarmAgent? GetSwarmAgent(Guid id);

    IReadOnlyList<TargetCompany> GetTargetCompanies();

    TargetCompany? GetTargetCompany(Guid id);

    IReadOnlyList<TargetContact> GetTargetContacts();

    TargetContact? GetTargetContact(Guid id);

    IReadOnlyList<ContactChannel> GetContactChannels();

    ContactChannel? GetContactChannel(Guid id);

    IReadOnlyList<EvidencePoint> GetEvidencePoints();

    EvidencePoint? GetEvidencePoint(Guid id);

    IReadOnlyList<LeadDossier> GetLeadDossiers();

    LeadDossier? GetLeadDossier(Guid id);
}
