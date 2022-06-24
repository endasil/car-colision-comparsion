#!/usr/bin/env bash

# Stop execution immediately if something exits with a non zero state
set -e
# every executed statement is printed to the shell for debugging or troubleshooting purposes.
set -x

#If the variable ANDROID_KEYSTORE_BASE64 is found, we will store the data from that secret in the 
#file defined by android_keystore_destination
android_keystore_destination=keystore.keystore

#upper_case_build_target contains the string stored in the BUILD_TARGET variable converted to all uppercase
upper_case_build_target=${BUILD_TARGET^^};

#If this is an android build and we have $ANDROID_KEYSTORE_BASE64 set, decode the base64 and store it in 
#the file efined by android_keystore_destination

if [ "$upper_case_build_target" = "ANDROID" ]
then
    if [ -n $ANDROID_KEYSTORE_BASE64 ]
    then
        echo "'\$ANDROID_KEYSTORE_BASE64' found, decoding content into ${android_keystore_destination}"
        echo $ANDROID_KEYSTORE_BASE64 | base64 --decode > ${android_keystore_destination}
    else
        echo '$ANDROID_KEYSTORE_BASE64'" env var not found, building with Unity's default debug keystore"
    fi
fi
