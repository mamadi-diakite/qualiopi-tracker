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

Early development. Setup instructions will be added as the code lands.

## License

[MIT](LICENSE)
