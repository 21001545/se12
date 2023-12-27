#!/bin/sh
/Applications/Unity/Hub/Editor/2022.1.24f1/Unity.app/Contents/MacOS/Unity -quit -batchmode -nographics -executeMethod Festa.BuildApp.BuildPlayer_IOS_Jenkins -CustomArgs:rebuild=false\;buildNumber=20\;branch=inhouse\;server=dev-inhouse

