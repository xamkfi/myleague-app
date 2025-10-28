using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Common;
using Domain.Repositories.Common;
using Domain.Entities.Common;

namespace MyLeague.Infrastructure.Services.Common
{
    /// <summary>
    /// Implementation that resolves person names via IPersonRepository (CommonDbContext).
    /// </summary>
    public sealed class PersonNameProvider : IPersonNameProvider
    {
        private readonly IPersonRepository _personRepository;

        public PersonNameProvider(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<string> GetFullNameAsync(Guid personId, CancellationToken cancellationToken = default)
        {
            Person? person = await _personRepository.GetByIdAsync(personId);
            if (person == null)
            {
                return "Unknown";
            }
            return string.IsNullOrWhiteSpace(person.FullName)
                ? "Unknown"
                : person.FullName;
        }
    }
}


