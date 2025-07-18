using SIP.API.Domain.DTOs.Users.Configurations;
using SIP.API.Domain.Entities.Users;

namespace SIP.API.Domain.Interfaces.Users.Configurations;

public interface IUserConfiguration
{
    public Task<User?> DefaultChangePasswordAsync(UserDefaultChangePasswordDTO dto);

    public Task<User?> UserChangePasswordAsync(UserChangePasswordDTO dto);

    public Task<User?> UserChangeSectorAsync(UserChangeSectorDTO dto);
}