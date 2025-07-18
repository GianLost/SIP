using Microsoft.EntityFrameworkCore;
using SIP.API.Domain.DTOs.Users.Configurations;
using SIP.API.Domain.Entities.Users;
using SIP.API.Domain.Interfaces.Users;
using SIP.API.Domain.Interfaces.Users.Configurations;
using SIP.API.Infrastructure.Database;

namespace SIP.API.Domain.Services.Users.Configurations;

public class UserConfigurationService(ApplicationContext context, IUser userService) : IUserConfiguration
{
    private readonly ApplicationContext _context = context;
    private readonly IUser _userService = userService;

    public async Task<User?> DefaultChangePasswordAsync(UserDefaultChangePasswordDTO dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Password))
            return null;

        User? user = await _userService.GetByIdAsync(dto.UserId);

        if (user == null)
            return null;

        user.PasswordHash = dto.Password;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return user;
    }

    public Task<User?> UserChangePasswordAsync(UserChangePasswordDTO dto)
    {
        throw new NotImplementedException();
    }

    public Task<User?> UserChangeSectorAsync(UserChangeSectorDTO dto)
    {
        throw new NotImplementedException();
    }
}