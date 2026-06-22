#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 || -z "${1:-}" ]]; then
  echo "Usage: $0 <version>"
  exit 1
fi

VERSION="$1"
ASSEMBLY_VERSION="${VERSION}.0"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

FILE="Directory.Build.props"

sed -i.bak -E \
  -e "s|<Version>[^<]+</Version>|<Version>${VERSION}</Version>|g" \
  -e "s|<InformationalVersion>[^<]+</InformationalVersion>|<InformationalVersion>${VERSION}</InformationalVersion>|g" \
  -e "s|<AssemblyVersion>[^<]+</AssemblyVersion>|<AssemblyVersion>${ASSEMBLY_VERSION}</AssemblyVersion>|g" \
  -e "s|<FileVersion>[^<]+</FileVersion>|<FileVersion>${ASSEMBLY_VERSION}</FileVersion>|g" \
  "$FILE"
rm -f "${FILE}.bak"

echo "Updated ${FILE} to version ${VERSION} (assembly/file ${ASSEMBLY_VERSION})"
