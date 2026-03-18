using System.Net;
using FamilyTree.Configuration;
using FamilyTree.Dto;
using FamilyTree.Dto.Response.Graph;
using FamilyTree.Entity;
using FamilyTree.Errors;
using FamilyTree.Features.Filtering;
using FamilyTree.Repositories.Core;
using FamilyTree.Services.Core;

namespace FamilyTree.Services;

public class PersonService : BaseService<Person, PersonFilterRequest>, IPersonService
{
    private readonly IPersonRepository _personRepository;
    private readonly IRelationshipRepository _relationshipRepository;
    private readonly IUnionRepository _unionRepository;
    private readonly IFamilyAuthorizationService _familyAuthorizationService;

    public PersonService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IPersonRepository personRepository,
        IRelationshipRepository relationshipRepository,
        IUnionRepository unionRepository,
        IFamilyAuthorizationService familyAuthorizationService)
        : base(unitOfWork, httpContextAccessor, personRepository)
    {
        _personRepository = personRepository;
        _relationshipRepository = relationshipRepository;
        _unionRepository = unionRepository;
        _familyAuthorizationService = familyAuthorizationService;
    }

    protected override string[] SearchableProperties =>
    [
        nameof(Person.FirstName),
        nameof(Person.LastName),
        nameof(Person.BirthPlace),
        nameof(Person.DeathPlace)
    ];

    protected override Task ValidateAsync(Person entity, bool isCreate)
    {
        if (string.IsNullOrWhiteSpace(entity.FirstName))
        {
            throw new HttpResponseException(BaseErrorCode.BASE_0003);
        }

        if (string.IsNullOrWhiteSpace(entity.LastName))
        {
            throw new HttpResponseException(BaseErrorCode.BASE_0003);
        }

        if (entity.BirthDate.HasValue && entity.DeathDate.HasValue && entity.DeathDate < entity.BirthDate)
        {
            throw new HttpResponseException(BaseErrorCode.BASE_0003);
        }

        return Task.CompletedTask;
    }

    public override async Task<Person?> AddAsync(Person entity)
    {
        if (entity.FamilyId.HasValue)
        {
            await _familyAuthorizationService.EnsureCanEditFamilyByIdAsync(entity.FamilyId.Value);
        }

        return await base.AddAsync(entity);
    }

    public override async Task<Person?> UpdateAsync(long id, Person entity)
    {
        Person? existingPerson = await _personRepository.GetAsync(id);

        if (existingPerson == null)
        {
            return null;
        }

        if (existingPerson.FamilyId.HasValue)
        {
            await _familyAuthorizationService.EnsureCanEditFamilyByIdAsync(existingPerson.FamilyId.Value);
        }

        if (entity.FamilyId.HasValue)
        {
            await _familyAuthorizationService.EnsureCanEditFamilyByIdAsync(entity.FamilyId.Value);
        }

        return await base.UpdateAsync(id, entity);
    }

    public override async Task<Person?> GetAsync(long id)
    {
        Person? person = await _personRepository.GetAsync(id);

        if (person == null)
        {
            return null;
        }

        await EnsureReadAccessForPersonAsync(person);

        return person;
    }

    public async Task<IList<PersonSummaryDto>> GetParentsAsync(long personId)
    {
        Person person = await GetRequiredPersonAsync(personId);

        await EnsureReadAccessForPersonAsync(person);

        return await _relationshipRepository.GetParentsAsync(person.Id);
    }

    public async Task<IList<PersonSummaryDto>> GetChildrenAsync(long personId)
    {
        Person person = await GetRequiredPersonAsync(personId);

        await EnsureReadAccessForPersonAsync(person);

        return await _relationshipRepository.GetChildrenAsync(person.Id);
    }

    public async Task<PersonTreeDto> GetTreeAsync(long personId)
    {
        Person person = await GetRequiredPersonAsync(personId);

        await EnsureReadAccessForPersonAsync(person);

        PersonSummaryDto? personSummary = await _personRepository.GetSummaryAsync(personId);

        if (personSummary == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0001)),
                HttpStatusCode.NotFound);
        }

        IList<PersonSummaryDto> parents = await _relationshipRepository.GetParentsAsync(personId);
        IList<PersonSummaryDto> children = await _relationshipRepository.GetChildrenAsync(personId);

        IList<PersonSummaryDto> familyMembers = new List<PersonSummaryDto>();

        if (personSummary.FamilyId.HasValue)
        {
            familyMembers = await _personRepository.GetFamilyMembersAsync(personSummary.FamilyId.Value);
            familyMembers = familyMembers
                .Where(x => x.Id != personSummary.Id)
                .ToList();
        }

        return new PersonTreeDto
        {
            Person = personSummary,
            Parents = parents,
            Children = children,
            FamilyMembers = familyMembers
        };
    }

    public async Task<PersonTreeGraphDto> GetGraphAsync(long personId)
    {
        return await GetGraphAsync(personId, GraphConfiguration.DefaultUpDepth, GraphConfiguration.DefaultDownDepth, true, true);
    }

    public async Task<PersonTreeGraphDto> GetGraphAsync(
        long personId,
        int up,
        int down,
        bool includePartners,
        bool includeSiblings)
    {
        if (up < 0)
        {
            up = 0;
        }

        if (down < 0)
        {
            down = 0;
        }

        if (up > GraphConfiguration.MaxUpDepth)
        {
            up = GraphConfiguration.MaxUpDepth;
        }

        if (down > GraphConfiguration.MaxDownDepth)
        {
            down = GraphConfiguration.MaxDownDepth;
        }

        Person person = await GetRequiredPersonAsync(personId);

        await EnsureReadAccessForPersonAsync(person);

        PersonSummaryDto? rootPerson = await _personRepository.GetSummaryAsync(personId);

        if (rootPerson == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0001)),
                HttpStatusCode.NotFound);
        }

        Dictionary<long, PersonTreeNodeDto> nodeMap = new Dictionary<long, PersonTreeNodeDto>();
        Dictionary<string, PersonTreeEdgeDto> edgeMap = new Dictionary<string, PersonTreeEdgeDto>();

        AddNode(nodeMap, rootPerson, true, 0);

        await ExpandAncestorsAsync(rootPerson.Id, up, nodeMap, edgeMap);
        await ExpandDescendantsAsync(rootPerson.Id, down, nodeMap, edgeMap);

        if (includePartners)
        {
            await ExpandPartnersAsync(new List<long> { rootPerson.Id }, nodeMap, edgeMap);
        }

        if (includeSiblings)
        {
            await ExpandSiblingsAsync(rootPerson.Id, nodeMap, edgeMap);
        }

        return new PersonTreeGraphDto
        {
            RootPersonId = rootPerson.Id,
            UpDepth = up,
            DownDepth = down,
            IncludePartners = includePartners,
            IncludeSiblings = includeSiblings,
            Nodes = nodeMap.Values
                .OrderBy(x => x.Level)
                .ThenBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .ToList(),
            Edges = edgeMap.Values
                .OrderBy(x => x.EdgeType)
                .ThenBy(x => x.SourceId)
                .ThenBy(x => x.TargetId)
                .ToList()
        };
    }

    private async Task ExpandAncestorsAsync(
        long rootPersonId,
        int upDepth,
        IDictionary<long, PersonTreeNodeDto> nodeMap,
        IDictionary<string, PersonTreeEdgeDto> edgeMap)
    {
        HashSet<long> currentGeneration = new HashSet<long> { rootPersonId };

        for (int level = 1; level <= upDepth; level++)
        {
            IList<PersonRelationRecordDto> relations =
                await _relationshipRepository.GetParentRelationsForChildrenAsync(currentGeneration.ToList());

            if (relations.Count == 0)
            {
                break;
            }

            List<long> parentIds = relations
                .Select(x => x.SourcePersonId)
                .Distinct()
                .ToList();

            IList<PersonSummaryDto> parentPersons = await _personRepository.GetSummariesByIdsAsync(parentIds);

            foreach (PersonSummaryDto parent in parentPersons)
            {
                AddNode(nodeMap, parent, false, -level);
            }

            foreach (PersonRelationRecordDto relation in relations)
            {
                AddEdge(
                    edgeMap,
                    $"parent-{relation.SourcePersonId}-{relation.TargetPersonId}",
                    relation.SourcePersonId,
                    relation.TargetPersonId,
                    relation.EdgeType);
            }

            currentGeneration = parentIds.ToHashSet();
        }
    }

    private async Task ExpandDescendantsAsync(
        long rootPersonId,
        int downDepth,
        IDictionary<long, PersonTreeNodeDto> nodeMap,
        IDictionary<string, PersonTreeEdgeDto> edgeMap)
    {
        HashSet<long> currentGeneration = new HashSet<long> { rootPersonId };

        for (int level = 1; level <= downDepth; level++)
        {
            IList<PersonRelationRecordDto> relations =
                await _relationshipRepository.GetChildRelationsForParentsAsync(currentGeneration.ToList());

            if (relations.Count == 0)
            {
                break;
            }

            List<long> childIds = relations
                .Select(x => x.TargetPersonId)
                .Distinct()
                .ToList();

            IList<PersonSummaryDto> childPersons = await _personRepository.GetSummariesByIdsAsync(childIds);

            foreach (PersonSummaryDto child in childPersons)
            {
                AddNode(nodeMap, child, false, level);
            }

            foreach (PersonRelationRecordDto relation in relations)
            {
                AddEdge(
                    edgeMap,
                    $"parent-{relation.SourcePersonId}-{relation.TargetPersonId}",
                    relation.SourcePersonId,
                    relation.TargetPersonId,
                    relation.EdgeType);
            }

            currentGeneration = childIds.ToHashSet();
        }
    }

    private async Task ExpandPartnersAsync(
        ICollection<long> personIds,
        IDictionary<long, PersonTreeNodeDto> nodeMap,
        IDictionary<string, PersonTreeEdgeDto> edgeMap)
    {
        IList<PersonRelationRecordDto> unionRelations = await _unionRepository.GetUnionRelationsAsync(personIds);

        if (unionRelations.Count == 0)
        {
            return;
        }

        HashSet<long> partnerIds = new HashSet<long>();

        foreach (PersonRelationRecordDto relation in unionRelations)
        {
            if (personIds.Contains(relation.SourcePersonId))
            {
                partnerIds.Add(relation.TargetPersonId);
            }

            if (personIds.Contains(relation.TargetPersonId))
            {
                partnerIds.Add(relation.SourcePersonId);
            }
        }

        IList<PersonSummaryDto> partners = await _personRepository.GetSummariesByIdsAsync(partnerIds.ToList());

        foreach (PersonSummaryDto partner in partners)
        {
            AddNode(nodeMap, partner, false, 0);
        }

        foreach (PersonRelationRecordDto relation in unionRelations)
        {
            string edgeId =
                $"union-{Math.Min(relation.SourcePersonId, relation.TargetPersonId)}-{Math.Max(relation.SourcePersonId, relation.TargetPersonId)}";

            AddEdge(
                edgeMap,
                edgeId,
                Math.Min(relation.SourcePersonId, relation.TargetPersonId),
                Math.Max(relation.SourcePersonId, relation.TargetPersonId),
                relation.EdgeType);
        }
    }

    private async Task ExpandSiblingsAsync(
        long rootPersonId,
        IDictionary<long, PersonTreeNodeDto> nodeMap,
        IDictionary<string, PersonTreeEdgeDto> edgeMap)
    {
        IList<PersonSummaryDto> parents = await _relationshipRepository.GetParentsAsync(rootPersonId);

        if (parents.Count == 0)
        {
            return;
        }

        HashSet<long> siblingIds = new HashSet<long>();

        foreach (PersonSummaryDto parent in parents)
        {
            IList<PersonSummaryDto> children = await _relationshipRepository.GetChildrenAsync(parent.Id);

            foreach (PersonSummaryDto child in children)
            {
                if (child.Id != rootPersonId)
                {
                    siblingIds.Add(child.Id);
                }
            }
        }

        IList<PersonSummaryDto> siblings = await _personRepository.GetSummariesByIdsAsync(siblingIds.ToList());

        foreach (PersonSummaryDto sibling in siblings)
        {
            AddNode(nodeMap, sibling, false, 0);
        }

        foreach (PersonSummaryDto parent in parents)
        {
            foreach (PersonSummaryDto sibling in siblings)
            {
                AddEdge(
                    edgeMap,
                    $"parent-{parent.Id}-{sibling.Id}",
                    parent.Id,
                    sibling.Id,
                    "parent-child");
            }
        }
    }

    private async Task<Person> GetRequiredPersonAsync(long personId)
    {
        Person? person = await _personRepository.GetAsync(personId);

        if (person == null)
        {
            throw new HttpResponseException(
                new ErrorResponse(BaseErrorCode.BASE_0001, BaseErrorCode.GetDescription(BaseErrorCode.BASE_0001)),
                HttpStatusCode.NotFound);
        }

        return person;
    }

    private async Task EnsureReadAccessForPersonAsync(Person person)
    {
        if (person.FamilyId.HasValue)
        {
            await _familyAuthorizationService.EnsureCanReadFamilyByIdAsync(person.FamilyId.Value);
        }
    }

    private static void AddNode(
        IDictionary<long, PersonTreeNodeDto> nodeMap,
        PersonSummaryDto person,
        bool isRoot,
        int level)
    {
        if (nodeMap.ContainsKey(person.Id))
        {
            if (isRoot)
            {
                nodeMap[person.Id].IsRoot = true;
            }

            if (Math.Abs(level) < Math.Abs(nodeMap[person.Id].Level))
            {
                nodeMap[person.Id].Level = level;
            }

            return;
        }

        nodeMap.Add(person.Id, new PersonTreeNodeDto
        {
            Id = person.Id,
            NodeType = "person",
            FirstName = person.FirstName,
            LastName = person.LastName,
            BirthDate = person.BirthDate,
            DeathDate = person.DeathDate,
            Gender = person.Gender,
            FamilyId = person.FamilyId,
            IsRoot = isRoot,
            IsPublic = person.IsPublic,
            Level = level
        });
    }

    private static void AddEdge(
        IDictionary<string, PersonTreeEdgeDto> edgeMap,
        string id,
        long sourceId,
        long targetId,
        string edgeType)
    {
        if (edgeMap.ContainsKey(id))
        {
            return;
        }

        edgeMap.Add(id, new PersonTreeEdgeDto
        {
            Id = id,
            SourceId = sourceId,
            TargetId = targetId,
            EdgeType = edgeType
        });
    }
}
