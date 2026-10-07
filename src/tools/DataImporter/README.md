# DataImporter

Imports persons from legacy JoomLeague `.jlg` XML export files into MyLeague through the HTTP API.

> **Status: does not work and is superseded.** The tool never logs in. Every endpoint it calls (`GET api/persons/by-email`, `GET api/persons/search`, `POST api/persons`) requires the Admin role (`[Authorize(Roles = AuthRoles.AdminOnly)]` on `PersonsController`). Every create therefore fails with `401 Unauthorized`, and the duplicate checks find nothing. The project still builds and is still in `MyLeague.sln`, but it has not been kept working against the API.
>
> Use [JoomleagueImporter](../JoomleagueImporter/README.md) instead. It reads the full JoomLeague SQL dump, creates persons together with players, teams, seasons, and matches, and authenticates properly.

## When to use it

Do not use it. This README describes what the code does, so that someone can fix it or delete it.

## Prerequisites (if fixed)

- .NET 10 SDK.
- A WebAPI, plus code that logs in as an admin before the import. The tool has no login code today.
- `.jlg` files in a `DataFiles` folder.

## Usage

```bash
dotnet run --project src/tools/DataImporter/DataImporter.csproj
```

There are no command-line options. The tool asks for the API URL and uses the configured value if you press Enter. It adds `http://` if the scheme is missing.

## Configuration

| Key | Default | Meaning |
|-----|---------|---------|
| `BaseUrl` (root level of `appsettings.json`, or the `BaseUrl` environment variable) | `http://localhost:8080` in `appsettings.json`. `https://localhost:5001` if the key is missing. | API base URL, the default answer to the prompt |

## Input and output

- **Input:** every `*.jlg` file in the first `DataFiles` folder found, in this order:
  1. Next to the build output.
  2. In the current directory.
  3. Up to four levels up from the build output.
  4. Next to `DataImporter.csproj`.

  `src/tools/DataImporter/DataFiles/` is gitignored, because the files contain personal data. Only `.gitkeep` is tracked.
- **Format:** JoomLeague XML. The tool reads `<record object="Person">` elements and takes these CDATA fields: `firstname`, `lastname`, `birthday`, `country`, `email`, `phone`, `mobile`, `address`, `zipcode`, `location`, `state`, `address_country`.
- **Behavior per person:**
  - Skipped if the first or last name is missing.
  - Treated as a duplicate if a person with the same email exists, or one with the same first name, last name, and birth date.
  - Otherwise created with `POST api/persons`.
- **Output:** a console summary of created, duplicate, skipped, and failed persons. The exit code is `1` if any create failed, else `0`.

## Caveats

- No authentication, as described above.
- Only persons are imported. No players, clubs, teams, or matches.

## Related

- [JoomleagueImporter](../JoomleagueImporter/README.md): the supported JoomLeague import
- [Root README](../../../README.md)
