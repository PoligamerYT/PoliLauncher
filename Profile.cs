using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL
{
    public class Profile
    {
        [JsonIgnore]
        public string Key { get; set; }
        [JsonProperty("created")]
        public DateTime Created { get; set; }
        [JsonProperty("icon")]
        public string Icon = "";
        [JsonProperty("lastUsed")]
        public DateTime LastUsed { get; set; }
        [JsonProperty("lastVersionId")]
        public string LastVersionId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public string Type = "";
        [JsonProperty("javaArgs")]
        public string JavaArgs { get; set; }
        [JsonProperty("resolution")]
        public Resolution Resolution { get; set; }
        [JsonProperty("javaDir")]
        public string JavaDir { get; set; }
    }
}
