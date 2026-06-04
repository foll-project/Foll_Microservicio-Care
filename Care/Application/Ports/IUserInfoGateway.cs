using foll_care.Care.Domain.Model.ValueObjects;
namespace foll_care.Care.Application.Ports;


public interface IUserInfoGateway
{
    Task<UserInfo?> FindByIdAsync(long userId);
}