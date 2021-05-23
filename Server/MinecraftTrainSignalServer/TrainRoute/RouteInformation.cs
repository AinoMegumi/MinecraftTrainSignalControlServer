using MinecraftTrainSignalServer.ExpandException;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using MinecraftTrainSignalServer.Response;

namespace MinecraftTrainSignalServer.TrainRoute
{
    public class RouteInformation
    {
        public string RouteName;
        public string RouteID;
        public List<TrafficInformation> TrafficInformations;
        public RouteInformation()
        {
            RouteName = string.Empty;
            RouteID = string.Empty;
            TrafficInformations = new List<TrafficInformation>();
        }
        public RouteInformation(string RouteID, RequestBody.AddTrainRoute TrainRouteInfo, out List<string> TrafficIDs)
        {
            RouteName = TrainRouteInfo.RouteName;
            this.RouteID = RouteID;
            TrafficInformations = new List<TrafficInformation>();
            TrafficIDs = new List<string>();
            for (int i = 0; i < TrainRouteInfo.RevealTypes.Count; i++)
            {
                string ID = Guid.NewGuid().ToString("N");
                while (TrafficInformations.Exists(m => m.TrafficID == ID)) ID = Guid.NewGuid().ToString("N");
                TrafficInformations.Add(new TrafficInformation(ID, TrainRouteInfo.RevealTypes[i], i));
                TrafficIDs.Add(ID);
            }
        }
        public void ChangeStopSignal(string TargetTrafficID)
        {
            int Index = TrafficInformations.FindIndex(m => m.TrafficID == TargetTrafficID);
            if (Index == -1) throw HttpException.ClientError.NotFound("Traffic ID is invalid");
            TrafficInformations[Index].CurrentSignalReveal = SignalControl.Stop;
            for (int i = Index - 1; i >= 0; i--)
            {
                if (TrafficInformations[i].CurrentSignalReveal == SignalControl.Stop) break;
                TrafficInformations[i].CurrentSignalReveal = NewSignalManager.GetNewSignal(TrafficInformations[i].TrafficTypeID, TrafficInformations[i + 1].CurrentSignalReveal);
            }
        }
        public void OpenReservedBlockage(string TargetTrafficID)
        {
            int Index = TrafficInformations.FindIndex(m => m.TrafficID == TargetTrafficID);
            if (Index == -1) throw HttpException.ClientError.NotFound("Traffic ID is invalid");
            if (Index == TrafficInformations.Count - 1) TrafficInformations[Index].CurrentSignalReveal = NewSignalManager.GetMaxSignal(TrafficInformations[Index].TrafficTypeID);
            else TrafficInformations[Index].CurrentSignalReveal = NewSignalManager.GetNewSignal(TrafficInformations[Index].TrafficTypeID, TrafficInformations[Index + 1].CurrentSignalReveal);
            for (int i = Index - 1; i >= 0; i--)
            {
                if (TrafficInformations[i].CurrentSignalReveal == SignalControl.Stop) break;
                TrafficInformations[i].CurrentSignalReveal = NewSignalManager.GetNewSignal(TrafficInformations[i].TrafficTypeID, TrafficInformations[i + 1].CurrentSignalReveal);
            }
        }
        public List<ResTrafficInformation> GetTrafficInformations()
        {
            return TrafficInformations.Select(s => new ResTrafficInformation { TrafficID = s.TrafficID, TrafficType = s.TrafficTypeID, Reveal = s.SignalReveal }).ToList();
        }
        public string AddTraffic(string BeforeTraffic, int TrafficTypeID)
        {
            int Index = TrafficInformations.FindIndex(m => m.TrafficID == BeforeTraffic);
            if (Index == -1) throw HttpException.ClientError.NotFound("Traffic ID is invalid");
            string ID = Guid.NewGuid().ToString("N");
            while (TrafficInformations.Exists(m => m.TrafficID == ID)) ID = Guid.NewGuid().ToString("N");
            TrafficInformations.Insert(Index + 1, new TrafficInformation(ID, TrafficTypeID, 0));
            for (int i = 0; i < TrafficInformations.Count; i++) TrafficInformations[i].OrderID = i;
            for (int i = Index + 1; i >= 0; i--)
            {
                if (TrafficInformations[Index] <= TrafficInformations[Index]) break;
                TrafficInformations[Index].CurrentSignalReveal = NewSignalManager.GetNewSignal(TrafficInformations[Index].TrafficTypeID, TrafficInformations[Index + 1].CurrentSignalReveal);
            }
            return ID;
        }
        public void ChangeAllTrafficToStop()
        {
            for (int i = 0; i < TrafficInformations.Count; i++) TrafficInformations[i].CurrentSignalReveal = SignalControl.Stop;
        }
        public void ResetAllTraffic()
        {
            for (int i = 0; i < TrafficInformations.Count; i++) TrafficInformations[i].ToMaxSignal();
        }
        public void UpdateTrafficType(string TargetTraffic, int NewType)
        {
            int Index = TrafficInformations.FindIndex(m => m.TrafficID == TargetTraffic);
            if (Index == -1) throw HttpException.ClientError.NotFound("Traffic ID is invalid");
            TrafficInformations[Index].TrafficTypeID = NewType;
            for (int i = Index; i >= 0; i--)
            {
                if (TrafficInformations[Index].CurrentSignalReveal == SignalControl.Stop) break;
                TrafficInformations[Index].ToMaxSignal();
            }
            for (int i = Index; i >= 0; i--)
            {
                if (TrafficInformations[Index] <= TrafficInformations[Index]) break;
                TrafficInformations[Index].CurrentSignalReveal = NewSignalManager.GetNewSignal(TrafficInformations[Index].TrafficTypeID, TrafficInformations[Index + 1].CurrentSignalReveal);
            }
        }
        public int GetCurrentReveal(string TrafficID)
        {
            int Index = TrafficInformations.FindIndex(m => m.TrafficID == TrafficID);
            if (Index == -1) throw HttpException.ClientError.NotFound($"{TrafficID}: Traffic ID is invalid");
            return TrafficInformations[Index].SignalReveal;
        }
        public int GetTrafficType(string TrafficID)
        {
            int Index = TrafficInformations.FindIndex(m => m.TrafficID == TrafficID);
            if (Index == -1) throw HttpException.ClientError.NotFound($"{TrafficID}: Traffic ID is invalid");
            return TrafficInformations[Index].TrafficTypeID;
        }
        public void DeleteTraffic(string TrafficID)
        {
            int Index = TrafficInformations.FindIndex(m => m.TrafficID == TrafficID);
            if (Index == -1) throw HttpException.ClientError.NotFound($"{TrafficID}: Traffic ID is invalid");
            TrafficInformations.RemoveAt(Index);
            for (int i = 0; i < TrafficInformations.Count; i++) TrafficInformations[i].OrderID = i;
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
