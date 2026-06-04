using foll_care.Care.Domain.Model.ValueObjects;

namespace foll_care.Care.Application.OutboundServices;

public interface IUserInfoService
{
    Task<UserInfo?> FindByIdAsync(long userId);
}
