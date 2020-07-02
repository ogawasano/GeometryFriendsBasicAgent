#!/usr/bin/env bash

# Greetings, participants of a Geometry Friends Game AI competition!
# This bash script will help you remove files that do not need to go
# into the competition server, since the server already has a copy of
# the game.

# This script is meant to be used in the base directory of your code, where
# this script should already reside (in the same directory as your .csproj file,
# and in the same directory that contains the GeometryFriendsGame folder). So,
# while in the directory described above, simply run:
#
#				./linux_and_macos_packaging_script.sh
#
# You may need to give execution permissions to the script if the above didn't work
#
#				chmod +x linux_and_macos_packaging_script.sh
#
# Now you can try the command again.
# If successful, you will have a new zip file called "my_submission.zip" (the name
# is irrelevant) that you can use when submitting to the competition server!

# WARNING: as mentioned in the submission guide on the competition website,
# the folders "Content", "Levels" and "Results" inside the folder
# GeometryFriendsGame/Release/ are not meant to be used by the participant
# and will be DELETED (when using this script, or later in the server)
# Be careful not to put auxiliary files in folders present in the
# 'exludes' array

zipname="my_submission"

declare -a excludes=(
	"$0"
	"windows_packaging_script.bat"
	".vs*"
	"obj*"
	"GeometryFriendsGame/Release/Content*"
	"GeometryFriendsGame/Release/Levels*"
	"GeometryFriendsGame/Release/Results*"
	"GeometryFriendsGame/Release/FarseerPhysics.dll"
	"GeometryFriendsGame/Release/FarseerPhysics.pdb"
	"GeometryFriendsGame/Release/GeometryFriends.exe"
	"GeometryFriendsGame/Release/GeometryFriends.pdb"
	"GeometryFriendsGame/Release/OpenTK.dll"
	"GeometryFriendsGame/Release/OpenTK.dll.config"
	"GeometryFriendsGame/Release/OpenTK.pdb"
	"GeometryFriendsGame/Release/OpenTK.xml"
	"GeometryFriendsGame/Release/WiimoteInput.dll"
	"GeometryFriendsGame/Release/WiimoteLib.dll"
)

zip -r $zipname . -x "${excludes[@]}"
