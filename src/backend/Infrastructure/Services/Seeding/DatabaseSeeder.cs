using Application.Configuration;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.ValueObjects.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.Services.Seeding;

/// <summary>
/// Seeds the database with default users at application startup.
/// In development: always creates a test user if one does not exist.
/// In any environment: creates an admin user if Seed:AdminEmail is configured and the user does not exist.
/// </summary>
public class DatabaseSeeder
{
    private const string DevTestEmail = "test@myleague.local";

    /// <summary>
    /// Seeds default users into the database
    /// </summary>
    /// <param name="serviceProvider">The scoped service provider</param>
    /// <param name="environment">The hosting environment</param>
    /// <param name="configuration">The application configuration</param>
    public async Task SeedAsync(
        IServiceProvider serviceProvider,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        CommonDbContext dbContext = serviceProvider.GetRequiredService<CommonDbContext>();
        ILogger<DatabaseSeeder> logger = serviceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();

        // In development, always ensure the test user exists
        if (environment.IsDevelopment())
        {
            await SeedUserAsync(dbContext, logger, DevTestEmail, "Test", "User", PersonRole.Admin);
            await SeedClubAdminAsync(dbContext, logger);
        }

        // In any environment, seed admin user if configured
        SeedConfiguration seedConfig = new();
        configuration.GetSection(SeedConfiguration.SectionName).Bind(seedConfig);

        if (!string.IsNullOrWhiteSpace(seedConfig.AdminEmail)
            && !string.Equals(seedConfig.AdminEmail, DevTestEmail, StringComparison.OrdinalIgnoreCase))
        {
            await SeedUserAsync(dbContext, logger, seedConfig.AdminEmail, "System", "Administrator", PersonRole.Admin);
        }
    }

    /// <summary>
    /// Creates a Person + User pair if a user with the given email does not already exist
    /// </summary>
    private static async Task SeedUserAsync(
        CommonDbContext dbContext,
        ILogger logger,
        string email,
        string firstName,
        string lastName,
        PersonRole role)
    {
        bool userExists = await dbContext.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        if (userExists)
        {
            logger.LogInformation("Seed user '{Email}' already exists, skipping.", email);
            return;
        }

        Person person = new(
            firstName,
            lastName,
            role: role,
            contactInfo: new ContactInfo(email));

        User user = new(email, person.Id, UserRole.SystemAdmin);
        user.IsActive = true;
        user.IsEmailVerified = true;

        dbContext.Persons.Add(person);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Seeded user '{Email}' with role {Role} (PersonId: {PersonId}, UserId: {UserId}).",
            email, role, person.Id, user.Id);
    }

    private const string DevClubAdminEmail = "clubadmin@myleague.local";
    private const string DevClubAdminClubName = "Tampere Titans";

    /// <summary>
    /// Ensures a development ClubAdmin user exists and is linked to the hockey/floorball
    /// seed club when that club is already present.
    /// </summary>
    private static async Task SeedClubAdminAsync(CommonDbContext dbContext, ILogger logger)
    {
        User? existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == DevClubAdminEmail);
        Guid personId;
        if (existingUser is null)
        {
            Person person = new(
                "Club",
                "Admin",
                role: PersonRole.User,
                contactInfo: new ContactInfo(DevClubAdminEmail));

            User user = new(DevClubAdminEmail, person.Id, UserRole.ClubAdmin);
            user.IsActive = true;
            user.IsEmailVerified = true;

            dbContext.Persons.Add(person);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            personId = person.Id;
            logger.LogInformation(
                "Seeded club admin '{Email}' (PersonId: {PersonId}, UserId: {UserId}).",
                DevClubAdminEmail, person.Id, user.Id);
        }
        else
        {
            personId = existingUser.PersonId;
        }

        Club? club = await dbContext.Clubs.FirstOrDefaultAsync(c => c.Name == DevClubAdminClubName);
        if (club is null)
        {
            logger.LogInformation(
                "Club '{ClubName}' is not seeded yet; club admin '{Email}' has no manager link.",
                DevClubAdminClubName,
                DevClubAdminEmail);
            return;
        }

        bool alreadyLinked = await dbContext.ClubManagers.AnyAsync(m =>
            m.PersonId == personId && m.ClubId == club.Id && m.IsActive);
        if (alreadyLinked)
        {
            return;
        }

        dbContext.ClubManagers.Add(new ClubManager(personId, club.Id));
        await dbContext.SaveChangesAsync();
        logger.LogInformation(
            "Linked club admin '{Email}' to club '{ClubName}'.",
            DevClubAdminEmail,
            DevClubAdminClubName);
    }
}
