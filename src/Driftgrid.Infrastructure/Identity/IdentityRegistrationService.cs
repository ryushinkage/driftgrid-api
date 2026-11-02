
using Driftgrid.Application.Abstractions.Identity;
using Driftgrid.Application.Features.Auth;
using Driftgrid.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Driftgrid.Domain.Entities;
using Npgsql;

namespace Driftgrid.Infrastructure.Identity;

public sealed class IdentityRegistrationService(
    UserManager<ApplicationUser> userManager,
    DriftgridDbContext dbContext)
    : IUserRegistrationService
{
    public async Task<RegisterPilotResult> RegisterPilotAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        email = email.Trim();

        var existingUser = await userManager.FindByEmailAsync(email); // проверка FindByEmailAsync() удобна, но два одновременных запроса теоретически могут пройти её одновременно

        if (existingUser is not null)
        {
            return new RegisterPilotResult(
                RegisterPilotStatus.DuplicateEmail);
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var createResult = await userManager.CreateAsync( // он проверяет пользователя и пароль, использует настроенную политику Identity и сохраняет хеш пароля
                user,
                password);

            if (!createResult.Succeeded)
            {
                if (createResult.Errors.Any(error =>
                    error.Code is "DuplicateEmail" or "DuplicateUserName"))
                {
                    return new RegisterPilotResult(
                        RegisterPilotStatus.DuplicateEmail);
                }

                return new RegisterPilotResult(
                    RegisterPilotStatus.InvalidInput,
                    Errors: createResult.Errors
                        .Select(error => error.Description)
                        .ToArray());
            }

            var roleResult = await userManager.AddToRoleAsync( // назначаем ApplicationRoles.Pilot на сервере, никакой роли в параметрах запроса нет
                user,
                ApplicationRoles.Pilot);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to assign the default Pilot role.");
            }
            
            var defaultCallsign =
                $"Pilot-{user.Id.ToString("N")[..8]}";

            dbContext.PilotProfiles.Add(
                new PilotProfile(
                    user.Id,
                    defaultCallsign));

            await dbContext.SaveChangesAsync(
                cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);

            return new RegisterPilotResult(
                RegisterPilotStatus.Created,
                UserId: user.Id,
                Email: user.Email);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "UserNameIndex"
            })
        {
            return new RegisterPilotResult(
                RegisterPilotStatus.DuplicateEmail);
        }
    }
}
