export function buildRosterImportPrompt(sportName: string, positions: readonly string[]): string {
  const positionList = positions.join(' | ');
  const goalie = positions.find((position) => position === 'Goalie' || position === 'Goalkeeper') ?? positions[0];
  const defender = positions.find((position) => position === 'Defenseman' || position === 'Defender');
  const forward = positions.find((position) => position === 'Forward' || position === 'Center' || position === 'RightWing');
  const sectionLines = [
    `- Rows under Mv, Maalivahti or Goalkeeper are players with position "${goalie}".`,
    defender ? `- Rows under Puolustajat or Defenders are players with position "${defender}".` : '',
    forward ? `- Rows under Hyökkääjät, Pelaajat or Forwards may use position "${forward}", or omit position.` : '',
  ].filter((line) => line.length > 0);

  return `You are converting ${sportName} team rosters into one JSON file for the MyLeague roster import. The source may be an Excel workbook, a PDF, a photo, or several sheets with different layouts.

# How to deliver the result
- Do not paste the JSON into the chat. Do not wrap it in a markdown code block.
- Create a downloadable file named myleague-roster-import.json and attach it so the user can download it.
- The file contains only the JSON object. No commentary, no markdown fences, no trailing text inside the file.
- In the chat, write one short sentence that the file is ready to download. Do not repeat the JSON there.

# Strict file rules
- The file is one JSON object that exactly matches the schema below.
- Use UTF-8. Preserve Finnish and Swedish characters verbatim.
- Use double quotes for every key and string. No trailing commas, no comments inside the JSON.
- Never invent players. If a name or number is not visible, omit that optional field.
- Each worksheet, or each block titled JOUKKUEEN NIMI, is one team. If the name cell is empty, use the sheet name.
- Skip heading rows. They are not players. Examples: Mv, Maalivahti, Pelaajat, Puolustajat, Hyökkääjät, Kenttäpelaajat.
${sectionLines.join('\n')}
- Split "Etunimi Sukunimi" into firstName and lastName. The last word is the last name. Hyphenated names stay in one part, for example "Juho-Pekka" is a first name.
- If a row has only one name, omit that row.
- jerseyNumber is an integer from 1 to 99. Omit 0 and anything that is not a jersey number.
- Do not include coach, jersey colour, captain or referee columns.

# Schema (field names are case-sensitive)
{
  "$schema": "myleague-roster-import/v1",
  "teams": [
    {
      "name": string,
      "players": [
        { "firstName": string, "lastName": string, "jerseyNumber"?: number, "position"?: "${positionList}" }
      ]
    }
  ]
}

# Checklist before you attach the file
- Every player has both firstName and lastName.
- Team names match the names printed on the sheets.
- No trailing commas. The JSON parses with JSON.parse on the first try.
- The chat message does not contain the JSON.

Now create the downloadable myleague-roster-import.json file for the attached roster.
`;
}

export function buildRosterPromptFileName(sport: string): string {
  const now = new Date();
  const pad = (value: number): string => value.toString().padStart(2, '0');
  const stamp = `${now.getFullYear()}${pad(now.getMonth() + 1)}${pad(now.getDate())}`;
  return `myleague-${sport}-roster-import-prompt-${stamp}.txt`;
}
