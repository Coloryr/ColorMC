using System;
using System.Collections.Generic;

namespace ColorMC.Gui.Objs.Config;

/// <summary>
/// 游戏统计储存
/// </summary>
public record CountObj
{
    /// <summary>
    /// 游戏时间统计
    /// </summary>
    public record GameTime
    {
        /// <summary>
        /// 是否是正在统计状态
        /// </summary>
        public bool Now;
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime StopTime { get; set; }
    }

    /// <summary>
    /// 启动统计
    /// </summary>
    public record LaunchLog
    {
        /// <summary>
        /// 启动时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 是否启动错误
        /// </summary>
        public bool Error { get; set; }
    }

    /// <summary>
    /// 游戏运行统计列表
    /// 键为游戏UUID
    /// 值为所有游戏运行统计
    /// </summary>
    public Dictionary<string, List<GameTime>> GameRuns { get; set; }
    /// <summary>
    /// 游戏启动统计列表
    /// 键为游戏UUID
    /// 值为所有游戏启动统计
    /// </summary>
    public Dictionary<string, List<LaunchLog>> LaunchLogs { get; set; }

    /// <summary>
    /// 所有运行时间
    /// </summary>
    public TimeSpan AllTime { get; set; }

    /// <summary>
    /// 所有启动次数统计
    /// </summary>
    public long LaunchCount { get; set; }
    /// <summary>
    /// 所有启动成功次数统计
    /// </summary>
    public long LaunchDoneCount { get; set; }
    /// <summary>
    /// 所有启动失败次数统计
    /// </summary>
    public long LaunchErrorCount { get; set; }
}
