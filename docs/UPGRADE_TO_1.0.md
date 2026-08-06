# Upgrade to Nadiano 1.0

## Supported source

The supported direct upgrade source is `0.2.0-beta.1`. Older Alpha installations should first create and verify a full backup.

## Procedure

1. Open the directory containing the Nadiano Compose file.
2. Create a complete cold backup as described in `BACKUP_AND_RESTORE.md`.
3. Change the image to `ghcr.io/juloc/nadiano:1.0.1`.
4. Keep the existing named volume mounted at `/data`.
5. Run `docker compose pull nadiano` and `docker compose up -d nadiano`.
6. Wait for `curl -fsS http://localhost:18200/health/ready`.
7. Verify `curl -fsS http://localhost:18200/api/diagnostics/version` reports `1.0.1`.
8. Open each existing profile and verify recent progress plus private library entries.

## Rollback

Do not start the Beta image against a database already migrated by 1.0. Stop Nadiano, restore the pre-upgrade backup, change the image tag back to `0.2.0-beta.1`, and start the container again.
