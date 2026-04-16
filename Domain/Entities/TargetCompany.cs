// Namespace `Leadgen.Model.Entities` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Model.Entities;

// Klasu `TargetCompany` definiram kao nositelja podataka ili ponašanja za ovu domensku cjelinu.
public class TargetCompany
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Svojstvo `Id` uvodim kako bih spremio jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
    public Guid Id { get; set; }

// Svojstvo `Name` uvodim kako bih spremio naziv kompanije radi identifikacije.
    public string Name { get; set; } = string.Empty;

// Svojstvo `Domain` uvodim kako bih spremio web domenu kompanije za identitet i outreach kontekst.
    public string Domain { get; set; } = string.Empty;

// Svojstvo `Industry` uvodim kako bih spremio industriju radi segmentacije.
    public string Industry { get; set; } = string.Empty;

// Svojstvo `HeadquartersCity` uvodim kako bih spremio grad sjedišta kao dio firmografije.
    public string HeadquartersCity { get; set; } = string.Empty;

// Svojstvo `HeadquartersCountry` uvodim kako bih spremio državu sjedišta radi regije i lokalizacije.
    public string HeadquartersCountry { get; set; } = string.Empty;

// Svojstvo `OrganizationStageLabel` uvodim kako bih spremio oznaku faze razvoja kompanije radi segmentacije.
    public string? OrganizationStageLabel { get; set; }

// Svojstvo `LastSignalAtUtc` uvodim kako bih spremio zadnji uočeni signal kako bismo znali koliko je kompanija svježa.
    public DateTime? LastSignalAtUtc { get; set; }

// Svojstvo `EmployeeCount` uvodim kako bih spremio broj zaposlenih radi procjene veličine.
    public int EmployeeCount { get; set; }

// Svojstvo `IsHeadquartersVerified` uvodim kako bih spremio oznaku da je sjedište potvrđeno.
    public bool IsHeadquartersVerified { get; set; }

// Svojstvo `MatchScore` uvodim kako bih spremio score podudaranja kompanije s misijom.
    public decimal MatchScore { get; set; }

// Svojstvo `Contacts` uvodim kako bih spremio kontakte unutar kompanije kako bismo modelirali buying committee.
    public List<TargetContact> Contacts { get; set; } = new();
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
