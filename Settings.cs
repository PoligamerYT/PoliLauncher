using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL
{
    public class Settings
    {
        [JsonProperty("crashAssistance")]
        public bool CrashAssistance { get; set; }
        [JsonProperty("enableAdvanced")]
        public bool AnableAdvanced { get; set; }
        [JsonProperty("enableAnalytics")]
        public bool EnableAnalytics { get; set; }
        [JsonProperty("enableHistorical")]
        public bool EnableHistorical { get; set; }
        [JsonProperty("enableReleases")]
        public bool EnableReleases { get; set; }
        [JsonProperty("enableSnapshots")]
        public bool EnableSnapshots { get; set; }
        [JsonProperty("keepLauncherOpen")]
        public bool KeepLauncherOpen { get; set; }
        [JsonProperty("profileSorting")]
        public string ProfileSorting { get; set; }
        [JsonProperty("showGameLog")]
        public bool ShowGameLog { get; set; }
        [JsonProperty("showMenu")]
        public bool ShowMenu { get; set; }
        [JsonProperty("soundOn")]
        public bool SoundOn { get; set; }
    }
}
