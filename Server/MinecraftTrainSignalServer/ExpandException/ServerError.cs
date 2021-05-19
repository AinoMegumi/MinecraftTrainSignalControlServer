namespace MinecraftTrainSignalServer.ExpandException
{
    public partial class HttpException
    {
        public class ServerError
        {
            public static HttpException InternalServerError(string Message = "Internal Server Error") => new HttpException(500, Message);
            public static HttpException ServiceUnavailable(string Message = "Service Unavailable") => new HttpException(503, Message);
        }
    }
}
