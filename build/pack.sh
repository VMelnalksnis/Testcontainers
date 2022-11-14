#!/bin/bash
set -e

# Expected format - v1.0.0:Paperless
IFS=":" read -r tag project <<<"$1"

if [[ $project == "Paperless" ]] || [[ $project == "Keycloak" ]]; then
	project_name="VMelnalksnis.Testcontainers.$project"
else
	echo "Unrecognized project: '$project'"
	exit 1
fi

version=$(echo "$tag" | tr -d v)
publish_dir="./source/$project_name/bin/Release"
full_version="$version.$2"

./build/restore.sh

dotnet pack \
	./source/"$project_name"/"$project_name".csproj \
	--configuration Release \
	-p:AssemblyVersion="$full_version" \
	-p:AssemblyFileVersion="$full_version" \
	-p:PackageVersion="$version" \
	-p:InformationalVersion="$version""$3" \
	/warnAsError \
	/nologo \
	/clp:NoSummary

echo "publish-directory=$publish_dir" >>"$GITHUB_OUTPUT"
