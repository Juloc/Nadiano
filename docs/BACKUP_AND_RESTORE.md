# Alpha backup and restore

Nadiano stores its writable state in the Compose volume mounted at `/data`. The current Alpha contains `nadiano.db`; future imported content and application-managed files will also live below the same directory. Back up the complete volume, not only the database file.

## Create a consistent backup

Run these commands from the directory containing `docker-compose.yml`:

```bash
set -euo pipefail
backup_name="nadiano-$(date -u +%Y%m%dT%H%M%SZ)"
mkdir -p "backups/$backup_name"

curl -fsS http://localhost:8098/api/diagnostics/version \
  > "backups/$backup_name/manifest.json"

docker compose stop nadiano

docker compose run --rm --no-deps --user 0 \
  --entrypoint sh \
  -v "$PWD/backups/$backup_name:/backup" \
  nadiano \
  -c 'tar -czf /backup/data.tar.gz -C /data .'

tar -tzf "backups/$backup_name/data.tar.gz" >/dev/null
test -s "backups/$backup_name/manifest.json"
sha256sum "backups/$backup_name/data.tar.gz" \
  > "backups/$backup_name/SHA256SUMS"

docker compose start nadiano
curl -fsS http://localhost:8098/health/ready
```

Stopping the application before copying keeps SQLite and all related files consistent. The manifest records application, database migration and bundled content versions. Store the whole backup directory outside the Docker host.

## Restore into an empty deployment

1. Keep the failed/current volume until the restored deployment has been checked.
2. Verify `SHA256SUMS` and list the archive before extraction.
3. Stop Nadiano.
4. Restore the complete archive into `/data`.
5. Start the same or a newer supported image; committed migrations run before readiness becomes healthy.

```bash
set -euo pipefail
backup_dir="$PWD/backups/nadiano-YYYYMMDDTHHMMSSZ"

(cd "$backup_dir" && sha256sum -c SHA256SUMS)
tar -tzf "$backup_dir/data.tar.gz" >/dev/null

docker compose stop nadiano

docker compose run --rm --no-deps --user 0 \
  --entrypoint sh \
  -v "$backup_dir:/backup:ro" \
  nadiano \
  -c 'find /data -mindepth 1 -maxdepth 1 -exec rm -rf -- {} + && tar -xzf /backup/data.tar.gz -C /data && chown -R nadiano:nadiano /data'

docker compose start nadiano
curl -fsS http://localhost:8098/health/ready
curl -fsS http://localhost:8098/api/diagnostics/version
```

After restore, verify every household profile, recent progress, completed lessons and the expected content version. If startup or verification fails, stop the new container and reattach the untouched previous volume or restore the previous backup.

## Alpha limitation

The Alpha provides a documented cold backup. An in-application online backup and automated restore validation are planned for a later release. Do not copy a running `/data` volume and treat it as a supported backup.
