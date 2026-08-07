#!/usr/bin/env python3
"""Generate a deterministic third-party dependency license report for releases."""

from __future__ import annotations

import json
import os
import sys
import xml.etree.ElementTree as ET
from pathlib import Path


def text(value: object | None) -> str:
    if value is None:
        return "unknown"
    if isinstance(value, list):
        return ", ".join(str(item) for item in value)
    if isinstance(value, dict):
        return str(value.get("type") or value.get("name") or "unknown")
    return str(value).replace("\t", " ").replace("\n", " ").strip() or "unknown"


def npm_entries(root: Path) -> set[tuple[str, str, str, str]]:
    entries: set[tuple[str, str, str, str]] = set()
    if not root.exists():
        return entries

    for package_json in root.rglob("package.json"):
        if "node_modules" not in package_json.parts:
            continue
        try:
            data = json.loads(package_json.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError):
            continue
        name = text(data.get("name"))
        version = text(data.get("version"))
        if name == "unknown" or version == "unknown":
            continue
        license_name = text(data.get("license") or data.get("licenses"))
        repository = data.get("repository")
        if isinstance(repository, dict):
            repository = repository.get("url")
        entries.add((name, version, license_name, text(repository)))
    return entries


def nuget_entries(root: Path) -> set[tuple[str, str, str, str]]:
    entries: set[tuple[str, str, str, str]] = set()
    if not root.exists():
        return entries

    for nuspec in root.glob("*/*/*.nuspec"):
        try:
            document = ET.parse(nuspec)
        except (OSError, ET.ParseError):
            continue
        metadata = next((node for node in document.getroot().iter() if node.tag.endswith("metadata")), None)
        if metadata is None:
            continue

        values: dict[str, str] = {}
        for child in metadata:
            key = child.tag.rsplit("}", 1)[-1]
            values[key] = (child.text or "").strip()
        package_id = values.get("id") or nuspec.stem
        version = values.get("version") or nuspec.parent.name
        license_name = values.get("license") or values.get("licenseUrl") or "unknown"
        project_url = values.get("projectUrl") or "unknown"
        entries.add((package_id, version, license_name, project_url))
    return entries


def main() -> int:
    repository_root = Path(__file__).resolve().parents[1]
    output = Path(sys.argv[1]) if len(sys.argv) > 1 else repository_root / "artifacts" / "third-party-licenses.tsv"
    npm_root = repository_root / "src" / "Nadiano.Web" / "node_modules"
    nuget_root = Path(os.environ.get("NUGET_PACKAGES", Path.home() / ".nuget" / "packages"))

    npm = sorted(npm_entries(npm_root), key=lambda item: (item[0].lower(), item[1]))
    nuget = sorted(nuget_entries(nuget_root), key=lambda item: (item[0].lower(), item[1]))
    if not npm or not nuget:
        missing = []
        if not npm:
            missing.append("npm")
        if not nuget:
            missing.append("NuGet")
        print(f"Missing dependency metadata for: {', '.join(missing)}", file=sys.stderr)
        return 1

    output.parent.mkdir(parents=True, exist_ok=True)
    with output.open("w", encoding="utf-8", newline="\n") as report:
        report.write("ecosystem\tpackage\tversion\tlicense\tproject_or_repository\n")
        for package_id, version, license_name, project_url in nuget:
            report.write(f"nuget\t{package_id}\t{version}\t{license_name}\t{project_url}\n")
        for package_id, version, license_name, project_url in npm:
            report.write(f"npm\t{package_id}\t{version}\t{license_name}\t{project_url}\n")

    print(f"Wrote {len(nuget) + len(npm)} dependency license records to {output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
