using ColorMC.Core;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Utils;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UIBinding;

public static class OtherBinding
{
    private static List<string> WindowTranTypes = new()
    {
        "普通透明", "模糊", "亚克力模糊", "强制亚克力模糊", "云母"
    };
    public static (int, int) GetDownloadSize()
    {
        return (DownloadManager.AllSize, DownloadManager.DoneSize);
    }

    public static CoreRunState GetDownloadState()
    {
        return DownloadManager.State;
    }

    public static List<string> GetDownloadSources()
    {
        var list = new List<string>();
        Array values = Enum.GetValues(typeof(SourceLocal));
        foreach (SourceLocal value in values)
        {
            list.Add(value.GetName());
        }

        return list;
    }

    public static List<string> GetWindowTranTypes()
    {
        return WindowTranTypes;
    }

    public static List<string> GetLanguages()
    {
        var list = new List<string>();
        Array values = Enum.GetValues(typeof(LanguageType));
        foreach (LanguageType value in values)
        {
            list.Add(value.GetName());
        }

        return list;
    }
}
