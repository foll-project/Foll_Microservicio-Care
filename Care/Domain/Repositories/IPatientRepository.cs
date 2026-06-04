using foll_care.Care.Domain.Model.Entities;
using foll_care.Shared.Domain.Repositories;

namespace foll_care.Care.Domain.Repositories;

public interface IPatientRepository : IBaseRepository<Patient>
{
    Task<Patient?> FindByDniAsync(string dni);
    Task<IEnumerable<Patient>> ListForUserAsync(long userId);
}
