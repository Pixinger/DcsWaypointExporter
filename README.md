# DCS Waypoint Exporter

*WARNING* Still in development. No official release available.

*Use at your own risk*

This tool is written in C#, and can import/export waypoints from the new route planning tool of DCS.

The exported route can be distributed via file or as an encoded string, that could be shared by TeamSpeak.

## Known issues
1. ***Empty dialog:*** If the application starts with an 'empty' dialog (you can basically do nothing), chances are high, that the autodetection of the
`..\SavedGames\DCS\..` failed to detect the correct directory. This usually happen if you have two or more folders with `DCS` in it's name.
You can specify your custom directory in the file `appsettings.json`. Please make sure to use the `/` character instead of `\`.
The file could look something like this:
```
{
    "DcsSavedGamesFolder": "C:\Users\MyName\Saved Games\DCS"
}
```

2. ***MAPS:*** We tried to get all map names right. But as we don't own all of them, there might be some issues. Let me know if some maps show no mission data.

3. ***Installer***: I know, we are still missing a good old installer.

4. For sure there are other things that we missed...
