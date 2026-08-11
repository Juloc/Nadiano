# Upgrade to current Nadiano 1.0 stable

Current stable target: **1.0.4**.

For the current release policy and version direction, see `MASTER_PLAN.md` and `ROADMAP.md`.

## Supported source

The stable release workflow continuously rehearses preservation of a Beta-era learner profile into the current stable image, plus cold restore and rollback using the matching pre-upgrade backup.

For a normal existing 1.0.x deployment, upgrade directly to the current stable image after creating a verified backup.

Older Alpha/Beta deployments should create and verify a complete `/data` backup before any upgrade.

## Procedure

1. Open the directory containing the Nadiano Compose file.
2. Create a complete cold backup as described in `BACKUP_AND_RESTORE.md`.
3. Change/pin the image to:

   `ghcr.io/juloc/nadiano:1.0.4`

4. Keep the existing persistent volume mounted at `/data`.
5. Run:

   ```bash
   docker compose pull nadiano
   docker compose up -d nadiano
   ```

6. Wait for readiness:

   ```bash
   curl -fsS http://localhost:18200/health/ready
   ```

7. Verify the version endpoint:

   ```bash
   curl -fsS http://localhost:18200/api/diagnostics/version
   ```

   It should report application version `1.0.4`.

8. Open each existing learner profile and verify:

   - recent progress;
   - lesson/course state;
   - review queue/recommendations;
   - private Songs/imported library entries;
   - MIDI setup preference where applicable.

9. Complete one normal practice attempt and confirm the result persists.

## Rollback

Do not attach a database already migrated by a newer release to an older image and assume it is compatible.

For rollback:

1. stop Nadiano;
2. restore the verified **pre-upgrade** `/data` backup;
3. select the image version that matches that backup;
4. start Nadiano;
5. verify readiness, version, profiles, progress and private imports.

The stable release workflow rehearses this backup/restore/rollback model before publishing the current stable release.

## Production Compose

The current `Juloc/docker` Nadiano deployment definition uses:

- image `ghcr.io/juloc/nadiano:1.0.4`;
- host port `18200` → container port `8080`;
- persistent `/data` volume.

Do not switch production to an unreleased tag merely because `main` contains newer planning or development work.