using Newtonsoft.Json;

namespace MinecraftTrainSignalServer.RequestBody
{
    public class UpdateRouteInformation
    {
        [JsonProperty(PropertyName = "name", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string NewRouteName;
        [JsonProperty(PropertyName = "newPassword", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public string NewPassword;
    }
}
