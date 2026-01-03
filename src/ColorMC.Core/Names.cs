namespace ColorMC.Core;

public static class Names
{
    public const string NameDownloadDir = "download";
    public const string NameOverrideDir = "overrides";
    public const string NameLibDir = "libraries";
    public const string NameInstanceDir = "instances";
    public const string NameRemoveDir = "remove";
    public const string NameBackupDir = "backup";
    public const string NameTempDir = "temp";
    public const string NameCacheDir = "cache";
    public const string NameVersionDir = "versions";
    public const string NameGameDir = ".minecraft";
    public const string NameGameLogDir = "logs";
    public const string NameGameCrashLogDir = "crash-reports";
    public const string NameGameDatapackDir = "datapacks";
    public const string NameGameModDir = "mods";
    public const string NameGameAssetsDir = "assets";
    public const string NameGameIndexDir = "indexes";
    public const string NameGameObjectDir = "objects";
    public const string NameGameSkinDir = "skins";
    public const string NameGameScreenShotDir = "screenshots";
    public const string NameGameResourcepackDir = "resourcepacks";
    public const string NameGameShaderpackDir = "shaderpacks";
    public const string NameGameSavesDir = "saves";
    public const string NameGameConfigDir = "config";
    public const string NameGameSchematicsDir = "schematics";
    public const string NameJavaDir = "java";
    public const string NameJsonDir = "patches";
    public const string NameDefaultDir = "default";

    public const string NameModInfoFile = "modfileinfo.json";
    public const string NameGameFile = "game.json";
    public const string NameModPackFile = "Modpack.json";
    public const string NameConfigFile = "config.json";
    public const string NameShaFile = "sha1";
    public const string NameColorMcInfoFile = "colormc.info.json";
    public const string NameMMCJsonFile = "mmc-pack.json";
    public const string NameMMCCfgFile = "instance.cfg";
    public const string NameHMCLFile = "mcbbs.packmeta";
    public const string NameManifestFile = "manifest.json";
    public const string NameModrinthFile = "modrinth.index.json";
    public const string NameIconFile = "icon.png";
    public const string NameServerFile = "server.json";
    public const string NameServerOldFile = "server.old.json";
    public const string NameLaunchCountFile = "launch.json";
    public const string NameLog4jFile = "log4j-rce-patch.xml";
    public const string NameLoaderFile = "loader.jar";
    public const string NameLevelFile = "level.dat";
    public const string NamePackMetaFile = "pack.mcmeta";
    public const string NamePackIconFile = "pack.png";
    public const string NameOptionFile = "options.txt";
    public const string NameGameServerFile = "servers.dat";
    public const string NameVersionFile = "version.json";
    public const string NameAuthFile = "auth.json";
    public const string NameMavenFile = "maven.json";
    public const string NameJavaFile = "java";
    public const string NameJavawFile = "javaw.exe";
    public const string NameOptifineFile = "optifine.json";
    public const string NameModListFile = "modlist.html";
    public const string NameLatestLogFile = "latest.log";
    public const string NameDebugLogFile = "debug.log";

    public const string NameMinecraftKey = "minecraft";
    public const string NameLangKey1 = "minecraft/lang/";
    public const string NameLangKey2 = "lang";
    public const string NameFmlKey = "fmlloader";
    public const string NameForgeKey = "forge";
    public const string NameMinecraftForgeKey = "minecraftforge";
    public const string NameNeoForgeKey = "neoforge";
    public const string NameNeoForgedKey = "neoforged";
    public const string NameFabricKey = "fabric";
    public const string NameFabricMcKey = "fabricmc";
    public const string NameFabricLoaderKey = "fabric-loader";
    public const string NameQuiltKey = "quilt";
    public const string NameQuiltMcKey = "quiltmc";
    public const string NameQuiltLoaderKey = "quilt-loader";

    public const string NameForgeFile1 = "installer";
    public const string NameForgeFile2 = "universal";
    public const string NameForgeFile3 = "client";
    public const string NameForgeFile4 = "launcher";
    public const string NameForgeInstallFile = "install_profile.json";

    public const string NameLogExt = ".log";
    public const string NameTxtExt = ".txt";
    public const string NameLogGzExt = ".log.gz";
    public const string NameZipExt = ".zip";
    public const string NameJarExt = ".jar";
    public const string NameJsonExt = ".json";
    public const string NameDisableExt = ".disable";
    public const string NameDisabledExt = ".disabled";
    public const string NameLitematicExt = ".litematic";
    public const string NameSchematicExt = ".schematic";
    public const string NameSchemExt = ".schem";
    public const string NameSha1Ext = ".sha1";
    public const string NameTarGzExt = ".tar.gz";
    public const string NameMrpackExt = ".mrpack";
    public const string NameDatExt = ".dat";
    public const string NameDatOldExt = ".dat_old";
    public const string NameRioExt = ".rio";
    public const string NameMcaExt = ".mca";
    public const string NamePngExt = ".png";
    public const string NameNbtExt = ".nbt";

    public const string NameDefaultGroup = " ";

    public const string NameArgJavaLocal = "%JAVA_LOCAL%";
    public const string NameArgJavaArg = "%JAVA_ARG%";
    public const string NameArgLauncherDir = "%LAUNCH_DIR%";
    public const string NameArgGameName = "%GAME_NAME%";
    public const string NameArgGameUUID = "%GAME_UUID%";
    public const string NameArgGameDir = "%GAME_DIR%";
    public const string NameArgGameBaseDir = "%GAME_BASE_DIR%";

    public const string NameMcModInfoFile = "mcmod.info";
    public const string NameMcModTomlFile = "META-INF/mods.toml";
    public const string NameNeoTomlFile = "META-INF/neoforge.mods.toml";
    public const string NameNeoToml1File = "neoforge.mods.toml";
    public const string NameModJarJarDir = "META-INF/jarjar/";

    public static readonly string[] NameGCArgG1GC =
    [
        "-XX:+UnlockExperimentalVMOptions",
        "-XX:+UseG1GC",
        "-XX:MaxGCPauseMillis=200",
        "-XX:G1NewSizePercent=30",
        "-XX:G1MaxNewSizePercent=40",
        "-XX:InitiatingHeapOccupancyPercent=35",
        "-XX:ConcGCThreads=4",
        "-XX:ParallelGCThreads=8"
    ];
    public static readonly string[] NameGCZGC =
    [
        "-XX:+UseZGC",
        "-XX:+ZGenerational"
    ];
}
