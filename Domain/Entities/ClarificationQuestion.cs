// Namespace `Leadgen.Model.Entities` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Model.Entities;

// Klasu `ClarificationQuestion` definiram kao nositelja podataka ili ponašanja za ovu domensku cjelinu.
public class ClarificationQuestion
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Svojstvo `Id` uvodim kako bih spremio jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
    public Guid Id { get; set; }

// Svojstvo `SlotName` uvodim kako bih spremio naziv intake slota koji pitanje razjašnjava.
    public string SlotName { get; set; } = string.Empty;

// Svojstvo `Prompt` uvodim kako bih spremio tekst pitanja koje treba postaviti korisniku ili operatoru.
    public string Prompt { get; set; } = string.Empty;

// Svojstvo `Reason` uvodim kako bih spremio razlog zašto se pitanje postavlja kako bi nejasnoća bila objašnjena.
    public string Reason { get; set; } = string.Empty;

// Svojstvo `IsAnswered` uvodim kako bih spremio oznaku je li pitanje zatvoreno kako bismo znali blokira li intake.
    public bool IsAnswered { get; set; }

// Svojstvo `Answer` uvodim kako bih spremio stvarni odgovor kada je pitanje riješeno.
    public string? Answer { get; set; }

// Svojstvo `CreatedAtUtc` uvodim kako bih spremio vrijeme kreiranja u UTC-u radi konzistentnog vremenskog praćenja.
    public DateTime CreatedAtUtc { get; set; }

// Svojstvo `AnsweredAtUtc` uvodim kako bih spremio vrijeme odgovora radi audit traga.
    public DateTime? AnsweredAtUtc { get; set; }
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
