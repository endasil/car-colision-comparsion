#!/usr/bin/env bash

# Stop execution immediately if something exits with a non zero state
set -e
# every executed statement is printed to the shell for debugging or troubleshooting purposes.
set -x

store_unity_license_path="$UNITY_DIR/.local/share/unity3d/Unity"
#create a directory where we will store a file with unity license information
mkdir -p "${store_unity_license_path}"

# Set the directory where we want to store the unity licensing file
unity_license_destination="${store_unity_license_path}/Unity_lic.ulf"

# Check if UNITY_LICENSE exist, -n means not null
if [ -n "$UNITY_LICENSE" ]
then
    echo "Writing '\$UNITY_LICENSE' to license file ${unity_license_destination}"
# Delete all \r stored in the variable UNITY_LICENSE and write the result to file
    echo "${UNITY_LICENSE}" | tr -d '\r' > "${unity_license_destination}"
else
    echo "'\$UNITY_LICENSE' env var not found"
	exit 8
fi
