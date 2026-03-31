using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Count;

/// <summary>
/// 游戏统计
/// </summary>
public partial class CountModel : ControlModel
{
    /// <summary>
    /// 累计启动次数
    /// </summary>
    [ObservableProperty]
    public partial long Count { get; set; }

    /// <summary>
    /// 累计启动成功次数
    /// </summary>
    [ObservableProperty]
    public partial long CountDone { get; set; }

    /// <summary>
    /// 累计启动失败次数
    /// </summary>
    [ObservableProperty]
    public partial long CountError { get; set; }

    /// <summary>
    /// 选中的时间
    /// </summary>
    [ObservableProperty]
    public partial DateTime Date { get; set; }

    /// <summary>
    /// 今日统计时间
    /// </summary>
    [ObservableProperty]
    public partial int DateCount { get; set; }

    /// <summary>
    /// 累计游玩时长
    /// </summary>
    [ObservableProperty]
    public partial string Time { get; set; }

    /// <summary>
    /// 今日游玩时长
    /// </summary>
    [ObservableProperty]
    public partial string TimeToday { get; set; }

    /// <summary>
    /// 选中的时间
    /// </summary>
    [ObservableProperty]
    public partial DateTime Date1 { get; set; }

    /// <summary>
    /// 选中时间的游玩时常
    /// </summary>
    [ObservableProperty]
    public partial string TimeDate { get; set; }

    /// <summary>
    /// 选中的游戏实例
    /// </summary>
    [ObservableProperty]
    public partial int GameIndex { get; set; } = -1;

    /// <summary>
    /// 游戏实例启动累计
    /// </summary>
    [ObservableProperty]
    public partial int GameCount { get; set; }

    /// <summary>
    /// 游戏实例启动成功累计
    /// </summary>
    [ObservableProperty]
    public partial int GameCountDone { get; set; }

    /// <summary>
    /// 游戏实例启动失败累计
    /// </summary>
    [ObservableProperty]
    public partial int GameCountError { get; set; }

    /// <summary>
    /// 游戏实例启动今日累计
    /// </summary>
    [ObservableProperty]
    public partial int GameCountToday { get; set; }

    /// <summary>
    /// 游戏实例游戏时间
    /// </summary>
    [ObservableProperty]
    public partial string GameTime { get; set; }

    /// <summary>
    /// 游戏实例上次游戏时间
    /// </summary>
    [ObservableProperty]
    public partial string GameTime1 { get; set; }

    /// <summary>
    /// 游戏实例列表
    /// </summary>
    private readonly List<GameSettingObj> _list = [];

    /// <summary>
    /// 游戏实例列表
    /// </summary>
    public ObservableCollection<string> Game { get; init; } = [];

    public CountModel(WindowModel model) : base(model)
    {
        Date1 = Date = DateTime.Now;
        var data = GameCountUtils.Count;
        if (data == null)
        {
            Count = 0;
            CountDone = 0;
            CountError = 0;
            Time = "";
            TimeToday = "";
        }
        else
        {
            Count = data.LaunchCount;
            CountDone = data.LaunchDoneCount;
            CountError = data.LaunchErrorCount;
            DateCount = (from item in data.LaunchLogs.Values
                          from item1 in item
                          where item1.Time.Year == Date.Year &&
                          item1.Time.Month == Date.Month &&
                          item1.Time.Day == Date.Day
                          select item).Count();
            Time = $"{data.AllTime.TotalHours:0}:{data.AllTime.Minutes}:{data.AllTime.Seconds}";
            TimeSpan temp = TimeSpan.Zero;
            //累计统计时间
            foreach (var item in data.GameRuns)
            {
                foreach (var item1 in item.Value)
                {
                    if (item1.StopTime.Ticks != 0
                        && item1.StartTime.Year == Date.Year
                        && item1.StartTime.Month == Date.Month
                        && item1.StartTime.Day == Date.Day)
                    {
                        temp += item1.StopTime - item1.StartTime;
                    }
                }
            }

            TimeDate = TimeToday = $"{temp.TotalHours:0}:{temp.Minutes}:{temp.Seconds}";
            var list = InstancesPath.Games;
            foreach (var item in list)
            {
                _list.Add(item);
                Game.Add(item.Name);
            }
        }

        GameCount = GameCountDone = GameCountError = GameCountToday = 0;
        GameTime = GameTime1 = "0";
    }

    /// <summary>
    /// 显示日期修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnDateChanged(DateTime value)
    {
        var data = GameCountUtils.Count;
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

    /// <summary>
    /// 显示日期修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnDate1Changed(DateTime value)
    {
        var data = GameCountUtils.Count;
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

    /// <summary>
    /// 游戏实例选中修改
    /// </summary>
    /// <param name="value"></param>
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
