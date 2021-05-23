using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MinecraftTrainSignalServer
{
    public class MasterConfig
    {
        [JsonProperty(PropertyName = "operation", Required = Required.Always)]
        public string OperationLogPath;
        [JsonProperty(PropertyName = "error", Required = Required.Always)]
        public string ErrorLogPath;
        [JsonProperty(PropertyName = "console_location", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string ManagementConsoleLocation;
    }
}
