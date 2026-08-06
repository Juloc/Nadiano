# Backup and restore

Nadiano stores all writable state in the container directory `/data`. The supplied Compose file mounts the named volume `nadiano-data` there. This includes the SQLite database, imported MusicXML/MXL files, profile progress and review data. Always back up the complete volume.

## Create a consistent backup

Run these commands from the directory containing `docker-compose.yml`:

```bash
set -euo pipefail
backup_name="nadiano-$(date -u +%Y%m%dT%H%M%SZ)"
mkdir -p "backups/$backup_name"

curl -fsS http://localhost:18200/api/diagnostics/version \
  > "backups/$backup_name/manifest.json"

docker compose stop nadiano

docker compose run --rm --no-deps \
  -v "$PWD/backups/$backup_name:/backup" \
  --entrypoint /bin/sh nadiano \
  -c 'tar -czf /backup/data.tar.gz -C /data .'

tar -tzf "backups/$backup_name/data.tar.gz" >/dev/null
sha256sum "backups/$backup_name/data.tar.gz" \
  > "backups/$backup_name/SHA256SUMS"

docker compose start nadiano
curl -fsS http://localhost:18200/health/ready
```

Stopping the application before copying keeps SQLite and imported files consistent. Store the complete backup directory outside the Docker host.

## Restore

```bash
set -euo pipefail
backup_dir="$PWD/backups/nadiano-YYYYMMDDTHHMMSSZ"

(cd "$backup_dir" && sha256sum -c SHA256SUMS)
tar -tzf "$backup_dir/data.tar.gz" >/dev/null

docker compose stop nadiano

docker compose run --rm --no-deps \
  -v "$backup_dir:/backup:ro" \
  --entrypoint /bin/sh nadiano \
  -c 'find /data -mindepth 1 -maxdepth 1 -exec rm -rf {} + && tar -xzf /backup/data.tar.gz -C /data'

docker compose start nadiano
curl -fsS http://localhost:18200/health/ready
curl -fsS http://localhost:18200/api/diagnostics/version
```

After restore, verify every profile, recent progress, completed lessons and imported library entries.

## Upgrade and rollback

1. Create and verify a backup before changing the image tag.
2. Pull and start the new image; committed migrations run before readiness becomes healthy.
3. Verify `/health/ready`, the version endpoint and one existing profile.
4. For rollback, stop the container, restore the pre-upgrade backup and select the previous image tag.

Do not attach a database already migrated by a newer release to an older image. Restore the matching pre-upgrade backup instead.
