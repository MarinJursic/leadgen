using Leadgen.Model.Entities;
using leadgen.Data;
using leadgen.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace leadgen.Controllers.Api;

[ApiController]
[Authorize]
public abstract class LeadgenCrudApiController<TEntity, TDto, TWriteDto> : ControllerBase
    where TEntity : class
{
    protected readonly LeadgenDbContext DbContext;

    protected LeadgenCrudApiController(LeadgenDbContext dbContext)
    {
        DbContext = dbContext;
    }

    [AllowAnonymous]
    [HttpGet]
    public virtual async Task<ActionResult<IReadOnlyList<TDto>>> GetAll([FromQuery] string? query = null)
    {
        var records = await ApplySearch(Query().AsNoTracking(), query)
            .Take(100)
            .ToListAsync();

        return Ok(records.Select(ToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    public virtual async Task<ActionResult<TDto>> GetById(Guid id)
    {
        var entity = await Query().AsNoTracking().FirstOrDefaultAsync(item => EF.Property<Guid>(item, "Id") == id);
        if (entity is null)
        {
            return NotFound();
        }

        return Ok(ToDto(entity));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    public virtual async Task<ActionResult<TDto>> Post([FromBody] TWriteDto request)
    {
        await ValidateWriteAsync(request, ModelState, null);
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var entity = CreateEntity(request);
        DbContext.Set<TEntity>().Add(entity);
        await DbContext.SaveChangesAsync();

        var id = GetId(entity);
        var saved = await Query().AsNoTracking().FirstAsync(item => EF.Property<Guid>(item, "Id") == id);
        return CreatedAtAction(nameof(GetById), new { id }, ToDto(saved));
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPut("{id:guid}")]
    public virtual async Task<ActionResult<TDto>> Put(Guid id, [FromBody] TWriteDto request)
    {
        var entity = await DbContext.Set<TEntity>().FirstOrDefaultAsync(item => EF.Property<Guid>(item, "Id") == id);
        if (entity is null)
        {
            return NotFound();
        }

        await ValidateWriteAsync(request, ModelState, id);
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        UpdateEntity(entity, request);
        await DbContext.SaveChangesAsync();

        var saved = await Query().AsNoTracking().FirstAsync(item => EF.Property<Guid>(item, "Id") == id);
        return Ok(ToDto(saved));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public virtual async Task<IActionResult> Delete(Guid id)
    {
        var entity = await DbContext.Set<TEntity>().FirstOrDefaultAsync(item => EF.Property<Guid>(item, "Id") == id);
        if (entity is null)
        {
            return NotFound();
        }

        await BeforeDeleteAsync(entity);
        DbContext.Set<TEntity>().Remove(entity);
        await DbContext.SaveChangesAsync();

        return NoContent();
    }

    protected virtual IQueryable<TEntity> Query()
    {
        return DbContext.Set<TEntity>();
    }

    protected virtual IQueryable<TEntity> ApplySearch(IQueryable<TEntity> queryable, string? query)
    {
        return queryable;
    }

    protected abstract TDto ToDto(TEntity entity);

    protected abstract TEntity CreateEntity(TWriteDto request);

    protected abstract void UpdateEntity(TEntity entity, TWriteDto request);

    protected virtual Task ValidateWriteAsync(TWriteDto request, ModelStateDictionary modelState, Guid? id)
    {
        return Task.CompletedTask;
    }

    protected virtual Task BeforeDeleteAsync(TEntity entity)
    {
        return Task.CompletedTask;
    }

    protected static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private static Guid GetId(TEntity entity)
    {
        return (Guid)(typeof(TEntity).GetProperty("Id")?.GetValue(entity)
            ?? throw new InvalidOperationException($"{typeof(TEntity).Name} has no Guid Id property."));
    }
}

[Route("api/missions")]
public sealed class BusinessDnaMissionsApiController
    : LeadgenCrudApiController<BusinessDnaMission, BusinessDnaMissionDto, BusinessDnaMissionWriteDto>
{
    public BusinessDnaMissionsApiController(LeadgenDbContext dbContext)
        : base(dbContext)
    {
    }

    protected override IQueryable<BusinessDnaMission> Query()
    {
        return DbContext.BusinessDnaMissions
            .Include(mission => mission.ClarificationQuestions)
            .Include(mission => mission.Runs)
            .Include(mission => mission.Attachments);
    }

    protected override IQueryable<BusinessDnaMission> ApplySearch(IQueryable<BusinessDnaMission> queryable, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return queryable.OrderByDescending(mission => mission.ConfidenceScore);
        }

        var term = query.Trim();
        return queryable
            .Where(mission =>
                mission.MissionName.Contains(term) ||
                mission.ProductName.Contains(term) ||
                mission.Persona.Contains(term) ||
                mission.PrimarySurface.Contains(term))
            .OrderByDescending(mission => mission.ConfidenceScore);
    }

    protected override BusinessDnaMissionDto ToDto(BusinessDnaMission entity)
    {
        return new BusinessDnaMissionDto
        {
            Id = entity.Id,
            MissionName = entity.MissionName,
            ProductName = entity.ProductName,
            Mechanic = entity.Mechanic,
            PrimarySurface = entity.PrimarySurface,
            SurfaceTags = entity.SurfaceTags,
            Persona = entity.Persona,
            Villain = entity.Villain,
            Delta = entity.Delta,
            ConfidenceScore = entity.ConfidenceScore,
            CreatedAtUtc = entity.CreatedAtUtc,
            Status = entity.Status,
            ClarificationQuestionCount = entity.ClarificationQuestions.Count,
            RunCount = entity.Runs.Count,
            AttachmentCount = entity.Attachments.Count
        };
    }

    protected override BusinessDnaMission CreateEntity(BusinessDnaMissionWriteDto request)
    {
        return new BusinessDnaMission
        {
            Id = Guid.NewGuid(),
            MissionName = request.MissionName.Trim(),
            ProductName = request.ProductName.Trim(),
            Mechanic = request.Mechanic.Trim(),
            PrimarySurface = request.PrimarySurface.Trim(),
            SurfaceTags = CleanTags(request.SurfaceTags),
            Persona = request.Persona.Trim(),
            Villain = request.Villain.Trim(),
            Delta = request.Delta.Trim(),
            ConfidenceScore = request.ConfidenceScore,
            CreatedAtUtc = NormalizeUtc(request.CreatedAtUtc),
            Status = request.Status
        };
    }

    protected override void UpdateEntity(BusinessDnaMission entity, BusinessDnaMissionWriteDto request)
    {
        entity.MissionName = request.MissionName.Trim();
        entity.ProductName = request.ProductName.Trim();
        entity.Mechanic = request.Mechanic.Trim();
        entity.PrimarySurface = request.PrimarySurface.Trim();
        entity.SurfaceTags = CleanTags(request.SurfaceTags);
        entity.Persona = request.Persona.Trim();
        entity.Villain = request.Villain.Trim();
        entity.Delta = request.Delta.Trim();
        entity.ConfidenceScore = request.ConfidenceScore;
        entity.CreatedAtUtc = NormalizeUtc(request.CreatedAtUtc);
        entity.Status = request.Status;
    }

    protected override async Task BeforeDeleteAsync(BusinessDnaMission entity)
    {
        var runIds = await DbContext.MissionRuns
            .Where(run => run.BusinessDnaMissionId == entity.Id)
            .Select(run => run.Id)
            .ToListAsync();
        var dossiers = await DbContext.LeadDossiers
            .Where(dossier => runIds.Contains(dossier.MissionRunId))
            .ToListAsync();
        DbContext.LeadDossiers.RemoveRange(dossiers);
    }

    private static List<string> CleanTags(IEnumerable<string> tags)
    {
        return tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

[Route("api/clarification-questions")]
public sealed class ClarificationQuestionsApiController
    : LeadgenCrudApiController<ClarificationQuestion, ClarificationQuestionDto, ClarificationQuestionWriteDto>
{
    public ClarificationQuestionsApiController(LeadgenDbContext dbContext)
        : base(dbContext)
    {
    }

    protected override IQueryable<ClarificationQuestion> Query()
    {
        return DbContext.ClarificationQuestions.Include(question => question.Mission);
    }

    protected override IQueryable<ClarificationQuestion> ApplySearch(IQueryable<ClarificationQuestion> queryable, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return queryable.OrderByDescending(question => question.CreatedAtUtc);
        }

        var term = query.Trim();
        return queryable
            .Where(question =>
                question.SlotName.Contains(term) ||
                question.Prompt.Contains(term) ||
                question.Reason.Contains(term) ||
                (question.Answer != null && question.Answer.Contains(term)))
            .OrderByDescending(question => question.CreatedAtUtc);
    }

    protected override ClarificationQuestionDto ToDto(ClarificationQuestion entity)
    {
        return new ClarificationQuestionDto
        {
            Id = entity.Id,
            Mission = ApiDtoMapper.ToMissionSummary(entity.Mission),
            SlotName = entity.SlotName,
            Prompt = entity.Prompt,
            Reason = entity.Reason,
            IsAnswered = entity.IsAnswered,
            Answer = entity.Answer,
            CreatedAtUtc = entity.CreatedAtUtc,
            AnsweredAtUtc = entity.AnsweredAtUtc
        };
    }

    protected override ClarificationQuestion CreateEntity(ClarificationQuestionWriteDto request)
    {
        return new ClarificationQuestion
        {
            Id = Guid.NewGuid(),
            BusinessDnaMissionId = request.BusinessDnaMissionId,
            SlotName = request.SlotName.Trim(),
            Prompt = request.Prompt.Trim(),
            Reason = request.Reason.Trim(),
            IsAnswered = request.IsAnswered,
            Answer = request.IsAnswered ? request.Answer?.Trim() : null,
            CreatedAtUtc = NormalizeUtc(request.CreatedAtUtc),
            AnsweredAtUtc = request.IsAnswered ? NormalizeUtc(request.AnsweredAtUtc ?? DateTime.UtcNow) : null
        };
    }

    protected override void UpdateEntity(ClarificationQuestion entity, ClarificationQuestionWriteDto request)
    {
        entity.BusinessDnaMissionId = request.BusinessDnaMissionId;
        entity.SlotName = request.SlotName.Trim();
        entity.Prompt = request.Prompt.Trim();
        entity.Reason = request.Reason.Trim();
        entity.IsAnswered = request.IsAnswered;
        entity.Answer = request.IsAnswered ? request.Answer?.Trim() : null;
        entity.CreatedAtUtc = NormalizeUtc(request.CreatedAtUtc);
        entity.AnsweredAtUtc = request.IsAnswered ? NormalizeUtc(request.AnsweredAtUtc ?? entity.AnsweredAtUtc ?? DateTime.UtcNow) : null;
    }

    protected override async Task ValidateWriteAsync(ClarificationQuestionWriteDto request, ModelStateDictionary modelState, Guid? id)
    {
        if (!await DbContext.BusinessDnaMissions.AnyAsync(mission => mission.Id == request.BusinessDnaMissionId))
        {
            modelState.AddModelError(nameof(request.BusinessDnaMissionId), "Select an existing mission.");
        }

        if (request.IsAnswered && string.IsNullOrWhiteSpace(request.Answer))
        {
            modelState.AddModelError(nameof(request.Answer), "Answered questions must include an answer.");
        }

        if (!request.IsAnswered && request.AnsweredAtUtc.HasValue)
        {
            modelState.AddModelError(nameof(request.AnsweredAtUtc), "Unanswered questions cannot have an answered time.");
        }
    }
}

[Route("api/mission-runs")]
public sealed class MissionRunsApiController
    : LeadgenCrudApiController<MissionRun, MissionRunDto, MissionRunWriteDto>
{
    public MissionRunsApiController(LeadgenDbContext dbContext)
        : base(dbContext)
    {
    }

    protected override IQueryable<MissionRun> Query()
    {
        return DbContext.MissionRuns
            .Include(run => run.Mission)
            .Include(run => run.AgentAssignments)
            .Include(run => run.TargetCompanies)
            .Include(run => run.LeadDossiers);
    }

    protected override IQueryable<MissionRun> ApplySearch(IQueryable<MissionRun> queryable, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return queryable.OrderByDescending(run => run.StartedAtUtc);
        }

        var term = query.Trim();
        return queryable
            .Where(run => run.RunCode.Contains(term) || run.SearchRegion.Contains(term))
            .OrderByDescending(run => run.StartedAtUtc);
    }

    protected override MissionRunDto ToDto(MissionRun entity)
    {
        return new MissionRunDto
        {
            Id = entity.Id,
            RunCode = entity.RunCode,
            Mission = ApiDtoMapper.ToMissionSummary(entity.Mission),
            StartedAtUtc = entity.StartedAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc,
            Status = entity.Status,
            SearchRegion = entity.SearchRegion,
            TokenBudget = entity.TokenBudget,
            EstimatedCostUsd = entity.EstimatedCostUsd,
            AssignmentCount = entity.AgentAssignments.Count,
            CompanyCount = entity.TargetCompanies.Count,
            DossierCount = entity.LeadDossiers.Count
        };
    }

    protected override MissionRun CreateEntity(MissionRunWriteDto request)
    {
        return new MissionRun
        {
            Id = Guid.NewGuid(),
            RunCode = request.RunCode.Trim(),
            BusinessDnaMissionId = request.BusinessDnaMissionId,
            StartedAtUtc = NormalizeUtc(request.StartedAtUtc),
            CompletedAtUtc = request.CompletedAtUtc.HasValue ? NormalizeUtc(request.CompletedAtUtc.Value) : null,
            Status = request.Status,
            SearchRegion = request.SearchRegion.Trim(),
            TokenBudget = request.TokenBudget,
            EstimatedCostUsd = request.EstimatedCostUsd
        };
    }

    protected override void UpdateEntity(MissionRun entity, MissionRunWriteDto request)
    {
        entity.RunCode = request.RunCode.Trim();
        entity.BusinessDnaMissionId = request.BusinessDnaMissionId;
        entity.StartedAtUtc = NormalizeUtc(request.StartedAtUtc);
        entity.CompletedAtUtc = request.CompletedAtUtc.HasValue ? NormalizeUtc(request.CompletedAtUtc.Value) : null;
        entity.Status = request.Status;
        entity.SearchRegion = request.SearchRegion.Trim();
        entity.TokenBudget = request.TokenBudget;
        entity.EstimatedCostUsd = request.EstimatedCostUsd;
    }

    protected override async Task ValidateWriteAsync(MissionRunWriteDto request, ModelStateDictionary modelState, Guid? id)
    {
        if (!await DbContext.BusinessDnaMissions.AnyAsync(mission => mission.Id == request.BusinessDnaMissionId))
        {
            modelState.AddModelError(nameof(request.BusinessDnaMissionId), "Select an existing mission.");
        }

        if (request.CompletedAtUtc.HasValue && request.CompletedAtUtc.Value < request.StartedAtUtc)
        {
            modelState.AddModelError(nameof(request.CompletedAtUtc), "Completed time cannot be before started time.");
        }

        var runCode = request.RunCode.Trim();
        if (await DbContext.MissionRuns.AnyAsync(run => run.RunCode == runCode && run.Id != id))
        {
            modelState.AddModelError(nameof(request.RunCode), "Run code must be unique.");
        }
    }

    protected override async Task BeforeDeleteAsync(MissionRun entity)
    {
        var dossiers = await DbContext.LeadDossiers.Where(dossier => dossier.MissionRunId == entity.Id).ToListAsync();
        DbContext.LeadDossiers.RemoveRange(dossiers);
    }
}

[Route("api/mission-agent-assignments")]
public sealed class MissionAgentAssignmentsApiController
    : LeadgenCrudApiController<MissionAgentAssignment, MissionAgentAssignmentDto, MissionAgentAssignmentWriteDto>
{
    public MissionAgentAssignmentsApiController(LeadgenDbContext dbContext)
        : base(dbContext)
    {
    }

    protected override IQueryable<MissionAgentAssignment> Query()
    {
        return DbContext.MissionAgentAssignments
            .Include(assignment => assignment.MissionRun)
            .Include(assignment => assignment.SwarmAgent);
    }

    protected override IQueryable<MissionAgentAssignment> ApplySearch(IQueryable<MissionAgentAssignment> queryable, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return queryable.OrderByDescending(assignment => assignment.AssignedAtUtc);
        }

        var term = query.Trim();
        return queryable
            .Where(assignment => assignment.Responsibility.Contains(term))
            .OrderByDescending(assignment => assignment.AssignedAtUtc);
    }

    protected override MissionAgentAssignmentDto ToDto(MissionAgentAssignment entity)
    {
        return new MissionAgentAssignmentDto
        {
            Id = entity.Id,
            MissionRun = ApiDtoMapper.ToMissionRunSummary(entity.MissionRun),
            SwarmAgent = ApiDtoMapper.ToSwarmAgentSummary(entity.SwarmAgent),
            AssignedAtUtc = entity.AssignedAtUtc,
            Responsibility = entity.Responsibility,
            TokenBudget = entity.TokenBudget,
            Status = entity.Status
        };
    }

    protected override MissionAgentAssignment CreateEntity(MissionAgentAssignmentWriteDto request)
    {
        return new MissionAgentAssignment
        {
            Id = Guid.NewGuid(),
            MissionRunId = request.MissionRunId,
            SwarmAgentId = request.SwarmAgentId,
            AssignedAtUtc = NormalizeUtc(request.AssignedAtUtc),
            Responsibility = request.Responsibility.Trim(),
            TokenBudget = request.TokenBudget,
            Status = request.Status
        };
    }

    protected override void UpdateEntity(MissionAgentAssignment entity, MissionAgentAssignmentWriteDto request)
    {
        entity.MissionRunId = request.MissionRunId;
        entity.SwarmAgentId = request.SwarmAgentId;
        entity.AssignedAtUtc = NormalizeUtc(request.AssignedAtUtc);
        entity.Responsibility = request.Responsibility.Trim();
        entity.TokenBudget = request.TokenBudget;
        entity.Status = request.Status;
    }

    protected override async Task ValidateWriteAsync(MissionAgentAssignmentWriteDto request, ModelStateDictionary modelState, Guid? id)
    {
        if (!await DbContext.MissionRuns.AnyAsync(run => run.Id == request.MissionRunId))
        {
            modelState.AddModelError(nameof(request.MissionRunId), "Select an existing run.");
        }

        if (!await DbContext.SwarmAgents.AnyAsync(agent => agent.Id == request.SwarmAgentId))
        {
            modelState.AddModelError(nameof(request.SwarmAgentId), "Select an existing agent.");
        }
    }
}

[Route("api/swarm-agents")]
public sealed class SwarmAgentsApiController
    : LeadgenCrudApiController<SwarmAgent, SwarmAgentDto, SwarmAgentWriteDto>
{
    public SwarmAgentsApiController(LeadgenDbContext dbContext)
        : base(dbContext)
    {
    }

    protected override IQueryable<SwarmAgent> Query()
    {
        return DbContext.SwarmAgents.Include(agent => agent.MissionAssignments);
    }

    protected override IQueryable<SwarmAgent> ApplySearch(IQueryable<SwarmAgent> queryable, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return queryable.OrderBy(agent => agent.Role).ThenBy(agent => agent.CodeName);
        }

        var term = query.Trim();
        return queryable
            .Where(agent => agent.CodeName.Contains(term) || agent.Provider.Contains(term) || agent.CurrentFocus.Contains(term))
            .OrderBy(agent => agent.Role)
            .ThenBy(agent => agent.CodeName);
    }

    protected override SwarmAgentDto ToDto(SwarmAgent entity)
    {
        return new SwarmAgentDto
        {
            Id = entity.Id,
            CodeName = entity.CodeName,
            Role = entity.Role,
            Provider = entity.Provider,
            Temperature = entity.Temperature,
            MaxConcurrentTasks = entity.MaxConcurrentTasks,
            IsActive = entity.IsActive,
            LastHeartbeatUtc = entity.LastHeartbeatUtc,
            CurrentFocus = entity.CurrentFocus,
            AssignmentCount = entity.MissionAssignments.Count
        };
    }

    protected override SwarmAgent CreateEntity(SwarmAgentWriteDto request)
    {
        return new SwarmAgent
        {
            Id = Guid.NewGuid(),
            CodeName = request.CodeName.Trim(),
            Role = request.Role,
            Provider = request.Provider.Trim(),
            Temperature = request.Temperature,
            MaxConcurrentTasks = request.MaxConcurrentTasks,
            IsActive = request.IsActive,
            LastHeartbeatUtc = NormalizeUtc(request.LastHeartbeatUtc),
            CurrentFocus = request.CurrentFocus.Trim()
        };
    }

    protected override void UpdateEntity(SwarmAgent entity, SwarmAgentWriteDto request)
    {
        entity.CodeName = request.CodeName.Trim();
        entity.Role = request.Role;
        entity.Provider = request.Provider.Trim();
        entity.Temperature = request.Temperature;
        entity.MaxConcurrentTasks = request.MaxConcurrentTasks;
        entity.IsActive = request.IsActive;
        entity.LastHeartbeatUtc = NormalizeUtc(request.LastHeartbeatUtc);
        entity.CurrentFocus = request.CurrentFocus.Trim();
    }

    protected override async Task ValidateWriteAsync(SwarmAgentWriteDto request, ModelStateDictionary modelState, Guid? id)
    {
        var codeName = request.CodeName.Trim();
        if (await DbContext.SwarmAgents.AnyAsync(agent => agent.CodeName == codeName && agent.Id != id))
        {
            modelState.AddModelError(nameof(request.CodeName), "Code name must be unique.");
        }
    }

    protected override async Task BeforeDeleteAsync(SwarmAgent entity)
    {
        var assignments = await DbContext.MissionAgentAssignments
            .Where(assignment => assignment.SwarmAgentId == entity.Id)
            .ToListAsync();
        DbContext.MissionAgentAssignments.RemoveRange(assignments);
    }
}

[Route("api/target-companies")]
public sealed class TargetCompaniesApiController
    : LeadgenCrudApiController<TargetCompany, TargetCompanyDto, TargetCompanyWriteDto>
{
    public TargetCompaniesApiController(LeadgenDbContext dbContext)
        : base(dbContext)
    {
    }

    protected override IQueryable<TargetCompany> Query()
    {
        return DbContext.TargetCompanies
            .Include(company => company.MissionRun)
            .Include(company => company.Contacts)
            .Include(company => company.LeadDossiers);
    }

    protected override IQueryable<TargetCompany> ApplySearch(IQueryable<TargetCompany> queryable, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return queryable.OrderByDescending(company => company.MatchScore);
        }

        var term = query.Trim();
        return queryable
            .Where(company =>
                company.Name.Contains(term) ||
                company.Domain.Contains(term) ||
                company.Industry.Contains(term) ||
                company.HeadquartersCity.Contains(term) ||
                company.HeadquartersCountry.Contains(term))
            .OrderByDescending(company => company.MatchScore);
    }

    protected override TargetCompanyDto ToDto(TargetCompany entity)
    {
        return new TargetCompanyDto
        {
            Id = entity.Id,
            MissionRun = ApiDtoMapper.ToMissionRunSummary(entity.MissionRun),
            Name = entity.Name,
            Domain = entity.Domain,
            Industry = entity.Industry,
            HeadquartersCity = entity.HeadquartersCity,
            HeadquartersCountry = entity.HeadquartersCountry,
            OrganizationStageLabel = entity.OrganizationStageLabel,
            LastSignalAtUtc = entity.LastSignalAtUtc,
            EmployeeCount = entity.EmployeeCount,
            IsHeadquartersVerified = entity.IsHeadquartersVerified,
            MatchScore = entity.MatchScore,
            ContactCount = entity.Contacts.Count,
            DossierCount = entity.LeadDossiers.Count
        };
    }

    protected override TargetCompany CreateEntity(TargetCompanyWriteDto request)
    {
        return new TargetCompany
        {
            Id = Guid.NewGuid(),
            MissionRunId = request.MissionRunId,
            Name = request.Name.Trim(),
            Domain = request.Domain.Trim(),
            Industry = request.Industry.Trim(),
            HeadquartersCity = request.HeadquartersCity.Trim(),
            HeadquartersCountry = request.HeadquartersCountry.Trim(),
            OrganizationStageLabel = request.OrganizationStageLabel?.Trim(),
            LastSignalAtUtc = request.LastSignalAtUtc.HasValue ? NormalizeUtc(request.LastSignalAtUtc.Value) : null,
            EmployeeCount = request.EmployeeCount,
            IsHeadquartersVerified = request.IsHeadquartersVerified,
            MatchScore = request.MatchScore
        };
    }

    protected override void UpdateEntity(TargetCompany entity, TargetCompanyWriteDto request)
    {
        entity.MissionRunId = request.MissionRunId;
        entity.Name = request.Name.Trim();
        entity.Domain = request.Domain.Trim();
        entity.Industry = request.Industry.Trim();
        entity.HeadquartersCity = request.HeadquartersCity.Trim();
        entity.HeadquartersCountry = request.HeadquartersCountry.Trim();
        entity.OrganizationStageLabel = request.OrganizationStageLabel?.Trim();
        entity.LastSignalAtUtc = request.LastSignalAtUtc.HasValue ? NormalizeUtc(request.LastSignalAtUtc.Value) : null;
        entity.EmployeeCount = request.EmployeeCount;
        entity.IsHeadquartersVerified = request.IsHeadquartersVerified;
        entity.MatchScore = request.MatchScore;
    }

    protected override async Task ValidateWriteAsync(TargetCompanyWriteDto request, ModelStateDictionary modelState, Guid? id)
    {
        if (!await DbContext.MissionRuns.AnyAsync(run => run.Id == request.MissionRunId))
        {
            modelState.AddModelError(nameof(request.MissionRunId), "Select an existing run.");
        }
    }

    protected override async Task BeforeDeleteAsync(TargetCompany entity)
    {
        var dossiers = await DbContext.LeadDossiers.Where(dossier => dossier.TargetCompanyId == entity.Id).ToListAsync();
        DbContext.LeadDossiers.RemoveRange(dossiers);
    }
}

[Route("api/target-contacts")]
public sealed class TargetContactsApiController
    : LeadgenCrudApiController<TargetContact, TargetContactDto, TargetContactWriteDto>
{
    public TargetContactsApiController(LeadgenDbContext dbContext)
        : base(dbContext)
    {
    }

    protected override IQueryable<TargetContact> Query()
    {
        return DbContext.TargetContacts
            .Include(contact => contact.TargetCompany)
            .Include(contact => contact.ContactChannels)
            .Include(contact => contact.EvidencePoints)
            .Include(contact => contact.LeadDossiers);
    }

    protected override IQueryable<TargetContact> ApplySearch(IQueryable<TargetContact> queryable, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return queryable.OrderByDescending(contact => contact.IsDecisionMaker).ThenBy(contact => contact.FullName);
        }

        var term = query.Trim();
        return queryable
            .Where(contact =>
                contact.FullName.Contains(term) ||
                contact.JobTitle.Contains(term) ||
                contact.Department.Contains(term) ||
                contact.OpportunitySummary.Contains(term))
            .OrderByDescending(contact => contact.IsDecisionMaker)
            .ThenBy(contact => contact.FullName);
    }

    protected override TargetContactDto ToDto(TargetContact entity)
    {
        return new TargetContactDto
        {
            Id = entity.Id,
            TargetCompany = ApiDtoMapper.ToTargetCompanySummary(entity.TargetCompany),
            FullName = entity.FullName,
            JobTitle = entity.JobTitle,
            Department = entity.Department,
            Seniority = entity.Seniority,
            IsDecisionMaker = entity.IsDecisionMaker,
            LinkedInUrl = entity.LinkedInUrl,
            XHandle = entity.XHandle,
            GitHubUsername = entity.GitHubUsername,
            OpportunitySummary = entity.OpportunitySummary,
            LastObservedAtUtc = entity.LastObservedAtUtc,
            ContactChannelCount = entity.ContactChannels.Count,
            EvidencePointCount = entity.EvidencePoints.Count,
            DossierCount = entity.LeadDossiers.Count
        };
    }

    protected override TargetContact CreateEntity(TargetContactWriteDto request)
    {
        return new TargetContact
        {
            Id = Guid.NewGuid(),
            TargetCompanyId = request.TargetCompanyId,
            FullName = request.FullName.Trim(),
            JobTitle = request.JobTitle.Trim(),
            Department = request.Department.Trim(),
            Seniority = request.Seniority.Trim(),
            IsDecisionMaker = request.IsDecisionMaker,
            LinkedInUrl = request.LinkedInUrl?.Trim(),
            XHandle = request.XHandle?.Trim(),
            GitHubUsername = request.GitHubUsername?.Trim(),
            OpportunitySummary = request.OpportunitySummary.Trim(),
            LastObservedAtUtc = NormalizeUtc(request.LastObservedAtUtc)
        };
    }

    protected override void UpdateEntity(TargetContact entity, TargetContactWriteDto request)
    {
        entity.TargetCompanyId = request.TargetCompanyId;
        entity.FullName = request.FullName.Trim();
        entity.JobTitle = request.JobTitle.Trim();
        entity.Department = request.Department.Trim();
        entity.Seniority = request.Seniority.Trim();
        entity.IsDecisionMaker = request.IsDecisionMaker;
        entity.LinkedInUrl = request.LinkedInUrl?.Trim();
        entity.XHandle = request.XHandle?.Trim();
        entity.GitHubUsername = request.GitHubUsername?.Trim();
        entity.OpportunitySummary = request.OpportunitySummary.Trim();
        entity.LastObservedAtUtc = NormalizeUtc(request.LastObservedAtUtc);
    }

    protected override async Task ValidateWriteAsync(TargetContactWriteDto request, ModelStateDictionary modelState, Guid? id)
    {
        if (!await DbContext.TargetCompanies.AnyAsync(company => company.Id == request.TargetCompanyId))
        {
            modelState.AddModelError(nameof(request.TargetCompanyId), "Select an existing company.");
        }
    }

    protected override async Task BeforeDeleteAsync(TargetContact entity)
    {
        var dossiers = await DbContext.LeadDossiers.Where(dossier => dossier.TargetContactId == entity.Id).ToListAsync();
        DbContext.LeadDossiers.RemoveRange(dossiers);
    }
}

[Route("api/contact-channels")]
public sealed class ContactChannelsApiController
    : LeadgenCrudApiController<ContactChannel, ContactChannelDto, ContactChannelWriteDto>
{
    public ContactChannelsApiController(LeadgenDbContext dbContext)
        : base(dbContext)
    {
    }

    protected override IQueryable<ContactChannel> Query()
    {
        return DbContext.ContactChannels.Include(channel => channel.TargetContact);
    }

    protected override IQueryable<ContactChannel> ApplySearch(IQueryable<ContactChannel> queryable, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return queryable.OrderByDescending(channel => channel.IsVerified).ThenBy(channel => channel.Type);
        }

        var term = query.Trim();
        return queryable
            .Where(channel => channel.Value.Contains(term) || channel.Source.Contains(term))
            .OrderByDescending(channel => channel.IsVerified)
            .ThenBy(channel => channel.Type);
    }

    protected override ContactChannelDto ToDto(ContactChannel entity)
    {
        return new ContactChannelDto
        {
            Id = entity.Id,
            TargetContact = ApiDtoMapper.ToTargetContactSummary(entity.TargetContact),
            Type = entity.Type,
            Value = entity.Value,
            IsVerified = entity.IsVerified,
            VerifiedAtUtc = entity.VerifiedAtUtc,
            Source = entity.Source,
            ConfidenceScore = entity.ConfidenceScore
        };
    }

    protected override ContactChannel CreateEntity(ContactChannelWriteDto request)
    {
        return new ContactChannel
        {
            Id = Guid.NewGuid(),
            TargetContactId = request.TargetContactId,
            Type = request.Type,
            Value = request.Value.Trim(),
            IsVerified = request.IsVerified,
            VerifiedAtUtc = request.VerifiedAtUtc.HasValue ? NormalizeUtc(request.VerifiedAtUtc.Value) : null,
            Source = request.Source.Trim(),
            ConfidenceScore = request.ConfidenceScore
        };
    }

    protected override void UpdateEntity(ContactChannel entity, ContactChannelWriteDto request)
    {
        entity.TargetContactId = request.TargetContactId;
        entity.Type = request.Type;
        entity.Value = request.Value.Trim();
        entity.IsVerified = request.IsVerified;
        entity.VerifiedAtUtc = request.VerifiedAtUtc.HasValue ? NormalizeUtc(request.VerifiedAtUtc.Value) : null;
        entity.Source = request.Source.Trim();
        entity.ConfidenceScore = request.ConfidenceScore;
    }

    protected override async Task ValidateWriteAsync(ContactChannelWriteDto request, ModelStateDictionary modelState, Guid? id)
    {
        if (!await DbContext.TargetContacts.AnyAsync(contact => contact.Id == request.TargetContactId))
        {
            modelState.AddModelError(nameof(request.TargetContactId), "Select an existing contact.");
        }

        if (request.IsVerified && !request.VerifiedAtUtc.HasValue)
        {
            modelState.AddModelError(nameof(request.VerifiedAtUtc), "Verified channels need a verification time.");
        }
    }
}

[Route("api/evidence-points")]
public sealed class EvidencePointsApiController
    : LeadgenCrudApiController<EvidencePoint, EvidencePointDto, EvidencePointWriteDto>
{
    public EvidencePointsApiController(LeadgenDbContext dbContext)
        : base(dbContext)
    {
    }

    protected override IQueryable<EvidencePoint> Query()
    {
        return DbContext.EvidencePoints.Include(evidence => evidence.TargetContact);
    }

    protected override IQueryable<EvidencePoint> ApplySearch(IQueryable<EvidencePoint> queryable, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return queryable.OrderByDescending(evidence => evidence.CapturedAtUtc);
        }

        var term = query.Trim();
        return queryable
            .Where(evidence =>
                evidence.Label.Contains(term) ||
                evidence.SourcePlatform.Contains(term) ||
                evidence.Summary.Contains(term) ||
                evidence.RawSnippet.Contains(term))
            .OrderByDescending(evidence => evidence.CapturedAtUtc);
    }

    protected override EvidencePointDto ToDto(EvidencePoint entity)
    {
        return new EvidencePointDto
        {
            Id = entity.Id,
            TargetContact = ApiDtoMapper.ToTargetContactSummary(entity.TargetContact),
            Kind = entity.Kind,
            Label = entity.Label,
            SourcePlatform = entity.SourcePlatform,
            SourceUrl = entity.SourceUrl,
            Summary = entity.Summary,
            RawSnippet = entity.RawSnippet,
            CapturedAtUtc = entity.CapturedAtUtc,
            ConfidenceScore = entity.ConfidenceScore,
            IsQualificationSignal = entity.IsQualificationSignal
        };
    }

    protected override EvidencePoint CreateEntity(EvidencePointWriteDto request)
    {
        return new EvidencePoint
        {
            Id = Guid.NewGuid(),
            TargetContactId = request.TargetContactId,
            Kind = request.Kind,
            Label = request.Label.Trim(),
            SourcePlatform = request.SourcePlatform.Trim(),
            SourceUrl = request.SourceUrl.Trim(),
            Summary = request.Summary.Trim(),
            RawSnippet = request.RawSnippet.Trim(),
            CapturedAtUtc = NormalizeUtc(request.CapturedAtUtc),
            ConfidenceScore = request.ConfidenceScore,
            IsQualificationSignal = request.IsQualificationSignal
        };
    }

    protected override void UpdateEntity(EvidencePoint entity, EvidencePointWriteDto request)
    {
        entity.TargetContactId = request.TargetContactId;
        entity.Kind = request.Kind;
        entity.Label = request.Label.Trim();
        entity.SourcePlatform = request.SourcePlatform.Trim();
        entity.SourceUrl = request.SourceUrl.Trim();
        entity.Summary = request.Summary.Trim();
        entity.RawSnippet = request.RawSnippet.Trim();
        entity.CapturedAtUtc = NormalizeUtc(request.CapturedAtUtc);
        entity.ConfidenceScore = request.ConfidenceScore;
        entity.IsQualificationSignal = request.IsQualificationSignal;
    }

    protected override async Task ValidateWriteAsync(EvidencePointWriteDto request, ModelStateDictionary modelState, Guid? id)
    {
        if (!await DbContext.TargetContacts.AnyAsync(contact => contact.Id == request.TargetContactId))
        {
            modelState.AddModelError(nameof(request.TargetContactId), "Select an existing contact.");
        }
    }
}

[Route("api/lead-dossiers")]
public sealed class LeadDossiersApiController
    : LeadgenCrudApiController<LeadDossier, LeadDossierDto, LeadDossierWriteDto>
{
    public LeadDossiersApiController(LeadgenDbContext dbContext)
        : base(dbContext)
    {
    }

    protected override IQueryable<LeadDossier> Query()
    {
        return DbContext.LeadDossiers
            .Include(dossier => dossier.MissionRun)
            .Include(dossier => dossier.TargetCompany)
            .Include(dossier => dossier.TargetContact);
    }

    protected override IQueryable<LeadDossier> ApplySearch(IQueryable<LeadDossier> queryable, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return queryable.OrderByDescending(dossier => dossier.LeadgenScore);
        }

        var term = query.Trim();
        return queryable
            .Where(dossier =>
                dossier.SuggestedApproach.Contains(term) ||
                dossier.AdvantagePoint.Contains(term))
            .OrderByDescending(dossier => dossier.LeadgenScore);
    }

    protected override LeadDossierDto ToDto(LeadDossier entity)
    {
        return new LeadDossierDto
        {
            Id = entity.Id,
            MissionRun = ApiDtoMapper.ToMissionRunSummary(entity.MissionRun),
            TargetCompany = ApiDtoMapper.ToTargetCompanySummary(entity.TargetCompany),
            TargetContact = ApiDtoMapper.ToTargetContactSummary(entity.TargetContact),
            LeadgenScore = entity.LeadgenScore,
            SuggestedApproach = entity.SuggestedApproach,
            AdvantagePoint = entity.AdvantagePoint,
            IsReadyForOutreach = entity.IsReadyForOutreach,
            CreatedAtUtc = entity.CreatedAtUtc,
            LastUpdatedAtUtc = entity.LastUpdatedAtUtc,
            SupportingEvidenceCount = entity.SupportingEvidenceCount
        };
    }

    protected override LeadDossier CreateEntity(LeadDossierWriteDto request)
    {
        return new LeadDossier
        {
            Id = Guid.NewGuid(),
            MissionRunId = request.MissionRunId,
            TargetCompanyId = request.TargetCompanyId,
            TargetContactId = request.TargetContactId,
            LeadgenScore = request.LeadgenScore,
            SuggestedApproach = request.SuggestedApproach.Trim(),
            AdvantagePoint = request.AdvantagePoint.Trim(),
            IsReadyForOutreach = request.IsReadyForOutreach,
            CreatedAtUtc = NormalizeUtc(request.CreatedAtUtc),
            LastUpdatedAtUtc = NormalizeUtc(request.LastUpdatedAtUtc),
            SupportingEvidenceCount = request.SupportingEvidenceCount
        };
    }

    protected override void UpdateEntity(LeadDossier entity, LeadDossierWriteDto request)
    {
        entity.MissionRunId = request.MissionRunId;
        entity.TargetCompanyId = request.TargetCompanyId;
        entity.TargetContactId = request.TargetContactId;
        entity.LeadgenScore = request.LeadgenScore;
        entity.SuggestedApproach = request.SuggestedApproach.Trim();
        entity.AdvantagePoint = request.AdvantagePoint.Trim();
        entity.IsReadyForOutreach = request.IsReadyForOutreach;
        entity.CreatedAtUtc = NormalizeUtc(request.CreatedAtUtc);
        entity.LastUpdatedAtUtc = NormalizeUtc(request.LastUpdatedAtUtc);
        entity.SupportingEvidenceCount = request.SupportingEvidenceCount;
    }

    protected override async Task ValidateWriteAsync(LeadDossierWriteDto request, ModelStateDictionary modelState, Guid? id)
    {
        if (!await DbContext.MissionRuns.AnyAsync(run => run.Id == request.MissionRunId))
        {
            modelState.AddModelError(nameof(request.MissionRunId), "Select an existing run.");
        }

        var company = await DbContext.TargetCompanies.AsNoTracking().FirstOrDefaultAsync(item => item.Id == request.TargetCompanyId);
        if (company is null)
        {
            modelState.AddModelError(nameof(request.TargetCompanyId), "Select an existing company.");
        }
        else if (company.MissionRunId != request.MissionRunId)
        {
            modelState.AddModelError(nameof(request.TargetCompanyId), "Selected company must belong to the selected run.");
        }

        var contact = await DbContext.TargetContacts.AsNoTracking().FirstOrDefaultAsync(item => item.Id == request.TargetContactId);
        if (contact is null)
        {
            modelState.AddModelError(nameof(request.TargetContactId), "Select an existing contact.");
        }
        else if (contact.TargetCompanyId != request.TargetCompanyId)
        {
            modelState.AddModelError(nameof(request.TargetContactId), "Selected contact must belong to the selected company.");
        }

        if (request.LastUpdatedAtUtc < request.CreatedAtUtc)
        {
            modelState.AddModelError(nameof(request.LastUpdatedAtUtc), "Last updated time cannot be before created time.");
        }
    }
}

[Route("api/mission-attachments")]
public sealed class MissionAttachmentsApiController
    : LeadgenCrudApiController<MissionAttachment, MissionAttachmentDto, MissionAttachmentWriteDto>
{
    public MissionAttachmentsApiController(LeadgenDbContext dbContext)
        : base(dbContext)
    {
    }

    protected override IQueryable<MissionAttachment> Query()
    {
        return DbContext.MissionAttachments.Include(attachment => attachment.Mission);
    }

    protected override IQueryable<MissionAttachment> ApplySearch(IQueryable<MissionAttachment> queryable, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return queryable.OrderByDescending(attachment => attachment.CreatedAtUtc);
        }

        var term = query.Trim();
        return queryable
            .Where(attachment => attachment.FileName.Contains(term) || attachment.ContentType.Contains(term))
            .OrderByDescending(attachment => attachment.CreatedAtUtc);
    }

    protected override MissionAttachmentDto ToDto(MissionAttachment entity)
    {
        return new MissionAttachmentDto
        {
            Id = entity.Id,
            Mission = ApiDtoMapper.ToMissionSummary(entity.Mission),
            FileName = entity.FileName,
            Url = entity.FilePath,
            ContentType = entity.ContentType,
            FileSize = entity.FileSize,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }

    protected override MissionAttachment CreateEntity(MissionAttachmentWriteDto request)
    {
        return new MissionAttachment
        {
            Id = Guid.NewGuid(),
            BusinessDnaMissionId = request.BusinessDnaMissionId,
            FileName = Path.GetFileName(request.FileName.Trim()),
            StorageFileName = Path.GetFileName(request.FilePath.Trim()),
            FilePath = request.FilePath.Trim(),
            ContentType = request.ContentType.Trim(),
            FileSize = request.FileSize,
            CreatedAtUtc = NormalizeUtc(request.CreatedAtUtc),
            UploadedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
        };
    }

    protected override void UpdateEntity(MissionAttachment entity, MissionAttachmentWriteDto request)
    {
        entity.BusinessDnaMissionId = request.BusinessDnaMissionId;
        entity.FileName = Path.GetFileName(request.FileName.Trim());
        entity.StorageFileName = Path.GetFileName(request.FilePath.Trim());
        entity.FilePath = request.FilePath.Trim();
        entity.ContentType = request.ContentType.Trim();
        entity.FileSize = request.FileSize;
        entity.CreatedAtUtc = NormalizeUtc(request.CreatedAtUtc);
    }

    protected override async Task ValidateWriteAsync(MissionAttachmentWriteDto request, ModelStateDictionary modelState, Guid? id)
    {
        if (!await DbContext.BusinessDnaMissions.AnyAsync(mission => mission.Id == request.BusinessDnaMissionId))
        {
            modelState.AddModelError(nameof(request.BusinessDnaMissionId), "Select an existing mission.");
        }

        if (!request.FilePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            modelState.AddModelError(nameof(request.FilePath), "Attachment paths must be stored under /uploads.");
        }
    }
}

internal static class ApiDtoMapper
{
    public static MissionSummaryDto? ToMissionSummary(BusinessDnaMission? mission)
    {
        return mission is null
            ? null
            : new MissionSummaryDto
            {
                Id = mission.Id,
                MissionName = mission.MissionName,
                ProductName = mission.ProductName,
                Status = mission.Status
            };
    }

    public static MissionRunSummaryDto? ToMissionRunSummary(MissionRun? run)
    {
        return run is null
            ? null
            : new MissionRunSummaryDto
            {
                Id = run.Id,
                RunCode = run.RunCode,
                Status = run.Status
            };
    }

    public static SwarmAgentSummaryDto? ToSwarmAgentSummary(SwarmAgent? agent)
    {
        return agent is null
            ? null
            : new SwarmAgentSummaryDto
            {
                Id = agent.Id,
                CodeName = agent.CodeName,
                Role = agent.Role
            };
    }

    public static TargetCompanySummaryDto? ToTargetCompanySummary(TargetCompany? company)
    {
        return company is null
            ? null
            : new TargetCompanySummaryDto
            {
                Id = company.Id,
                Name = company.Name,
                Domain = company.Domain
            };
    }

    public static TargetContactSummaryDto? ToTargetContactSummary(TargetContact? contact)
    {
        return contact is null
            ? null
            : new TargetContactSummaryDto
            {
                Id = contact.Id,
                FullName = contact.FullName,
                JobTitle = contact.JobTitle
            };
    }
}
