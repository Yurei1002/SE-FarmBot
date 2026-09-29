using VRage.Utils;
using Sandbox.ModAPI;
using System.Collections.Generic;


namespace FarmBot
{

    public class FarmBotData
    {
        public int DataVersion;
        public bool AutomaticPlanting;
        public Dictionary<long, int> CropByPlot = new Dictionary<long, int>();
    }

    public class Persistence
    {        
        private const int DataVersion = 1;
        private const string StorageKey = "FarmBot";

        public FarmBotData Load()
        {
            string data;

            if (!MyAPIGateway.Utilities.GetVariable(StorageKey, out data))
            {
                MyLog.Default.WriteLineAndConsole($"[FarmBot] No data found.");
                return null;
            }
            MyLog.Default.WriteLineAndConsole($"[FarmBot] Data loaded.");
            return null;
        }

        public void Save(FarmBotData data)
        {
            MyAPIGateway.Utilities.SetVariable(StorageKey, data);
            MyLog.Default.WriteLineAndConsole($"[FarmBot] Data saved.");
        }
    }
}