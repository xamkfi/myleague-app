export interface MatchData {
  id: string;
  homeTeam: string;
  awayTeam: string;
  homeScore?: string;
  awayScore?: string;
  date: string;
  link: string;
  status: 'finished' | 'upcoming' | 'live';
  competition: string;
  venue: string;
  kickoffTime?: string;
}

export const mockMatches: MatchData[] = [
  // Finished matches
  {
    id: "match_001",
    homeTeam: "FC Barcelona",
    awayTeam: "Real Madrid",
    homeScore: "2",
    awayScore: "1",
    date: "15.12.2024",
    link: "/matches/001",
    status: "finished",
    competition: "La Liga",
    venue: "Camp Nou"
  },
  {
    id: "match_002",
    homeTeam: "Manchester United",
    awayTeam: "Liverpool",
    homeScore: "0",
    awayScore: "3",
    date: "18.12.2024",
    link: "/matches/002",
    status: "finished",
    competition: "Premier League",
    venue: "Old Trafford"
  },
  {
    id: "match_003",
    homeTeam: "Bayern Munich",
    awayTeam: "Borussia Dortmund",
    homeScore: "4",
    awayScore: "2",
    date: "20.12.2024",
    link: "/matches/003",
    status: "finished",
    competition: "Bundesliga",
    venue: "Allianz Arena"
  },
  // Upcoming matches
  {
    id: "match_004",
    homeTeam: "AC Milan",
    awayTeam: "Inter Milan",
    date: "28.12.2024",
    kickoffTime: "20:45",
    link: "/matches/004",
    status: "upcoming",
    competition: "Serie A",
    venue: "San Siro"
  },
  {
    id: "match_005",
    homeTeam: "Chelsea",
    awayTeam: "Arsenal",
    date: "30.12.2024",
    kickoffTime: "17:30",
    link: "/matches/005",
    status: "upcoming",
    competition: "Premier League",
    venue: "Stamford Bridge"
  },
  {
    id: "match_006",
    homeTeam: "PSG",
    awayTeam: "Marseille",
    date: "02.01.2025",
    kickoffTime: "21:00",
    link: "/matches/006",
    status: "upcoming",
    competition: "Ligue 1",
    venue: "Parc des Princes"
  }
]; 