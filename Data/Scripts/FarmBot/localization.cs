using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using System.IO;
using VRage.Utils;



namespace FarmBot
{
    public static class Localization
    {
        private static MyLanguagesEnum currentLanguage;
        public static void Initialize()
        {
            currentLanguage = MyAPIGateway.Session.Config.Language;
            
            MyLog.Default.WriteLineAndConsole($"[FarmBot] Detected Game Language: {currentLanguage}");
            
        }
    }
}

