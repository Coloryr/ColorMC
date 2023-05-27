using Avalonia.Platform;
using Avalonia;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ColorMC.Gui.UI.Model.Count;

public partial class CountModel : ObservableObject
{
    private IUserControl Con;

    [ObservableProperty]
    private long count;
    [ObservableProperty]
    private long countDone;
    [ObservableProperty]
    private long countError;
    [ObservableProperty]
    private long countToday;
    [ObservableProperty]
    private DateTimeOffset date;

    public CountModel(IUserControl con)
    {
        Con = con;

        var date = DateTime.Now;
        var data = GameCountUtils.Count;
        if (data == null)
        {
            count = 0;
            countDone = 0;
            countError = 0;
            countToday = 0;
        }
        else 
        {
            count = data.LaunchCount;
            countDone = data.LaunchDoneCount;
            countError = data.LaunchErrorCount;
            countToday = (from item in data.LaunchLogs.Values
                          from item1 in item
                          where item1.Time.Year == date.Year &&
                          item1.Time.Month == date.Month &&
                          item1.Time.Day == date.Day
                          select item).Count();
        }
    }
}
