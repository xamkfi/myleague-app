export interface Goalkeeper {
  Id: number;
  Number: number;
  Name: string;
  Age: number;
  MatchesPlayed: number;
  SavePercentage: number;
  GoalsAgainstAverage: number;
  ShutOuts: number;
}

export interface FieldPlayer {
  Id: number;
  Number: number;
  Name: string;
  Age: number;
  MatchesPlayed: number;
  GoalsScored: number;
  Assists: number;
  Points: number;
}

export interface Coach {
  Id: number;
  Name: string;
  Age: number;
}

export interface Team {
  Id: number;
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
