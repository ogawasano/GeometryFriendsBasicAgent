GOTO end_comment

Greetings, participants of a Geometry Friends Game AI competition!
This batch script will help you remove files that do not need to go
into the competition server, since the server already has a copy of
the game.

BEFORE RUNNING THIS SCRIPT: this script uses the 7z command, a command
line tool of the 7zip program (https://www.7-zip.org/). Therefore, this
script assumes that the command is available in your PATH environment variable

This script is meant to be used in the base directory of your code, where
this script should already reside (in the same directory as your .csproj file,
and in the same directory that contains the GeometryFriendsGame folder). So,
while in the directory described above, you can try to double click this file
or run it in cmd or powershell (again, in the mentioned directory) using:

				windows_packaging_script.bat

If successful, you will have a new zip file called "my_submission.zip" (the name
is irrelevant) that you can use when submitting to the competition server!

WARNING: as mentioned in the submission guide on the competition website,
the folders "Content", "Levels" and "Results" inside the folder
GeometryFriendsGame/Release/ are not meant to be used by the participant
and will be DELETED (when using this script, or later in the server)
Be careful not to put auxiliary files in folders that are deleted by this script

:end_comment

SET zipname=my_submission

7z a -tzip %zipname% ^
-x!%~nx0 ^
-x!linux_and_macos_packaging_script.sh ^
-x!obj ^
-x!.vs ^
-x!GeometryFriendsGame/Release/Content ^
-x!GeometryFriendsGame/Release/Levels ^
-x!GeometryFriendsGame/Release/Results ^
-x!GeometryFriendsGame/Release/FarseerPhysics.dll ^
-x!GeometryFriendsGame/Release/FarseerPhysics.pdb ^
-x!GeometryFriendsGame/Release/GeometryFriends.exe ^
-x!GeometryFriendsGame/Release/GeometryFriends.pdb ^
-x!GeometryFriendsGame/Release/OpenTK.dll ^
-x!GeometryFriendsGame/Release/OpenTK.dll.config ^
-x!GeometryFriendsGame/Release/OpenTK.pdb ^
-x!GeometryFriendsGame/Release/OpenTK.xml ^
-x!GeometryFriendsGame/Release/WiimoteInput.dll ^
-x!GeometryFriendsGame/Release/WiimoteLib.dll
