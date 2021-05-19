namespace MinecraftTrainSignalServer.ExpandException
{
    public partial class HttpException
    {
        public class ClientError
        {
            public static HttpException BadRequest(string Message = "Bad Request") => new HttpException(400, Message);
            public static HttpException Unauthorized(string Message = "Unauthorized") => new HttpException(401, Message);
            public static HttpException Forbidden(string Message = "Forbidden") => new HttpException(403, Message);
            public static HttpException NotFound(string Message = "Not Found") => new HttpException(404, Message);
            public static HttpException MethodNotAllowed(string Message = "Method Not Allowed") => new HttpException(405, Message);
            public static HttpException RequestTimeout(string Message = "Request Time Out") => new HttpException(408, Message);
            public static HttpException Gone(string Message = "Gone") => new HttpException(410, Message);
            public static HttpException LengthRequired(string Message = "Length Required") => new HttpException(411, Message);
            public static HttpException URITooLong(string Message = "URI Too Long") => new HttpException(414, Message);
            public static HttpException UnsupportedMediaType(string Message = "Unsupported Media Type") => new HttpException(415, Message);
            public static HttpException TooManyRequests(string Message = "Too Many Requests") => new HttpException(429, Message);
        }
    }
}
