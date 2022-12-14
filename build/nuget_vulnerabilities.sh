#!/bin/bash
set -e

./build/restore.sh

packages=$(dotnet list package --include-transitive --vulnerable)

if [[ "$packages" == *"has the following vulnerable packages"* ]]; then
	echo "$packages"
	exit 1
fi
