using VRage.Utils;
using Sandbox.ModAPI;


namespace FarmBot
{
    public class Persistence
    {
        private const int DataVersion = 1;
        private const string StorageKey = "FarmBot";

        public void Load()
        {
            string data;

            if (!MyAPIGateway.Utilities.GetVariable(StorageKey, out data))
            {
                MyLog.Default.WriteLineAndConsole($"[FarmBot] No data found.");
                return;
            }
            MyLog.Default.WriteLineAndConsole($"[FarmBot] Data loaded.");
        }

        public void Save(string data)
        {
            MyAPIGateway.Utilities.SetVariable(StorageKey, data);
            MyLog.Default.WriteLineAndConsole($"[FarmBot] Data saved.");
        }
    }
}