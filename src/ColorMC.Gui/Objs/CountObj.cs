using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public record CountObj
{
    public record GameTime
    {
        public bool Now;
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
    }

    public record LaunchLog
    { 
        public DateTime Time { get; set; }
        public bool Error { get; set; }
    }

    public Dictionary<string, List<GameTime>> GameRuns { get; set; }
    public Dictionary<string, List<LaunchLog>> LaunchLogs { get; set; }

    public long LaunchCount { get; set; }
    public long LaunchDoneCount { get; set; }
    public long LaunchErrorCount { get; set; }
}
