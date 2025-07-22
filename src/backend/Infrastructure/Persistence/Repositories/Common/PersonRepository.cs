using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common
{
    /// <summary>
    /// Implementation of the person repository
    /// </summary>
    public class PersonRepository : RepositoryBase<Person, CommonDbContext>, IPersonRepository
    {
        /// <summary>
        /// Initializes a new instance of the PersonRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public PersonRepository(CommonDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a person by ID
        /// </summary>
        /// <param name="id">The person ID</param>
        /// <returns>The person if found, null otherwise</returns>
        public override async Task<Person?> GetByIdAsync(Guid id)
        {
            return await _entities
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Gets multiple persons by their IDs
        /// </summary>
        /// <param name="ids">The person IDs</param>
        /// <returns>A collection of persons found with the specified IDs</returns>
        public async Task<IEnumerable<Person>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            return await _entities
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();
        }

        /// <summary>
        /// Gets a person by full name
        /// </summary>
        /// <param name="firstName">The first name</param>
        /// <param name="lastName">The last name</param>
        /// <returns>The person if found, null otherwise</returns>
        public async Task<Person?> GetByFullNameAsync(string firstName, string lastName)
        {
            return await _entities
                .FirstOrDefaultAsync(p => p.FirstName == firstName && p.LastName == lastName);
        }

        /// <summary>
        /// Gets all persons
        /// </summary>
        /// <returns>A collection of all persons</returns>
        public override async Task<IEnumerable<Person>> GetAllAsync()
        {
            return await _entities
                .ToListAsync();
        }

        /// <summary>
        /// Gets persons by first name
        /// </summary>
        /// <param name="firstName">The first name to filter by</param>
        /// <returns>A collection of persons with the specified first name</returns>
        public async Task<IEnumerable<Person>> GetByFirstNameAsync(string firstName)
        {
            return await _entities
                .Where(p => p.FirstName == firstName)
                .ToListAsync();
        }

        /// <summary>
        /// Gets persons by last name
        /// </summary>
        /// <param name="lastName">The last name to filter by</param>
        /// <returns>A collection of persons with the specified last name</returns>
        public async Task<IEnumerable<Person>> GetByLastNameAsync(string lastName)
        {
            return await _entities
                .Where(p => p.LastName == lastName)
                .ToListAsync();
        }

        /// <summary>
        /// Gets persons by age range
        /// </summary>
        /// <param name="minAge">The minimum age</param>
        /// <param name="maxAge">The maximum age</param>
        /// <returns>A collection of persons within the specified age range</returns>
        public async Task<IEnumerable<Person>> GetByAgeRangeAsync(int minAge, int maxAge)
        {
            DateTime maxBirthDate = DateTime.UtcNow.AddYears(-minAge);
            DateTime minBirthDate = DateTime.UtcNow.AddYears(-maxAge - 1);
            
            return await _entities
                .Where(p => p.BirthDate >= minBirthDate && p.BirthDate <= maxBirthDate)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new person
        /// </summary>
        /// <param name="person">The person to add</param>
        public async override Task AddAsync(Person person)
        {
            await _entities.AddAsync(person);
        }

        /// <summary>
        /// Updates an existing person
        /// </summary>
        /// <param name="person">The person to update</param>
        public override Task UpdateAsync(Person person)
        {
            _entities.Update(person);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes a person
        /// </summary>
        /// <param name="id">The ID of the person to delete</param>
        public async Task DeleteAsync(Guid id)
        {
            Person? person = await GetByIdAsync(id);
            if (person != null)
            {
                _entities.Remove(person);
            }
        }

        /// <summary>
        /// Searches for persons by name
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <returns>A collection of persons matching the search term</returns>
        public async Task<IEnumerable<Person>> SearchByNameAsync(string searchTerm)
        {
            string lowerSearchTerm = searchTerm.ToLower();

            return await _entities
                .Where(p => p.FirstName.Contains(lowerSearchTerm) || 
                           p.LastName.Contains(lowerSearchTerm) ||
                           EF.Functions.ILike((p.FirstName + " " + p.LastName), $"%{lowerSearchTerm}%") ||
                           EF.Functions.ILike(p.LastName + " " + p.FirstName, $"%{lowerSearchTerm}%"))
                .ToListAsync();
        }

        /// <summary>
        /// Checks if a person exists
        /// </summary>
        /// <param name="id">The person ID</param>
        /// <returns>True if the person exists, false otherwise</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _entities.AnyAsync(p => p.Id == id);
        }

        /// <summary>
        /// Checks if a person with the given full name exists
        /// </summary>
        /// <param name="firstName">The first name</param>
        /// <param name="lastName">The last name</param>
        /// <returns>True if a person with the full name exists, false otherwise</returns>
        public async Task<bool> ExistsByFullNameAsync(string firstName, string lastName)
        {
            return await _entities.AnyAsync(p => p.FirstName == firstName && p.LastName == lastName);
        }

        public Task<Person?> GetByEmailAsync(string email)
        {
            return _entities
                .FirstOrDefaultAsync(p => p.ContactInfo != null && p.ContactInfo.Email == email);
        }
    }
} 
