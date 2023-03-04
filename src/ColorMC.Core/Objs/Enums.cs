namespace ColorMC.Core.Objs;

public enum SourceType
{
    CurseForge, Modrinth
}

public enum PathType
{
    BasePath, GamePath, ModPath, ConfigPath, ShaderpacksPath, ResourcepackPath
}

public enum FileType
{
    ModPack = 0, Mod, World, Shaderpack, Resourcepack, DataPacks, Schematic,
    Java, Game, Config, AuthConfig, Pic, UI, Optifne
}

public enum PackType
{
    ColorMC, CurseForge, Modrinth, MMC, HMCL
}

public enum LanguageType
{
    zh_cn, en_us
}

public enum SourceLocal
{
    Offical = 0,
    BMCLAPI = 1,
    MCBBS = 2
}