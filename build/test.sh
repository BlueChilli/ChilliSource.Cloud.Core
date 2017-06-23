#!/usr/bin/env bash
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
BUILD_FILE_PATH="${SCRIPT_DIR}/build.sh --target WatchFiles"

echo "Running >> '$BUILD_FILE_PATH'"
source $BUILD_FILE_PATH