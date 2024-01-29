﻿namespace Mangarr.Stack.Settings;

public partial class SettingsApi
{
    public AniListSettings AniList { get; }

    public LibrarySettings Library { get; }

    public ConversionSettings Conversion { get; }

    public SettingsApi(AniListSettings aniListSettings, LibrarySettings librarySettings, ConversionSettings conversion)
    {
        AniList = aniListSettings;
        Library = librarySettings;
        Conversion = conversion;
    }
}
