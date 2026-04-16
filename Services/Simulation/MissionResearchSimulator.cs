// Namespace `Leadgen.Model.Entities` uvozim jer datoteka koristi tipove iz tog prostora imena.
using Leadgen.Model.Entities;

// Namespace `Leadgen.Lab1Runner.Services` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Lab1Runner.Services;

// Klasu `MissionResearchSimulator` definiram kao nositelja podataka ili ponašanja za ovu domensku cjelinu.
public class MissionResearchSimulator
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Ovdje uvodim simuliramo scout fazu swarm istraživanja.
    public async Task<List<TargetCompany>> RunScoutAsync(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        BusinessDnaMission mission,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        CancellationToken cancellationToken = default)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Kratki async delay koristim da simuliram stvaran rad agenta bez vanjskih integracija.
        await Task.Delay(250, cancellationToken);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var run = GetLatestRun(mission);
// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return run?.TargetCompanies
// Where koristim da u nastavak prođu samo zapisi koji zadovoljavaju uvjet.
            .Where(company => company.IsHeadquartersVerified)
// Silazno sortiranje koristim kada želim da najvažniji ili najsvježiji rezultati budu prvi.
            .OrderByDescending(company => company.MatchScore)
// Take koristim kako bih ograničio broj rezultata i zadržao fokusiran demo output.
            .Take(2)
// ToList koristim da materijaliziram upit i dalje radim nad konkretnom listom.
            .ToList() ?? new List<TargetCompany>();
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim simuliramo sentinel fazu koja hvata svježe signale.
    public async Task<List<TargetCompany>> RunSentinelAsync(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        BusinessDnaMission mission,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        CancellationToken cancellationToken = default)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Kratki async delay koristim da simuliram stvaran rad agenta bez vanjskih integracija.
        await Task.Delay(325, cancellationToken);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var run = GetLatestRun(mission);
// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return run?.TargetCompanies
// Where koristim da u nastavak prođu samo zapisi koji zadovoljavaju uvjet.
            .Where(company => company.LastSignalAtUtc.HasValue)
// Silazno sortiranje koristim kada želim da najvažniji ili najsvježiji rezultati budu prvi.
            .OrderByDescending(company => company.LastSignalAtUtc)
// Take koristim kako bih ograničio broj rezultata i zadržao fokusiran demo output.
            .Take(2)
// ToList koristim da materijaliziram upit i dalje radim nad konkretnom listom.
            .ToList() ?? new List<TargetCompany>();
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim simuliramo anchor fazu rezolucije decision makera.
    public async Task<List<TargetContact>> RunAnchorAsync(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        IEnumerable<TargetCompany> companies,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        CancellationToken cancellationToken = default)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Kratki async delay koristim da simuliram stvaran rad agenta bez vanjskih integracija.
        await Task.Delay(220, cancellationToken);

// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return companies
// SelectMany koristim da spljoštim ugniježđene kolekcije i lakše upitam cijeli objektni graf.
            .SelectMany(company => company.Contacts)
// Where koristim da u nastavak prođu samo zapisi koji zadovoljavaju uvjet.
            .Where(contact => contact.IsDecisionMaker)
// GroupBy koristim kada želim računati agregate ili birati predstavnike po grupi.
            .GroupBy(contact => contact.Id)
// Select koristim da podatke projiciram u oblik koji bolje odgovara ispisu ili summary rezultatu.
            .Select(group => group.First())
// Uzlazno sortiranje koristim kada želim determinističan i pregledan redoslijed.
            .OrderBy(contact => contact.FullName)
// ToList koristim da materijaliziram upit i dalje radim nad konkretnom listom.
            .ToList();
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim simuliramo soul fazu izdvajanja kvalifikacijskih dokaza.
    public async Task<List<EvidencePoint>> RunSoulAsync(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        IEnumerable<TargetContact> contacts,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        CancellationToken cancellationToken = default)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Kratki async delay koristim da simuliram stvaran rad agenta bez vanjskih integracija.
        await Task.Delay(260, cancellationToken);

// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return contacts
// SelectMany koristim da spljoštim ugniježđene kolekcije i lakše upitam cijeli objektni graf.
            .SelectMany(contact => contact.EvidencePoints)
// Where koristim da u nastavak prođu samo zapisi koji zadovoljavaju uvjet.
            .Where(evidence => evidence.IsQualificationSignal)
// Silazno sortiranje koristim kada želim da najvažniji ili najsvježiji rezultati budu prvi.
            .OrderByDescending(evidence => evidence.CapturedAtUtc)
// ToList koristim da materijaliziram upit i dalje radim nad konkretnom listom.
            .ToList();
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim uzimamo najnoviji run jer simulator radi nad zadnjim stanjem misije.
    private static MissionRun? GetLatestRun(BusinessDnaMission mission)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return mission.Runs
// Silazno sortiranje koristim kada želim da najvažniji ili najsvježiji rezultati budu prvi.
            .OrderByDescending(run => run.StartedAtUtc)
// FirstOrDefault koristim kada želim prvi podudarni zapis, ali i sigurno ponašanje ako ga nema.
            .FirstOrDefault();
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
