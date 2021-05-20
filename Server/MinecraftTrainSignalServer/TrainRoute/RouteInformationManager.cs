using Meigetsu.Net.Database;
using MinecraftTrainSignalServer.ExpandException;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftTrainSignalServer.TrainRoute
{
    public class RouteInformationManager : IDisposable
    {
        private static readonly MySQLConnectManager MySQL
            = new MySQLConnectManager(
                ConfigurationManager.AppSettings["MySQL_User"],
                ConfigurationManager.AppSettings["MySQL_Pass"],
                ConfigurationManager.AppSettings["MySQL_Database"],
                ConfigurationManager.AppSettings["MySQL_Host"]
                );
        private readonly RouteInformation Route;
        private string Password;
        public string ID { get => Route.RouteID; }
        public string Name { get => Route.RouteName; }
        private static string ToPasswordHash(string RawPassword)
        {
            byte[] input = Encoding.UTF8.GetBytes(RawPassword);
            SHA256 sha = new SHA256CryptoServiceProvider();
            byte[] hash = sha.ComputeHash(input);
            for (int i = 0; i < 9; i++) hash = sha.ComputeHash(hash);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash) sb.AppendFormat("{0:X2}", b);
            return sb.ToString();
        }
        public RouteInformationManager(RequestBody.AddTrainRoute TrainRouteInfo, out List<string> TrafficIDs)
        {
            MySQL.Execute("CREATE TABLE IF NOT EXISTS routes (ID TEXT NOT NULL, Name TEXT NOT NULL, Password TEXT NULL)");
            MySQLDataManager Data = MySQL.Query("SELECT ID FROM routes");
            
            string RouteID = Guid.NewGuid().ToString("N");
            if (Data.Count != 0)
            {
                while (Data.Exists(m => m["ID"] == RouteID)) RouteID = Guid.NewGuid().ToString("N");
            }
            Route = new RouteInformation(RouteID, TrainRouteInfo, out TrafficIDs);
            Password = ToPasswordHash(TrainRouteInfo.RoutePassword);
            MySQL.Execute("INSERT INTO routes(ID,Name,Password) VALUES (@id, @name, @password)", new MySqlParameter("id", RouteID), new MySqlParameter("name", TrainRouteInfo.RouteName), new MySqlParameter("password", Password));
            MySQL.Execute($"CREATE TABLE {RouteID} (ID TEXT NOT NULL, Type INT, Reveal INT, OrderID INT)");
        }
        public RouteInformationManager(string RouteID)
        {
            MySQLDataManager Data = MySQL.Query("SELECT Name,Password FROM routes WHERE ID=@routeid", new MySqlParameter("routeid", RouteID));
            if (Data.Count != 1) throw HttpException.ClientError.NotFound($"{RouteID}: Route ID is invalid.");
            Route = new RouteInformation()
            {
                RouteID = RouteID,
                RouteName = Data[0]["Name"],
                TrafficInformations = new List<TrafficInformation>()
            };
            Password = Data[0]["Password"];
            Data = MySQL.Query($"SELECT OrderID,ID,Type,Reveal FROM {RouteID}");
            foreach (Dictionary<string, string> i in Data)
            {
                Route.TrafficInformations.Add(
                    new TrafficInformation()
                    {
                        TrafficID = i["ID"],
                        TrafficTypeID = int.Parse(i["Type"]),
                        SignalReveal = int.Parse(i["Reveal"]),
                        OrderID = int.Parse(i["OrderID"])
                    }
                    );
            }
            Route.TrafficInformations.Sort((a, b) => a.OrderID - b.OrderID);
        }
        public void Dispose()
        {
            MySQLDataManager Data = MySQL.Query("SELECT Password,Name FROM routes WHERE ID=@routeid", new MySqlParameter("routeid", Route.RouteID));
            // 路線削除されている場合は終了
            if (Data.Count == 0) return;
            if (!Data[0]["Name"].Equals(Route.RouteName)) MySQL.Execute("UPDATE routes SET Name=@NewVal WHERE ID=@routeid", new MySqlParameter("routeid", Route.RouteID), new MySqlParameter("NewVal", Route.RouteName));
            if (!Data[0]["Password"].Equals(Password)) MySQL.Execute("UPDATE routes SET Password=@NewVal WHERE ID=@routeid", new MySqlParameter("routeid", Route.RouteID), new MySqlParameter("NewVal", Password));
            Data = MySQL.Query($"SELECT OrderID,ID,Type,Reveal FROM {Route.RouteID}", new MySqlParameter("routeid", Route.RouteID));
            if (Route.TrafficInformations.Count != Data.Count)
            {
                MySQL.Execute($"DELETE FROM {Route.RouteID}");
                foreach (TrafficInformation t in Route.TrafficInformations)
                {
                    MySQL.Execute($"INSERT INTO {Route.RouteID} (ID, Type, Reveal, OrderID) VALUES (@id, @type, @reveal, @order)",
                        new MySqlParameter("id", t.TrafficID),
                        new MySqlParameter("type", t.TrafficTypeID),
                        new MySqlParameter("reveal", t.SignalReveal),
                        new MySqlParameter("order", t.OrderID)
                        );
                }
            }
            else
            {
                foreach (TrafficInformation t in Route.TrafficInformations)
                {
                    int i = Data.FindIndex(m => m["ID"].Equals(t.TrafficID));
                    if (i == -1) continue;
                    if (int.Parse(Data[i]["OrderID"]) != t.OrderID) MySQL.Execute($"UPDATE {Route.RouteID} SET OrderID=@NewVal WHERE ID=@trafficid", new MySqlParameter("NewVal", t.OrderID), new MySqlParameter("trafficid", t.TrafficID));
                    if (int.Parse(Data[i]["Type"]) != t.TrafficTypeID) MySQL.Execute($"UPDATE {Route.RouteID} SET Type=@NewVal WHERE ID=@trafficid", new MySqlParameter("NewVal", t.TrafficTypeID), new MySqlParameter("trafficid", t.TrafficID));
                    if (int.Parse(Data[i]["Reveal"]) != t.SignalReveal) MySQL.Execute($"UPDATE {Route.RouteID} SET Reveal=@NewVal WHERE ID=@trafficid", new MySqlParameter("NewVal", t.SignalReveal), new MySqlParameter("trafficid", t.TrafficID));
                }
            }
        }
        public bool ProtectedPassword() => Password.Length != 0;
        public bool PasswordIsValid(string RawPassword) => Password == ToPasswordHash(RawPassword);
        public void ChangeStopSignal(string TargetTrafficID) => Route.ChangeStopSignal(TargetTrafficID);
        public List<string> GetTrafficIDs() => Route.GetTrafficIDs();
        public string AddTraffic(string BeforeTraffic, int TrafficTypeID) => Route.AddTraffic(BeforeTraffic, TrafficTypeID);
        public void ChangeAllTrafficToStop() => Route.ChangeAllTrafficToStop();
        public void ResetAllTraffic() => Route.ResetAllTraffic();
        public void UpdateTrafficType(string TargetTraffic, int NewType) => Route.UpdateTrafficType(TargetTraffic, NewType);
    
        public void DeleteRoute()
        {
            MySQL.Execute("DELETE FROM routes WHERE ID=@routeid", new MySqlParameter("routeid", Route.RouteID));
            MySQL.Execute($"DROP TABLE IF EXISTS {Route.RouteID}");
        }
        public int GetCurrentReveal(string TrafficID) => Route.GetCurrentReveal(TrafficID);
        public void DeleteTraffic(string TrafficID) => Route.DeleteTraffic(TrafficID);
        public void ChangePassword(string NewPassword) { Password = ToPasswordHash(NewPassword); }
        public void ChangeRouteName(string NewName) { Route.RouteName = NewName; }
    }
}
