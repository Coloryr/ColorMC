using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Game;

public record QuiltMetaObj
{
    public record Game
    {
        public string version { get; set; }
        public bool stable { get; set; }
    }
    public record Mappings
    {
        public string gameVersion { get; set; }
        public string separator { get; set; }
        public int build { get; set; }
        public string maven { get; set; }
        public string version { get; set; }
        public string hashed { get; set; }
    }
    public record Hashed
    {
        public string maven { get; set; }
        public string version { get; set; }
    }
    public record Loader
    {
        public string separator { get; set; }
        public int build { get; set; }
        public string maven { get; set; }
        public string version { get; set; }
    }
    public record Installer
    {
        public string url { get; set; }
        public string maven { get; set; }
        public string version { get; set; }
    }
    public List<Game> game { get; set; }
    public List<Mappings> mappings { get; set; }
    public List<Hashed> hashed { get; set; }
    public List<Loader> loader { get; set; }
    public List<Installer> installer { get; set; }
}
