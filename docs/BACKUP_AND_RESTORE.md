# Backup and restore

Nadiano stores all writable state below `/app/data`, mounted by the supplied Compose file from `./nadiano/data`. This includes the SQLite database, imported MusicXML/MXL files, profile progress and review data. Always back up the complete data directory.

## Create a consistent backup

Run these commands from the directory containing `docker-compose.yml`:

```bash
set -euo pipefail
backup_name="nadiano-$(date -u +%Y%m%dT%H%M%SZ)"
mkdir -p "backups/$backup_name"

curl -fsS http://localhost:8087/api/diagnostics/version \
  > "backups/$backup_name/manifest.json"

docker compose stop nadiano

tar -czf "backups/$backup_name/data.tar.gz" -C ./nadiano/data .
tar -tzf "backups/$backup_name/data.tar.gz" >/dev/null
sha256sum "backups/$backup_name/data.tar.gz" \
  > "backups/$backup_name/SHA256SUMS"

docker compose start nadiano
curl -fsS http://localhost:8087/health/ready
```

Stopping the application before copying keeps SQLite and imported files consistent. Store the complete backup directory outside the Docker host.

## Restore

```bash
set -euo pipefail
backup_dir="$PWD/backups/nadiano-YYYYMMDDTHHMMSSZ"

(cd "$backup_dir" && sha256sum -c SHA256SUMS)
tar -tzf "$backup_dir/data.tar.gz" >/dev/null

docker compose stop nadiano
mv ./nadiano/data "./nadiano/data.before-restore-$(date -u +%Y%m%dT%H%M%SZ)"
mkdir -p ./nadiano/data
tar -xzf "$backup_dir/data.tar.gz" -C ./nadiano/data

docker compose start nadiano
curl -fsS http://localhost:8087/health/ready
curl -fsS http://localhost:8087/api/diagnostics/version
```

After restore, verify every profile, recent progress, completed lessons and imported library entries. Keep the previous data directory until the restored deployment has been checked.

## Upgrade and rollback

1. Create a backup before changing the image tag.
2. Pull and start the new image; committed migrations run before readiness becomes healthy.
3. Verify `/health/ready`, the version endpoint and one profile.
4. For rollback, stop the container, restore the pre-upgrade backup and select the previous image tag.

Do not attach a database already migrated by a newer release to an older image. Restore the matching pre-upgrade backup instead.
