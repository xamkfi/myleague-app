/**
 * A person attached to a match (referee or scorekeeper) with a display name.
 * For referees `id` is the referee/official ID; for scorekeepers it is the person ID.
 * `name` is empty when the backend could not resolve the person.
 */
export interface MatchPersonDto {
  id: string;
  name: string;
}
