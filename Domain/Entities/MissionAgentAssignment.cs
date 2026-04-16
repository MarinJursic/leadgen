// Namespace `Leadgen.Model.Enums` uvozim jer datoteka koristi tipove iz tog prostora imena.
using Leadgen.Model.Enums;

// Namespace `Leadgen.Model.Entities` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Model.Entities;

// Klasu `MissionAgentAssignment` definiram kao nositelja podataka ili ponašanja za ovu domensku cjelinu.
public class MissionAgentAssignment
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Svojstvo `Id` uvodim kako bih spremio jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
    public Guid Id { get; set; }

// Svojstvo `MissionRunId` uvodim kako bih spremio vezu na konkretno izvođenje misije.
    public Guid MissionRunId { get; set; }

// Svojstvo `SwarmAgentId` uvodim kako bih spremio vezu na agenta kojem je zadatak dodijeljen.
    public Guid SwarmAgentId { get; set; }

// Svojstvo `AssignedAtUtc` uvodim kako bih spremio vrijeme dodjele zadatka.
    public DateTime AssignedAtUtc { get; set; }

// Svojstvo `Responsibility` uvodim kako bih spremio opis odgovornosti agenta unutar runa.
    public string Responsibility { get; set; } = string.Empty;

// Svojstvo `TokenBudget` uvodim kako bih spremio budžet tokena kako bismo mogli pratiti trošak i kapacitet.
    public int TokenBudget { get; set; }

// Svojstvo `Status` uvodim kako bih spremio trenutni status kako bi tijek misije bio eksplicitno modeliran.
    public MissionStatus Status { get; set; }
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
