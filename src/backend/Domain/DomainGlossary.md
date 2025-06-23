# Floorball League Management System - Domain Glossary

This glossary defines the key terms used in the floorball league management system's domain model. These terms form our "ubiquitous language" - a shared vocabulary used by both developers and domain experts.

## Core Entities

### Person
A physical person in the system. A Person can have multiple roles (Player, Referee, Coach, etc.). Person stores basic identity information and contact details.

### Club
An organization that manages and sponsors floorball teams. A Club typically has multiple teams, possibly in different divisions.


### FloorballTeam
A floorball team belonging to a Club, competing in a specific Division. Has a roster of players.

### FloorballSeason
A specific season of floorball competition, defined by start and end dates. Contains multiple matches.

### FloorballMatch
A single floorball game between two teams (home and away). Tracks scores, events, periods, status, referees, etc.

### FloorballPlayer
A Person with a player role in floorball. Tracks player-specific attributes like position preferences and career statistics.

### FloorballReferee 
A Person with a referee role in floorball. Tracks referee-specific attributes like license status and match count.

### FloorballTeamManager
A Person with a team management role. Handles administrative responsibilities for a floorball team.

## Value Objects

### Address
Represents a physical address with street, city, postal code, and country.

### ContactInfo
Represents contact details with email and phone numbers.

### Position
Represents a player's position preferences in floorball. Contains primary and secondary positions.

### Score
Represents the score of a floorball match with home and away scores.

### FloorballTeamPlayer
Represents a player's assignment to a specific team, including jersey number and team-specific statistics.

## Enums

### FloorballPosition
The playing positions in floorball: Forward, Center, Defender, Goalkeeper.

### FloorballDivision
The competition divisions/levels in floorball.

### FloorballMatchStatus
The status of a match: Scheduled, In Progress, Completed, Postponed, Cancelled.

### FloorballEventType
Types of events that can occur during a match: Goal, Penalty, etc.

### FloorballPenaltyType
Types of penalties that can be assigned during a floorball match.

## Domain Events

### FloorballMatchCreatedEvent
Raised when a new floorball match is created.

### FloorballMatchRescheduledEvent
Raised when a match's schedule is changed.

### FloorballMatchStatusChangedEvent
Raised when a match's status changes (e.g., from Scheduled to In Progress).

### FloorballGoalScoredEvent
Raised when a goal is scored during a match.

### FloorballPenaltyAssignedEvent
Raised when a penalty is assigned during a match.

### FloorballOfficialAssignedEvent
Raised when a referee is assigned to a match.

## News Article
A published piece of content containing information relevant to the league, including match reports, announcements, player updates, and general league news. Each article has a unique identifier, title, HTML content, and optional metadata such as author, category, and tags.
## News Category
A high-level classification system for organizing news content into logical groups such as Match Reports, League News, Player Updates, Team News, Announcements, Events, Transfers, Injuries, and Awards.
## News Tags
Flexible labels applied to news articles for enhanced organization and searchability. Tags allow for cross-cutting categorization beyond the primary category system.
## Content HTML
Rich formatted content of a news article stored as HTML markup, allowing for proper formatting, links, and embedded media within the article body.
## News Summary
An optional brief description or excerpt of a news article used for preview purposes in lists, feeds, or search results.

## Aggregate Roots

The following entities serve as aggregate roots in our domain model:

1. **Club** - Manages membership and teams
2. **Person** - Manages personal details and contacts
3. **FloorballSeason** - Manages schedule and participating teams
4. **FloorballMatch** - Manages all match-related data and events
5. **FloorballTeam** - Manages roster and team details
6. **FloorballPlayer** - Manages player-specific attributes
7. **FloorballTeamManager** - Manages team administrative responsibilities
8. **FloorballReferee** - Manages referee qualifications and match assignments
