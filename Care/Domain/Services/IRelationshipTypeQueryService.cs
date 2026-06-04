using foll_care.Care.Domain.Model.Entities;
using foll_care.Care.Domain.Model.Queries;

namespace foll_care.Care.Domain.Services;

public interface IRelationshipTypeQueryService
{
    Task<IEnumerable<RelationshipType>> Handle(ListRelationshipTypesQuery query);
}
