// Namespace `Leadgen.Model.Entities` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Model.Entities;

// Klasu `LeadDossier` definiram kao nositelja podataka ili ponašanja za ovu domensku cjelinu.
public class LeadDossier
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Svojstvo `Id` uvodim kako bih spremio jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
    public Guid Id { get; set; }

// Svojstvo `MissionRunId` uvodim kako bih spremio vezu na konkretno izvođenje misije.
    public Guid MissionRunId { get; set; }

// Svojstvo `TargetCompanyId` uvodim kako bih spremio vezu na target kompaniju.
    public Guid TargetCompanyId { get; set; }

// Svojstvo `TargetContactId` uvodim kako bih spremio vezu na target kontakt.
    public Guid TargetContactId { get; set; }

// Svojstvo `LeadgenScore` uvodim kako bih spremio prioritetni score kako bismo mogli rangirati leadove.
    public int LeadgenScore { get; set; }

// Svojstvo `SuggestedApproach` uvodim kako bih spremio predloženi outreach pristup temeljen na dokazima.
    public string SuggestedApproach { get; set; } = string.Empty;

// Svojstvo `AdvantagePoint` uvodim kako bih spremio ključnu prednost ili ulaznu točku za outreach.
    public string AdvantagePoint { get; set; } = string.Empty;

// Svojstvo `IsReadyForOutreach` uvodim kako bih spremio oznaku je li dossier dovoljno jak za kontaktiranje.
    public bool IsReadyForOutreach { get; set; }

// Svojstvo `CreatedAtUtc` uvodim kako bih spremio vrijeme kreiranja u UTC-u radi konzistentnog vremenskog praćenja.
    public DateTime CreatedAtUtc { get; set; }

// Svojstvo `LastUpdatedAtUtc` uvodim kako bih spremio vrijeme zadnje izmjene radi praćenja svježine.
    public DateTime LastUpdatedAtUtc { get; set; }

// Svojstvo `SupportingEvidenceCount` uvodim kako bih spremio broj dokaza koji podupiru dossier.
    public int SupportingEvidenceCount { get; set; }
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
