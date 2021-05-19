using Newtonsoft.Json;
using System.Collections.Generic;

namespace MinecraftTrainSignalServer.RequestBody
{
    public class AddTrainRoute
    {
        [JsonProperty(PropertyName = "reveals", Required = Required.Always)]
        public List<int> RevealTypes;
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string RouteName;
        [JsonProperty(PropertyName = "password", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string RoutePassword;
    }
}
