using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Minecraft;

public record PlayerAttributes
{
    public record Privileges
    {
        public record Enable
        { 
            public bool enabled { get; set; }
        }

        public Enable onlineChat { get; set; }
        public Enable multiplayerServer { get; set; }
        public Enable multiplayerRealms { get; set; }
        public Enable telemetry { get; set; }
    }

    public record ProfanityFilterPreferences
    { 
        public bool profanityFilterOn { get; set; }
    }
    public record BanStatus
    {
        public record BannedScopes
        {
            public record MultiPlayer
            { 
                public string banId { get; set; }
                public long expires { get; set; }
                public string reason { get; set; }
                public string reasonMessage { get; set; }
            }

            public MultiPlayer MULTIPLAYER { get; set; }
        }

        public BannedScopes bannedScopes { get; set; }
    }

    public Privileges privileges { get; set; }
    public ProfanityFilterPreferences profanityFilterPreferences { get; set; }
    public BanStatus banStatus { get; set; }
}
