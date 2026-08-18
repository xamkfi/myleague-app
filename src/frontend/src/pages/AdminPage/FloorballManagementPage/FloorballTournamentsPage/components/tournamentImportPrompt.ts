/**
 * Static AI prompt that the admin can hand to any vision-capable LLM (ChatGPT, Claude,
 * Gemini, etc.) together with a tournament schedule screenshot. The model's response is
 * the JSON file the import modal accepts.
 *
 * Kept as a separate module (a) so we can copy-to-clipboard / download-as-file without
 * duplicating the text and (b) so unit tests and the dev console can import it. The text
 * deliberately spells out the schema so the prompt is self-contained — the user doesn't
 * have to attach our TypeScript types to the chat.
 */
export const TOURNAMENT_IMPORT_AI_PROMPT: string = `You are converting a floorball tournament schedule (and optional team-roster sheets) into a single JSON file for the MyLeague tournament import feature. Output ONLY the JSON object — no markdown fences, no commentary, no trailing text.

# Strict output rules
- Return one JSON object that exactly matches the schema below.
- Use UTF-8. Preserve Finnish/Swedish/etc. diacritics verbatim ("Minttusilmät", "Pärnäläinen", …).
- Use ISO 8601 datetimes with an explicit timezone offset for matches and playoff slots, e.g. "2026-05-23T18:00:00+03:00". The Finnish summer offset is +03:00, winter is +02:00.
- Use plain ISO dates ("YYYY-MM-DD") with NO time for the tournament's startDate/endDate.
- Use double quotes for every key and string. No trailing commas, no comments inside the JSON.
- Never invent data. If a value isn't visible in the image, omit the optional field. Required fields must always be present.
- If something looks ambiguous (handwritten number, cropped name), pick the most likely interpretation and continue — do not stop to ask.

# Schema (all field names case-sensitive)
{
  "$schema": "myleague-tournament-import/v1",
  "tournament": {
    "name": string,                                  // e.g. "PMT 2026 - Miehet"
    "startDate": "YYYY-MM-DD",
    "endDate": "YYYY-MM-DD",
    "venue": string | null,                          // primary hall, optional
    "contentHtml": string | null,                    // short HTML description, optional
    "groupStageNumberOfPeriods": number,             // typical: 2
    "groupStagePeriodDurationMinutes": number,       // typical: 15
    "groupStageAllowOvertime": boolean,              // typical: false for group stage
    "groupStageOvertimeDurationMinutes": number,     // typical: 5
    "groupStageAllowShootout": boolean,              // typical: false
    "playoffNumberOfPeriods": number,                // typical: 2
    "playoffPeriodDurationMinutes": number,          // typical: 15
    "playoffAllowOvertime": boolean,                 // typical: true
    "playoffOvertimeDurationMinutes": number,        // typical: 5
    "playoffAllowShootout": boolean,                 // typical: true
    "teamsAdvancingPerGroup": number,                // teams per group that go to playoffs (usually 2)
    "hasPlayoffStage": boolean,                      // false if it's group stage only
    "hasThirdPlaceMatch": boolean,                   // true if there is a bronze match
    "teamCategory"?: "Adult" | "Youth" | "Women"     // optional; pre-fills the import modal. Infer from the title (Miehet→Adult, Naiset→Women)
  },
  "clubs": [                                          // one entry per unique club. fields beyond "name" are optional
    { "name": string, "city"?: string, "country"?: string, "websiteUrl"?: string, "logoUrl"?: string, "contactEmail"?: string }
  ],
  "teams": [                                          // one entry per team in the tournament
    {
      "name": string,                                 // team name as printed on the schedule
      "clubName": string,                             // must match a name in "clubs"
      "category"?: "Adult" | "Youth" | "Women",       // optional per-team override; otherwise tournament.teamCategory / the admin dropdown is used
      "homeArena"?: string,                           // optional, only if printed on the sheet
      "primaryJerseyColor"?: string,                  // optional
      "secondaryJerseyColor"?: string,                // optional
      "players"?: [                                   // OPTIONAL roster. Only include teams whose roster is visible in the image.
        { "firstName": string, "lastName": string, "jerseyNumber"?: number, "position"?: "Goalkeeper" | "Defender" | "Forward" }
      ]
    }
  ],
  "groups": [                                         // group stage groups
    { "name": string, "teamNames": [string, ...] }    // teamNames must match entries in "teams"
  ],
  "matches": [                                        // every group-stage match on the schedule, in order
    {
      "matchNumber"?: number,                         // the "#" column on the schedule (optional but helpful)
      "scheduledDateTime": "YYYY-MM-DDThh:mm:ss+03:00",
      "field"?: string,                               // court / field label as printed (e.g. "1", "2")
      "homeTeamName": string,                         // must match a "teams[].name"
      "awayTeamName": string,                         // must match a "teams[].name"
      "groupName": string                             // must match a "groups[].name"
    }
  ],
  "playoffSchedule"?: [                               // OPTIONAL. Include when the schedule lists playoff kickoff times even before teams are known.
    { "round": "QuarterFinal" | "SemiFinal" | "ThirdPlaceMatch" | "Final", "order": number, "scheduledDateTime": "...+03:00", "venue"?: string }
  ]
}

# How to fill in "playoffSchedule"
- "order" is 0-based within its round: QF1→0, QF2→1, …; SF1→0, SF2→1; the final and 3rd-place match are usually order 0.
- Include a "ThirdPlaceMatch" slot only if the schedule shows a bronze match.
- Skip the whole field if the schedule contains no playoff times at all.

# What if the image has MULTIPLE tournaments?
The import handles one tournament per file. If the image clearly shows separate brackets (e.g. "MIEHET" and "NAISET"), respond with the JSON for the tournament the user explicitly mentions in their message. If they don't say which, default to the first one and add a single short note BEFORE the JSON saying which one you picked.

# What if a roster sheet is provided?
Match roster entries to the team name on the same sheet. Use firstName/lastName as printed. Skip jerseyNumber/position if not shown. If no rosters are provided, simply omit the "players" array for every team.

# Quality checklist before you respond
- Every team listed in "matches" exists in "teams".
- Every team in "teams" appears in at least one "groups[].teamNames" entry (unless the tournament is single-group).
- All match datetimes fall between tournament.startDate and tournament.endDate (inclusive).
- No trailing commas anywhere; the JSON parses with JSON.parse on the first try.

Now produce the JSON for the attached schedule image(s).
`;

/**
 * Suggests a filename for the "Download prompt as file" fallback. Time-stamped so the user
 * can keep multiple variants side-by-side.
 */
export function buildPromptFileName(): string {
  const now: Date = new Date();
  const pad = (n: number): string => n.toString().padStart(2, '0');
  return `myleague-tournament-import-prompt-${now.getFullYear()}${pad(now.getMonth() + 1)}${pad(now.getDate())}.txt`;
}
