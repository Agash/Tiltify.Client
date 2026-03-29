#!/usr/bin/env bash
# Regenerates the Kiota-generated client from the Tiltify v5 OpenAPI spec.
#
# Prerequisites:
#   - kiota CLI installed: https://aka.ms/kiota/docs/installation
#   - Internet access (to fetch the live OpenAPI spec, optional)
#
# Usage:
#   ./scripts/regen-kiota.sh [--fetch]
#
#   --fetch   Download the latest OpenAPI spec from Tiltify before regenerating.
#             Without this flag, the local tiltify-openapi.json is used as-is.
#
# The kiota-lock.json in src/Tiltify.Client.Generated/ records the exact spec hash,
# Kiota version, and generation parameters. Run `kiota update` to regenerate using
# the locked spec, or use this script to also refresh the spec first.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
SPEC_FILE="$REPO_ROOT/tiltify-openapi.json"
GENERATED_DIR="$REPO_ROOT/src/Tiltify.Client.Generated"
CSPROJ_FILE="$GENERATED_DIR/Tiltify.Client.Generated.csproj"

FETCH=false
for arg in "$@"; do
  case "$arg" in
    --fetch) FETCH=true ;;
    *) echo "Unknown argument: $arg"; exit 1 ;;
  esac
done

if [ "$FETCH" = true ]; then
  echo "Fetching latest Tiltify v5 OpenAPI spec..."
  curl -fsSL "https://v5api.tiltify.com/api/public/openapi" -o "$SPEC_FILE"
  echo "Saved to $SPEC_FILE"
fi

if [ ! -f "$SPEC_FILE" ]; then
  echo "Error: $SPEC_FILE not found."
  echo "Run with --fetch to download the spec, or place tiltify-openapi.json in the repo root."
  exit 1
fi

echo "Regenerating Kiota client..."

# Preserve the csproj before clean-output wipes it.
CSPROJ_BACKUP=$(cat "$CSPROJ_FILE")

kiota update \
  --output "$GENERATED_DIR" \
  --clean-output

# kiota --clean-output deletes everything including the .csproj; restore it.
echo "$CSPROJ_BACKUP" > "$CSPROJ_FILE"

echo "Done. Review changes in $GENERATED_DIR before committing."
