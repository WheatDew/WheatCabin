Documentation: http://rteditor.battlehub.net/

Quick Start:
1. Create new scene.
2. Click Tools > Runtime Editor > Create RTEditor from Main Menu.
3. Click Tools > Runtime SaveLoad > Config > Build All !!! 
4. Click Play.

Demo:
1. Import Assets/Battlehub/1 UniversalRP Support package.
2  Click Tools > Runtime Editor > Use built-in RenderPipelineAsset.
3. Import Assets/Battlehub/RTEditor Demo package.
4. Remove everything from Assets/Battlehub/RTSL_Data folder except /Libraries subfolder !!!
5. Click Tools > Runtime Editor > Show me examples.
6. Open RTEditor scene.
7. Click Tools > Runtime SaveLoad > Config > Build All !!!
8. Click Play.

Troubleshooting:
If you have compiler errors, check if the correct dependencies are installed (Window->Package Manager)

For a list of required dependencies, see the Assets\Battlehub\RTEditor\package.json file:

 "dependencies": {
    "com.unity.probuilder": "4.2.3",
    "com.unity.mathematics": "1.2.1",
    "com.unity.textmeshpro": "2.0.1",
    "com.unity.inputsystem": "1.0.1"
  },