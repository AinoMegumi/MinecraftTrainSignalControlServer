using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MinecraftTrainSignalServer.ExpandException;
using System.IO;
using Newtonsoft.Json;
using MinecraftTrainSignalServer.TrainRoute;

namespace MinecraftTrainSignalServer.Controllers
{
    [Route("/v1/")]
    [ApiController]
    public class SignalManagementController : BaseController
    {
        [HttpPost]
        public async Task<ActionResult> AddTrainRoute()
        {
            try
            {
                using StreamReader sr = new StreamReader(Request.Body);
                string Body = await sr.ReadToEndAsync();
                if (string.IsNullOrEmpty(Body)) throw HttpException.ClientError.BadRequest("Request Body is empty.");
                RequestBody.AddTrainRoute RouteInfo = JsonConvert.DeserializeObject<RequestBody.AddTrainRoute>(Body);
                using RouteInformationManager Route = new RouteInformationManager(RouteInfo, out List<string> TrafficIDs);
                var json = new
                {
                    routeid = Route.ID,
                    traffics = TrafficIDs
                };
                Response.StatusCode = 201;
                return new JsonResult(json);
            }
            catch (HttpException hex)
            {
                await WriteErrorLog(hex);
                Response.StatusCode = hex.StatusCode;
                return Content(hex.Message);
            }
            catch (Exception e)
            {
                await WriteErrorLog(e);
                return new JsonResult(InternalServerError);
            }
            finally
            {
                await WriteOperationLog();
            }
        }
        [HttpGet("{routeid}")]
        public async Task<ActionResult> GetTrainRouteInfoFromRouteID([FromRoute(Name = "routeid")] string RouteID)
        {
            try
            {
                using RouteInformationManager Route = new RouteInformationManager(RouteID);
                var json = new
                {
                    name = Route.Name,
                    traffics = Route.GetTrafficInformations()
                };
                return new JsonResult(json);
            }
            catch (HttpException hex)
            {
                await WriteErrorLog(hex);
                Response.StatusCode = hex.StatusCode;
                return Content(hex.Message, "text/plain");
            }
            catch (Exception e)
            {
                await WriteErrorLog(e);
                return Content(InternalServerError, "text/plain");
            }
            finally
            {
                await WriteOperationLog();
            }
        }
        [HttpPatch("{routeid}")]
        public async Task<string> UpdateRouteInformation([FromRoute(Name = "routeid")] string RouteID)
        {
            try
            {
                using (RouteInformationManager Route = new RouteInformationManager(RouteID))
                {
                    if (Route.ProtectedPassword() && !Route.PasswordIsValid(GetPassword())) throw HttpException.ClientError.Unauthorized("Password is invalid");
                    using (StreamReader sr = new StreamReader(Request.Body))
                    {
                        string Body = await sr.ReadToEndAsync();
                        if (string.IsNullOrEmpty(Body)) throw HttpException.ClientError.BadRequest("Request Body is empty.");
                        RequestBody.UpdateRouteInformation Info = JsonConvert.DeserializeObject<RequestBody.UpdateRouteInformation>(Body);
                        if (!string.IsNullOrEmpty(Info.NewPassword)) Route.ChangePassword(Info.NewPassword);
                        if (!string.IsNullOrEmpty(Info.NewRouteName)) Route.ChangeRouteName(Info.NewRouteName);
                    }
                }
                return string.Empty;
            }
            catch (HttpException hex)
            {
                await WriteErrorLog(hex);
                Response.StatusCode = hex.StatusCode;
                return hex.Message;
            }
            catch (Exception e)
            {
                await WriteErrorLog(e);
                return InternalServerError;
            }
            finally
            {
                await WriteOperationLog();
            }
        }
        [HttpDelete("{routeid}")]
        public async Task<string> DeleteTrainRouteInfo([FromRoute(Name = "routeid")] string RouteID)
        {
            try
            {
                using (RouteInformationManager Route = new RouteInformationManager(RouteID))
                {
                    if (Route.ProtectedPassword() && !Route.PasswordIsValid(GetPassword())) throw HttpException.ClientError.Unauthorized("Password is invalid");
                    Route.DeleteRoute();
                }
                return string.Empty;
            }
            catch (HttpException hex)
            {
                await WriteErrorLog(hex);
                Response.StatusCode = hex.StatusCode;
                return hex.Message;
            }
            catch (Exception e)
            {
                await WriteErrorLog(e);
                return InternalServerError;
            }
            finally
            {
                await WriteOperationLog();
            }
        }
        [HttpPost("{routeid}/{trafficid}")]
        public async Task<string> SetTrafficToStopSignal([FromRoute(Name = "routeid")] string RouteID, [FromRoute(Name = "trafficid")] string TrafficID)
        {
            try
            {
                using (RouteInformationManager Route = new RouteInformationManager(RouteID))
                {
                    if (Route.ProtectedPassword() && !Route.PasswordIsValid(GetPassword())) throw HttpException.ClientError.Unauthorized("Password is invalid");
                    if (TrafficID == "all") Route.ChangeAllTrafficToStop();
                    else if (TrafficID == "clear") Route.ResetAllTraffic();
                    else Route.ChangeStopSignal(TrafficID);
                }
                return string.Empty;
            }
            catch (HttpException hex)
            {
                await WriteErrorLog(hex);
                Response.StatusCode = hex.StatusCode;
                return hex.Message;
            }
            catch (Exception e)
            {
                await WriteErrorLog(e);
                return InternalServerError;
            }
            finally
            {
                await WriteOperationLog();
            }
        }
        [HttpPost("{routeid}/{trafficid}/open")]
        public async Task<string> OpenReservedBlockage([FromRoute(Name = "routeid")] string RouteID, [FromRoute(Name = "trafficid")] string TrafficID)
        {
            try
            {
                using (RouteInformationManager Route = new RouteInformationManager(RouteID))
                {
                    if (Route.ProtectedPassword() && !Route.PasswordIsValid(GetPassword())) throw HttpException.ClientError.Unauthorized("Password is invalid");
                    Route.OpenReservedBlockage(TrafficID);
                }
                return string.Empty;
            }
            catch (HttpException hex)
            {
                await WriteErrorLog(hex);
                Response.StatusCode = hex.StatusCode;
                return hex.Message;
            }
            catch (Exception e)
            {
                await WriteErrorLog(e);
                return InternalServerError;
            }
            finally
            {
                await WriteOperationLog();
            }
        }
        [HttpGet("{routeid}/{trafficid}/reveal")]
        public async Task<string> GetTrafficReveal([FromRoute(Name = "routeid")] string RouteID, [FromRoute(Name = "trafficid")] string TrafficID)
        {
            try
            {
                using RouteInformationManager Route = new RouteInformationManager(RouteID);
                return Route.GetCurrentReveal(TrafficID).ToString();
            }
            catch (HttpException hex)
            {
                await WriteErrorLog(hex);
                Response.StatusCode = hex.StatusCode;
                return hex.Message;
            }
            catch (Exception e)
            {
                await WriteErrorLog(e);
                return InternalServerError;
            }
            finally
            {
                await WriteOperationLog();
            }
        }
        [HttpGet("{routeid}/{trafficid}/type")]
        public async Task<string> GetTrafficType([FromRoute(Name = "routeid")] string RouteID, [FromRoute(Name = "trafficid")] string TrafficID)
        {
            try
            {
                using RouteInformationManager Route = new RouteInformationManager(RouteID);
                return Route.GetTrafficType(TrafficID).ToString();
            }
            catch (HttpException hex)
            {
                await WriteErrorLog(hex);
                Response.StatusCode = hex.StatusCode;
                return hex.Message;
            }
            catch (Exception e)
            {
                await WriteErrorLog(e);
                return InternalServerError;
            }
            finally
            {
                await WriteOperationLog();
            }
        }
        [HttpDelete("{routeid}/{trafficid}")]
        public async Task<string> DeleteTraffic([FromRoute(Name = "routeid")] string RouteID, [FromRoute(Name = "trafficid")] string TrafficID)
        {
            try
            {
                using (RouteInformationManager Route = new RouteInformationManager(RouteID))
                {
                    if (Route.ProtectedPassword() && !Route.PasswordIsValid(GetPassword())) throw HttpException.ClientError.Unauthorized("Password is invalid");
                    Route.DeleteTraffic(TrafficID);
                }
                return string.Empty;
            }
            catch (HttpException hex)
            {
                await WriteErrorLog(hex);
                Response.StatusCode = hex.StatusCode;
                return hex.Message;
            }
            catch (Exception e)
            {
                await WriteErrorLog(e);
                return InternalServerError;
            }
            finally
            {
                await WriteOperationLog();
            }
        }
        [HttpPost("{routeid}/{trafficid}/{traffictypeid}")]
        public async Task<string> AddTraffic([FromRoute(Name = "routeid")] string RouteID, [FromRoute(Name = "trafficid")] string TrafficID, [FromRoute(Name = "traffictypeid")] int TrafficTypeID)
        {
            try
            {
                using RouteInformationManager Route = new RouteInformationManager(RouteID);
                if (Route.ProtectedPassword() && !Route.PasswordIsValid(GetPassword())) throw HttpException.ClientError.Unauthorized("Password is invalid");
                return Route.AddTraffic(TrafficID, TrafficTypeID);
            }
            catch (HttpException hex)
            {
                await WriteErrorLog(hex);
                Response.StatusCode = hex.StatusCode;
                return hex.Message;
            }
            catch (Exception e)
            {
                await WriteErrorLog(e);
                return InternalServerError;
            }
            finally
            {
                await WriteOperationLog();
            }
        }
        [HttpPut("{routeid}/{trafficid}/{traffictypeid}")]
        public async Task<string> UpdateTrafficType([FromRoute(Name = "routeid")] string RouteID, [FromRoute(Name = "trafficid")] string TrafficID, [FromRoute(Name = "traffictypeid")] int NewTrafficTypeID)
        {
            try
            {
                using (RouteInformationManager Route = new RouteInformationManager(RouteID))
                {
                    if (Route.ProtectedPassword() && !Route.PasswordIsValid(GetPassword())) throw HttpException.ClientError.Unauthorized("Password is invalid");
                    Route.UpdateTrafficType(TrafficID, NewTrafficTypeID);
                }
                return string.Empty;
            }
            catch (HttpException hex)
            {
                await WriteErrorLog(hex);
                Response.StatusCode = hex.StatusCode;
                return hex.Message;
            }
            catch (Exception e)
            {
                await WriteErrorLog(e);
                return InternalServerError;
            }
            finally
            {
                await WriteOperationLog();
            }
        }
    }
}
