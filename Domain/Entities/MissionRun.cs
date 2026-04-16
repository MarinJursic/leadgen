// Namespace `Leadgen.Model.Enums` uvozim jer datoteka koristi tipove iz tog prostora imena.
using Leadgen.Model.Enums;

// Namespace `Leadgen.Model.Entities` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Model.Entities;

// Klasu `MissionRun` definiram kao nositelja podataka ili ponašanja za ovu domensku cjelinu.
public class MissionRun
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Svojstvo `Id` uvodim kako bih spremio jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
    public Guid Id { get; set; }

// Svojstvo `RunCode` uvodim kako bih spremio čitljivi kod izvođenja radi praćenja u logovima i ispisima.
    public string RunCode { get; set; } = string.Empty;

// Svojstvo `BusinessDnaMissionId` uvodim kako bih spremio vezu natrag na izvornu misiju.
    public Guid BusinessDnaMissionId { get; set; }

// Svojstvo `StartedAtUtc` uvodim kako bih spremio vrijeme početka izvođenja.
    public DateTime StartedAtUtc { get; set; }

// Svojstvo `CompletedAtUtc` uvodim kako bih spremio vrijeme završetka ako je run dovršen.
    public DateTime? CompletedAtUtc { get; set; }

// Svojstvo `Status` uvodim kako bih spremio trenutni status kako bi tijek misije bio eksplicitno modeliran.
    public MissionStatus Status { get; set; }

// Svojstvo `SearchRegion` uvodim kako bih spremio regiju pretrage jer tržište i signal razlikuju rezultate.
    public string SearchRegion { get; set; } = string.Empty;

// Svojstvo `TokenBudget` uvodim kako bih spremio budžet tokena kako bismo mogli pratiti trošak i kapacitet.
    public int TokenBudget { get; set; }

// Svojstvo `EstimatedCostUsd` uvodim kako bih spremio procijenjeni trošak izvođenja u USD za demo ekonomike.
    public decimal EstimatedCostUsd { get; set; }

// Svojstvo `AgentAssignments` uvodim kako bih spremio dodjele agenata kako bi N:N odnos bio eksplicitno modeliran.
    public List<MissionAgentAssignment> AgentAssignments { get; set; } = new();

// Svojstvo `TargetCompanies` uvodim kako bih spremio kandidate kompanija pronađene u runu.
    public List<TargetCompany> TargetCompanies { get; set; } = new();

// Svojstvo `LeadDossiers` uvodim kako bih spremio konačne dossier zapise koji izlaze iz istraživanja.
    public List<LeadDossier> LeadDossiers { get; set; } = new();
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
