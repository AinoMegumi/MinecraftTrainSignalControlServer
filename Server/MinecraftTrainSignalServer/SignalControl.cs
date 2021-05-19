using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftTrainSignalServer
{
    public enum SignalControl
    {
        Stop = 0, // 停止
        Vigilant = 1, // 警戒
        Caution = 2, // 注意
        Decelerate = 3, // 減速
        Progress = 4, // 進行
        FastProgress = 5 // 高速進行
    }
    public class NewSignalManager
    {
        public static bool IsMaxSignal(int TrafficTypeID, SignalControl CurrentSignalReveal)
        {
            return CurrentSignalReveal == GetMaxSignal(TrafficTypeID);
        }
        public static SignalControl GetMaxSignal(int TrafficTypeID)
        {
            return TrafficTypeID == 0 ? SignalControl.Caution : (TrafficTypeID >= 6 ? SignalControl.FastProgress : SignalControl.Progress);
        }
        public static SignalControl GetNewSignal(int TrafficTypeID, SignalControl NextTrafficReveal)
        {
            if (TrafficTypeID == 0) return SignalControl.Caution;
            else if (TrafficTypeID == 1) return SignalControl.Progress;
            return TrafficTypeID switch
            {
                2 => GetNewSignal2(NextTrafficReveal),
                3 => GetNewSignal3(NextTrafficReveal),
                4 => GetNewSignal4(NextTrafficReveal),
                5 => GetNewSignal5(NextTrafficReveal),
                6 => GetNewSignal6(NextTrafficReveal),
                7 => GetNewSignal7(NextTrafficReveal),
                _ => throw new ArgumentOutOfRangeException("TrafficTypeID is invalid"),
            };
        } 

        private static SignalControl GetNewSignal2(SignalControl NextTrafficReveal)
        {
            return NextTrafficReveal == SignalControl.Stop || NextTrafficReveal == SignalControl.Vigilant
                ? SignalControl.Caution 
                : SignalControl.Progress;
        }

        private static SignalControl GetNewSignal3(SignalControl NextTrafficReveal)
        {
            return NextTrafficReveal == SignalControl.Stop
                ? SignalControl.Caution 
                : (NextTrafficReveal == SignalControl.Vigilant ? SignalControl.Caution : SignalControl.Progress);
        }

        private static SignalControl GetNewSignal4(SignalControl NextTrafficReveal)
        {
            return NextTrafficReveal == SignalControl.Stop || NextTrafficReveal == SignalControl.Vigilant
                ? SignalControl.Caution
                : (NextTrafficReveal == SignalControl.Caution ? SignalControl.Decelerate : SignalControl.Progress);
        }

        private static SignalControl GetNewSignal5(SignalControl NextTrafficReveal)
        {
            return NextTrafficReveal != SignalControl.Progress ? ++NextTrafficReveal : SignalControl.Progress;
        }

        private static SignalControl GetNewSignal6(SignalControl NextTrafficReveal)
        {
            return NextTrafficReveal == SignalControl.Stop || NextTrafficReveal == SignalControl.Vigilant
                ? SignalControl.Caution
                : (NextTrafficReveal == SignalControl.Caution || NextTrafficReveal == SignalControl.Decelerate ? SignalControl.Progress : SignalControl.FastProgress);
        }

        private static SignalControl GetNewSignal7(SignalControl NextTrafficReveal)
        {
            return NextTrafficReveal == SignalControl.Stop || NextTrafficReveal == SignalControl.Vigilant
                ? SignalControl.Caution
                : NextTrafficReveal == SignalControl.FastProgress ? SignalControl.FastProgress : ++NextTrafficReveal;
        }
    }
}
