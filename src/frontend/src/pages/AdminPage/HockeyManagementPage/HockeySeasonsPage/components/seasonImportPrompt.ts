export const HOCKEY_SEASON_IMPORT_AI_PROMPT: string = `You are converting an ice hockey league-season schedule (and optional team-roster sheets) into a single JSON file for the MyLeague season import feature. Output ONLY the JSON object — no markdown fences, no commentary, no trailing text.

# Strict output rules
- Return one JSON object that exactly matches the schema below.
- Use UTF-8. Preserve Finnish/Swedish/etc. diacritics verbatim.
- Use ISO 8601 datetimes with an explicit timezone offset for matches, e.g. "2025-09-13T18:00:00+03:00". The Finnish summer offset is +03:00, winter is +02:00.
- Use plain ISO dates ("YYYY-MM-DD") with NO time for the season's startDate/endDate.
- Use double quotes for every key and string. No trailing commas, no comments inside the JSON.
- Never invent data. If a value isn't visible in the image, omit the optional field. Required fields must always be present.
- If something looks ambiguous, pick the most likely interpretation and continue.

# Schema (all field names case-sensitive)
{
  "$schema": "myleague-season-import/v1",
  "season": {
    "name": string,
    "startDate": "YYYY-MM-DD",
    "endDate": "YYYY-MM-DD",
    "teamCategory"?: "Adult" | "Youth" | "Women",
    "defaultVenue"?: string,
    "seasonCode"?: string
  },
  "divisions": [
    { "name": string, "level"?: number }
  ],
  "clubs": [
    { "name": string, "city"?: string, "country"?: string, "websiteUrl"?: string, "logoUrl"?: string, "contactEmail"?: string }
  ],
  "teams": [
    {
      "name": string,
      "clubName": string,
      "divisionName": string,
      "category"?: "Adult" | "Youth" | "Women",
      "homeArena"?: string,
      "primaryJerseyColor"?: string,
      "secondaryJerseyColor"?: string,
      "players"?: [
        { "firstName": string, "lastName": string, "jerseyNumber"?: number, "position"?: "Goalie" | "Defenseman" | "Center" | "LeftWing" | "RightWing" }
      ]
    }
  ],
  "matches": [
    {
      "matchNumber"?: number,
      "scheduledDateTime": "YYYY-MM-DDThh:mm:ss+03:00",
      "homeTeamName": string,
      "awayTeamName": string,
      "divisionName"?: string,
      "venue"?: string,
      "field"?: string
    }
  ]
}

# Quality checklist before you respond
- Every team listed in "matches" exists in "teams".
- Every team.divisionName exists in "divisions".
- All match datetimes fall between season.startDate and season.endDate (inclusive).
- No trailing commas; the JSON parses with JSON.parse on the first try.

Now produce the JSON for the attached schedule image(s).
`;

export function buildHockeySeasonPromptFileName(): string {
  const now: Date = new Date();
  const pad = (n: number): string => n.toString().padStart(2, '0');
  return `myleague-hockey-season-import-prompt-${now.getFullYear()}${pad(now.getMonth() + 1)}${pad(now.getDate())}.txt`;
}
