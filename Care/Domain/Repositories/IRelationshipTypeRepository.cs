using foll_care.Care.Domain.Model.Entities;

namespace foll_care.Care.Domain.Repositories;

public interface IRelationshipTypeRepository
{
    Task<RelationshipType?> FindByIdAsync(short id);
    Task<IEnumerable<RelationshipType>> ListAsync();
}
