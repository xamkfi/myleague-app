using Domain.Entities.Common;

namespace Domain.Repositories.Common;

/// <summary>
/// Repository for managing persons
/// </summary>
public interface IPersonRepository
{
    /// <summary>
    /// Gets a person by ID
    /// </summary>
    /// <param name="id">The person ID</param>
    /// <returns>The person if found, null otherwise</returns>
    Task<Person?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets multiple persons by their IDs
    /// </summary>
    /// <param name="ids">The person IDs</param>
    /// <returns>A collection of persons found with the specified IDs</returns>
    Task<IEnumerable<Person>> GetByIdsAsync(IEnumerable<Guid> ids);
    
    /// <summary>
    /// Gets a person by full name
    /// </summary>
    /// <param name="firstName">The first name</param>
    /// <param name="lastName">The last name</param>
    /// <returns>The person if found, null otherwise</returns>
    Task<Person?> GetByFullNameAsync(string firstName, string lastName);
    
    /// <summary>
    /// Gets a person by email
    /// </summary>
    /// <param name="email">The email</param>
    /// <returns>The person if found, null otherwise</returns>
    Task<Person?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets all persons
    /// </summary>
    /// <returns>A collection of all persons</returns>
    Task<IEnumerable<Person>> GetAllAsync();
    
    /// <summary>
    /// Gets persons by first name
    /// </summary>
    /// <param name="firstName">The first name to filter by</param>
    /// <returns>A collection of persons with the specified first name</returns>
    Task<IEnumerable<Person>> GetByFirstNameAsync(string firstName);
    
    /// <summary>
    /// Gets persons by last name
    /// </summary>
    /// <param name="lastName">The last name to filter by</param>
    /// <returns>A collection of persons with the specified last name</returns>
    Task<IEnumerable<Person>> GetByLastNameAsync(string lastName);
    
    
    /// <summary>
    /// Gets persons by age range
    /// </summary>
    /// <param name="minAge">The minimum age</param>
    /// <param name="maxAge">The maximum age</param>
    /// <returns>A collection of persons within the specified age range</returns>
    Task<IEnumerable<Person>> GetByAgeRangeAsync(int minAge, int maxAge);
    
    /// <summary>
    /// Adds a new person
    /// </summary>
    /// <param name="person">The person to add</param>
    Task AddAsync(Person person);
    
    /// <summary>
    /// Updates an existing person
    /// </summary>
    /// <param name="person">The person to update</param>
    Task UpdateAsync(Person person);
    
    /// <summary>
    /// Deletes a person
    /// </summary>
    /// <param name="id">The ID of the person to delete</param>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Searches for persons by name.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="count">The maximum number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of persons matching the search term.</returns>
    Task<IEnumerable<Person>> SearchByNameAsync(string searchTerm, int count, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a person exists
    /// </summary>
    /// <param name="id">The person ID</param>
    /// <returns>True if the person exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
    
    /// <summary>
    /// Checks if a person with the given full name exists
    /// </summary>
    /// <param name="firstName">The first name</param>
    /// <param name="lastName">The last name</param>
    /// <returns>True if a person with the full name exists, false otherwise</returns>
    Task<bool> ExistsByFullNameAsync(string firstName, string lastName);
} 