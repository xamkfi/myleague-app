using Domain.Common;
using Domain.Entities.Common;
using Domain.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories;

namespace MyLeague.Infrastructure.Persistence.Repositories.Common
{
    /// <summary>
    /// Implementation of the person repository
    /// </summary>
    public class PersonRepository : RepositoryBase<Person, CommonDbContext>, IPersonRepository
    {
        private readonly ILogger<PersonRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the PersonRepository class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        public PersonRepository(CommonDbContext dbContext, ILogger<PersonRepository> logger) : base(dbContext)
        {
            _logger = logger;
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
            List<Guid>? idList = ids?.ToList();
            if (idList == null || idList.Count == 0)
            {
                return new List<Person>();
            }

            return await _entities
                .Where(p => idList.Contains(p.Id))
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
        /// Gets all Persons
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="birthDate"></param>
        /// <param name="isRegistered"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Person>> GetAllAsync(
            int page,
            int pageSize,
            string? firstName,
            string? lastName,
            string? birthDate,
            bool? isRegistered,
            CancellationToken cancellationToken = default)
        {
            if (page <= 0)
            {
                _logger.LogWarning("GetAllAsync called with invalid page number: {Page}", page);
                page = 1;
            }

            if (pageSize <= 0)
            {
                _logger.LogWarning("GetAllAsync called with invalid page size: {PageSize}", pageSize);
                pageSize = 10;
            }

            // Build query
            IQueryable<Person> query = _entities.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(firstName))
            {
                query = query.Where(p => p.FirstName.Contains(firstName));
            }

            if (!string.IsNullOrWhiteSpace(lastName))
            {
                query = query.Where(p => p.LastName.Contains(lastName));
            }

            if (!string.IsNullOrWhiteSpace(birthDate))
            {
                if (DateTime.TryParse(birthDate, out DateTime date))
                {
                    query = query.Where(p => p.BirthDate.HasValue && p.BirthDate.Value.Date == date.Date);
                }
                else
                {
                    _logger.LogWarning("Invalid birth date format provided: {BirthDate}", birthDate);
                }
            }

            // Only apply isRegistered filter if it has a value
            if (isRegistered.HasValue)
            {
                query = query.Where(p => p.IsRegistered == isRegistered.Value);
            }

            // Apply pagination and ordering to the final query
            return await query
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets the total count of persons matching the filters
        /// </summary>
        /// <param name="firstName">Optional first name filter</param>
        /// <param name="lastName">Optional last name filter</param>
        /// <param name="birthDate">Optional birth date filter</param>
        /// <param name="isRegistered">Optional registration status filter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Total count of persons matching the filters</returns>
        public async Task<int> GetCountAsync(
            string? firstName,
            string? lastName,
            string? birthDate,
            bool? isRegistered,
            CancellationToken cancellationToken = default)
        {
            // Build query with same filters as GetAllAsync
            IQueryable<Person> query = _entities.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(firstName))
            {
                query = query.Where(p => p.FirstName.Contains(firstName));
            }

            if (!string.IsNullOrWhiteSpace(lastName))
            {
                query = query.Where(p => p.LastName.Contains(lastName));
            }

            if (!string.IsNullOrWhiteSpace(birthDate))
            {
                if (DateTime.TryParse(birthDate, out DateTime date))
                {
                    query = query.Where(p => p.BirthDate.HasValue && p.BirthDate.Value.Date == date.Date);
                }
                else
                {
                    _logger.LogWarning("Invalid birth date format provided: {BirthDate}", birthDate);
                }
            }

            // Only apply isRegistered filter if it has a value
            if (isRegistered.HasValue)
            {
                query = query.Where(p => p.IsRegistered == isRegistered.Value);
            }

            return await query.CountAsync(cancellationToken);
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
                .Where(p => p.BirthDate.HasValue && p.BirthDate.Value >= minBirthDate && p.BirthDate.Value <= maxBirthDate)
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
        /// <param name="searchTerm">The search term.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A paged result containing persons matching the search term.</returns>
        public async Task<PagedResult<Person>> SearchByNameAsync(string searchTerm, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            string lowercasedTerm = searchTerm.ToLower();
            
            // Build the base query
            IQueryable<Person> query = _entities
                .Where(p => (p.FirstName.ToLower() + " " + p.LastName.ToLower()).Contains(lowercasedTerm) ||
                            (p.LastName.ToLower() + " " + p.FirstName.ToLower()).Contains(lowercasedTerm));

            // Get total count before pagination
            int totalCount = await query.CountAsync(cancellationToken);

            // Apply ordering and pagination
            List<Person> items = await query
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // Create and return paged result
            return PagedResult.Create(items, totalCount, page, pageSize);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Guid>> GetIdsByNameContainsAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Array.Empty<Guid>();
            }

            string lowercasedTerm = searchTerm.Trim().ToLower();

            return await _entities
                .Where(p =>
                    (p.FirstName.ToLower() + " " + p.LastName.ToLower()).Contains(lowercasedTerm) ||
                    (p.LastName.ToLower() + " " + p.FirstName.ToLower()).Contains(lowercasedTerm))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);
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
