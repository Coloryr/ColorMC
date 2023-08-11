using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Model.Count;

public partial class CountModel : BaseModel
{
    [ObservableProperty]
    private long _count;
    [ObservableProperty]
    private long _countDone;
    [ObservableProperty]
    private long _countError;
    [ObservableProperty]
    private int _countToday;
    [ObservableProperty]
    private DateTime _date;
    [ObservableProperty]
    private int _dateCount;

    [ObservableProperty]
    private string _time;
    [ObservableProperty]
    private string _timeToday;
    [ObservableProperty]
    private DateTime _date1;
    [ObservableProperty]
    private string _timeDate;

    [ObservableProperty]
    private int _gameIndex = -1;

    [ObservableProperty]
    private int _gameCount;
    [ObservableProperty]
    private int _gameCountDone;
    [ObservableProperty]
    private int _gameCountError;
    [ObservableProperty]
    private int _gameCountToday;
    [ObservableProperty]
    private string _gameTime;
    [ObservableProperty]
    private string _gameTime1;

    private readonly List<GameSettingObj> _list = new();

    public ObservableCollection<string> Game { get; init; } = new();

    public CountModel(IUserControl con) : base(con)
    {
        _date1 = _date = DateTime.Now;
        var data = Utils.GameCountUtils.Count;
        if (data == null)
        {
            _count = 0;
            _countDone = 0;
            _countError = 0;
            _countToday = 0;
            _time = "";
            _timeToday = "";
        }
        else
        {
            _count = data.LaunchCount;
            _countDone = data.LaunchDoneCount;
            _countError = data.LaunchErrorCount;
            _dateCount = _countToday = (from item in data.LaunchLogs.Values
                                        from item1 in item
                                        where item1.Time.Year == _date.Year &&
                                        item1.Time.Month == _date.Month &&
                                        item1.Time.Day == _date.Day
                                        select item).Count();
            _time = $"{data.AllTime.TotalHours:0}:{data.AllTime.Minutes}:{data.AllTime.Seconds}";
            TimeSpan temp = TimeSpan.Zero;
            foreach (var item in data.GameRuns)
            {
                foreach (var item1 in item.Value)
                {
                    if (item1.StopTime.Ticks != 0
                        && item1.StartTime.Year == _date.Year
                        && item1.StartTime.Month == _date.Month
                        && item1.StartTime.Day == _date.Day)
                    {
                        temp += item1.StopTime - item1.StartTime;
                    }
                }
            }
            _timeDate = _timeToday = temp.ToString();
            var list = GameBinding.GetGames();
            foreach (var item in list)
            {
                _list.Add(item);
                Game.Add(item.Name);
            }
        }

        _gameCount = _gameCountDone = _gameCountError = _gameCountToday = 0;
        _gameTime = _gameTime1 = "0";
    }

    partial void OnDateChanged(DateTime value)
    {
        var data = Utils.GameCountUtils.Count;
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
        var data = Utils.GameCountUtils.Count;
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

        if (_list.Count == 0)
            return;

        var data = Utils.GameCountUtils.Count;
        if (data == null)
        {
            return;
        }

        var date = DateTime.Now;
        var game = _list[value];
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

    public override void Close()
    {
        
    }
}
