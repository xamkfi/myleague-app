// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Seeder;


public sealed class SeederConfiguration
{
    public string BaseUrl { get; set; } = "http://localhost:8080/";
    public List<PersonSeed> Persons { get; set; } = new List<PersonSeed>();
    public List<ClubSeed> Clubs { get; set; } = new List<ClubSeed>();
    public List<DivisionSeed> Divisions { get; set; } = new List<DivisionSeed>();
    public List<PersonSeed> PlayerPersons { get; set; } = new List<PersonSeed>();
    public List<PersonSeed> GoaliePersons { get; set; } = new List<PersonSeed>();
    public List<PersonSeed> RefereePersons { get; set; } = new List<PersonSeed>();
    public List<FloorballSeasonSeed> FloorballSeasons { get; set; } = new List<FloorballSeasonSeed>();
    public List<FloorballTeamSeed> FloorballTeams { get; set; } = new List<FloorballTeamSeed>();
    public List<FloorballMatchSeed> FloorballMatches { get; set; } = new List<FloorballMatchSeed>();

    public static SeederConfiguration Load()
    {
        // Build path to data/testdata.json relative to the base directory
        string testDataPath = Path.Combine(AppContext.BaseDirectory, "data", "testdata.json");

        ConfigurationBuilder builder = new ConfigurationBuilder();
        builder.SetBasePath(AppContext.BaseDirectory);
        builder.AddJsonFile("appsettings.json", optional: true);
        builder.AddJsonFile("appsettings.Development.json", optional: true);

        // Load testdata.json if it exists - it will override/merge with appsettings
        if (File.Exists(testDataPath))
        {
            Console.WriteLine($"Loading test data from: {testDataPath}");
            builder.AddJsonFile(testDataPath, optional: true);
        }

        builder.AddEnvironmentVariables();
        IConfigurationRoot configuration = builder.Build();

        SeederConfiguration cfg = new SeederConfiguration();
        IConfigurationSection seederSection = configuration.GetSection("Seeder");
        seederSection.Bind(cfg);

        string? envBase = configuration["Seeder:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(envBase))
        {
            cfg.BaseUrl = envBase!;
        }

        string? rootBase = configuration["BaseUrl"];
        if (!string.IsNullOrWhiteSpace(rootBase))
        {
            cfg.BaseUrl = rootBase!;
        }

        string? envVar = Environment.GetEnvironmentVariable("SEEDER_BASEURL");
        if (!string.IsNullOrWhiteSpace(envVar))
        {
            cfg.BaseUrl = envVar!;
        }

        return cfg;
    }
}
