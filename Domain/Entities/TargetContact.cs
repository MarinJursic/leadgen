// Namespace `Leadgen.Model.Entities` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Model.Entities;

// Klasu `TargetContact` definiram kao nositelja podataka ili ponašanja za ovu domensku cjelinu.
public class TargetContact
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Svojstvo `Id` uvodim kako bih spremio jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
    public Guid Id { get; set; }

// Svojstvo `FullName` uvodim kako bih spremio puno ime kontakta radi identifikacije.
    public string FullName { get; set; } = string.Empty;

// Svojstvo `JobTitle` uvodim kako bih spremio naziv funkcije kontakta radi kvalifikacije.
    public string JobTitle { get; set; } = string.Empty;

// Svojstvo `Department` uvodim kako bih spremio odjel radi boljeg razumijevanja organizacijske uloge.
    public string Department { get; set; } = string.Empty;

// Svojstvo `Seniority` uvodim kako bih spremio senioritet radi procjene odlučivačke snage.
    public string Seniority { get; set; } = string.Empty;

// Svojstvo `IsDecisionMaker` uvodim kako bih spremio oznaku je li kontakt stvarni decision maker.
    public bool IsDecisionMaker { get; set; }

// Svojstvo `LinkedInUrl` uvodim kako bih spremio LinkedIn profil jer je često ključni B2B signal.
    public string? LinkedInUrl { get; set; }

// Svojstvo `XHandle` uvodim kako bih spremio X identitet kad postoji javni signal na toj mreži.
    public string? XHandle { get; set; }

// Svojstvo `GitHubUsername` uvodim kako bih spremio GitHub identitet za tehničke persone.
    public string? GitHubUsername { get; set; }

// Svojstvo `OpportunitySummary` uvodim kako bih spremio kratki opis zašto je kontakt zanimljiv.
    public string OpportunitySummary { get; set; } = string.Empty;

// Svojstvo `LastObservedAtUtc` uvodim kako bih spremio zadnje opažanje kontakta radi svježine.
    public DateTime LastObservedAtUtc { get; set; }

// Svojstvo `ContactChannels` uvodim kako bih spremio kontaktne kanale kojima se može pristupiti osobi.
    public List<ContactChannel> ContactChannels { get; set; } = new();

// Svojstvo `EvidencePoints` uvodim kako bih spremio dokaze vezane uz taj kontakt.
    public List<EvidencePoint> EvidencePoints { get; set; } = new();
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
