# Roadmap

This is the working plan for qualiopi-tracker. Stories are grouped by epic and tagged with the milestone they belong to.

## Scope

In scope:
- A provider profile with its activity categories, and the 7 criteria and 32 indicators with their applicability.
- An evidence library with versions and validity dates, linked to indicators, and the coverage of each indicator and criterion.
- Audits, non-conformities and corrective actions, a readiness dashboard, reminders, the evidence pack export and read-only auditor access.

Out of scope:
- Not a certification body and not an official tool. Passing the audit is never guaranteed.
- The official indicator texts are not reproduced; the app uses numbers and short titles written for it.
- Not a document management system: files are stored as evidence only, with simple versions.
- No real provider data. The demo runs on a fictional training provider.

## Glossary

| Term | Meaning |
|------|---------|
| Provider | The training organization being certified |
| Activity category | Training courses, skills assessments, accreditation of prior experience (VAE) or apprenticeship. Some indicators apply only to some categories |
| Criterion | One of the 7 quality criteria |
| Indicator | One of the 32 quality indicators, each under one criterion |
| Applicability | Whether an indicator applies to the provider, and why not when it does not |
| Evidence | A document or link that shows how the provider meets one or more indicators |
| Validity date | The date after which a piece of evidence must be renewed |
| Coverage | For a criterion or indicator, whether valid evidence exists |
| Audit | Initial, surveillance or renewal audit, with a date and an auditor |
| Non-conformity | A gap found in an audit, minor or major, on one indicator |
| Corrective action | What the provider does to close a non-conformity, with an owner and a due date |
| Evidence pack | The export of indicators with their evidence, prepared for the auditor |

## Roles

| Role | Can do |
|------|--------|
| Quality lead | Manage the provider profile, indicators, audits and non-conformities, invite members |
| Member | Add and update evidence, own corrective actions |
| Auditor (read-only guest) | See the evidence pack shared for one audit |

## Milestones

| Milestone | Goal |
|-----------|------|
| M0 Foundation | Runnable app: accounts, provider profile with activity categories, criteria and indicators with applicability |
| M1 Evidence | Evidence library, links to indicators, validity dates, coverage |
| M2 Audits | Audit calendar, non-conformities, corrective actions |
| M3 Readiness | Dashboard, reminders, evidence pack export, auditor access |
| M4 Quality | CI, tests, accessibility, docs |

---

## EPIC 1 — Project setup

| ID | Story | Milestone |
|----|-------|-----------|
| SETUP-1 | Repository with README, LICENSE (MIT), .gitignore, .editorconfig | M0 |
| SETUP-2 | .NET solution: Api, Domain, Application, Infrastructure and Tests projects | M0 |
| SETUP-3 | SQLite + EF Core, first migration, database created on startup in dev | M0 |
| SETUP-4 | Angular workspace with the main app and shared libraries | M0 |
| SETUP-5 | GitHub Actions: build and test both API and front end on each push | M4 |

## EPIC 2 — Accounts and provider

| ID | Story | Milestone |
|----|-------|-----------|
| AUTH-1 | As a visitor, I create a provider and its first quality lead account | M0 |
| AUTH-2 | As a user, I log in and get a token | M0 |
| AUTH-3 | As a user, I log out, and I am sent back to login when my session expires | M0 |
| AUTH-4 | As a user, I change my password | M0 |
| AUTH-5 | A demo provider with members, evidence and an audit history is seeded with a documented demo password | M0 |
| ORG-1 | As a quality lead, I edit the provider profile: name, registration number, activity categories | M0 |
| ORG-2 | As a quality lead, I invite members and remove them | M1 |

Acceptance notes:
- Passwords are hashed. Demo emails use `@example.com`.

## EPIC 3 — Framework and applicability

| ID | Story | Milestone |
|----|-------|-----------|
| FRAME-1 | The app ships the 7 criteria and 32 indicators with numbers, short titles and the activity categories they apply to | M0 |
| FRAME-2 | As a quality lead, I see the indicators that apply to my activity categories, grouped by criterion | M0 |
| FRAME-3 | As a quality lead, I mark an applicable indicator as not applicable with a reason (for example, no subcontracting) | M0 |
| FRAME-4 | As a quality lead, I add my own notes to an indicator: how it applies to us, who owns it | M0 |
| FRAME-5 | As a quality lead, I assign an owner to each indicator | M1 |
| FRAME-6 | Changing activity categories updates applicability and lists the indicators added or removed | M1 |

## EPIC 4 — Evidence

| ID | Story | Milestone |
|----|-------|-----------|
| EVID-1 | As a member, I add evidence: a file (PDF, image, Office document) or a link, with a title and a description | M1 |
| EVID-2 | As a member, I attach a piece of evidence to one or more indicators | M1 |
| EVID-3 | As a member, I set a validity date; evidence past that date counts as expired | M1 |
| EVID-4 | As a member, I upload a new version of a file. Earlier versions are kept and listed | M1 |
| EVID-5 | As a member, I search and filter evidence by indicator, criterion, owner and status | M1 |
| EVID-6 | Files are checked by content, size limited and stored outside the web root | M1 |

## EPIC 5 — Coverage

| ID | Story | Milestone |
|----|-------|-----------|
| COVER-1 | Each applicable indicator is covered, expiring soon, expired or missing, from its evidence | M1 |
| COVER-2 | Each criterion shows its coverage rate | M1 |
| COVER-3 | As a quality lead, I see the list of missing and expired indicators first | M1 |

Acceptance notes:
- Coverage is a pure function covered by unit tests: applicability, evidence dates and the "expiring soon" window in, status out.

## EPIC 6 — Audits and non-conformities

| ID | Story | Milestone |
|----|-------|-----------|
| AUDIT-1 | As a quality lead, I record audits: type (initial, surveillance, renewal), dates, certification body, auditor | M2 |
| AUDIT-2 | The app suggests the next audit window from the certification date | M2 |
| AUDIT-3 | As a quality lead, I record non-conformities found in an audit: indicator, minor or major, auditor's finding | M2 |
| AUDIT-4 | As a quality lead, I add corrective actions with owner and due date; the due date defaults from the non-conformity level and can be changed | M2 |
| AUDIT-5 | As an owner, I update my actions and attach the evidence that closes them | M2 |
| AUDIT-6 | As a quality lead, I close a non-conformity when all its actions are done | M2 |
| AUDIT-7 | As a quality lead, I see open non-conformities and late actions | M2 |

## EPIC 7 — Readiness

| ID | Story | Milestone |
|----|-------|-----------|
| READY-1 | Dashboard: coverage by criterion, expiring evidence, open non-conformities, next audit | M3 |
| READY-2 | Members get in-app reminders for evidence expiring soon and actions due soon | M3 |
| READY-3 | As a quality lead, I export the evidence pack for an audit: indicators, notes, evidence list, as a ZIP with an index PDF | M3 |
| READY-4 | As a quality lead, I share read-only access to the evidence pack with the auditor, with an end date | M3 |
| READY-5 | As a quality lead, I see the history of changes on indicators, evidence and non-conformities | M3 |

## EPIC 8 — Quality and docs

| ID | Story | Milestone |
|----|-------|-----------|
| QA-1 | API integration tests for applicability, evidence, coverage and non-conformities | M4 |
| QA-2 | Unit tests for coverage and audit window rules | M4 |
| QA-3 | End-to-end test: add evidence, link it, see coverage change, export the pack (Playwright) | M4 |
| QA-4 | Accessibility pass on the dashboard and indicator pages | M4 |
| QA-5 | Docs: how the framework is modeled, demo data, limits | M4 |

---

## Later ideas

- Email reminders
- Import of evidence from a shared drive
- Several sites or certification scopes for one provider
- French and English UI (the domain is French, so a French UI comes first)
