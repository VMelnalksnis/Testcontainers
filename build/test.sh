#!/bin/bash
set -e

./build/build.sh
dotnet test -p:CollectCoverage=true -p:BuildInParallel=false -m:8 --configuration Release --no-build
