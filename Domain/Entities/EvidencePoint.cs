// Namespace `Leadgen.Model.Enums` uvozim jer datoteka koristi tipove iz tog prostora imena.
using Leadgen.Model.Enums;

// Namespace `Leadgen.Model.Entities` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Model.Entities;

// Klasu `EvidencePoint` definiram kao nositelja podataka ili ponašanja za ovu domensku cjelinu.
public class EvidencePoint
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Svojstvo `Id` uvodim kako bih spremio jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
    public Guid Id { get; set; }

// Svojstvo `Kind` uvodim kako bih spremio vrstu evidence točke kako bi se mogla grupirati i filtrirati.
    public EvidenceKind Kind { get; set; }

// Svojstvo `Label` uvodim kako bih spremio kratku etiketu dokaza radi čitljivog ispisa i grupiranja.
    public string Label { get; set; } = string.Empty;

// Svojstvo `SourcePlatform` uvodim kako bih spremio platformu s koje dokaz dolazi.
    public string SourcePlatform { get; set; } = string.Empty;

// Svojstvo `SourceUrl` uvodim kako bih spremio direktni URL izvora kako bi dokaz ostao provjerljiv.
    public string SourceUrl { get; set; } = string.Empty;

// Svojstvo `Summary` uvodim kako bih spremio sažetak dokaza za brzo razumijevanje bez otvaranja izvora.
    public string Summary { get; set; } = string.Empty;

// Svojstvo `RawSnippet` uvodim kako bih spremio sirovi izvadak jer često trebamo citat ili originalni signal.
    public string RawSnippet { get; set; } = string.Empty;

// Svojstvo `CapturedAtUtc` uvodim kako bih spremio vrijeme hvatanja dokaza radi recentnosti.
    public DateTime CapturedAtUtc { get; set; }

// Svojstvo `ConfidenceScore` uvodim kako bih spremio stupanj sigurnosti kako bismo mogli filtrirati spremnost misije.
    public decimal ConfidenceScore { get; set; }

// Svojstvo `IsQualificationSignal` uvodim kako bih spremio oznaku služi li dokaz za kvalifikaciju leada.
    public bool IsQualificationSignal { get; set; }
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
