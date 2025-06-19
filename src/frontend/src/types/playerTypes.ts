export interface Goalkeeper {
  Id: string;
  Number: number;
  Name: string;
  Age: number;
  MatchesPlayed: number;
  SavePercentage: number;
  GoalsAgainstAverage: number;
  ShutOuts: number;
}

export interface FieldPlayer {
  Id: string;
  Number: number;
  Name: string;
  Age: number;
  MatchesPlayed: number;
  GoalsScored: number;
  Assists: number;
  Points: number;
}

export interface Coach {
  Id: string;
  Name: string;
  Age: number;
}

export interface Team {
  Id: string;
  Name: string;
  Players: {
    Goalkeepers: Goalkeeper[];
    Fieldplayers: FieldPlayer[];
    Coach: Coach[];
  };
}

export interface TeamsData {
  Teams: Team[];
}

export type Player = (Goalkeeper | FieldPlayer) & {
  teamName?: string;
};
