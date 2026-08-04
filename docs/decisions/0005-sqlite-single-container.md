# ADR-0005: Use SQLite and one application container for 1.0

- Status: Accepted
- Date: 2026-08-04

## Context

The first production target is a small self-hosted household deployment. It needs reliable persistence, simple backup and low operational overhead. A separate database service would add configuration and failure modes without an expected concurrency requirement.

## Decision

Use EF Core with SQLite stored in the persistent `/data` volume. Run the ASP.NET Core application in one non-root Linux container. Store larger imported content files under application-managed directories in the same volume.

Use committed migrations and tested consistent backup/restore procedures.

## Consequences

- minimal Compose and resource use;
- simple local transactions and deployment;
- database writes must respect SQLite concurrency characteristics;
- multi-instance horizontal scaling is not supported for 1.0;
- backups must include both database and imported files consistently;
- future database migration requires an explicit export/migration plan.

## Alternatives considered

- PostgreSQL from the start: rejected because current scale and availability requirements do not justify a second service.
- JSON files only: rejected because relational progress, attempts, constraints and migrations need structured persistence.
- Embedded database plus separate object store: rejected because it adds operations without current scale needs.

## Reconsideration triggers

- concurrent writers or data volume produce measured SQLite limitations;
- high availability or multiple application replicas become required;
- external account/service architecture needs shared transactional storage;
- backup or reporting requirements exceed the embedded model.
