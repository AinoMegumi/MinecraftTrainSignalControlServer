using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MinecraftTrainSignalServer.TrainRoute
{
    public class TrafficInformation
    {
        public int TrafficTypeID;
        public string TrafficID;
        public SignalControl CurrentSignalReveal;
        public int SignalReveal { get => (int)CurrentSignalReveal; set => CurrentSignalReveal = (SignalControl)value; }
        public int OrderID;
        public bool IsMax { get => NewSignalManager.IsMaxSignal(TrafficTypeID, CurrentSignalReveal); }
        public TrafficInformation() : this("", 3, 0) { }
        public TrafficInformation(string ID, int TrafficType, int Order)
        {
            TrafficID = ID;
            TrafficTypeID = TrafficType;
            CurrentSignalReveal = NewSignalManager.GetMaxSignal(TrafficType);
            OrderID = Order;
        }
        public void ToMaxSignal()
        {
            CurrentSignalReveal = NewSignalManager.GetMaxSignal(TrafficTypeID);
        }
        public static bool operator > (TrafficInformation a, TrafficInformation b)
        {
            return a.CurrentSignalReveal > b.CurrentSignalReveal;
        }
        public static bool operator < (TrafficInformation a, TrafficInformation b)
        {
            return a.CurrentSignalReveal < b.CurrentSignalReveal;
        }
        public static bool operator >= (TrafficInformation a, TrafficInformation b)
        {
            return a.CurrentSignalReveal >= b.CurrentSignalReveal;
        }
        public static bool operator <= (TrafficInformation a, TrafficInformation b)
        {
            return a.CurrentSignalReveal <= b.CurrentSignalReveal;
        }
        public static bool operator == (TrafficInformation a, TrafficInformation b)
        {
            return a.CurrentSignalReveal == b.CurrentSignalReveal;
        }
        public static bool operator != (TrafficInformation a, TrafficInformation b)
        {
            return a.CurrentSignalReveal != b.CurrentSignalReveal;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
