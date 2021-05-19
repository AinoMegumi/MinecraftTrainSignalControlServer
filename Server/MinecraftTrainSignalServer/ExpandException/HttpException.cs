using System;

namespace MinecraftTrainSignalServer.ExpandException
{
    public partial class HttpException : Exception
    {
        public int StatusCode { get; private set; }
        public HttpException(int HttpStatusCode, string Message) : base(Message) { StatusCode = HttpStatusCode; }
    }
}
