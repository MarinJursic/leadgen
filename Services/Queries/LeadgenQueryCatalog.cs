// Namespace `Leadgen.Lab1Runner.Seed` uvozim jer datoteka koristi tipove iz tog prostora imena.
using Leadgen.Lab1Runner.Seed;
// Namespace `Leadgen.Model.Entities` uvozim jer datoteka koristi tipove iz tog prostora imena.
using Leadgen.Model.Entities;
// Namespace `Leadgen.Model.Enums` uvozim jer datoteka koristi tipove iz tog prostora imena.
using Leadgen.Model.Enums;

// Namespace `Leadgen.Lab1Runner.Queries` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Lab1Runner.Queries;

// Statičku klasu `LeadgenQueryCatalog` koristim jer služi kao katalog pomoćnih funkcija bez potrebe za instanciranjem.
public static class LeadgenQueryCatalog
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Ovdje uvodim prikazujemo kako filtrirati misije po confidence pragu.
    public static IEnumerable<MissionReadinessSummary> GetMissionsBelowConfidenceThreshold(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        LeadgenLabDataset dataset,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        decimal threshold = 0.80m)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return dataset.Missions
// Where koristim da u nastavak prođu samo zapisi koji zadovoljavaju uvjet.
            .Where(mission => mission.ConfidenceScore < threshold)
// Uzlazno sortiranje koristim kada želim determinističan i pregledan redoslijed.
            .OrderBy(mission => mission.ConfidenceScore)
// Select koristim da podatke projiciram u oblik koji bolje odgovara ispisu ili summary rezultatu.
            .Select(mission => new MissionReadinessSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                mission.MissionName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                mission.ConfidenceScore,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                mission.Status,
// Count koristim kada je količina sama po sebi važna metrika za ispis ili odluku.
                mission.ClarificationQuestions.Count(question => !question.IsAnswered)));
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim grupiramo otvorena pitanja po slotu.
    public static IEnumerable<SlotQuestionSummary> GetUnansweredClarificationQuestionsBySlot(LeadgenLabDataset dataset)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return dataset.Missions
// SelectMany koristim da spljoštim ugniježđene kolekcije i lakše upitam cijeli objektni graf.
            .SelectMany(mission => mission.ClarificationQuestions)
// Where koristim da u nastavak prođu samo zapisi koji zadovoljavaju uvjet.
            .Where(question => !question.IsAnswered)
// GroupBy koristim kada želim računati agregate ili birati predstavnike po grupi.
            .GroupBy(question => question.SlotName)
// Silazno sortiranje koristim kada želim da najvažniji ili najsvježiji rezultati budu prvi.
            .OrderByDescending(group => group.Count())
// Sekundarni kriterij sortiranja dodajem kako bi redoslijed ostao stabilan među jednakim vrijednostima.
            .ThenBy(group => group.Key)
// Select koristim da podatke projiciram u oblik koji bolje odgovara ispisu ili summary rezultatu.
            .Select(group => new SlotQuestionSummary(group.Key, group.Count()));
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim računamo workload agenata po ulozi.
    public static IEnumerable<AgentWorkloadSummary> GetAgentWorkloadByRole(LeadgenLabDataset dataset)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var agentsById = dataset.Agents.ToDictionary(agent => agent.Id);

// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return dataset.Missions
// SelectMany koristim da spljoštim ugniježđene kolekcije i lakše upitam cijeli objektni graf.
            .SelectMany(mission => mission.Runs)
// SelectMany koristim da spljoštim ugniježđene kolekcije i lakše upitam cijeli objektni graf.
            .SelectMany(run => run.AgentAssignments)
// Where koristim da u nastavak prođu samo zapisi koji zadovoljavaju uvjet.
            .Where(assignment => agentsById.TryGetValue(assignment.SwarmAgentId, out var agent) && agent.IsActive)
// GroupBy koristim kada želim računati agregate ili birati predstavnike po grupi.
            .GroupBy(assignment => agentsById[assignment.SwarmAgentId].Role)
// Silazno sortiranje koristim kada želim da najvažniji ili najsvježiji rezultati budu prvi.
            .OrderByDescending(group => group.Count())
// Sekundarni kriterij sortiranja dodajem kako bi redoslijed ostao stabilan među jednakim vrijednostima.
            .ThenBy(group => group.Key)
// Select koristim da podatke projiciram u oblik koji bolje odgovara ispisu ili summary rezultatu.
            .Select(group => new AgentWorkloadSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                group.Key,
// Count koristim kada je količina sama po sebi važna metrika za ispis ili odluku.
                group.Count(),
// Sum koristim kako bih dobio agregatnu metriku poput ukupnog budžeta.
                group.Sum(assignment => assignment.TokenBudget)));
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim izdvajamo kompanije s najboljim fitom.
    public static IEnumerable<CompanyFitSummary> GetBestFitCompanies(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        LeadgenLabDataset dataset,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        decimal minimumMatchScore = 0.85m,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        int take = 5)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return EnumerateCompanies(dataset)
// Where koristim da u nastavak prođu samo zapisi koji zadovoljavaju uvjet.
            .Where(item => item.Company.IsHeadquartersVerified && item.Company.MatchScore >= minimumMatchScore)
// Silazno sortiranje koristim kada želim da najvažniji ili najsvježiji rezultati budu prvi.
            .OrderByDescending(item => item.Company.MatchScore)
// Take koristim kako bih ograničio broj rezultata i zadržao fokusiran demo output.
            .Take(take)
// Select koristim da podatke projiciram u oblik koji bolje odgovara ispisu ili summary rezultatu.
            .Select(item => new CompanyFitSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Mission.MissionName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Company.Name,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Company.MatchScore,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                $"{item.Company.HeadquartersCity}, {item.Company.HeadquartersCountry}",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Company.OrganizationStageLabel ?? "Unspecified"));
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim nalazimo kontakte spremne za outreach.
    public static IEnumerable<OutreachReadyContactSummary> GetOutreachReadyContacts(LeadgenLabDataset dataset)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return EnumerateContacts(dataset)
// Select koristim da podatke projiciram u oblik koji bolje odgovara ispisu ili summary rezultatu.
            .Select(item => new
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
            {
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Mission,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Company,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Contact,
// Count koristim kada je količina sama po sebi važna metrika za ispis ili odluku.
                VerifiedChannels = item.Contact.ContactChannels.Count(channel =>
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                    channel.IsVerified &&
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                    (channel.Type == ContactChannelType.WorkEmail || channel.Type == ContactChannelType.Phone)),
// Count koristim kada je količina sama po sebi važna metrika za ispis ili odluku.
                QualificationSignals = item.Contact.EvidencePoints.Count(evidence => evidence.IsQualificationSignal)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            })
// Where koristim da u nastavak prođu samo zapisi koji zadovoljavaju uvjet.
            .Where(item => item.VerifiedChannels > 0 && item.QualificationSignals > 0)
// Silazno sortiranje koristim kada želim da najvažniji ili najsvježiji rezultati budu prvi.
            .OrderByDescending(item => item.QualificationSignals)
// Sekundarni kriterij sortiranja dodajem kako bi redoslijed ostao stabilan među jednakim vrijednostima.
            .ThenByDescending(item => item.VerifiedChannels)
// Select koristim da podatke projiciram u oblik koji bolje odgovara ispisu ili summary rezultatu.
            .Select(item => new OutreachReadyContactSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Mission.MissionName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Company.Name,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Contact.FullName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Contact.JobTitle,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.VerifiedChannels,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.QualificationSignals));
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim biramo najbolji dossier po misiji.
    public static IEnumerable<TopDossierSummary> GetTopDossierByMission(LeadgenLabDataset dataset)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return EnumerateDossiers(dataset)
// GroupBy koristim kada želim računati agregate ili birati predstavnike po grupi.
            .GroupBy(item => item.Mission.Id)
// Silazno sortiranje koristim kada želim da najvažniji ili najsvježiji rezultati budu prvi.
            .Select(group => group.OrderByDescending(item => item.Dossier.LeadgenScore).First())
// Silazno sortiranje koristim kada želim da najvažniji ili najsvježiji rezultati budu prvi.
            .OrderByDescending(item => item.Dossier.LeadgenScore)
// Select koristim da podatke projiciram u oblik koji bolje odgovara ispisu ili summary rezultatu.
            .Select(item => new TopDossierSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Mission.MissionName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Company.Name,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Contact.FullName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Dossier.LeadgenScore,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Dossier.AdvantagePoint));
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim pokazujemo raspodjelu evidence signala.
    public static IEnumerable<EvidenceDistributionSummary> GetEvidenceDistribution(LeadgenLabDataset dataset)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return EnumerateEvidence(dataset)
// GroupBy koristim kada želim računati agregate ili birati predstavnike po grupi.
            .GroupBy(item => new { item.Evidence.Kind, item.Evidence.Label, item.Evidence.SourcePlatform })
// Silazno sortiranje koristim kada želim da najvažniji ili najsvježiji rezultati budu prvi.
            .OrderByDescending(group => group.Count())
// Sekundarni kriterij sortiranja dodajem kako bi redoslijed ostao stabilan među jednakim vrijednostima.
            .ThenBy(group => group.Key.Kind)
// Sekundarni kriterij sortiranja dodajem kako bi redoslijed ostao stabilan među jednakim vrijednostima.
            .ThenBy(group => group.Key.Label)
// Select koristim da podatke projiciram u oblik koji bolje odgovara ispisu ili summary rezultatu.
            .Select(group => new EvidenceDistributionSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                group.Key.Kind,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                group.Key.Label,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                group.Key.SourcePlatform,
// Count koristim kada je količina sama po sebi važna metrika za ispis ili odluku.
                group.Count()));
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim otkrivamo jake leadove kojima nedostaje ključan kanal.
    public static IEnumerable<MissingChannelSummary> GetHighScoreLeadsMissingKeyChannels(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        LeadgenLabDataset dataset,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        int minimumLeadScore = 8)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return EnumerateDossiers(dataset)
// Where koristim da u nastavak prođu samo zapisi koji zadovoljavaju uvjet.
            .Where(item => item.Dossier.LeadgenScore >= minimumLeadScore)
// SelectMany koristim da spljoštim ugniježđene kolekcije i lakše upitam cijeli objektni graf.
            .SelectMany(item =>
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
            {
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
                var missingChannels = new List<string>();

// Uvjet uvodim kako bih ograničio tok izvršavanja samo na slučajeve koji zadovoljavaju traženo pravilo.
                if (!item.Contact.ContactChannels.Any(channel => channel.IsVerified && channel.Type == ContactChannelType.WorkEmail))
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
                {
// Add koristim kada želim proširiti postojeću kolekciju jednim novim elementom.
                    missingChannels.Add("Verified work email");
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
                }

// Uvjet uvodim kako bih ograničio tok izvršavanja samo na slučajeve koji zadovoljavaju traženo pravilo.
                if (!item.Contact.ContactChannels.Any(channel => channel.IsVerified && channel.Type == ContactChannelType.Phone))
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
                {
// Add koristim kada želim proširiti postojeću kolekciju jednim novim elementom.
                    missingChannels.Add("Verified phone");
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
                }

// Ovdje vraćam novokreirani objekt jer metoda služi kao centralizirani konstruktor ili projekcija podataka.
                return missingChannels.Select(missingChannel => new MissingChannelSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                    item.Mission.MissionName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                    item.Company.Name,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                    item.Contact.FullName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                    item.Dossier.LeadgenScore,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                    missingChannel));
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            })
// Silazno sortiranje koristim kada želim da najvažniji ili najsvježiji rezultati budu prvi.
            .OrderByDescending(item => item.LeadgenScore)
// Sekundarni kriterij sortiranja dodajem kako bi redoslijed ostao stabilan među jednakim vrijednostima.
            .ThenBy(item => item.ContactName);
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim izdvajamo svježe kvalifikacijske signale.
    public static IEnumerable<RecentSignalSummary> GetRecentSignals(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        LeadgenLabDataset dataset,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        DateTime utcNow,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        int days = 30)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var cutoff = utcNow.AddDays(-days);

// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return EnumerateEvidence(dataset)
// Where koristim da u nastavak prođu samo zapisi koji zadovoljavaju uvjet.
            .Where(item => item.Evidence.IsQualificationSignal && item.Evidence.CapturedAtUtc >= cutoff)
// Silazno sortiranje koristim kada želim da najvažniji ili najsvježiji rezultati budu prvi.
            .OrderByDescending(item => item.Evidence.CapturedAtUtc)
// Select koristim da podatke projiciram u oblik koji bolje odgovara ispisu ili summary rezultatu.
            .Select(item => new RecentSignalSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Mission.MissionName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Company.Name,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Contact.FullName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Evidence.Label,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Evidence.SourcePlatform,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                item.Evidence.CapturedAtUtc));
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim računamo prosjek scorea po glavnoj površini proizvoda.
    public static IEnumerable<AverageLeadScoreSummary> GetAverageLeadScoreByPrimarySurface(LeadgenLabDataset dataset)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return EnumerateDossiers(dataset)
// GroupBy koristim kada želim računati agregate ili birati predstavnike po grupi.
            .GroupBy(item => item.Mission.PrimarySurface)
// Silazno sortiranje koristim kada želim da najvažniji ili najsvježiji rezultati budu prvi.
            .OrderByDescending(group => group.Average(item => item.Dossier.LeadgenScore))
// Select koristim da podatke projiciram u oblik koji bolje odgovara ispisu ili summary rezultatu.
            .Select(group => new AverageLeadScoreSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                group.Key,
// Average koristim kada trebam reprezentativnu vrijednost za cijelu grupu.
                Math.Round(group.Average(item => item.Dossier.LeadgenScore), 2),
// Count koristim kada je količina sama po sebi važna metrika za ispis ili odluku.
                group.Count()));
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim normaliziramo prolaz kroz kompanije iz dubokog objektnog grafa.
    private static IEnumerable<(BusinessDnaMission Mission, MissionRun Run, TargetCompany Company)> EnumerateCompanies(LeadgenLabDataset dataset)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return dataset.Missions
// SelectMany koristim da spljoštim ugniježđene kolekcije i lakše upitam cijeli objektni graf.
            .SelectMany(mission => mission.Runs.SelectMany(run => run.TargetCompanies.Select(company => (mission, run, company))));
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim normaliziramo prolaz kroz kontakte.
    private static IEnumerable<(BusinessDnaMission Mission, MissionRun Run, TargetCompany Company, TargetContact Contact)> EnumerateContacts(LeadgenLabDataset dataset)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return EnumerateCompanies(dataset)
// SelectMany koristim da spljoštim ugniježđene kolekcije i lakše upitam cijeli objektni graf.
            .SelectMany(item => item.Company.Contacts.Select(contact => (item.Mission, item.Run, item.Company, contact)));
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim normaliziramo prolaz kroz dosjee i njihove povezane entitete.
    private static IEnumerable<(BusinessDnaMission Mission, MissionRun Run, TargetCompany Company, TargetContact Contact, LeadDossier Dossier)> EnumerateDossiers(LeadgenLabDataset dataset)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Petlju koristim kada svaki element kolekcije treba obraditi ili ispisati.
        foreach (var mission in dataset.Missions)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        {
// Petlju koristim kada svaki element kolekcije treba obraditi ili ispisati.
            foreach (var run in mission.Runs)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
            {
// Petlju koristim kada svaki element kolekcije treba obraditi ili ispisati.
                foreach (var dossier in run.LeadDossiers)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
                {
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
                    var company = run.TargetCompanies.FirstOrDefault(targetCompany => targetCompany.Id == dossier.TargetCompanyId);
// Uvjet uvodim kako bih ograničio tok izvršavanja samo na slučajeve koji zadovoljavaju traženo pravilo.
                    if (company is null)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
                    {
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                        continue;
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
                    }

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
                    var contact = company.Contacts.FirstOrDefault(targetContact => targetContact.Id == dossier.TargetContactId);
// Uvjet uvodim kako bih ograničio tok izvršavanja samo na slučajeve koji zadovoljavaju traženo pravilo.
                    if (contact is null)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
                    {
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
                        continue;
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
                    }

// Yield return koristim kako bih rezultate vraćao lijeno bez gradnje dodatne privremene liste.
                    yield return (mission, run, company, contact, dossier);
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
                }
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
            }
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
        }
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim normaliziramo prolaz kroz evidence točke.
    private static IEnumerable<(BusinessDnaMission Mission, TargetCompany Company, TargetContact Contact, EvidencePoint Evidence)> EnumerateEvidence(LeadgenLabDataset dataset)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return EnumerateContacts(dataset)
// SelectMany koristim da spljoštim ugniježđene kolekcije i lakše upitam cijeli objektni graf.
            .SelectMany(item => item.Contact.EvidencePoints.Select(evidence => (item.Mission, item.Company, item.Contact, evidence)));
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}

// Record `MissionReadinessSummary` koristim jer je to lagan i čitljiv DTO za rezultate LINQ upita.
public sealed record MissionReadinessSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string MissionName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    decimal ConfidenceScore,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    MissionStatus Status,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    int UnansweredQuestions);

// Record `SlotQuestionSummary` koristim jer je to lagan i čitljiv DTO za rezultate LINQ upita.
public sealed record SlotQuestionSummary(string SlotName, int QuestionCount);

// Record `AgentWorkloadSummary` koristim jer je to lagan i čitljiv DTO za rezultate LINQ upita.
public sealed record AgentWorkloadSummary(AgentRole Role, int AssignmentCount, int TotalTokenBudget);

// Record `CompanyFitSummary` koristim jer je to lagan i čitljiv DTO za rezultate LINQ upita.
public sealed record CompanyFitSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string MissionName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string CompanyName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    decimal MatchScore,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string Headquarters,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string StageLabel);

// Record `OutreachReadyContactSummary` koristim jer je to lagan i čitljiv DTO za rezultate LINQ upita.
public sealed record OutreachReadyContactSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string MissionName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string CompanyName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string ContactName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string JobTitle,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    int VerifiedChannels,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    int QualificationSignals);

// Record `TopDossierSummary` koristim jer je to lagan i čitljiv DTO za rezultate LINQ upita.
public sealed record TopDossierSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string MissionName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string CompanyName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string ContactName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    int LeadgenScore,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string AdvantagePoint);

// Record `EvidenceDistributionSummary` koristim jer je to lagan i čitljiv DTO za rezultate LINQ upita.
public sealed record EvidenceDistributionSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    EvidenceKind Kind,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string Label,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string SourcePlatform,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    int Count);

// Record `MissingChannelSummary` koristim jer je to lagan i čitljiv DTO za rezultate LINQ upita.
public sealed record MissingChannelSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string MissionName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string CompanyName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string ContactName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    int LeadgenScore,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string MissingChannel);

// Record `RecentSignalSummary` koristim jer je to lagan i čitljiv DTO za rezultate LINQ upita.
public sealed record RecentSignalSummary(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string MissionName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string CompanyName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string ContactName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string Label,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    string SourcePlatform,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
    DateTime CapturedAtUtc);

// Record `AverageLeadScoreSummary` koristim jer je to lagan i čitljiv DTO za rezultate LINQ upita.
public sealed record AverageLeadScoreSummary(string GroupKey, double AverageScore, int DossierCount);
