using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Java;

public record AdoptiumObj
{
    public record Binary
    {
        public record Package
        { 
            public string checksum { get; set; }
            public string checksum_link { get; set; }
            public int download_count { get; set; }
            public string link { get; set; }
            public string metadata_link { get; set; }
            public string name { get; set; }
            public string signature_link { get; set; }
            public long size { get; set; }
        }
        public string architecture { get; set; }
        public int download_count { get; set; }
        public string heap_size { get; set; }
        public string image_type { get; set; }
        public string jvm_impl { get; set; }
        public string os { get; set; }
        public Package package { get; set; }
        public string project { get; set; }
        public string scm_ref { get; set; }
        public string updated_at { get; set; }
    }
    public record Version
    { 
        public int build { get; set; }
        public int major { get; set; }
        public int minor { get; set; }
        public string openjdk_version { get; set; }
        public int security { get; set; }
        public string semver { get; set; }
    }
    public Binary binary { get; set; }
    public string release_link { get; set; }
    public string release_name { get; set; }
    public string vendor { get; set; }
    public Version version { get; set; }
}
