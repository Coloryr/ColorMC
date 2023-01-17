using ColorMC.Core;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UIBinding;

public static class OtherBinding
{
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
        return new()
        {
            Localizer.Instance["OtherBinding.TranTypes.Item1"],
            Localizer.Instance["OtherBinding.TranTypes.Item2"],
            Localizer.Instance["OtherBinding.TranTypes.Item3"],
            Localizer.Instance["OtherBinding.TranTypes.Item4"],
            Localizer.Instance["OtherBinding.TranTypes.Item5"]
        };
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
