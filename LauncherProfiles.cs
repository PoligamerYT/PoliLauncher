using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL
{
    public class LauncherProfiles
    {
        [JsonProperty(PropertyName = "profiles")]
        public Dictionary<string, Profile> Profiles { get; set; }
        [JsonProperty("settings")]
        public Settings Settings { get; set; }
        [JsonProperty("version")]
        public int Version { get; set; }
        [JsonProperty("selectedProfile")]
        public string SelectedProfile { get; set; }
    }
}
