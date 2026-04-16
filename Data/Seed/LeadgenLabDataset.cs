// Namespace `Leadgen.Model.Entities` uvozim jer datoteka koristi tipove iz tog prostora imena.
using Leadgen.Model.Entities;

// Namespace `Leadgen.Lab1Runner.Seed` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Lab1Runner.Seed;

// Klasu `LeadgenLabDataset` definiram kao nositelja podataka ili ponašanja za ovu domensku cjelinu.
public class LeadgenLabDataset
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Ovdje uvodim metodu `new` kojom kapsuliram ponovljivu ili smisleno odvojenu logiku.
    public List<BusinessDnaMission> Missions { get; init; } = new();

// Ovdje uvodim metodu `new` kojom kapsuliram ponovljivu ili smisleno odvojenu logiku.
    public List<SwarmAgent> Agents { get; init; } = new();
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
