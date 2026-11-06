# qualiopi-tracker

A web app that helps French training providers prepare and keep their Qualiopi certification.

Qualiopi is the quality certification a training provider needs to receive public or pooled training funds. Its national framework has 7 criteria and 32 indicators, checked in an initial audit, a surveillance audit, then a renewal audit every three years.

The provider says which activities it runs, and the app shows the indicators that apply. The team stores its evidence, links each piece to indicators with a validity date, and sees the coverage of each criterion, missing or expired evidence, the non-conformities found in audits and their corrective actions. Before an audit, the app builds the evidence pack for the auditor.

## Who it is for

Small training providers who manage Qualiopi today with shared folders and spreadsheets.

## What it is not

- Not a certification body and not an official tool. Passing the audit is never guaranteed.
- The official indicator texts are not reproduced; the app uses short titles written for it.
- Not a document management system.

## Stack

- Front end: Angular workspace (app + libraries)
- Back end: ASP.NET Core Web API (C#)
- Database: SQLite with EF Core

## Status

Early development (milestone M0). What works today:

- Create a provider with its registration number and activity categories, together with its first quality lead account. Log in, log out, change your password. Sessions expire after 60 minutes.
- The quality lead edits the provider profile; members can read it.
- See the 7 criteria and 32 indicators with the ones that apply to your activity categories, grouped by criterion. Indicators outside your activities can be shown on demand.
- The quality lead marks an applicable indicator as not applicable with a reason, and adds notes to any indicator: how it applies to you, who owns it.

Evidence, coverage, audits, non-conformities and the audit pack are planned (see [ROADMAP.md](ROADMAP.md)).

## Getting started

### Prerequisites

- [.NET SDK 10.0.403](https://dotnet.microsoft.com/download) or a later 10.0 feature band
- [Node.js 24](https://nodejs.org/) with npm 11

### Run the API

The access tokens are signed with a key that is never stored in the repository. The API refuses to start without it, in every environment. Set a base64 key of at least 32 bytes in the `Jwt__SigningKey` environment variable, in the terminal that runs the API.

Bash:

```bash
export Jwt__SigningKey="$(openssl rand -base64 32)"
cd src/backend
dotnet run --project src/QualiopiTracker.Api --launch-profile http
```

PowerShell:

```powershell
$key = New-Object byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($key)
$env:Jwt__SigningKey = [Convert]::ToBase64String($key)
cd src/backend
dotnet run --project src/QualiopiTracker.Api --launch-profile http
```

Keep the same key between restarts if you want to stay logged in; a new key signs you out.

The API listens on http://localhost:5088. In Development it creates the SQLite database in `src/backend/src/QualiopiTracker.Api/App_Data/`, applies the migrations, and adds the demo data below when the database has no users.

### Run the web app

```bash
cd src/frontend
npm ci
npm start
```

Open http://localhost:4200. Calls to `/api` are sent to the API on port 5088.

### Demo data

All demo accounts use the password `TestDemo123!`. They are fictional and only exist in Development.

| Name | Email | Role | Provider |
|------|-------|------|----------|
| Claire Morel | claire.morel@example.com | Quality lead | Horizon Skills Academy |
| Hugo Lambert | hugo.lambert@example.com | Member | Horizon Skills Academy |
| Sarah Benali | sarah.benali@example.com | Member | Horizon Skills Academy |
| Julien Faure | julien.faure@example.com | Quality lead | Pathway Career Assessments |

Horizon Skills Academy runs training courses and apprenticeships, has a registration number, notes on indicator 2, and indicator 27 (subcontracting) marked as not applicable. Pathway Career Assessments only runs skills assessments, so the apprenticeship indicators are outside its activities; it has notes on indicator 4.

To start again from an empty demo, stop the API and delete `src/backend/src/QualiopiTracker.Api/App_Data/`.

### Run the tests

Back end (with coverage, migrations and generated code excluded). The tests set their own signing key, so no environment variable is needed:

```bash
cd src/backend
dotnet test --settings coverage.runsettings --collect "XPlat Code Coverage"
```

Front end (Vitest, the run fails below 100% coverage):

```bash
cd src/frontend
npm run test:ci
```

## Project layout

```
src/backend/    ASP.NET Core solution: Domain, Application.Contracts, Application, Infrastructure, Api, and their test projects
src/frontend/   Angular workspace: projects/apps/qualiopi-tracker-app and the shared libraries in projects/libs/shared
```

## License

[MIT](LICENSE)
