#!/bin/bash
set -e

./build/build.sh
dotnet test -p:CollectCoverage=true -p:BuildInParallel=true -m:8 --configuration Release --no-build
