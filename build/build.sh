#!/bin/bash
set -e

./build/restore.sh
dotnet build --configuration Release --no-restore /warnAsError /nologo /clp:NoSummary
