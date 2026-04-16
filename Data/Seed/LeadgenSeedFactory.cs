// Namespace `Leadgen.Model.Entities` uvozim jer datoteka koristi tipove iz tog prostora imena.
using Leadgen.Model.Entities;
// Namespace `Leadgen.Model.Enums` uvozim jer datoteka koristi tipove iz tog prostora imena.
using Leadgen.Model.Enums;

// Namespace `Leadgen.Lab1Runner.Seed` definiram kako bih tipove grupirao prema projektu i odgovornosti.
namespace Leadgen.Lab1Runner.Seed;

// Statičku klasu `LeadgenSeedFactory` koristim jer služi kao katalog pomoćnih funkcija bez potrebe za instanciranjem.
public static class LeadgenSeedFactory
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
{
// Ovdje uvodim metodu `Create` kojom kapsuliram ponovljivu ili smisleno odvojenu logiku.
    public static LeadgenLabDataset Create()
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var now = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc);
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var agents = CreateAgents(now);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var missions = new List<BusinessDnaMission>
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        {
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateSqlOptimizationMission(now, agents),
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateSupportQaMission(now, agents),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateVenueBookingMission(now, agents)
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
        };

// Ovdje vraćam novokreirani objekt jer metoda služi kao centralizirani konstruktor ili projekcija podataka.
        return new LeadgenLabDataset
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        {
// Ovim retkom postavljam `Missions` kako bih objektu ili konfiguraciji dao kolekciju seed misija koje runner koristi.
            Missions = missions,
// Ovim retkom postavljam `Agents` kako bih objektu ili konfiguraciji dao kolekciju swarm agenata koja se dijeli kroz scenarije.
            Agents = agents
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
        };
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim gradimo popis seed agenata koje svi scenariji dijele.
    private static List<SwarmAgent> CreateAgents(DateTime now)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        return
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Instanciram novi objekt jer mi treba svjež zapis u domenskom modelu ili pomoćnoj strukturi.
            new SwarmAgent
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
            {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
                Id = Guid.NewGuid(),
// Ovim retkom postavljam `CodeName` kako bih objektu ili konfiguraciji dao pozivni naziv agenta radi čitljivog operativnog identiteta.
                CodeName = "STRAT-01",
// Ovim retkom postavljam `Role` kako bih objektu ili konfiguraciji dao specijaliziranu ulogu agenta kako bi swarm bio podijeljen po funkcijama.
                Role = AgentRole.Strategist,
// Ovim retkom postavljam `Provider` kako bih objektu ili konfiguraciji dao model/provider sloj koji agenta pogoni.
                Provider = "OpenAI",
// Ovim retkom postavljam `Temperature` kako bih objektu ili konfiguraciji dao temperaturu modela kako bismo sugerirali stil i varijabilnost rada.
                Temperature = 0.10m,
// Ovu enum vrijednost uvodim kao vrijednost `MaxConcurrentTasks` kao jednu od dozvoljenih opcija ovog enuma.
                MaxConcurrentTasks = 3,
// Ovim retkom postavljam `IsActive` kako bih objektu ili konfiguraciji dao oznaku je li agent aktivan u trenutnom datasetu.
                IsActive = true,
// Ovim retkom postavljam `LastHeartbeatUtc` kako bih objektu ili konfiguraciji dao zadnji heartbeat radi simulacije živog sustava.
                LastHeartbeatUtc = now.AddMinutes(-2),
// Ovim retkom postavljam `CurrentFocus` kako bih objektu ili konfiguraciji dao trenutni fokus agenta kako bi runner imao operativni kontekst.
                CurrentFocus = "Planning mission decomposition"
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            },
// Instanciram novi objekt jer mi treba svjež zapis u domenskom modelu ili pomoćnoj strukturi.
            new SwarmAgent
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
            {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
                Id = Guid.NewGuid(),
// Ovim retkom postavljam `CodeName` kako bih objektu ili konfiguraciji dao pozivni naziv agenta radi čitljivog operativnog identiteta.
                CodeName = "SCOUT-01",
// Ovim retkom postavljam `Role` kako bih objektu ili konfiguraciji dao specijaliziranu ulogu agenta kako bi swarm bio podijeljen po funkcijama.
                Role = AgentRole.Scout,
// Ovim retkom postavljam `Provider` kako bih objektu ili konfiguraciji dao model/provider sloj koji agenta pogoni.
                Provider = "OpenAI",
// Ovim retkom postavljam `Temperature` kako bih objektu ili konfiguraciji dao temperaturu modela kako bismo sugerirali stil i varijabilnost rada.
                Temperature = 0.15m,
// Ovu enum vrijednost uvodim kao vrijednost `MaxConcurrentTasks` kao jednu od dozvoljenih opcija ovog enuma.
                MaxConcurrentTasks = 5,
// Ovim retkom postavljam `IsActive` kako bih objektu ili konfiguraciji dao oznaku je li agent aktivan u trenutnom datasetu.
                IsActive = true,
// Ovim retkom postavljam `LastHeartbeatUtc` kako bih objektu ili konfiguraciji dao zadnji heartbeat radi simulacije živog sustava.
                LastHeartbeatUtc = now.AddMinutes(-3),
// Ovim retkom postavljam `CurrentFocus` kako bih objektu ili konfiguraciji dao trenutni fokus agenta kako bi runner imao operativni kontekst.
                CurrentFocus = "Discovering candidate organizations"
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            },
// Instanciram novi objekt jer mi treba svjež zapis u domenskom modelu ili pomoćnoj strukturi.
            new SwarmAgent
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
            {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
                Id = Guid.NewGuid(),
// Ovim retkom postavljam `CodeName` kako bih objektu ili konfiguraciji dao pozivni naziv agenta radi čitljivog operativnog identiteta.
                CodeName = "ANCHOR-01",
// Ovim retkom postavljam `Role` kako bih objektu ili konfiguraciji dao specijaliziranu ulogu agenta kako bi swarm bio podijeljen po funkcijama.
                Role = AgentRole.Anchor,
// Ovim retkom postavljam `Provider` kako bih objektu ili konfiguraciji dao model/provider sloj koji agenta pogoni.
                Provider = "OpenAI",
// Ovim retkom postavljam `Temperature` kako bih objektu ili konfiguraciji dao temperaturu modela kako bismo sugerirali stil i varijabilnost rada.
                Temperature = 0.20m,
// Ovu enum vrijednost uvodim kao vrijednost `MaxConcurrentTasks` kao jednu od dozvoljenih opcija ovog enuma.
                MaxConcurrentTasks = 4,
// Ovim retkom postavljam `IsActive` kako bih objektu ili konfiguraciji dao oznaku je li agent aktivan u trenutnom datasetu.
                IsActive = true,
// Ovim retkom postavljam `LastHeartbeatUtc` kako bih objektu ili konfiguraciji dao zadnji heartbeat radi simulacije živog sustava.
                LastHeartbeatUtc = now.AddMinutes(-4),
// Ovim retkom postavljam `CurrentFocus` kako bih objektu ili konfiguraciji dao trenutni fokus agenta kako bi runner imao operativni kontekst.
                CurrentFocus = "Resolving decision makers"
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            },
// Instanciram novi objekt jer mi treba svjež zapis u domenskom modelu ili pomoćnoj strukturi.
            new SwarmAgent
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
            {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
                Id = Guid.NewGuid(),
// Ovim retkom postavljam `CodeName` kako bih objektu ili konfiguraciji dao pozivni naziv agenta radi čitljivog operativnog identiteta.
                CodeName = "SOUL-01",
// Ovim retkom postavljam `Role` kako bih objektu ili konfiguraciji dao specijaliziranu ulogu agenta kako bi swarm bio podijeljen po funkcijama.
                Role = AgentRole.Soul,
// Ovim retkom postavljam `Provider` kako bih objektu ili konfiguraciji dao model/provider sloj koji agenta pogoni.
                Provider = "OpenAI",
// Ovim retkom postavljam `Temperature` kako bih objektu ili konfiguraciji dao temperaturu modela kako bismo sugerirali stil i varijabilnost rada.
                Temperature = 0.35m,
// Ovu enum vrijednost uvodim kao vrijednost `MaxConcurrentTasks` kao jednu od dozvoljenih opcija ovog enuma.
                MaxConcurrentTasks = 4,
// Ovim retkom postavljam `IsActive` kako bih objektu ili konfiguraciji dao oznaku je li agent aktivan u trenutnom datasetu.
                IsActive = true,
// Ovim retkom postavljam `LastHeartbeatUtc` kako bih objektu ili konfiguraciji dao zadnji heartbeat radi simulacije živog sustava.
                LastHeartbeatUtc = now.AddMinutes(-5),
// Ovim retkom postavljam `CurrentFocus` kako bih objektu ili konfiguraciji dao trenutni fokus agenta kako bi runner imao operativni kontekst.
                CurrentFocus = "Mining qualification signals"
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            },
// Instanciram novi objekt jer mi treba svjež zapis u domenskom modelu ili pomoćnoj strukturi.
            new SwarmAgent
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
            {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
                Id = Guid.NewGuid(),
// Ovim retkom postavljam `CodeName` kako bih objektu ili konfiguraciji dao pozivni naziv agenta radi čitljivog operativnog identiteta.
                CodeName = "SENTINEL-01",
// Ovim retkom postavljam `Role` kako bih objektu ili konfiguraciji dao specijaliziranu ulogu agenta kako bi swarm bio podijeljen po funkcijama.
                Role = AgentRole.Sentinel,
// Ovim retkom postavljam `Provider` kako bih objektu ili konfiguraciji dao model/provider sloj koji agenta pogoni.
                Provider = "OpenAI",
// Ovim retkom postavljam `Temperature` kako bih objektu ili konfiguraciji dao temperaturu modela kako bismo sugerirali stil i varijabilnost rada.
                Temperature = 0.20m,
// Ovu enum vrijednost uvodim kao vrijednost `MaxConcurrentTasks` kao jednu od dozvoljenih opcija ovog enuma.
                MaxConcurrentTasks = 5,
// Ovim retkom postavljam `IsActive` kako bih objektu ili konfiguraciji dao oznaku je li agent aktivan u trenutnom datasetu.
                IsActive = true,
// Ovim retkom postavljam `LastHeartbeatUtc` kako bih objektu ili konfiguraciji dao zadnji heartbeat radi simulacije živog sustava.
                LastHeartbeatUtc = now.AddMinutes(-2),
// Ovim retkom postavljam `CurrentFocus` kako bih objektu ili konfiguraciji dao trenutni fokus agenta kako bi runner imao operativni kontekst.
                CurrentFocus = "Collecting market and news evidence"
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            },
// Instanciram novi objekt jer mi treba svjež zapis u domenskom modelu ili pomoćnoj strukturi.
            new SwarmAgent
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
            {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
                Id = Guid.NewGuid(),
// Ovim retkom postavljam `CodeName` kako bih objektu ili konfiguraciji dao pozivni naziv agenta radi čitljivog operativnog identiteta.
                CodeName = "STITCH-01",
// Ovim retkom postavljam `Role` kako bih objektu ili konfiguraciji dao specijaliziranu ulogu agenta kako bi swarm bio podijeljen po funkcijama.
                Role = AgentRole.Stitcher,
// Ovim retkom postavljam `Provider` kako bih objektu ili konfiguraciji dao model/provider sloj koji agenta pogoni.
                Provider = "OpenAI",
// Ovim retkom postavljam `Temperature` kako bih objektu ili konfiguraciji dao temperaturu modela kako bismo sugerirali stil i varijabilnost rada.
                Temperature = 0.10m,
// Ovu enum vrijednost uvodim kao vrijednost `MaxConcurrentTasks` kao jednu od dozvoljenih opcija ovog enuma.
                MaxConcurrentTasks = 4,
// Ovim retkom postavljam `IsActive` kako bih objektu ili konfiguraciji dao oznaku je li agent aktivan u trenutnom datasetu.
                IsActive = true,
// Ovim retkom postavljam `LastHeartbeatUtc` kako bih objektu ili konfiguraciji dao zadnji heartbeat radi simulacije živog sustava.
                LastHeartbeatUtc = now.AddMinutes(-6),
// Ovim retkom postavljam `CurrentFocus` kako bih objektu ili konfiguraciji dao trenutni fokus agenta kako bi runner imao operativni kontekst.
                CurrentFocus = "Verifying identities and contact vectors"
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            },
// Instanciram novi objekt jer mi treba svjež zapis u domenskom modelu ili pomoćnoj strukturi.
            new SwarmAgent
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
            {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
                Id = Guid.NewGuid(),
// Ovim retkom postavljam `CodeName` kako bih objektu ili konfiguraciji dao pozivni naziv agenta radi čitljivog operativnog identiteta.
                CodeName = "SNIPER-01",
// Ovim retkom postavljam `Role` kako bih objektu ili konfiguraciji dao specijaliziranu ulogu agenta kako bi swarm bio podijeljen po funkcijama.
                Role = AgentRole.Sniper,
// Ovim retkom postavljam `Provider` kako bih objektu ili konfiguraciji dao model/provider sloj koji agenta pogoni.
                Provider = "OpenAI",
// Ovim retkom postavljam `Temperature` kako bih objektu ili konfiguraciji dao temperaturu modela kako bismo sugerirali stil i varijabilnost rada.
                Temperature = 0.25m,
// Ovu enum vrijednost uvodim kao vrijednost `MaxConcurrentTasks` kao jednu od dozvoljenih opcija ovog enuma.
                MaxConcurrentTasks = 2,
// Ovim retkom postavljam `IsActive` kako bih objektu ili konfiguraciji dao oznaku je li agent aktivan u trenutnom datasetu.
                IsActive = true,
// Ovim retkom postavljam `LastHeartbeatUtc` kako bih objektu ili konfiguraciji dao zadnji heartbeat radi simulacije živog sustava.
                LastHeartbeatUtc = now.AddMinutes(-8),
// Ovim retkom postavljam `CurrentFocus` kako bih objektu ili konfiguraciji dao trenutni fokus agenta kako bi runner imao operativni kontekst.
                CurrentFocus = "Resolving edge-case ambiguity"
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
            }
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
        ];
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim gradimo najzreliju tehničku misiju za demo upite i simulaciju.
    private static BusinessDnaMission CreateSqlOptimizationMission(DateTime now, IReadOnlyCollection<SwarmAgent> agents)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var mission = new BusinessDnaMission
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
            Id = Guid.NewGuid(),
// Ovim retkom postavljam `MissionName` kako bih objektu ili konfiguraciji dao čitljiv naziv misije kako bi se scenarij lako razlikovao u runneru i upitima.
            MissionName = "Mission A - SQL Optimization",
// Ovim retkom postavljam `ProductName` kako bih objektu ili konfiguraciji dao naziv proizvoda koji seed misija predstavlja.
            ProductName = "LatencyLens",
// Ovim retkom postavljam `Mechanic` kako bih objektu ili konfiguraciji dao kratak opis mehanike proizvoda kako bi Business DNA bila konkretna.
            Mechanic = "Identifies query bottlenecks and infrastructure waste in cloud SQL workloads.",
// Ovim retkom postavljam `PrimarySurface` kako bih objektu ili konfiguraciji dao glavnu površinu proizvoda jer utječe na ICP i kasnije grupiranje.
            PrimarySurface = "API",
// Ovim retkom postavljam `SurfaceTags` kako bih objektu ili konfiguraciji dao dodatne oznake površine kako bi se nijanse proizvoda zadržale u modelu.
            SurfaceTags = new List<string> { "web dashboard", "developer workflow", "cloud monitoring" },
// Ovim retkom postavljam `Persona` kako bih objektu ili konfiguraciji dao ciljanu personu jer leadgen kreće od toga kome zapravo prodajemo.
            Persona = "CTO, VP Engineering, and Platform Lead",
// Ovim retkom postavljam `Villain` kako bih objektu ili konfiguraciji dao glavni problem koji proizvod rješava kako bi outreach imao jasan neprijatelj/problem.
            Villain = "Slow RDS queries, infrastructure overprovisioning, and incident-driven tuning",
// Ovim retkom postavljam `Delta` kako bih objektu ili konfiguraciji dao obećanu promjenu vrijednosti koju proizvod donosi.
            Delta = "Lower latency and lower infrastructure cost with faster remediation cycles",
// Ovim retkom postavljam `ConfidenceScore` kako bih objektu ili konfiguraciji dao stupanj sigurnosti kako bismo mogli filtrirati spremnost misije.
            ConfidenceScore = 0.92m,
// Ovim retkom postavljam `CreatedAtUtc` kako bih objektu ili konfiguraciji dao vrijeme kreiranja u UTC-u radi konzistentnog vremenskog praćenja.
            CreatedAtUtc = now.AddDays(-45),
// Ovim retkom postavljam `Status` kako bih objektu ili konfiguraciji dao trenutni status kako bi tijek misije bio eksplicitno modeliran.
            Status = MissionStatus.Completed
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
        };

// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        mission.ClarificationQuestions.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateQuestion("Persona", "Are we targeting the budget owner or the infrastructure operator?", "The product touches both platform and finance concerns.", true, now.AddDays(-44), "Prioritize the engineering leader with budget influence.", now.AddDays(-44).AddHours(2)),
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateQuestion("Surface", "Does the solution live only in an API, or is there also a management dashboard?", "Surface affects where the ICP experiences the product.", true, now.AddDays(-44), "Both API and dashboard exist; API is primary.", now.AddDays(-44).AddHours(3)),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateQuestion("Delta", "Which metric matters more: latency reduction or cost reduction?", "The outreach hook changes depending on the primary delta.", true, now.AddDays(-43), "Lead with latency reduction and support it with cost savings.", now.AddDays(-43).AddHours(1))
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var run = CreateRun(mission, "RUN-SQL-001", MissionStatus.Completed, "UK/EU cloud-native companies", 12500, 14.25m, now.AddDays(-21), now.AddDays(-20));
// Add koristim kada želim proširiti postojeću kolekciju jednim novim elementom.
        mission.Runs.Add(run);

// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Strategist), "Decompose the mission and allocate swarm tasks.", 2200, MissionStatus.Completed, now.AddDays(-21));
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Scout), "Identify cloud-native companies with infrastructure pain signals.", 2400, MissionStatus.Completed, now.AddDays(-21).AddMinutes(10));
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Anchor), "Resolve engineering decision makers for shortlisted companies.", 2100, MissionStatus.Completed, now.AddDays(-21).AddMinutes(25));
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Soul), "Mine technical complaints and qualification signals from public activity.", 2800, MissionStatus.Completed, now.AddDays(-21).AddMinutes(40));
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Sentinel), "Collect company proof and recent market context.", 1800, MissionStatus.Completed, now.AddDays(-21).AddMinutes(50));
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Stitcher), "Verify contact channels and profile linkage.", 1200, MissionStatus.Completed, now.AddDays(-21).AddMinutes(55));
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Sniper), "Resolve ambiguous identity overlap for one CTO profile.", 800, MissionStatus.Completed, now.AddDays(-21).AddMinutes(58));

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var nebula = CreateCompany("NebulaOps", "nebulaops.io", "Cloud infrastructure", "London", "United Kingdom", "Scale-up", now.AddDays(-7), 240, true, 0.95m);
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var sarah = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Sarah Patel",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "CTO",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Engineering",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Executive",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            true,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/sarah-patel-nebulaops",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "@sarahships",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "spatel-cloud",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Publicly discussing database latency and hiring around platform reliability.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-3));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        sarah.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "sarah.patel@nebulaops.io", true, now.AddDays(-5), "Apollo match", 0.97m),
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/sarah-patel-nebulaops", true, now.AddDays(-9), "Netrows profile", 0.98m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.GitHub, "https://github.com/spatel-cloud", true, now.AddDays(-10), "GitHub profile", 0.91m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        sarah.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateEvidence(EvidenceKind.Signal, "Latency complaint", "X", "https://x.example.com/sarahships/posts/1001", "Complained about rising read replica lag during peak traffic.", "\"Replica lag is eating our SLA tonight.\"", now.AddDays(-6), 0.93m, true),
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateEvidence(EvidenceKind.Content, "Tech stack mention", "GitHub", "https://github.com/spatel-cloud/repo/issues/44", "Opened an issue about query timeout thresholds in a load-balancing component.", "Investigating timeout tuning for high-concurrency workloads.", now.AddDays(-8), 0.89m, true),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Organization, "Infrastructure hiring signal", "Company careers", "https://nebulaops.io/careers/platform", "Company is hiring a senior database reliability engineer.", "Need experience with query optimization and PostgreSQL scaling.", now.AddDays(-9), 0.88m, true)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var mark = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Mark Chen",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "VP Engineering",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Engineering",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "VP",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            true,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/mark-chen-nebulaops",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "@markbuilds",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Owns platform modernization and cloud spend review.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-5));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        mark.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "mark.chen@nebulaops.io", true, now.AddDays(-5), "Apollo match", 0.96m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/mark-chen-nebulaops", true, now.AddDays(-8), "Netrows profile", 0.97m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        mark.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateEvidence(EvidenceKind.Content, "Cost optimization post", "LinkedIn", "https://linkedin.example.com/posts/mark-chen-501", "Shared a post about improving infra efficiency without slowing teams down.", "Platform cost is now a board-level metric for us.", now.AddDays(-12), 0.86m, true),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Profile, "Role verification", "LinkedIn", "https://linkedin.example.com/in/mark-chen-nebulaops", "Current title confirmed as VP Engineering.", "VP Engineering at NebulaOps", now.AddDays(-12), 0.95m, false)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        nebula.Contacts.AddRange([sarah, mark]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var queryForge = CreateCompany("QueryForge", "queryforge.dev", "Developer tooling", "Berlin", "Germany", "Growth", now.AddDays(-10), 115, true, 0.90m);
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var elena = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Elena Kovac",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Head of Platform",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Platform",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Director",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            true,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/elena-kovac-queryforge",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "@elenaplatform",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "elena-kovac",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Leading platform reliability improvements during customer growth.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-6));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        elena.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "elena.kovac@queryforge.dev", true, now.AddDays(-7), "Apollo match", 0.95m),
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/elena-kovac-queryforge", true, now.AddDays(-11), "Netrows profile", 0.97m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.X, "@elenaplatform", true, now.AddDays(-14), "Profile match", 0.84m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        elena.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateEvidence(EvidenceKind.Signal, "Scale bottleneck signal", "X", "https://x.example.com/elenaplatform/posts/202", "Discussed the cost of runaway analytics queries after onboarding enterprise customers.", "\"Every new tenant brings another expensive dashboard query.\"", now.AddDays(-10), 0.90m, true),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Organization, "Expansion signal", "Company blog", "https://queryforge.dev/blog/enterprise", "Announced new enterprise expansion requiring platform hardening.", "We are expanding into larger customer segments with stricter SLAs.", now.AddDays(-15), 0.83m, true)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var tom = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Tom Weber",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Staff Data Engineer",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Data",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Staff",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            false,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/tom-weber-queryforge",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "tomweber-data",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Technical influencer with direct exposure to performance bottlenecks.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-11));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        tom.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/tom-weber-queryforge", true, now.AddDays(-11), "Netrows profile", 0.94m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.GitHub, "https://github.com/tomweber-data", true, now.AddDays(-16), "GitHub profile", 0.89m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        tom.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateEvidence(EvidenceKind.Content, "Optimization commit trail", "GitHub", "https://github.com/tomweber-data/queryforge/commit/abc123", "Recent commits focus on indexing and query-plan inspection.", "Added query-plan logging for slow endpoints.", now.AddDays(-14), 0.87m, true),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Profile, "Role verification", "LinkedIn", "https://www.linkedin.com/in/tom-weber-queryforge", "Current title confirmed as Staff Data Engineer.", "Staff Data Engineer at QueryForge", now.AddDays(-13), 0.95m, false)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        queryForge.Contacts.AddRange([elena, tom]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var fluxLedger = CreateCompany("FluxLedger", "fluxledger.com", "Fintech infrastructure", "Amsterdam", "Netherlands", "Scale-up", now.AddDays(-11), 310, true, 0.88m);
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var nina = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Nina Rossi",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "CTO",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Engineering",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Executive",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            true,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/nina-rossi-fluxledger",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "@ninarossi_io",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Evaluating reliability vendors after latency incidents in reporting systems.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-4));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        nina.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "nina.rossi@fluxledger.com", true, now.AddDays(-6), "Apollo match", 0.96m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/nina-rossi-fluxledger", true, now.AddDays(-9), "Netrows profile", 0.97m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        nina.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateEvidence(EvidenceKind.Signal, "Incident aftermath signal", "Company interview", "https://fluxledger.com/news/platform-interview", "Referenced a recent latency incident and the need for stronger observability.", "We learned that query hot spots were invisible until customers complained.", now.AddDays(-11), 0.85m, true),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Verification, "Decision-maker verification", "LinkedIn", "https://www.linkedin.com/in/nina-rossi-fluxledger", "Current CTO profile confirmed.", "CTO at FluxLedger", now.AddDays(-11), 0.96m, false)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var daniel = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Daniel Novak",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Platform Engineering Manager",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Platform",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Manager",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            false,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/daniel-novak-fluxledger",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "dnovak-platform",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Runs day-to-day platform performance work and tool evaluations.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-7));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        daniel.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "daniel.novak@fluxledger.com", true, now.AddDays(-7), "Apollo match", 0.94m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.GitHub, "https://github.com/dnovak-platform", true, now.AddDays(-12), "GitHub profile", 0.88m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        daniel.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateEvidence(EvidenceKind.Content, "Database tuning note", "GitHub", "https://github.com/dnovak-platform/notes/12", "Documented load-test findings tied to database contention.", "Observed lock contention under reporting load.", now.AddDays(-12), 0.84m, true),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Contact, "Email verification", "Apollo", "https://apollo.example.com/fluxledger/daniel", "Verified work email available.", "Verified work email match.", now.AddDays(-7), 0.94m, false)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        fluxLedger.Contacts.AddRange([nina, daniel]);

// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        run.TargetCompanies.AddRange([nebula, queryForge, fluxLedger]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        run.LeadDossiers.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateDossier(run, nebula, sarah, 10, "Lead with the replica-lag pain signal and position a fast diagnostic proof-of-value.", "Sarah publicly complained about replica lag and is hiring around database reliability.", now.AddDays(-20)),
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateDossier(run, queryForge, elena, 9, "Open with cost and query-sprawl control for enterprise growth.", "Elena discussed expensive tenant analytics queries as scale increased.", now.AddDays(-20).AddMinutes(10)),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateDossier(run, fluxLedger, nina, 8, "Anchor the outreach to incident recovery and invisible query hot spots.", "Nina linked customer-facing latency to missing visibility into query hot spots.", now.AddDays(-20).AddMinutes(20))
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);

// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return mission;
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim gradimo poslovni support QA scenarij kako bismo imali drugi vertikalni use case.
    private static BusinessDnaMission CreateSupportQaMission(DateTime now, IReadOnlyCollection<SwarmAgent> agents)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var mission = new BusinessDnaMission
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
            Id = Guid.NewGuid(),
// Ovim retkom postavljam `MissionName` kako bih objektu ili konfiguraciji dao čitljiv naziv misije kako bi se scenarij lako razlikovao u runneru i upitima.
            MissionName = "Mission B - Support QA Automation",
// Ovim retkom postavljam `ProductName` kako bih objektu ili konfiguraciji dao naziv proizvoda koji seed misija predstavlja.
            ProductName = "QA Orbit",
// Ovim retkom postavljam `Mechanic` kako bih objektu ili konfiguraciji dao kratak opis mehanike proizvoda kako bi Business DNA bila konkretna.
            Mechanic = "Automates QA review of support interactions and surfaces coaching opportunities.",
// Ovim retkom postavljam `PrimarySurface` kako bih objektu ili konfiguraciji dao glavnu površinu proizvoda jer utječe na ICP i kasnije grupiranje.
            PrimarySurface = "SaaS platform",
// Ovim retkom postavljam `SurfaceTags` kako bih objektu ili konfiguraciji dao dodatne oznake površine kako bi se nijanse proizvoda zadržale u modelu.
            SurfaceTags = new List<string> { "web app", "operations dashboard", "team coaching workflow" },
// Ovim retkom postavljam `Persona` kako bih objektu ili konfiguraciji dao ciljanu personu jer leadgen kreće od toga kome zapravo prodajemo.
            Persona = "Head of Support, QA Manager, and Customer Operations Lead",
// Ovim retkom postavljam `Villain` kako bih objektu ili konfiguraciji dao glavni problem koji proizvod rješava kako bi outreach imao jasan neprijatelj/problem.
            Villain = "Manual call review, spreadsheet tracking, and low QA coverage",
// Ovim retkom postavljam `Delta` kako bih objektu ili konfiguraciji dao obećanu promjenu vrijednosti koju proizvod donosi.
            Delta = "Higher QA coverage with less manager time and better coaching consistency",
// Ovim retkom postavljam `ConfidenceScore` kako bih objektu ili konfiguraciji dao stupanj sigurnosti kako bismo mogli filtrirati spremnost misije.
            ConfidenceScore = 0.87m,
// Ovim retkom postavljam `CreatedAtUtc` kako bih objektu ili konfiguraciji dao vrijeme kreiranja u UTC-u radi konzistentnog vremenskog praćenja.
            CreatedAtUtc = now.AddDays(-40),
// Ovim retkom postavljam `Status` kako bih objektu ili konfiguraciji dao trenutni status kako bi tijek misije bio eksplicitno modeliran.
            Status = MissionStatus.Completed
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
        };

// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        mission.ClarificationQuestions.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateQuestion("Persona", "Should the ICP lean toward support leadership or quality specialists?", "Both can benefit, but buying authority differs.", true, now.AddDays(-39), "Prioritize support leadership with operational pain.", now.AddDays(-39).AddHours(4)),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateQuestion("Villain", "Is the current process spreadsheet-heavy or QA-tool-heavy but ineffective?", "The villain changes the outreach framing.", true, now.AddDays(-38), "Most teams rely on spreadsheets and ad hoc reviews.", now.AddDays(-38).AddHours(1))
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var run = CreateRun(mission, "RUN-QA-001", MissionStatus.Completed, "North America and EMEA support organizations", 9800, 11.80m, now.AddDays(-18), now.AddDays(-17));
// Add koristim kada želim proširiti postojeću kolekciju jednim novim elementom.
        mission.Runs.Add(run);

// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Strategist), "Shape the support QA mission plan.", 1800, MissionStatus.Completed, now.AddDays(-18));
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Scout), "Find support-heavy organizations with scaling pain.", 1800, MissionStatus.Completed, now.AddDays(-18).AddMinutes(10));
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Anchor), "Resolve support and operations leaders.", 1700, MissionStatus.Completed, now.AddDays(-18).AddMinutes(20));
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Soul), "Mine public operations pain signals and hiring patterns.", 2200, MissionStatus.Completed, now.AddDays(-18).AddMinutes(30));
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Sentinel), "Collect company growth and support-expansion signals.", 1400, MissionStatus.Completed, now.AddDays(-18).AddMinutes(35));
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Stitcher), "Verify work emails and relevant social profiles.", 900, MissionStatus.Completed, now.AddDays(-18).AddMinutes(40));

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var careBridge = CreateCompany("CareBridge Support", "carebridgesupport.com", "Healthcare support services", "New York", "United States", "Enterprise business unit", now.AddDays(-5), 540, true, 0.93m);
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var alicia = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Alicia Monroe",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Head of Support",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Support",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Director",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            true,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/alicia-monroe-carebridge",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "@aliciasupport",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Publicly emphasizing coaching consistency and quality coverage as the support org grows.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-4));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        alicia.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "alicia.monroe@carebridgesupport.com", true, now.AddDays(-6), "Apollo match", 0.98m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/alicia-monroe-carebridge", true, now.AddDays(-8), "Netrows profile", 0.97m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        alicia.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateEvidence(EvidenceKind.Signal, "Coverage gap signal", "LinkedIn", "https://linkedin.example.com/posts/alicia-monroe-44", "Posted about managers manually sampling too few calls to coach effectively.", "Manual QA still leaves most calls unseen.", now.AddDays(-6), 0.92m, true),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Organization, "Team growth signal", "Company careers", "https://carebridgesupport.com/careers", "Company is hiring multiple QA analysts for support.", "Hiring QA analysts to improve coaching coverage.", now.AddDays(-9), 0.86m, true)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var kevin = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Kevin Park",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "QA Operations Manager",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Operations",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Manager",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            true,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/kevin-park-carebridge",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Owns day-to-day QA processes and tooling decisions.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-8));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        kevin.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "kevin.park@carebridgesupport.com", true, now.AddDays(-7), "Apollo match", 0.95m),
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/kevin-park-carebridge", true, now.AddDays(-10), "Netrows profile", 0.96m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.Phone, "+1-212-555-0149", false, null, "Open registry", 0.58m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        kevin.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateEvidence(EvidenceKind.Content, "Workflow friction note", "Operations webinar", "https://carebridgesupport.com/webinar/qa", "Explained that scorecards and coaching notes live in separate spreadsheets.", "Managers are copy-pasting QA outcomes between tools.", now.AddDays(-11), 0.84m, true),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Contact, "Email verification", "Apollo", "https://apollo.example.com/carebridge/kevin", "Verified work email available.", "Verified work email match.", now.AddDays(-7), 0.95m, false)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        careBridge.Contacts.AddRange([alicia, kevin]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var ticketPilot = CreateCompany("TicketPilot", "ticketpilot.io", "Customer support platform", "Toronto", "Canada", "Growth", now.AddDays(-8), 175, true, 0.89m);
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var emma = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Emma Wright",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Director of Customer Experience",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Customer Experience",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Director",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            true,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/emma-wright-ticketpilot",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "@emma_cx",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Focused on scaling quality without slowing frontline productivity.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-7));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        emma.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "emma.wright@ticketpilot.io", true, now.AddDays(-8), "Apollo match", 0.96m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/emma-wright-ticketpilot", true, now.AddDays(-10), "Netrows profile", 0.97m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        emma.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateEvidence(EvidenceKind.Signal, "Coaching consistency signal", "LinkedIn", "https://linkedin.example.com/posts/emma-wright-77", "Shared that managers need more consistent review data to coach newer reps.", "We need better QA coverage without adding another ops burden.", now.AddDays(-8), 0.88m, true),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Organization, "Expansion signal", "Press release", "https://ticketpilot.io/news/europe", "Announced expansion into new support markets.", "Expansion increases training and QA complexity.", now.AddDays(-14), 0.80m, true)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var rahul = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Rahul Singh",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Support QA Lead",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Support QA",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Lead",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            false,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/rahul-singh-ticketpilot",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Runs scorecards and calibration sessions for the support team.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-8));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        rahul.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "rahul.singh@ticketpilot.io", true, now.AddDays(-9), "Apollo match", 0.95m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/rahul-singh-ticketpilot", true, now.AddDays(-11), "Netrows profile", 0.96m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        rahul.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Content, "Calibration workload signal", "Community forum", "https://community.ticketpilot.io/posts/qa-calibration", "Mentioned the manual work needed for weekly QA calibration.", "Calibration prep is still very spreadsheet-driven.", now.AddDays(-13), 0.82m, true)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        ticketPilot.Contacts.AddRange([emma, rahul]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var serviceSail = CreateCompany("ServiceSail", "servicesail.com", "B2B support outsourcing", "Dublin", "Ireland", "Regional operator", now.AddDays(-12), 290, true, 0.86m);
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var chloe = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Chloe Byrne",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "VP Customer Operations",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Operations",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "VP",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            true,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/chloe-byrne-servicesail",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "@chloecxo",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Oversees operational quality and client reporting across several support teams.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-9));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        chloe.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "chloe.byrne@servicesail.com", true, now.AddDays(-10), "Apollo match", 0.95m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/chloe-byrne-servicesail", true, now.AddDays(-12), "Netrows profile", 0.96m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        chloe.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Signal, "Reporting burden signal", "Industry interview", "https://servicesail.com/interviews/chloe-byrne", "Discussed the burden of turning QA data into client-facing reports.", "Managers spend too much time assembling QA evidence for clients.", now.AddDays(-12), 0.83m, true)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var mateo = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Mateo Silva",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Quality Manager",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Quality",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Manager",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            true,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/mateo-silva-servicesail",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Responsible for quality scoring standards and process consistency.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-10));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        mateo.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "mateo.silva@servicesail.com", true, now.AddDays(-10), "Apollo match", 0.94m),
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/mateo-silva-servicesail", true, now.AddDays(-13), "Netrows profile", 0.95m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.Phone, "+353-1-555-0173", false, null, "Business directory", 0.60m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        mateo.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Content, "Spreadsheet dependency", "Operations meetup", "https://meetup.example.com/servicesail-qa", "Explained that scorecards still move through spreadsheets before reviews.", "We still stitch together QA views manually.", now.AddDays(-16), 0.81m, true)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        serviceSail.Contacts.AddRange([chloe, mateo]);

// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        run.TargetCompanies.AddRange([careBridge, ticketPilot, serviceSail]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        run.LeadDossiers.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateDossier(run, careBridge, alicia, 10, "Lead with QA coverage gaps and the coaching consistency problem.", "Alicia said most calls still go unseen and the company is hiring QA analysts.", now.AddDays(-17)),
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateDossier(run, ticketPilot, emma, 9, "Anchor the pitch to scaling QA without slowing managers down.", "Emma linked expansion to rising QA complexity and coaching inconsistency.", now.AddDays(-17).AddMinutes(10)),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateDossier(run, serviceSail, mateo, 8, "Show how automated scorecards reduce spreadsheet assembly time.", "Mateo still relies on spreadsheets for QA workflows.", now.AddDays(-17).AddMinutes(20))
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);

// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return mission;
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim gradimo treći scenarij s namjernim otvorenim pitanjima.
    private static BusinessDnaMission CreateVenueBookingMission(DateTime now, IReadOnlyCollection<SwarmAgent> agents)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var mission = new BusinessDnaMission
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
            Id = Guid.NewGuid(),
// Ovim retkom postavljam `MissionName` kako bih objektu ili konfiguraciji dao čitljiv naziv misije kako bi se scenarij lako razlikovao u runneru i upitima.
            MissionName = "Mission C - Corporate Venue Booking Engine",
// Ovim retkom postavljam `ProductName` kako bih objektu ili konfiguraciji dao naziv proizvoda koji seed misija predstavlja.
            ProductName = "VenueThread",
// Ovim retkom postavljam `Mechanic` kako bih objektu ili konfiguraciji dao kratak opis mehanike proizvoda kako bi Business DNA bila konkretna.
            Mechanic = "Centralizes venue and vendor booking workflows for complex corporate event operations.",
// Ovim retkom postavljam `PrimarySurface` kako bih objektu ili konfiguraciji dao glavnu površinu proizvoda jer utječe na ICP i kasnije grupiranje.
            PrimarySurface = "Web platform",
// Ovim retkom postavljam `SurfaceTags` kako bih objektu ili konfiguraciji dao dodatne oznake površine kako bi se nijanse proizvoda zadržale u modelu.
            SurfaceTags = new List<string> { "vendor portal", "operations dashboard", "multi-location workflow" },
// Ovim retkom postavljam `Persona` kako bih objektu ili konfiguraciji dao ciljanu personu jer leadgen kreće od toga kome zapravo prodajemo.
            Persona = "Operations Director, Venue Manager, and Partnerships Lead",
// Ovim retkom postavljam `Villain` kako bih objektu ili konfiguraciji dao glavni problem koji proizvod rješava kako bi outreach imao jasan neprijatelj/problem.
            Villain = "Manual email chains, spreadsheets, and fragmented booking coordination",
// Ovim retkom postavljam `Delta` kako bih objektu ili konfiguraciji dao obećanu promjenu vrijednosti koju proizvod donosi.
            Delta = "Faster booking turnaround with fewer operational mistakes",
// Ovim retkom postavljam `ConfidenceScore` kako bih objektu ili konfiguraciji dao stupanj sigurnosti kako bismo mogli filtrirati spremnost misije.
            ConfidenceScore = 0.76m,
// Ovim retkom postavljam `CreatedAtUtc` kako bih objektu ili konfiguraciji dao vrijeme kreiranja u UTC-u radi konzistentnog vremenskog praćenja.
            CreatedAtUtc = now.AddDays(-28),
// Ovim retkom postavljam `Status` kako bih objektu ili konfiguraciji dao trenutni status kako bi tijek misije bio eksplicitno modeliran.
            Status = MissionStatus.NeedsClarification
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
        };

// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        mission.ClarificationQuestions.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateQuestion("Persona", "Are we targeting the operator who runs bookings or the executive who owns venue utilization?", "Buying authority and pain points can differ.", true, now.AddDays(-27), "Target operations leadership first.", now.AddDays(-27).AddHours(3)),
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateQuestion("Surface", "Does the product cover only internal operations or also a vendor-facing portal?", "This changes the mission's product surface.", false, now.AddDays(-26), null, null),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateQuestion("Delta", "Is the primary promise speed, fewer errors, or better vendor coordination?", "The outreach hook needs a sharper delta.", false, now.AddDays(-26).AddHours(1), null, null)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var run = CreateRun(mission, "RUN-VENUE-001", MissionStatus.NeedsClarification, "Central European venue groups", 6400, 7.10m, now.AddDays(-9), null);
// Add koristim kada želim proširiti postojeću kolekciju jednim novim elementom.
        mission.Runs.Add(run);

// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Strategist), "Prepare a draft venue-ops mission map pending clarification.", 1200, MissionStatus.NeedsClarification, now.AddDays(-9));
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Scout), "Identify venue groups with multi-location complexity.", 1500, MissionStatus.NeedsClarification, now.AddDays(-9).AddMinutes(10));
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Anchor), "Resolve venue operations leadership roles.", 1400, MissionStatus.NeedsClarification, now.AddDays(-9).AddMinutes(20));
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        AssignAgent(run, FindAgent(agents, AgentRole.Soul), "Mine public operations pain signals from venue operators.", 1600, MissionStatus.NeedsClarification, now.AddDays(-9).AddMinutes(35));

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var skyline = CreateCompany("Skyline Venue Group", "skylinevenues.hr", "Corporate venues", "Zagreb", "Croatia", "Regional operator", now.AddDays(-6), 80, true, 0.82m);
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var ivana = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Ivana Horvat",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Operations Director",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Operations",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Director",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            true,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/ivana-horvat-skyline",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Oversees venue coordination across several premium locations.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-6));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        ivana.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "ivana.horvat@skylinevenues.hr", true, now.AddDays(-6), "Directory match", 0.90m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/ivana-horvat-skyline", true, now.AddDays(-8), "Netrows profile", 0.95m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        ivana.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Signal, "Coordination friction", "Industry panel", "https://events.example.com/panel/ivana", "Discussed how vendor coordination still happens across email and spreadsheets.", "The handoff between venues and vendors is still too manual.", now.AddDays(-8), 0.84m, true)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var petar = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Petar Marin",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Vendor Manager",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Partnerships",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Manager",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            false,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/petar-marin-skyline",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Runs supplier and vendor communication for bookings.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-7));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        petar.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "petar.marin@skylinevenues.hr", true, now.AddDays(-7), "Directory match", 0.89m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/petar-marin-skyline", true, now.AddDays(-9), "Netrows profile", 0.93m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        petar.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Content, "Vendor workflow note", "LinkedIn", "https://linkedin.example.com/posts/petar-marin-2", "Shared notes about confirming vendor availability across multiple event requests.", "Availability tracking still lives in shared sheets.", now.AddDays(-9), 0.79m, true)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        skyline.Contacts.AddRange([ivana, petar]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var atlas = CreateCompany("Atlas Events Collective", "atlasevents.at", "Event operations", "Vienna", "Austria", "Regional operator", now.AddDays(-7), 110, true, 0.79m);
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var sofia = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Sofia Klein",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Venue Operations Lead",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Operations",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Lead",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            true,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/sofia-klein-atlas",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Coordinates booking flow across venues and internal teams.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-7));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        sofia.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "sofia.klein@atlasevents.at", true, now.AddDays(-7), "Directory match", 0.88m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/sofia-klein-atlas", true, now.AddDays(-10), "Netrows profile", 0.94m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        sofia.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Signal, "Booking turnaround delay", "Operations blog", "https://atlasevents.at/blog/ops-update", "Mentioned the difficulty of reducing turnaround time when booking data is fragmented.", "Our teams still chase booking context across inboxes.", now.AddDays(-10), 0.82m, true)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var lukas = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Lukas Gruber",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Partnerships Manager",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Partnerships",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Manager",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            false,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/lukas-gruber-atlas",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Influences how vendors and venues collaborate operationally.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-8));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        lukas.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "lukas.gruber@atlasevents.at", true, now.AddDays(-8), "Directory match", 0.87m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/lukas-gruber-atlas", true, now.AddDays(-11), "Netrows profile", 0.92m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        lukas.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Content, "Vendor communication load", "Conference recap", "https://events.example.com/recap/lukas", "Described the manual communication load involved in confirming vendor details.", "Each booking still triggers too many repetitive follow-ups.", now.AddDays(-11), 0.78m, true)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        atlas.Contacts.AddRange([sofia, lukas]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var meridian = CreateCompany("Meridian Spaces", "meridianspaces.cz", "Flexible event spaces", "Prague", "Czech Republic", "Multi-site operator", now.AddDays(-13), 95, true, 0.77m);
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var petra = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Petra Novak",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Regional Operations Manager",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Operations",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Manager",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            true,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/petra-novak-meridian",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Coordinates regional venue operations and process standardization.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-10));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        petra.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "petra.novak@meridianspaces.cz", true, now.AddDays(-11), "Directory match", 0.86m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/petra-novak-meridian", true, now.AddDays(-12), "Netrows profile", 0.92m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        petra.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Signal, "Process consistency signal", "Operations interview", "https://meridianspaces.cz/interview/petra", "Explained that every venue still tracks key steps a little differently.", "We need more consistency across venue teams.", now.AddDays(-13), 0.80m, true)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);

// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var jan = CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Jan Svoboda",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Booking Systems Lead",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Systems",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Lead",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            false,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "https://www.linkedin.com/in/jan-svoboda-meridian",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            null,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            "Owns some of the internal process and systems improvements.",
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            now.AddDays(-12));
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        jan.ContactChannels.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovim retkom dodajem još jedan seed zapis kako bi dataset bio reprezentativan za demo.
            CreateChannel(ContactChannelType.WorkEmail, "jan.svoboda@meridianspaces.cz", true, now.AddDays(-12), "Directory match", 0.85m),
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/jan-svoboda-meridian", true, now.AddDays(-13), "Netrows profile", 0.91m)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        jan.EvidencePoints.AddRange(
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        [
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
            CreateEvidence(EvidenceKind.Content, "Workflow mapping note", "LinkedIn", "https://linkedin.example.com/posts/jan-svoboda-19", "Shared internal workflow mapping work for booking operations.", "We are documenting too many manual handoffs.", now.AddDays(-15), 0.77m, true)
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ]);
// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        meridian.Contacts.AddRange([petra, jan]);

// AddRange koristim kako bih više seed elemenata dodao sažeto i čitljivo odjednom.
        run.TargetCompanies.AddRange([skyline, atlas, meridian]);

// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return mission;
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim tražimo točno jednog agenta po ulozi kako bismo izbjegli nejasna mapiranja.
    private static SwarmAgent FindAgent(IEnumerable<SwarmAgent> agents, AgentRole role)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam rezultat metode kako bi ga pozivatelj mogao dalje koristiti.
        return agents.Single(agent => agent.Role == role);
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim pakiramo zajedničku logiku kreiranja run entiteta.
    private static MissionRun CreateRun(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        BusinessDnaMission mission,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string runCode,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        MissionStatus status,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string searchRegion,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        int tokenBudget,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        decimal estimatedCostUsd,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        DateTime startedAtUtc,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        DateTime? completedAtUtc)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam novokreirani objekt jer metoda služi kao centralizirani konstruktor ili projekcija podataka.
        return new MissionRun
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
            Id = Guid.NewGuid(),
// Ovim retkom postavljam `RunCode` kako bih objektu ili konfiguraciji dao čitljivi kod izvođenja radi praćenja u logovima i ispisima.
            RunCode = runCode,
// Ovim retkom postavljam `BusinessDnaMissionId` kako bih objektu ili konfiguraciji dao vezu natrag na izvornu misiju.
            BusinessDnaMissionId = mission.Id,
// Ovim retkom postavljam `StartedAtUtc` kako bih objektu ili konfiguraciji dao vrijeme početka izvođenja.
            StartedAtUtc = startedAtUtc,
// Ovim retkom postavljam `CompletedAtUtc` kako bih objektu ili konfiguraciji dao vrijeme završetka ako je run dovršen.
            CompletedAtUtc = completedAtUtc,
// Ovim retkom postavljam `Status` kako bih objektu ili konfiguraciji dao trenutni status kako bi tijek misije bio eksplicitno modeliran.
            Status = status,
// Ovim retkom postavljam `SearchRegion` kako bih objektu ili konfiguraciji dao regiju pretrage jer tržište i signal razlikuju rezultate.
            SearchRegion = searchRegion,
// Ovim retkom postavljam `TokenBudget` kako bih objektu ili konfiguraciji dao budžet tokena kako bismo mogli pratiti trošak i kapacitet.
            TokenBudget = tokenBudget,
// Ovim retkom postavljam `EstimatedCostUsd` kako bih objektu ili konfiguraciji dao procijenjeni trošak izvođenja u USD za demo ekonomike.
            EstimatedCostUsd = estimatedCostUsd
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
        };
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim pakiramo zajedničko kreiranje clarification pitanja.
    private static ClarificationQuestion CreateQuestion(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string slotName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string prompt,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string reason,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        bool isAnswered,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        DateTime createdAtUtc,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string? answer,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        DateTime? answeredAtUtc)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam novokreirani objekt jer metoda služi kao centralizirani konstruktor ili projekcija podataka.
        return new ClarificationQuestion
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
            Id = Guid.NewGuid(),
// Ovim retkom postavljam `SlotName` kako bih objektu ili konfiguraciji dao naziv intake slota koji pitanje razjašnjava.
            SlotName = slotName,
// Ovim retkom postavljam `Prompt` kako bih objektu ili konfiguraciji dao tekst pitanja koje treba postaviti korisniku ili operatoru.
            Prompt = prompt,
// Ovim retkom postavljam `Reason` kako bih objektu ili konfiguraciji dao razlog zašto se pitanje postavlja kako bi nejasnoća bila objašnjena.
            Reason = reason,
// Ovim retkom postavljam `IsAnswered` kako bih objektu ili konfiguraciji dao oznaku je li pitanje zatvoreno kako bismo znali blokira li intake.
            IsAnswered = isAnswered,
// Ovim retkom postavljam `Answer` kako bih objektu ili konfiguraciji dao stvarni odgovor kada je pitanje riješeno.
            Answer = answer,
// Ovim retkom postavljam `CreatedAtUtc` kako bih objektu ili konfiguraciji dao vrijeme kreiranja u UTC-u radi konzistentnog vremenskog praćenja.
            CreatedAtUtc = createdAtUtc,
// Ovim retkom postavljam `AnsweredAtUtc` kako bih objektu ili konfiguraciji dao vrijeme odgovora radi audit traga.
            AnsweredAtUtc = answeredAtUtc
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
        };
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim vežemo agenta i run preko assignment entiteta.
    private static void AssignAgent(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        MissionRun run,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        SwarmAgent agent,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string responsibility,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        int tokenBudget,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        MissionStatus status,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        DateTime assignedAtUtc)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Lokalnu varijablu uvodim kako bih međurezultat učinio čitljivijim i lakšim za ponovno korištenje.
        var assignment = new MissionAgentAssignment
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
            Id = Guid.NewGuid(),
// Ovim retkom postavljam `MissionRunId` kako bih objektu ili konfiguraciji dao vezu na konkretno izvođenje misije.
            MissionRunId = run.Id,
// Ovim retkom postavljam `SwarmAgentId` kako bih objektu ili konfiguraciji dao vezu na agenta kojem je zadatak dodijeljen.
            SwarmAgentId = agent.Id,
// Ovim retkom postavljam `AssignedAtUtc` kako bih objektu ili konfiguraciji dao vrijeme dodjele zadatka.
            AssignedAtUtc = assignedAtUtc,
// Ovim retkom postavljam `Responsibility` kako bih objektu ili konfiguraciji dao opis odgovornosti agenta unutar runa.
            Responsibility = responsibility,
// Ovim retkom postavljam `TokenBudget` kako bih objektu ili konfiguraciji dao budžet tokena kako bismo mogli pratiti trošak i kapacitet.
            TokenBudget = tokenBudget,
// Ovim retkom postavljam `Status` kako bih objektu ili konfiguraciji dao trenutni status kako bi tijek misije bio eksplicitno modeliran.
            Status = status
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
        };

// Add koristim kada želim proširiti postojeću kolekciju jednim novim elementom.
        run.AgentAssignments.Add(assignment);
// Add koristim kada želim proširiti postojeću kolekciju jednim novim elementom.
        agent.MissionAssignments.Add(assignment);
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim pakiramo seed podatke za kompaniju.
    private static TargetCompany CreateCompany(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string name,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string domain,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string industry,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string headquartersCity,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string headquartersCountry,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string? organizationStageLabel,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        DateTime? lastSignalAtUtc,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        int employeeCount,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        bool isHeadquartersVerified,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        decimal matchScore)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam novokreirani objekt jer metoda služi kao centralizirani konstruktor ili projekcija podataka.
        return new TargetCompany
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
            Id = Guid.NewGuid(),
// Ovim retkom postavljam `Name` kako bih objektu ili konfiguraciji dao naziv kompanije radi identifikacije.
            Name = name,
// Ovim retkom postavljam `Domain` kako bih objektu ili konfiguraciji dao web domenu kompanije za identitet i outreach kontekst.
            Domain = domain,
// Ovim retkom postavljam `Industry` kako bih objektu ili konfiguraciji dao industriju radi segmentacije.
            Industry = industry,
// Ovim retkom postavljam `HeadquartersCity` kako bih objektu ili konfiguraciji dao grad sjedišta kao dio firmografije.
            HeadquartersCity = headquartersCity,
// Ovim retkom postavljam `HeadquartersCountry` kako bih objektu ili konfiguraciji dao državu sjedišta radi regije i lokalizacije.
            HeadquartersCountry = headquartersCountry,
// Ovim retkom postavljam `OrganizationStageLabel` kako bih objektu ili konfiguraciji dao oznaku faze razvoja kompanije radi segmentacije.
            OrganizationStageLabel = organizationStageLabel,
// Ovim retkom postavljam `LastSignalAtUtc` kako bih objektu ili konfiguraciji dao zadnji uočeni signal kako bismo znali koliko je kompanija svježa.
            LastSignalAtUtc = lastSignalAtUtc,
// Ovim retkom postavljam `EmployeeCount` kako bih objektu ili konfiguraciji dao broj zaposlenih radi procjene veličine.
            EmployeeCount = employeeCount,
// Ovim retkom postavljam `IsHeadquartersVerified` kako bih objektu ili konfiguraciji dao oznaku da je sjedište potvrđeno.
            IsHeadquartersVerified = isHeadquartersVerified,
// Ovim retkom postavljam `MatchScore` kako bih objektu ili konfiguraciji dao score podudaranja kompanije s misijom.
            MatchScore = matchScore
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
        };
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim pakiramo seed podatke za kontakt.
    private static TargetContact CreateContact(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string fullName,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string jobTitle,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string department,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string seniority,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        bool isDecisionMaker,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string? linkedInUrl,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string? xHandle,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string? gitHubUsername,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string opportunitySummary,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        DateTime lastObservedAtUtc)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam novokreirani objekt jer metoda služi kao centralizirani konstruktor ili projekcija podataka.
        return new TargetContact
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
            Id = Guid.NewGuid(),
// Ovim retkom postavljam `FullName` kako bih objektu ili konfiguraciji dao puno ime kontakta radi identifikacije.
            FullName = fullName,
// Ovim retkom postavljam `JobTitle` kako bih objektu ili konfiguraciji dao naziv funkcije kontakta radi kvalifikacije.
            JobTitle = jobTitle,
// Ovim retkom postavljam `Department` kako bih objektu ili konfiguraciji dao odjel radi boljeg razumijevanja organizacijske uloge.
            Department = department,
// Ovim retkom postavljam `Seniority` kako bih objektu ili konfiguraciji dao senioritet radi procjene odlučivačke snage.
            Seniority = seniority,
// Ovim retkom postavljam `IsDecisionMaker` kako bih objektu ili konfiguraciji dao oznaku je li kontakt stvarni decision maker.
            IsDecisionMaker = isDecisionMaker,
// Ovim retkom postavljam `LinkedInUrl` kako bih objektu ili konfiguraciji dao LinkedIn profil jer je često ključni B2B signal.
            LinkedInUrl = linkedInUrl,
// Ovim retkom postavljam `XHandle` kako bih objektu ili konfiguraciji dao X identitet kad postoji javni signal na toj mreži.
            XHandle = xHandle,
// Ovim retkom postavljam `GitHubUsername` kako bih objektu ili konfiguraciji dao GitHub identitet za tehničke persone.
            GitHubUsername = gitHubUsername,
// Ovim retkom postavljam `OpportunitySummary` kako bih objektu ili konfiguraciji dao kratki opis zašto je kontakt zanimljiv.
            OpportunitySummary = opportunitySummary,
// Ovim retkom postavljam `LastObservedAtUtc` kako bih objektu ili konfiguraciji dao zadnje opažanje kontakta radi svježine.
            LastObservedAtUtc = lastObservedAtUtc
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
        };
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim pakiramo seed podatke za kontaktni kanal.
    private static ContactChannel CreateChannel(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        ContactChannelType type,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string value,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        bool isVerified,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        DateTime? verifiedAtUtc,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string source,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        decimal confidenceScore)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam novokreirani objekt jer metoda služi kao centralizirani konstruktor ili projekcija podataka.
        return new ContactChannel
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
            Id = Guid.NewGuid(),
// Ovim retkom postavljam `Type` kako bih objektu ili konfiguraciji dao vrstu kanala ili entiteta kako bi logika mogla razlikovati scenarije.
            Type = type,
// Ovim retkom postavljam `Value` kako bih objektu ili konfiguraciji dao stvarnu vrijednost kanala, npr. email ili URL.
            Value = value,
// Ovim retkom postavljam `IsVerified` kako bih objektu ili konfiguraciji dao oznaku verifikacije kako bismo znali koliko je kontakt pouzdan.
            IsVerified = isVerified,
// Ovim retkom postavljam `VerifiedAtUtc` kako bih objektu ili konfiguraciji dao vrijeme potvrde kanala radi svježine podataka.
            VerifiedAtUtc = verifiedAtUtc,
// Ovim retkom postavljam `Source` kako bih objektu ili konfiguraciji dao izvor iz kojeg je podatak potvrđen.
            Source = source,
// Ovim retkom postavljam `ConfidenceScore` kako bih objektu ili konfiguraciji dao stupanj sigurnosti kako bismo mogli filtrirati spremnost misije.
            ConfidenceScore = confidenceScore
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
        };
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim pakiramo seed podatke za evidence točku.
    private static EvidencePoint CreateEvidence(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        EvidenceKind kind,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string label,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string sourcePlatform,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string sourceUrl,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string summary,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string rawSnippet,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        DateTime capturedAtUtc,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        decimal confidenceScore,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        bool isQualificationSignal)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam novokreirani objekt jer metoda služi kao centralizirani konstruktor ili projekcija podataka.
        return new EvidencePoint
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
            Id = Guid.NewGuid(),
// Ovim retkom postavljam `Kind` kako bih objektu ili konfiguraciji dao vrstu evidence točke kako bi se mogla grupirati i filtrirati.
            Kind = kind,
// Ovim retkom postavljam `Label` kako bih objektu ili konfiguraciji dao kratku etiketu dokaza radi čitljivog ispisa i grupiranja.
            Label = label,
// Ovim retkom postavljam `SourcePlatform` kako bih objektu ili konfiguraciji dao platformu s koje dokaz dolazi.
            SourcePlatform = sourcePlatform,
// Ovim retkom postavljam `SourceUrl` kako bih objektu ili konfiguraciji dao direktni URL izvora kako bi dokaz ostao provjerljiv.
            SourceUrl = sourceUrl,
// Ovim retkom postavljam `Summary` kako bih objektu ili konfiguraciji dao sažetak dokaza za brzo razumijevanje bez otvaranja izvora.
            Summary = summary,
// Ovim retkom postavljam `RawSnippet` kako bih objektu ili konfiguraciji dao sirovi izvadak jer često trebamo citat ili originalni signal.
            RawSnippet = rawSnippet,
// Ovim retkom postavljam `CapturedAtUtc` kako bih objektu ili konfiguraciji dao vrijeme hvatanja dokaza radi recentnosti.
            CapturedAtUtc = capturedAtUtc,
// Ovim retkom postavljam `ConfidenceScore` kako bih objektu ili konfiguraciji dao stupanj sigurnosti kako bismo mogli filtrirati spremnost misije.
            ConfidenceScore = confidenceScore,
// Ovim retkom postavljam `IsQualificationSignal` kako bih objektu ili konfiguraciji dao oznaku služi li dokaz za kvalifikaciju leada.
            IsQualificationSignal = isQualificationSignal
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
        };
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }

// Ovdje uvodim pakiramo seed podatke za konačni dossier.
    private static LeadDossier CreateDossier(
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        MissionRun run,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        TargetCompany company,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        TargetContact contact,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        int leadgenScore,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string suggestedApproach,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        string advantagePoint,
// Ovaj red zadržavam jer sudjeluje u definiciji modela, seed podataka, upita ili toka izvođenja.
        DateTime createdAtUtc)
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
    {
// Ovdje vraćam novokreirani objekt jer metoda služi kao centralizirani konstruktor ili projekcija podataka.
        return new LeadDossier
// Ovom sintaksom otvaram blok ili izraz kako bih grupirao povezanu logiku.
        {
// Ovim retkom postavljam `Id` kako bih objektu ili konfiguraciji dao jedinstveni identifikator kako bi se svaki zapis mogao pouzdano povezati s drugim zapisima.
            Id = Guid.NewGuid(),
// Ovim retkom postavljam `MissionRunId` kako bih objektu ili konfiguraciji dao vezu na konkretno izvođenje misije.
            MissionRunId = run.Id,
// Ovim retkom postavljam `TargetCompanyId` kako bih objektu ili konfiguraciji dao vezu na target kompaniju.
            TargetCompanyId = company.Id,
// Ovim retkom postavljam `TargetContactId` kako bih objektu ili konfiguraciji dao vezu na target kontakt.
            TargetContactId = contact.Id,
// Ovim retkom postavljam `LeadgenScore` kako bih objektu ili konfiguraciji dao prioritetni score kako bismo mogli rangirati leadove.
            LeadgenScore = leadgenScore,
// Ovim retkom postavljam `SuggestedApproach` kako bih objektu ili konfiguraciji dao predloženi outreach pristup temeljen na dokazima.
            SuggestedApproach = suggestedApproach,
// Ovim retkom postavljam `AdvantagePoint` kako bih objektu ili konfiguraciji dao ključnu prednost ili ulaznu točku za outreach.
            AdvantagePoint = advantagePoint,
// Ovim retkom postavljam `IsReadyForOutreach` kako bih objektu ili konfiguraciji dao oznaku je li dossier dovoljno jak za kontaktiranje.
            IsReadyForOutreach = leadgenScore >= 8,
// Ovim retkom postavljam `CreatedAtUtc` kako bih objektu ili konfiguraciji dao vrijeme kreiranja u UTC-u radi konzistentnog vremenskog praćenja.
            CreatedAtUtc = createdAtUtc,
// Ovim retkom postavljam `LastUpdatedAtUtc` kako bih objektu ili konfiguraciji dao vrijeme zadnje izmjene radi praćenja svježine.
            LastUpdatedAtUtc = createdAtUtc.AddHours(4),
// Ovim retkom postavljam `SupportingEvidenceCount` kako bih objektu ili konfiguraciji dao broj dokaza koji podupiru dossier.
            SupportingEvidenceCount = contact.EvidencePoints.Count
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
        };
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
    }
// Ovom sintaksom zatvaram prethodni blok ili inicijalizator kada je cjelina završena.
}
