// Namespace `Leadgen.Model.Enums` uvozim jer datoteka koristi tipove iz tog prostora imena.
using Leadgen.Model.Enums;

// Namespace `Leadgen.Model.Entities` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Model.Entities;

// Klasu `BusinessDnaMission` definiram kao nositelja podataka ili ponašanja za ovu domensku cjelinu.
public class BusinessDnaMission
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Svojstvo `Id` uvodim kako bih spremio jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
    public Guid Id { get; set; }

// Svojstvo `MissionName` uvodim kako bih spremio čitljiv naziv misije kako bi se scenarij lako razlikovao u runneru i upitima.
    public string MissionName { get; set; } = string.Empty;

// Svojstvo `ProductName` uvodim kako bih spremio naziv proizvoda koji seed misija predstavlja.
    public string ProductName { get; set; } = string.Empty;

// Svojstvo `Mechanic` uvodim kako bih spremio kratak opis mehanike proizvoda kako bi Business DNA bila konkretna.
    public string Mechanic { get; set; } = string.Empty;

// Svojstvo `PrimarySurface` uvodim kako bih spremio glavnu površinu proizvoda jer utječe na ICP i kasnije grupiranje.
    public string PrimarySurface { get; set; } = string.Empty;

// Svojstvo `SurfaceTags` uvodim kako bih spremio dodatne oznake površine kako bi se nijanse proizvoda zadržale u modelu.
    public List<string> SurfaceTags { get; set; } = new();

// Svojstvo `Persona` uvodim kako bih spremio ciljanu personu jer leadgen kreće od toga kome zapravo prodajemo.
    public string Persona { get; set; } = string.Empty;

// Svojstvo `Villain` uvodim kako bih spremio glavni problem koji proizvod rješava kako bi outreach imao jasan neprijatelj/problem.
    public string Villain { get; set; } = string.Empty;

// Svojstvo `Delta` uvodim kako bih spremio obećanu promjenu vrijednosti koju proizvod donosi.
    public string Delta { get; set; } = string.Empty;

// Svojstvo `ConfidenceScore` uvodim kako bih spremio stupanj sigurnosti kako bismo mogli filtrirati spremnost misije.
    public decimal ConfidenceScore { get; set; }

// Svojstvo `CreatedAtUtc` uvodim kako bih spremio vrijeme kreiranja u UTC-u radi konzistentnog vremenskog praćenja.
    public DateTime CreatedAtUtc { get; set; }

// Svojstvo `Status` uvodim kako bih spremio trenutni status kako bi tijek misije bio eksplicitno modeliran.
    public MissionStatus Status { get; set; }

// Svojstvo `ClarificationQuestions` uvodim kako bih spremio pitanja za razjašnjenje kako bi intake podržao nejasne inpute.
    public List<ClarificationQuestion> ClarificationQuestions { get; set; } = new();

// Svojstvo `Runs` uvodim kako bih spremio kolekciju izvođenja iste misije jer jedna misija može imati više iteracija.
    public List<MissionRun> Runs { get; set; } = new();
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
