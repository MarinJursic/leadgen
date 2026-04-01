using Leadgen.Model.Entities;
using Leadgen.Model.Enums;

namespace Leadgen.Lab1Runner.Seed;

public static class LeadgenSeedFactory
{
    public static LeadgenLabDataset Create()
    {
        var now = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc);
        var agents = CreateAgents(now);

        var missions = new List<BusinessDnaMission>
        {
            CreateSqlOptimizationMission(now, agents),
            CreateSupportQaMission(now, agents),
            CreateVenueBookingMission(now, agents)
        };

        return new LeadgenLabDataset
        {
            Missions = missions,
            Agents = agents
        };
    }

    private static List<SwarmAgent> CreateAgents(DateTime now)
    {
        return
        [
            new SwarmAgent
            {
                Id = Guid.NewGuid(),
                CodeName = "STRAT-01",
                Role = AgentRole.Strategist,
                Provider = "OpenAI",
                Temperature = 0.10m,
                MaxConcurrentTasks = 3,
                IsActive = true,
                LastHeartbeatUtc = now.AddMinutes(-2),
                CurrentFocus = "Planning mission decomposition"
            },
            new SwarmAgent
            {
                Id = Guid.NewGuid(),
                CodeName = "SCOUT-01",
                Role = AgentRole.Scout,
                Provider = "OpenAI",
                Temperature = 0.15m,
                MaxConcurrentTasks = 5,
                IsActive = true,
                LastHeartbeatUtc = now.AddMinutes(-3),
                CurrentFocus = "Discovering candidate organizations"
            },
            new SwarmAgent
            {
                Id = Guid.NewGuid(),
                CodeName = "ANCHOR-01",
                Role = AgentRole.Anchor,
                Provider = "OpenAI",
                Temperature = 0.20m,
                MaxConcurrentTasks = 4,
                IsActive = true,
                LastHeartbeatUtc = now.AddMinutes(-4),
                CurrentFocus = "Resolving decision makers"
            },
            new SwarmAgent
            {
                Id = Guid.NewGuid(),
                CodeName = "SOUL-01",
                Role = AgentRole.Soul,
                Provider = "OpenAI",
                Temperature = 0.35m,
                MaxConcurrentTasks = 4,
                IsActive = true,
                LastHeartbeatUtc = now.AddMinutes(-5),
                CurrentFocus = "Mining qualification signals"
            },
            new SwarmAgent
            {
                Id = Guid.NewGuid(),
                CodeName = "SENTINEL-01",
                Role = AgentRole.Sentinel,
                Provider = "OpenAI",
                Temperature = 0.20m,
                MaxConcurrentTasks = 5,
                IsActive = true,
                LastHeartbeatUtc = now.AddMinutes(-2),
                CurrentFocus = "Collecting market and news evidence"
            },
            new SwarmAgent
            {
                Id = Guid.NewGuid(),
                CodeName = "STITCH-01",
                Role = AgentRole.Stitcher,
                Provider = "OpenAI",
                Temperature = 0.10m,
                MaxConcurrentTasks = 4,
                IsActive = true,
                LastHeartbeatUtc = now.AddMinutes(-6),
                CurrentFocus = "Verifying identities and contact vectors"
            },
            new SwarmAgent
            {
                Id = Guid.NewGuid(),
                CodeName = "SNIPER-01",
                Role = AgentRole.Sniper,
                Provider = "OpenAI",
                Temperature = 0.25m,
                MaxConcurrentTasks = 2,
                IsActive = true,
                LastHeartbeatUtc = now.AddMinutes(-8),
                CurrentFocus = "Resolving edge-case ambiguity"
            }
        ];
    }

    private static BusinessDnaMission CreateSqlOptimizationMission(DateTime now, IReadOnlyCollection<SwarmAgent> agents)
    {
        var mission = new BusinessDnaMission
        {
            Id = Guid.NewGuid(),
            MissionName = "Mission A - SQL Optimization",
            ProductName = "LatencyLens",
            Mechanic = "Identifies query bottlenecks and infrastructure waste in cloud SQL workloads.",
            PrimarySurface = "API",
            SurfaceTags = new List<string> { "web dashboard", "developer workflow", "cloud monitoring" },
            Persona = "CTO, VP Engineering, and Platform Lead",
            Villain = "Slow RDS queries, infrastructure overprovisioning, and incident-driven tuning",
            Delta = "Lower latency and lower infrastructure cost with faster remediation cycles",
            ConfidenceScore = 0.92m,
            CreatedAtUtc = now.AddDays(-45),
            Status = MissionStatus.Completed
        };

        mission.ClarificationQuestions.AddRange(
        [
            CreateQuestion("Persona", "Are we targeting the budget owner or the infrastructure operator?", "The product touches both platform and finance concerns.", true, now.AddDays(-44), "Prioritize the engineering leader with budget influence.", now.AddDays(-44).AddHours(2)),
            CreateQuestion("Surface", "Does the solution live only in an API, or is there also a management dashboard?", "Surface affects where the ICP experiences the product.", true, now.AddDays(-44), "Both API and dashboard exist; API is primary.", now.AddDays(-44).AddHours(3)),
            CreateQuestion("Delta", "Which metric matters more: latency reduction or cost reduction?", "The outreach hook changes depending on the primary delta.", true, now.AddDays(-43), "Lead with latency reduction and support it with cost savings.", now.AddDays(-43).AddHours(1))
        ]);

        var run = CreateRun(mission, "RUN-SQL-001", MissionStatus.Completed, "UK/EU cloud-native companies", 12500, 14.25m, now.AddDays(-21), now.AddDays(-20));
        mission.Runs.Add(run);

        AssignAgent(run, FindAgent(agents, AgentRole.Strategist), "Decompose the mission and allocate swarm tasks.", 2200, MissionStatus.Completed, now.AddDays(-21));
        AssignAgent(run, FindAgent(agents, AgentRole.Scout), "Identify cloud-native companies with infrastructure pain signals.", 2400, MissionStatus.Completed, now.AddDays(-21).AddMinutes(10));
        AssignAgent(run, FindAgent(agents, AgentRole.Anchor), "Resolve engineering decision makers for shortlisted companies.", 2100, MissionStatus.Completed, now.AddDays(-21).AddMinutes(25));
        AssignAgent(run, FindAgent(agents, AgentRole.Soul), "Mine technical complaints and qualification signals from public activity.", 2800, MissionStatus.Completed, now.AddDays(-21).AddMinutes(40));
        AssignAgent(run, FindAgent(agents, AgentRole.Sentinel), "Collect company proof and recent market context.", 1800, MissionStatus.Completed, now.AddDays(-21).AddMinutes(50));
        AssignAgent(run, FindAgent(agents, AgentRole.Stitcher), "Verify contact channels and profile linkage.", 1200, MissionStatus.Completed, now.AddDays(-21).AddMinutes(55));
        AssignAgent(run, FindAgent(agents, AgentRole.Sniper), "Resolve ambiguous identity overlap for one CTO profile.", 800, MissionStatus.Completed, now.AddDays(-21).AddMinutes(58));

        var nebula = CreateCompany("NebulaOps", "nebulaops.io", "Cloud infrastructure", "London", "United Kingdom", "Scale-up", now.AddDays(-7), 240, true, 0.95m);
        var sarah = CreateContact(
            "Sarah Patel",
            "CTO",
            "Engineering",
            "Executive",
            true,
            "https://www.linkedin.com/in/sarah-patel-nebulaops",
            "@sarahships",
            "spatel-cloud",
            "Publicly discussing database latency and hiring around platform reliability.",
            now.AddDays(-3));
        sarah.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "sarah.patel@nebulaops.io", true, now.AddDays(-5), "Apollo match", 0.97m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/sarah-patel-nebulaops", true, now.AddDays(-9), "Netrows profile", 0.98m),
            CreateChannel(ContactChannelType.GitHub, "https://github.com/spatel-cloud", true, now.AddDays(-10), "GitHub profile", 0.91m)
        ]);
        sarah.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Signal, "Latency complaint", "X", "https://x.example.com/sarahships/posts/1001", "Complained about rising read replica lag during peak traffic.", "\"Replica lag is eating our SLA tonight.\"", now.AddDays(-6), 0.93m, true),
            CreateEvidence(EvidenceKind.Content, "Tech stack mention", "GitHub", "https://github.com/spatel-cloud/repo/issues/44", "Opened an issue about query timeout thresholds in a load-balancing component.", "Investigating timeout tuning for high-concurrency workloads.", now.AddDays(-8), 0.89m, true),
            CreateEvidence(EvidenceKind.Organization, "Infrastructure hiring signal", "Company careers", "https://nebulaops.io/careers/platform", "Company is hiring a senior database reliability engineer.", "Need experience with query optimization and PostgreSQL scaling.", now.AddDays(-9), 0.88m, true)
        ]);

        var mark = CreateContact(
            "Mark Chen",
            "VP Engineering",
            "Engineering",
            "VP",
            true,
            "https://www.linkedin.com/in/mark-chen-nebulaops",
            "@markbuilds",
            null,
            "Owns platform modernization and cloud spend review.",
            now.AddDays(-5));
        mark.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "mark.chen@nebulaops.io", true, now.AddDays(-5), "Apollo match", 0.96m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/mark-chen-nebulaops", true, now.AddDays(-8), "Netrows profile", 0.97m)
        ]);
        mark.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Content, "Cost optimization post", "LinkedIn", "https://linkedin.example.com/posts/mark-chen-501", "Shared a post about improving infra efficiency without slowing teams down.", "Platform cost is now a board-level metric for us.", now.AddDays(-12), 0.86m, true),
            CreateEvidence(EvidenceKind.Profile, "Role verification", "LinkedIn", "https://linkedin.example.com/in/mark-chen-nebulaops", "Current title confirmed as VP Engineering.", "VP Engineering at NebulaOps", now.AddDays(-12), 0.95m, false)
        ]);
        nebula.Contacts.AddRange([sarah, mark]);

        var queryForge = CreateCompany("QueryForge", "queryforge.dev", "Developer tooling", "Berlin", "Germany", "Growth", now.AddDays(-10), 115, true, 0.90m);
        var elena = CreateContact(
            "Elena Kovac",
            "Head of Platform",
            "Platform",
            "Director",
            true,
            "https://www.linkedin.com/in/elena-kovac-queryforge",
            "@elenaplatform",
            "elena-kovac",
            "Leading platform reliability improvements during customer growth.",
            now.AddDays(-6));
        elena.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "elena.kovac@queryforge.dev", true, now.AddDays(-7), "Apollo match", 0.95m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/elena-kovac-queryforge", true, now.AddDays(-11), "Netrows profile", 0.97m),
            CreateChannel(ContactChannelType.X, "@elenaplatform", true, now.AddDays(-14), "Profile match", 0.84m)
        ]);
        elena.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Signal, "Scale bottleneck signal", "X", "https://x.example.com/elenaplatform/posts/202", "Discussed the cost of runaway analytics queries after onboarding enterprise customers.", "\"Every new tenant brings another expensive dashboard query.\"", now.AddDays(-10), 0.90m, true),
            CreateEvidence(EvidenceKind.Organization, "Expansion signal", "Company blog", "https://queryforge.dev/blog/enterprise", "Announced new enterprise expansion requiring platform hardening.", "We are expanding into larger customer segments with stricter SLAs.", now.AddDays(-15), 0.83m, true)
        ]);

        var tom = CreateContact(
            "Tom Weber",
            "Staff Data Engineer",
            "Data",
            "Staff",
            false,
            "https://www.linkedin.com/in/tom-weber-queryforge",
            null,
            "tomweber-data",
            "Technical influencer with direct exposure to performance bottlenecks.",
            now.AddDays(-11));
        tom.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/tom-weber-queryforge", true, now.AddDays(-11), "Netrows profile", 0.94m),
            CreateChannel(ContactChannelType.GitHub, "https://github.com/tomweber-data", true, now.AddDays(-16), "GitHub profile", 0.89m)
        ]);
        tom.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Content, "Optimization commit trail", "GitHub", "https://github.com/tomweber-data/queryforge/commit/abc123", "Recent commits focus on indexing and query-plan inspection.", "Added query-plan logging for slow endpoints.", now.AddDays(-14), 0.87m, true),
            CreateEvidence(EvidenceKind.Profile, "Role verification", "LinkedIn", "https://www.linkedin.com/in/tom-weber-queryforge", "Current title confirmed as Staff Data Engineer.", "Staff Data Engineer at QueryForge", now.AddDays(-13), 0.95m, false)
        ]);
        queryForge.Contacts.AddRange([elena, tom]);

        var fluxLedger = CreateCompany("FluxLedger", "fluxledger.com", "Fintech infrastructure", "Amsterdam", "Netherlands", "Scale-up", now.AddDays(-11), 310, true, 0.88m);
        var nina = CreateContact(
            "Nina Rossi",
            "CTO",
            "Engineering",
            "Executive",
            true,
            "https://www.linkedin.com/in/nina-rossi-fluxledger",
            "@ninarossi_io",
            null,
            "Evaluating reliability vendors after latency incidents in reporting systems.",
            now.AddDays(-4));
        nina.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "nina.rossi@fluxledger.com", true, now.AddDays(-6), "Apollo match", 0.96m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/nina-rossi-fluxledger", true, now.AddDays(-9), "Netrows profile", 0.97m)
        ]);
        nina.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Signal, "Incident aftermath signal", "Company interview", "https://fluxledger.com/news/platform-interview", "Referenced a recent latency incident and the need for stronger observability.", "We learned that query hot spots were invisible until customers complained.", now.AddDays(-11), 0.85m, true),
            CreateEvidence(EvidenceKind.Verification, "Decision-maker verification", "LinkedIn", "https://www.linkedin.com/in/nina-rossi-fluxledger", "Current CTO profile confirmed.", "CTO at FluxLedger", now.AddDays(-11), 0.96m, false)
        ]);

        var daniel = CreateContact(
            "Daniel Novak",
            "Platform Engineering Manager",
            "Platform",
            "Manager",
            false,
            "https://www.linkedin.com/in/daniel-novak-fluxledger",
            null,
            "dnovak-platform",
            "Runs day-to-day platform performance work and tool evaluations.",
            now.AddDays(-7));
        daniel.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "daniel.novak@fluxledger.com", true, now.AddDays(-7), "Apollo match", 0.94m),
            CreateChannel(ContactChannelType.GitHub, "https://github.com/dnovak-platform", true, now.AddDays(-12), "GitHub profile", 0.88m)
        ]);
        daniel.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Content, "Database tuning note", "GitHub", "https://github.com/dnovak-platform/notes/12", "Documented load-test findings tied to database contention.", "Observed lock contention under reporting load.", now.AddDays(-12), 0.84m, true),
            CreateEvidence(EvidenceKind.Contact, "Email verification", "Apollo", "https://apollo.example.com/fluxledger/daniel", "Verified work email available.", "Verified work email match.", now.AddDays(-7), 0.94m, false)
        ]);
        fluxLedger.Contacts.AddRange([nina, daniel]);

        run.TargetCompanies.AddRange([nebula, queryForge, fluxLedger]);
        run.LeadDossiers.AddRange(
        [
            CreateDossier(run, nebula, sarah, 10, "Lead with the replica-lag pain signal and position a fast diagnostic proof-of-value.", "Sarah publicly complained about replica lag and is hiring around database reliability.", now.AddDays(-20)),
            CreateDossier(run, queryForge, elena, 9, "Open with cost and query-sprawl control for enterprise growth.", "Elena discussed expensive tenant analytics queries as scale increased.", now.AddDays(-20).AddMinutes(10)),
            CreateDossier(run, fluxLedger, nina, 8, "Anchor the outreach to incident recovery and invisible query hot spots.", "Nina linked customer-facing latency to missing visibility into query hot spots.", now.AddDays(-20).AddMinutes(20))
        ]);

        return mission;
    }

    private static BusinessDnaMission CreateSupportQaMission(DateTime now, IReadOnlyCollection<SwarmAgent> agents)
    {
        var mission = new BusinessDnaMission
        {
            Id = Guid.NewGuid(),
            MissionName = "Mission B - Support QA Automation",
            ProductName = "QA Orbit",
            Mechanic = "Automates QA review of support interactions and surfaces coaching opportunities.",
            PrimarySurface = "SaaS platform",
            SurfaceTags = new List<string> { "web app", "operations dashboard", "team coaching workflow" },
            Persona = "Head of Support, QA Manager, and Customer Operations Lead",
            Villain = "Manual call review, spreadsheet tracking, and low QA coverage",
            Delta = "Higher QA coverage with less manager time and better coaching consistency",
            ConfidenceScore = 0.87m,
            CreatedAtUtc = now.AddDays(-40),
            Status = MissionStatus.Completed
        };

        mission.ClarificationQuestions.AddRange(
        [
            CreateQuestion("Persona", "Should the ICP lean toward support leadership or quality specialists?", "Both can benefit, but buying authority differs.", true, now.AddDays(-39), "Prioritize support leadership with operational pain.", now.AddDays(-39).AddHours(4)),
            CreateQuestion("Villain", "Is the current process spreadsheet-heavy or QA-tool-heavy but ineffective?", "The villain changes the outreach framing.", true, now.AddDays(-38), "Most teams rely on spreadsheets and ad hoc reviews.", now.AddDays(-38).AddHours(1))
        ]);

        var run = CreateRun(mission, "RUN-QA-001", MissionStatus.Completed, "North America and EMEA support organizations", 9800, 11.80m, now.AddDays(-18), now.AddDays(-17));
        mission.Runs.Add(run);

        AssignAgent(run, FindAgent(agents, AgentRole.Strategist), "Shape the support QA mission plan.", 1800, MissionStatus.Completed, now.AddDays(-18));
        AssignAgent(run, FindAgent(agents, AgentRole.Scout), "Find support-heavy organizations with scaling pain.", 1800, MissionStatus.Completed, now.AddDays(-18).AddMinutes(10));
        AssignAgent(run, FindAgent(agents, AgentRole.Anchor), "Resolve support and operations leaders.", 1700, MissionStatus.Completed, now.AddDays(-18).AddMinutes(20));
        AssignAgent(run, FindAgent(agents, AgentRole.Soul), "Mine public operations pain signals and hiring patterns.", 2200, MissionStatus.Completed, now.AddDays(-18).AddMinutes(30));
        AssignAgent(run, FindAgent(agents, AgentRole.Sentinel), "Collect company growth and support-expansion signals.", 1400, MissionStatus.Completed, now.AddDays(-18).AddMinutes(35));
        AssignAgent(run, FindAgent(agents, AgentRole.Stitcher), "Verify work emails and relevant social profiles.", 900, MissionStatus.Completed, now.AddDays(-18).AddMinutes(40));

        var careBridge = CreateCompany("CareBridge Support", "carebridgesupport.com", "Healthcare support services", "New York", "United States", "Enterprise business unit", now.AddDays(-5), 540, true, 0.93m);
        var alicia = CreateContact(
            "Alicia Monroe",
            "Head of Support",
            "Support",
            "Director",
            true,
            "https://www.linkedin.com/in/alicia-monroe-carebridge",
            "@aliciasupport",
            null,
            "Publicly emphasizing coaching consistency and quality coverage as the support org grows.",
            now.AddDays(-4));
        alicia.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "alicia.monroe@carebridgesupport.com", true, now.AddDays(-6), "Apollo match", 0.98m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/alicia-monroe-carebridge", true, now.AddDays(-8), "Netrows profile", 0.97m)
        ]);
        alicia.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Signal, "Coverage gap signal", "LinkedIn", "https://linkedin.example.com/posts/alicia-monroe-44", "Posted about managers manually sampling too few calls to coach effectively.", "Manual QA still leaves most calls unseen.", now.AddDays(-6), 0.92m, true),
            CreateEvidence(EvidenceKind.Organization, "Team growth signal", "Company careers", "https://carebridgesupport.com/careers", "Company is hiring multiple QA analysts for support.", "Hiring QA analysts to improve coaching coverage.", now.AddDays(-9), 0.86m, true)
        ]);

        var kevin = CreateContact(
            "Kevin Park",
            "QA Operations Manager",
            "Operations",
            "Manager",
            true,
            "https://www.linkedin.com/in/kevin-park-carebridge",
            null,
            null,
            "Owns day-to-day QA processes and tooling decisions.",
            now.AddDays(-8));
        kevin.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "kevin.park@carebridgesupport.com", true, now.AddDays(-7), "Apollo match", 0.95m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/kevin-park-carebridge", true, now.AddDays(-10), "Netrows profile", 0.96m),
            CreateChannel(ContactChannelType.Phone, "+1-212-555-0149", false, null, "Open registry", 0.58m)
        ]);
        kevin.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Content, "Workflow friction note", "Operations webinar", "https://carebridgesupport.com/webinar/qa", "Explained that scorecards and coaching notes live in separate spreadsheets.", "Managers are copy-pasting QA outcomes between tools.", now.AddDays(-11), 0.84m, true),
            CreateEvidence(EvidenceKind.Contact, "Email verification", "Apollo", "https://apollo.example.com/carebridge/kevin", "Verified work email available.", "Verified work email match.", now.AddDays(-7), 0.95m, false)
        ]);
        careBridge.Contacts.AddRange([alicia, kevin]);

        var ticketPilot = CreateCompany("TicketPilot", "ticketpilot.io", "Customer support platform", "Toronto", "Canada", "Growth", now.AddDays(-8), 175, true, 0.89m);
        var emma = CreateContact(
            "Emma Wright",
            "Director of Customer Experience",
            "Customer Experience",
            "Director",
            true,
            "https://www.linkedin.com/in/emma-wright-ticketpilot",
            "@emma_cx",
            null,
            "Focused on scaling quality without slowing frontline productivity.",
            now.AddDays(-7));
        emma.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "emma.wright@ticketpilot.io", true, now.AddDays(-8), "Apollo match", 0.96m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/emma-wright-ticketpilot", true, now.AddDays(-10), "Netrows profile", 0.97m)
        ]);
        emma.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Signal, "Coaching consistency signal", "LinkedIn", "https://linkedin.example.com/posts/emma-wright-77", "Shared that managers need more consistent review data to coach newer reps.", "We need better QA coverage without adding another ops burden.", now.AddDays(-8), 0.88m, true),
            CreateEvidence(EvidenceKind.Organization, "Expansion signal", "Press release", "https://ticketpilot.io/news/europe", "Announced expansion into new support markets.", "Expansion increases training and QA complexity.", now.AddDays(-14), 0.80m, true)
        ]);

        var rahul = CreateContact(
            "Rahul Singh",
            "Support QA Lead",
            "Support QA",
            "Lead",
            false,
            "https://www.linkedin.com/in/rahul-singh-ticketpilot",
            null,
            null,
            "Runs scorecards and calibration sessions for the support team.",
            now.AddDays(-8));
        rahul.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "rahul.singh@ticketpilot.io", true, now.AddDays(-9), "Apollo match", 0.95m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/rahul-singh-ticketpilot", true, now.AddDays(-11), "Netrows profile", 0.96m)
        ]);
        rahul.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Content, "Calibration workload signal", "Community forum", "https://community.ticketpilot.io/posts/qa-calibration", "Mentioned the manual work needed for weekly QA calibration.", "Calibration prep is still very spreadsheet-driven.", now.AddDays(-13), 0.82m, true)
        ]);
        ticketPilot.Contacts.AddRange([emma, rahul]);

        var serviceSail = CreateCompany("ServiceSail", "servicesail.com", "B2B support outsourcing", "Dublin", "Ireland", "Regional operator", now.AddDays(-12), 290, true, 0.86m);
        var chloe = CreateContact(
            "Chloe Byrne",
            "VP Customer Operations",
            "Operations",
            "VP",
            true,
            "https://www.linkedin.com/in/chloe-byrne-servicesail",
            "@chloecxo",
            null,
            "Oversees operational quality and client reporting across several support teams.",
            now.AddDays(-9));
        chloe.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "chloe.byrne@servicesail.com", true, now.AddDays(-10), "Apollo match", 0.95m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/chloe-byrne-servicesail", true, now.AddDays(-12), "Netrows profile", 0.96m)
        ]);
        chloe.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Signal, "Reporting burden signal", "Industry interview", "https://servicesail.com/interviews/chloe-byrne", "Discussed the burden of turning QA data into client-facing reports.", "Managers spend too much time assembling QA evidence for clients.", now.AddDays(-12), 0.83m, true)
        ]);

        var mateo = CreateContact(
            "Mateo Silva",
            "Quality Manager",
            "Quality",
            "Manager",
            true,
            "https://www.linkedin.com/in/mateo-silva-servicesail",
            null,
            null,
            "Responsible for quality scoring standards and process consistency.",
            now.AddDays(-10));
        mateo.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "mateo.silva@servicesail.com", true, now.AddDays(-10), "Apollo match", 0.94m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/mateo-silva-servicesail", true, now.AddDays(-13), "Netrows profile", 0.95m),
            CreateChannel(ContactChannelType.Phone, "+353-1-555-0173", false, null, "Business directory", 0.60m)
        ]);
        mateo.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Content, "Spreadsheet dependency", "Operations meetup", "https://meetup.example.com/servicesail-qa", "Explained that scorecards still move through spreadsheets before reviews.", "We still stitch together QA views manually.", now.AddDays(-16), 0.81m, true)
        ]);
        serviceSail.Contacts.AddRange([chloe, mateo]);

        run.TargetCompanies.AddRange([careBridge, ticketPilot, serviceSail]);
        run.LeadDossiers.AddRange(
        [
            CreateDossier(run, careBridge, alicia, 10, "Lead with QA coverage gaps and the coaching consistency problem.", "Alicia said most calls still go unseen and the company is hiring QA analysts.", now.AddDays(-17)),
            CreateDossier(run, ticketPilot, emma, 9, "Anchor the pitch to scaling QA without slowing managers down.", "Emma linked expansion to rising QA complexity and coaching inconsistency.", now.AddDays(-17).AddMinutes(10)),
            CreateDossier(run, serviceSail, mateo, 8, "Show how automated scorecards reduce spreadsheet assembly time.", "Mateo still relies on spreadsheets for QA workflows.", now.AddDays(-17).AddMinutes(20))
        ]);

        return mission;
    }

    private static BusinessDnaMission CreateVenueBookingMission(DateTime now, IReadOnlyCollection<SwarmAgent> agents)
    {
        var mission = new BusinessDnaMission
        {
            Id = Guid.NewGuid(),
            MissionName = "Mission C - Corporate Venue Booking Engine",
            ProductName = "VenueThread",
            Mechanic = "Centralizes venue and vendor booking workflows for complex corporate event operations.",
            PrimarySurface = "Web platform",
            SurfaceTags = new List<string> { "vendor portal", "operations dashboard", "multi-location workflow" },
            Persona = "Operations Director, Venue Manager, and Partnerships Lead",
            Villain = "Manual email chains, spreadsheets, and fragmented booking coordination",
            Delta = "Faster booking turnaround with fewer operational mistakes",
            ConfidenceScore = 0.76m,
            CreatedAtUtc = now.AddDays(-28),
            Status = MissionStatus.NeedsClarification
        };

        mission.ClarificationQuestions.AddRange(
        [
            CreateQuestion("Persona", "Are we targeting the operator who runs bookings or the executive who owns venue utilization?", "Buying authority and pain points can differ.", true, now.AddDays(-27), "Target operations leadership first.", now.AddDays(-27).AddHours(3)),
            CreateQuestion("Surface", "Does the product cover only internal operations or also a vendor-facing portal?", "This changes the mission's product surface.", false, now.AddDays(-26), null, null),
            CreateQuestion("Delta", "Is the primary promise speed, fewer errors, or better vendor coordination?", "The outreach hook needs a sharper delta.", false, now.AddDays(-26).AddHours(1), null, null)
        ]);

        var run = CreateRun(mission, "RUN-VENUE-001", MissionStatus.NeedsClarification, "Central European venue groups", 6400, 7.10m, now.AddDays(-9), null);
        mission.Runs.Add(run);

        AssignAgent(run, FindAgent(agents, AgentRole.Strategist), "Prepare a draft venue-ops mission map pending clarification.", 1200, MissionStatus.NeedsClarification, now.AddDays(-9));
        AssignAgent(run, FindAgent(agents, AgentRole.Scout), "Identify venue groups with multi-location complexity.", 1500, MissionStatus.NeedsClarification, now.AddDays(-9).AddMinutes(10));
        AssignAgent(run, FindAgent(agents, AgentRole.Anchor), "Resolve venue operations leadership roles.", 1400, MissionStatus.NeedsClarification, now.AddDays(-9).AddMinutes(20));
        AssignAgent(run, FindAgent(agents, AgentRole.Soul), "Mine public operations pain signals from venue operators.", 1600, MissionStatus.NeedsClarification, now.AddDays(-9).AddMinutes(35));

        var skyline = CreateCompany("Skyline Venue Group", "skylinevenues.hr", "Corporate venues", "Zagreb", "Croatia", "Regional operator", now.AddDays(-6), 80, true, 0.82m);
        var ivana = CreateContact(
            "Ivana Horvat",
            "Operations Director",
            "Operations",
            "Director",
            true,
            "https://www.linkedin.com/in/ivana-horvat-skyline",
            null,
            null,
            "Oversees venue coordination across several premium locations.",
            now.AddDays(-6));
        ivana.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "ivana.horvat@skylinevenues.hr", true, now.AddDays(-6), "Directory match", 0.90m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/ivana-horvat-skyline", true, now.AddDays(-8), "Netrows profile", 0.95m)
        ]);
        ivana.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Signal, "Coordination friction", "Industry panel", "https://events.example.com/panel/ivana", "Discussed how vendor coordination still happens across email and spreadsheets.", "The handoff between venues and vendors is still too manual.", now.AddDays(-8), 0.84m, true)
        ]);

        var petar = CreateContact(
            "Petar Marin",
            "Vendor Manager",
            "Partnerships",
            "Manager",
            false,
            "https://www.linkedin.com/in/petar-marin-skyline",
            null,
            null,
            "Runs supplier and vendor communication for bookings.",
            now.AddDays(-7));
        petar.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "petar.marin@skylinevenues.hr", true, now.AddDays(-7), "Directory match", 0.89m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/petar-marin-skyline", true, now.AddDays(-9), "Netrows profile", 0.93m)
        ]);
        petar.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Content, "Vendor workflow note", "LinkedIn", "https://linkedin.example.com/posts/petar-marin-2", "Shared notes about confirming vendor availability across multiple event requests.", "Availability tracking still lives in shared sheets.", now.AddDays(-9), 0.79m, true)
        ]);
        skyline.Contacts.AddRange([ivana, petar]);

        var atlas = CreateCompany("Atlas Events Collective", "atlasevents.at", "Event operations", "Vienna", "Austria", "Regional operator", now.AddDays(-7), 110, true, 0.79m);
        var sofia = CreateContact(
            "Sofia Klein",
            "Venue Operations Lead",
            "Operations",
            "Lead",
            true,
            "https://www.linkedin.com/in/sofia-klein-atlas",
            null,
            null,
            "Coordinates booking flow across venues and internal teams.",
            now.AddDays(-7));
        sofia.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "sofia.klein@atlasevents.at", true, now.AddDays(-7), "Directory match", 0.88m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/sofia-klein-atlas", true, now.AddDays(-10), "Netrows profile", 0.94m)
        ]);
        sofia.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Signal, "Booking turnaround delay", "Operations blog", "https://atlasevents.at/blog/ops-update", "Mentioned the difficulty of reducing turnaround time when booking data is fragmented.", "Our teams still chase booking context across inboxes.", now.AddDays(-10), 0.82m, true)
        ]);

        var lukas = CreateContact(
            "Lukas Gruber",
            "Partnerships Manager",
            "Partnerships",
            "Manager",
            false,
            "https://www.linkedin.com/in/lukas-gruber-atlas",
            null,
            null,
            "Influences how vendors and venues collaborate operationally.",
            now.AddDays(-8));
        lukas.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "lukas.gruber@atlasevents.at", true, now.AddDays(-8), "Directory match", 0.87m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/lukas-gruber-atlas", true, now.AddDays(-11), "Netrows profile", 0.92m)
        ]);
        lukas.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Content, "Vendor communication load", "Conference recap", "https://events.example.com/recap/lukas", "Described the manual communication load involved in confirming vendor details.", "Each booking still triggers too many repetitive follow-ups.", now.AddDays(-11), 0.78m, true)
        ]);
        atlas.Contacts.AddRange([sofia, lukas]);

        var meridian = CreateCompany("Meridian Spaces", "meridianspaces.cz", "Flexible event spaces", "Prague", "Czech Republic", "Multi-site operator", now.AddDays(-13), 95, true, 0.77m);
        var petra = CreateContact(
            "Petra Novak",
            "Regional Operations Manager",
            "Operations",
            "Manager",
            true,
            "https://www.linkedin.com/in/petra-novak-meridian",
            null,
            null,
            "Coordinates regional venue operations and process standardization.",
            now.AddDays(-10));
        petra.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "petra.novak@meridianspaces.cz", true, now.AddDays(-11), "Directory match", 0.86m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/petra-novak-meridian", true, now.AddDays(-12), "Netrows profile", 0.92m)
        ]);
        petra.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Signal, "Process consistency signal", "Operations interview", "https://meridianspaces.cz/interview/petra", "Explained that every venue still tracks key steps a little differently.", "We need more consistency across venue teams.", now.AddDays(-13), 0.80m, true)
        ]);

        var jan = CreateContact(
            "Jan Svoboda",
            "Booking Systems Lead",
            "Systems",
            "Lead",
            false,
            "https://www.linkedin.com/in/jan-svoboda-meridian",
            null,
            null,
            "Owns some of the internal process and systems improvements.",
            now.AddDays(-12));
        jan.ContactChannels.AddRange(
        [
            CreateChannel(ContactChannelType.WorkEmail, "jan.svoboda@meridianspaces.cz", true, now.AddDays(-12), "Directory match", 0.85m),
            CreateChannel(ContactChannelType.LinkedIn, "https://www.linkedin.com/in/jan-svoboda-meridian", true, now.AddDays(-13), "Netrows profile", 0.91m)
        ]);
        jan.EvidencePoints.AddRange(
        [
            CreateEvidence(EvidenceKind.Content, "Workflow mapping note", "LinkedIn", "https://linkedin.example.com/posts/jan-svoboda-19", "Shared internal workflow mapping work for booking operations.", "We are documenting too many manual handoffs.", now.AddDays(-15), 0.77m, true)
        ]);
        meridian.Contacts.AddRange([petra, jan]);

        run.TargetCompanies.AddRange([skyline, atlas, meridian]);

        return mission;
    }

    private static SwarmAgent FindAgent(IEnumerable<SwarmAgent> agents, AgentRole role)
    {
        return agents.Single(agent => agent.Role == role);
    }

    private static MissionRun CreateRun(
        BusinessDnaMission mission,
        string runCode,
        MissionStatus status,
        string searchRegion,
        int tokenBudget,
        decimal estimatedCostUsd,
        DateTime startedAtUtc,
        DateTime? completedAtUtc)
    {
        return new MissionRun
        {
            Id = Guid.NewGuid(),
            RunCode = runCode,
            BusinessDnaMissionId = mission.Id,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc,
            Status = status,
            SearchRegion = searchRegion,
            TokenBudget = tokenBudget,
            EstimatedCostUsd = estimatedCostUsd
        };
    }

    private static ClarificationQuestion CreateQuestion(
        string slotName,
        string prompt,
        string reason,
        bool isAnswered,
        DateTime createdAtUtc,
        string? answer,
        DateTime? answeredAtUtc)
    {
        return new ClarificationQuestion
        {
            Id = Guid.NewGuid(),
            SlotName = slotName,
            Prompt = prompt,
            Reason = reason,
            IsAnswered = isAnswered,
            Answer = answer,
            CreatedAtUtc = createdAtUtc,
            AnsweredAtUtc = answeredAtUtc
        };
    }

    private static void AssignAgent(
        MissionRun run,
        SwarmAgent agent,
        string responsibility,
        int tokenBudget,
        MissionStatus status,
        DateTime assignedAtUtc)
    {
        var assignment = new MissionAgentAssignment
        {
            Id = Guid.NewGuid(),
            MissionRunId = run.Id,
            SwarmAgentId = agent.Id,
            AssignedAtUtc = assignedAtUtc,
            Responsibility = responsibility,
            TokenBudget = tokenBudget,
            Status = status
        };

        run.AgentAssignments.Add(assignment);
        agent.MissionAssignments.Add(assignment);
    }

    private static TargetCompany CreateCompany(
        string name,
        string domain,
        string industry,
        string headquartersCity,
        string headquartersCountry,
        string? organizationStageLabel,
        DateTime? lastSignalAtUtc,
        int employeeCount,
        bool isHeadquartersVerified,
        decimal matchScore)
    {
        return new TargetCompany
        {
            Id = Guid.NewGuid(),
            Name = name,
            Domain = domain,
            Industry = industry,
            HeadquartersCity = headquartersCity,
            HeadquartersCountry = headquartersCountry,
            OrganizationStageLabel = organizationStageLabel,
            LastSignalAtUtc = lastSignalAtUtc,
            EmployeeCount = employeeCount,
            IsHeadquartersVerified = isHeadquartersVerified,
            MatchScore = matchScore
        };
    }

    private static TargetContact CreateContact(
        string fullName,
        string jobTitle,
        string department,
        string seniority,
        bool isDecisionMaker,
        string? linkedInUrl,
        string? xHandle,
        string? gitHubUsername,
        string opportunitySummary,
        DateTime lastObservedAtUtc)
    {
        return new TargetContact
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            JobTitle = jobTitle,
            Department = department,
            Seniority = seniority,
            IsDecisionMaker = isDecisionMaker,
            LinkedInUrl = linkedInUrl,
            XHandle = xHandle,
            GitHubUsername = gitHubUsername,
            OpportunitySummary = opportunitySummary,
            LastObservedAtUtc = lastObservedAtUtc
        };
    }

    private static ContactChannel CreateChannel(
        ContactChannelType type,
        string value,
        bool isVerified,
        DateTime? verifiedAtUtc,
        string source,
        decimal confidenceScore)
    {
        return new ContactChannel
        {
            Id = Guid.NewGuid(),
            Type = type,
            Value = value,
            IsVerified = isVerified,
            VerifiedAtUtc = verifiedAtUtc,
            Source = source,
            ConfidenceScore = confidenceScore
        };
    }

    private static EvidencePoint CreateEvidence(
        EvidenceKind kind,
        string label,
        string sourcePlatform,
        string sourceUrl,
        string summary,
        string rawSnippet,
        DateTime capturedAtUtc,
        decimal confidenceScore,
        bool isQualificationSignal)
    {
        return new EvidencePoint
        {
            Id = Guid.NewGuid(),
            Kind = kind,
            Label = label,
            SourcePlatform = sourcePlatform,
            SourceUrl = sourceUrl,
            Summary = summary,
            RawSnippet = rawSnippet,
            CapturedAtUtc = capturedAtUtc,
            ConfidenceScore = confidenceScore,
            IsQualificationSignal = isQualificationSignal
        };
    }

    private static LeadDossier CreateDossier(
        MissionRun run,
        TargetCompany company,
        TargetContact contact,
        int leadgenScore,
        string suggestedApproach,
        string advantagePoint,
        DateTime createdAtUtc)
    {
        return new LeadDossier
        {
            Id = Guid.NewGuid(),
            MissionRunId = run.Id,
            TargetCompanyId = company.Id,
            TargetContactId = contact.Id,
            LeadgenScore = leadgenScore,
            SuggestedApproach = suggestedApproach,
            AdvantagePoint = advantagePoint,
            IsReadyForOutreach = leadgenScore >= 8,
            CreatedAtUtc = createdAtUtc,
            LastUpdatedAtUtc = createdAtUtc.AddHours(4),
            SupportingEvidenceCount = contact.EvidencePoints.Count
        };
    }
}
