using Driftgrid.Infrastructure.Identity;
using Driftgrid.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Driftgrid.IntegrationTests.Identity;

public sealed class IdentitySchemaTests(
    DriftgridApiFactory factory)
    : IClassFixture<DriftgridApiFactory>
{
    [Fact]
    public async Task IdentityRoles_AreCreatedByMigration()
    {
        using var scope = factory.Services.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        Assert.True(
            await roleManager.RoleExistsAsync(
                ApplicationRoles.Pilot));

        Assert.True(
            await roleManager.RoleExistsAsync(
                ApplicationRoles.Judge));

        Assert.True(
            await roleManager.RoleExistsAsync(
                ApplicationRoles.Admin));
    }
}