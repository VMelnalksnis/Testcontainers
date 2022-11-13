#!/bin/bash
set -e

./restore.sh
dotnet build --configuration Release --no-restore /warnAsError /nologo /clp:NoSummary
