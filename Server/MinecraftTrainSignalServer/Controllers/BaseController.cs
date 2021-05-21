using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftTrainSignalServer.Controllers
{
    public class BaseController : ControllerBase
    {
        private readonly MasterConfig Master;
        private static void CreateDirectory(string FileCreatePath)
        {
            if (Directory.GetDirectoryRoot(FileCreatePath) == FileCreatePath) return;
            DirectoryInfo Dir = Directory.GetParent(FileCreatePath);
            if (!Dir.Exists) Dir.Create();
        }
        public BaseController()
        {
            Master = JsonConvert.DeserializeObject<MasterConfig>(System.IO.File.ReadAllText("./master.json"));
            CreateDirectory(Master.ErrorLogPath);
            CreateDirectory(Master.OperationLogPath);
        }
        protected string InternalServerError
        {
            get
            {
                Response.StatusCode = 500;
                return "Internal Server Error";
            }
        }
        private string ConnectedIPAddress { get => Request.HttpContext.Connection.RemoteIpAddress.ToString(); }
        private static string CurrentTime
        {
            get => string.Format("{0:F}", DateTime.Now);
        }
        protected async Task WriteOperationLog()
        {
            using StreamWriter sr = new StreamWriter(new FileStream(Master.OperationLogPath, FileMode.Append, FileAccess.Write));
            string LogMessage = $"{CurrentTime} {Request.Method} {Request.Path} {Response.StatusCode} : {ConnectedIPAddress}";
            await sr.WriteLineAsync(LogMessage);
        }
        protected async Task WriteErrorLog(Exception e)
        {
            using StreamWriter sr = new StreamWriter(new FileStream(Master.ErrorLogPath, FileMode.Append, FileAccess.Write));
            string LogMessage = $"{CurrentTime} {Request.Method} {Request.Path} : {ConnectedIPAddress} {e.Message}";
            await sr.WriteLineAsync(LogMessage);
        }
        protected string GetPassword()
        {
            return Request.Headers.ContainsKey("Authorization")
                ? Encoding.UTF8.GetString(Convert.FromBase64String(Request.Headers["Authorization"]))
                : string.Empty;
        }
    }
}
