using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DomainEvents.Floorball;
using Domain.Entities.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.DomainEvents.Projections.Floorball
{
    public sealed class FloorballMatchCreatedProjection : IDomainEventHandler<FloorballMatchCreatedEvent>
    {
        private readonly FloorballDbContext _dbContext;
        private readonly ILogger<FloorballMatchCreatedProjection> _logger;

        public FloorballMatchCreatedProjection(
            FloorballDbContext dbContext,
            ILogger<FloorballMatchCreatedProjection> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task HandleAsync(FloorballMatchCreatedEvent domainEvent)
        {
            // Idempotence: jos rivi on jo olemassa, ei tehdä mitään.
            bool exists = await _dbContext.FloorballMatches
                .AsNoTracking()
                .AnyAsync(m => m.Id == domainEvent.MatchId);
            if (exists)
            {
                _logger.LogDebug("Projection skipped – FloorballMatch {MatchId} already exists", domainEvent.MatchId);
                return;
            }

            // Luo uusi FloorballMatch-rivi. Käytämme parametrillista konstruktoria,
            // joten haetaan tarvittavat navigaatio-entiteetit kevyesti.
            FloorballSeason? season = await _dbContext.FloorballSeasons.FindAsync(domainEvent.SeasonId);
            FloorballTeam? homeTeam = await _dbContext.FloorballTeams.FindAsync(domainEvent.HomeTeamId);
            FloorballTeam? awayTeam = await _dbContext.FloorballTeams.FindAsync(domainEvent.AwayTeamId);

            if (season == null || homeTeam == null || awayTeam == null)
            {
                _logger.LogWarning("Projection failed – required entities missing for match {MatchId}", domainEvent.MatchId);
                return; // tai heitä poikkeus jos haluat pysäyttää käsittelyn
            }

            FloorballMatch match = new FloorballMatch(
                season,
                homeTeam,
                awayTeam,
                domainEvent.ScheduledDateTime,
                domainEvent.Venue);

            _dbContext.FloorballMatches.Add(match);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Projection created FloorballMatch {MatchId}", domainEvent.MatchId);
        }
    }
}
