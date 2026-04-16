// Namespace `Leadgen.Model.Enums` uvozim jer datoteka koristi tipove iz tog prostora imena.
using Leadgen.Model.Enums;

// Namespace `Leadgen.Model.Entities` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Model.Entities;

// Klasu `SwarmAgent` definiram kao nositelja podataka ili ponašanja za ovu domensku cjelinu.
public class SwarmAgent
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Svojstvo `Id` uvodim kako bih spremio jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
    public Guid Id { get; set; }

// Svojstvo `CodeName` uvodim kako bih spremio pozivni naziv agenta radi čitljivog operativnog identiteta.
    public string CodeName { get; set; } = string.Empty;

// Svojstvo `Role` uvodim kako bih spremio specijaliziranu ulogu agenta kako bi swarm bio podijeljen po funkcijama.
    public AgentRole Role { get; set; }

// Svojstvo `Provider` uvodim kako bih spremio model/provider sloj koji agenta pogoni.
    public string Provider { get; set; } = string.Empty;

// Svojstvo `Temperature` uvodim kako bih spremio temperaturu modela kako bismo sugerirali stil i varijabilnost rada.
    public decimal Temperature { get; set; }

// Svojstvo `MaxConcurrentTasks` uvodim kako bih spremio maksimalan broj paralelnih zadataka koje agent nosi.
    public int MaxConcurrentTasks { get; set; }

// Svojstvo `IsActive` uvodim kako bih spremio oznaku je li agent aktivan u trenutnom datasetu.
    public bool IsActive { get; set; }

// Svojstvo `LastHeartbeatUtc` uvodim kako bih spremio zadnji heartbeat radi simulacije živog sustava.
    public DateTime LastHeartbeatUtc { get; set; }

// Svojstvo `CurrentFocus` uvodim kako bih spremio trenutni fokus agenta kako bi runner imao operativni kontekst.
    public string CurrentFocus { get; set; } = string.Empty;

// Svojstvo `MissionAssignments` uvodim kako bih spremio dodjele misija koje agent trenutno ili povijesno ima.
    public List<MissionAgentAssignment> MissionAssignments { get; set; } = new();
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
