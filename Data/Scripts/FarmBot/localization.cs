using System.Collections.Generic;
using System.IO;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Utils;



namespace FarmBot
{
    public static class Localization
    {
        private static MyLanguagesEnum currentLanguage;
        private static MyObjectBuilder_Checkpoint.ModItem modItem;
        private static Dictionary<string, string> translations = new Dictionary<string, string>();
        private static Dictionary<string, string> englishTranslations = new Dictionary<string, string>();

        public static void Initialize(MyObjectBuilder_Checkpoint.ModItem mod)
        {

            if (MyAPIGateway.Utilities.IsDedicated)
            {
                MyLog.Default.WriteLineAndConsole($"[FarmBot] Skipping localization for dedicated server.");
                return;
            }
            
            modItem = mod;
            currentLanguage = MyAPIGateway.Session.Config.Language;
            MyLog.Default.WriteLineAndConsole($"[FarmBot] Detected Game Language: {currentLanguage}");

            string languageFile = GetLanguageFile();
            LoadLanguageFile(languageFile);
            MyLog.Default.WriteLineAndConsole($"[FarmBot] Localization file: {languageFile}");
            MyLog.Default.WriteLineAndConsole($"[FarmBot] Loaded localization entries: {translations.Count}");
        }

        private static string GetLanguageFile()
        {
            if (currentLanguage == MyLanguagesEnum.English)
                return "Data/Localization/english.json";
            
            string language = currentLanguage.ToString().ToLower();
            string file = "Data/Localization/" + language + ".json";

            if (MyAPIGateway.Utilities.FileExistsInModLocation(file, modItem))
            {
                LoadLanguageFile(file);
                LoadEnglishFile();
                return file;
            }

            LoadEnglishFile();
            return "Data/Localization/english.json";
        }

        private static void LoadLanguageFile(string file)
        {
            TextReader reader = MyAPIGateway.Utilities.ReadFileInModLocation(file, modItem);
            if (reader == null)
            return;

            string json = reader.ReadToEnd();
            reader.Dispose();
            translations = Parser.JsonParser(json);
        }

        private static void LoadEnglishFile()
        {
            string file = "Data/Localization/english.json";

            if (!MyAPIGateway.Utilities.FileExistsInModLocation(file, modItem))
                return;

            TextReader reader = MyAPIGateway.Utilities.ReadFileInModLocation(file, modItem);

            if (reader == null)
                return;

            string json = reader.ReadToEnd();
            reader.Dispose();
            englishTranslations = Parser.JsonParser(json);
        }

        public static string Get(string key)
        {
            string value;

            if (translations.TryGetValue(key, out value))
                return value;

            return key;
        }
    }
}
