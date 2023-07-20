using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Model.Count;

public partial class CountModel : ObservableObject
{
    private readonly IUserControl Con;

    [ObservableProperty]
    private long count;
    [ObservableProperty]
    private long countDone;
    [ObservableProperty]
    private long countError;
    [ObservableProperty]
    private int countToday;
    [ObservableProperty]
    private DateTime date;
    [ObservableProperty]
    private int dateCount;

    [ObservableProperty]
    private string time;
    [ObservableProperty]
    private string timeToday;
    [ObservableProperty]
    private DateTime date1;
    [ObservableProperty]
    private string timeDate;

    [ObservableProperty]
    private int gameIndex = -1;

    [ObservableProperty]
    private int gameCount;
    [ObservableProperty]
    private int gameCountDone;
    [ObservableProperty]
    private int gameCountError;
    [ObservableProperty]
    private int gameCountToday;
    [ObservableProperty]
    private string gameTime;
    [ObservableProperty]
    private string gameTime1;

    private readonly List<GameSettingObj> List = new();
    public ObservableCollection<string> Game { get; init; } = new();

    public CountModel(IUserControl con)
    {
        Con = con;

        date1 = date = DateTime.Now;
        var data = Utils.GameCount.Count;
        if (data == null)
        {
            count = 0;
            countDone = 0;
            countError = 0;
            countToday = 0;
            time = "";
            timeToday = "";
        }
        else
        {
            count = data.LaunchCount;
            countDone = data.LaunchDoneCount;
            countError = data.LaunchErrorCount;
            dateCount = countToday = (from item in data.LaunchLogs.Values
                                      from item1 in item
                                      where item1.Time.Year == date.Year &&
                                      item1.Time.Month == date.Month &&
                                      item1.Time.Day == date.Day
                                      select item).Count();
            time = $"{data.AllTime.TotalHours:0}:{data.AllTime.Minutes}:{data.AllTime.Seconds}";
            TimeSpan temp = TimeSpan.Zero;
            foreach (var item in data.GameRuns)
            {
                foreach (var item1 in item.Value)
                {
                    if (item1.StopTime.Ticks != 0
                        && item1.StartTime.Year == date.Year
                        && item1.StartTime.Month == date.Month
                        && item1.StartTime.Day == date.Day)
                    {
                        temp += item1.StopTime - item1.StartTime;
                    }
                }
            }
            timeDate = timeToday = temp.ToString();
            var list = GameBinding.GetGames();
            foreach (var item in list)
            {
                List.Add(item);
                Game.Add(item.Name);
            }
        }

        gameCount = gameCountDone = gameCountError = gameCountToday = 0;
        gameTime = gameTime1 = "0";
    }

    partial void OnDateChanged(DateTime value)
    {
        var data = Utils.GameCount.Count;
        if (data == null)
        {
            DateCount = 0;
        }
        else
        {
            DateCount = (from item in data.LaunchLogs.Values
                         from item1 in item
                         where item1.Time.Year == value.Year &&
                         item1.Time.Month == value.Month &&
                         item1.Time.Day == value.Day
                         select item).Count();
        }
    }

    partial void OnDate1Changed(DateTime value)
    {
        var data = Utils.GameCount.Count;
        if (data == null)
        {
            TimeDate = "";
        }
        else
        {
            TimeSpan temp = TimeSpan.Zero;
            foreach (var item in data.GameRuns)
            {
                foreach (var item1 in item.Value)
                {
                    if (item1.StopTime.Ticks != 0
                        && item1.StartTime.Year == value.Year
                        && item1.StartTime.Month == value.Month
                        && item1.StartTime.Day == value.Day)
                    {
                        temp += item1.StopTime - item1.StartTime;
                    }
                }
            }
            TimeDate = temp.ToString();
        }
    }

    partial void OnGameIndexChanged(int value)
    {
        GameCount = GameCountError = GameCountDone = GameCountToday = 0;
        GameTime = GameTime1 = "0";

        if (List.Count == 0)
            return;

        var data = Utils.GameCount.Count;
        if (data == null)
        {
            return;
        }

        var date = DateTime.Now;
        var game = List[value];
        if (data.GameRuns.TryGetValue(game.UUID, out var list))
        {
            TimeSpan temp = TimeSpan.Zero;
            TimeSpan temp1 = TimeSpan.Zero;
            foreach (var item in list)
            {
                temp += item.StopTime - item.StartTime;
                if (item.StopTime.Ticks != 0
                        && item.StartTime.Year == date.Year
                        && item.StartTime.Month == date.Month
                        && item.StartTime.Day == date.Day)
                {
                    temp1 += item.StopTime - item.StartTime;
                }
            }
            GameTime = $"{temp.TotalHours:0}:{temp.Minutes}:{temp.Seconds}";
            GameTime1 = temp1.ToString();
        }

        if (data.LaunchLogs.TryGetValue(game.UUID, out var list1))
        {
            int count1 = 0;
            int count2 = 0;
            int count3 = 0;
            int count4 = 0;
            foreach (var item in list1)
            {
                count1++;
                if (item.Error)
                {
                    count2++;
                }
                else
                {
                    count3++;
                }
                if (item.Time.Ticks != 0
                        && item.Time.Year == date.Year
                        && item.Time.Month == date.Month
                        && item.Time.Day == date.Day)
                {
                    count4++;
                }
            }
            GameCount = count1;
            GameCountError = count2;
            GameCountDone = count3;
            GameCountToday = count4;
        }
    }
}
