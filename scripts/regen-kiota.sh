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
# Tiltify's spec leaves every paginated list response untyped; scripts/patch-openapi.cs types them from their
# examples before generation (see the comment there). tiltify-openapi.json is the spec as Tiltify publishes it;
# the patched copy is generated and not committed.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
SPEC_FILE="$REPO_ROOT/tiltify-openapi.json"
PATCHED_SPEC="$REPO_ROOT/tiltify-openapi.patched.json"
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

echo "Typing paginated list responses..."
dotnet run "$SCRIPT_DIR/patch-openapi.cs" -- "$SPEC_FILE" "$PATCHED_SPEC"

echo "Regenerating Kiota client..."

# Preserve the csproj before clean-output wipes it.
CSPROJ_BACKUP=$(cat "$CSPROJ_FILE")

# The lock records the spec path; generating from the patched copy writes a fresh one.
rm -f "$GENERATED_DIR/kiota-lock.json"

kiota generate \
  --openapi "$PATCHED_SPEC" \
  --language CSharp \
  --class-name TiltifyApiClient \
  --namespace-name Tiltify.Client.Generated \
  --output "$GENERATED_DIR" \
  --exclude-backward-compatible \
  --additional-data true \
  --clean-output

# kiota --clean-output deletes everything including the .csproj; restore it.
echo "$CSPROJ_BACKUP" > "$CSPROJ_FILE"

echo "Done. Review changes in $GENERATED_DIR before committing."
