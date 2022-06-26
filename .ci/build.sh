#!/usr/bin/env bash


set -x

echo "Building for $BUILD_TARGET"

export BUILD_PATH=$UNITY_DIR/Builds/$BUILD_TARGET/
mkdir -p $BUILD_PATH


store_unity_license_path="$HOME/.local/share/unity3d/Unity"
unity_license_destination="${store_unity_license_path}/Unity_lic.ulf"


${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' unity-editor} \
  -projectPath $UNITY_DIR \
  -quit \
  -batchmode \
  -nographics \  
  -buildTarget $BUILD_TARGET \
  -customBuildTarget $BUILD_TARGET \
  -customBuildName $BUILD_NAME \
  -customBuildPath $BUILD_PATH \
  -executeMethod BuildCommand.PerformBuild \
  -logFile log.txt  >/dev/null 2>&1

UNITY_EXIT_CODE=$?

cat log.txt
set -e
if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo "Run succeeded, no failures occurred";
elif [ $UNITY_EXIT_CODE -eq 2 ]; then
  echo "Run succeeded, some tests failed";
elif [ $UNITY_EXIT_CODE -eq 3 ]; then
  echo "Run failure (other failure)";
else
  echo "Unexpected exit code $UNITY_EXIT_CODE";
fi

ls -la $BUILD_PATH
[ -n "$(ls -A $BUILD_PATH)" ] # fail job if build folder is empty
