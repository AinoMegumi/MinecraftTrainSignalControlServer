using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MinecraftTrainSignalServer
{
    public class MasterConfig
    {
        [JsonProperty(PropertyName = "operation")]
        public string OperationLogPath;
        [JsonProperty(PropertyName = "error")]
        public string ErrorLogPath;
    }
}
