using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;

namespace FarmBot
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class Main : MySessionComponentBase
    {
        private bool farmPlotsSearched = false;
        private FarmPlotManager farmPlotManager;
        private FarmBotMenu farmBotMenu;
        private Networking networking;
        private FarmingManager farmingManager;
        private Persistence persistence;

        private int farmingTimer = 0;

        public override void LoadData()
        {
            //MyLog.Default.WriteLineAndConsole("[FarmBot] >>> MAIN LOADDATA <<<");

            Localization.Initialize(ModContext.ModItem);

            persistence = new Persistence();
            persistence.Load();

            farmPlotManager = new FarmPlotManager();
            farmBotMenu = new FarmBotMenu(farmPlotManager);
            networking = new Networking(farmPlotManager);
            farmingManager = new FarmingManager();


            networking.SetFarmPlotReceiver(
                farmBotMenu.ApplyServerState
            );

            farmBotMenu.SetNetworking(networking);

        }

        public override void UpdateBeforeSimulation()
        {
            if (MyAPIGateway.Multiplayer != null &&
                !MyAPIGateway.Multiplayer.IsServer)
                return;

            if (!farmPlotsSearched)
            {
                farmPlotsSearched = true;

                List<FarmPlot> farmPlots =
                    FarmPlotFinder.FindFarmPlots();

                farmPlotManager.Initialize(farmPlots);
                farmBotMenu.BuildMenuFromPlots(farmPlots);

                //MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmPlotManager initialized with {farmPlots.Count} plots.");
            }

            farmingTimer++;

            if (farmingTimer < 600)
                return;

            farmingTimer = 0;

            RunFarming();
        }

        private void RunFarming()
        {
            List<FarmPlot> farmPlots =
                farmPlotManager.GetFarmPlots();

            Dictionary<long, int> cropConfiguration =
                networking.GetCropConfiguration();

            foreach (FarmPlot farmPlot in farmPlots)
            {
                int crop;

                if (!cropConfiguration.TryGetValue(
                    farmPlot.EntityID,
                    out crop))
                    continue;

                if (crop <= 0 || crop > 4)
                    continue;

                string seedIdText =
                    GetSeedId(crop);

                if (seedIdText == null)
                    continue;

                MyDefinitionId seedId;

                if (!MyDefinitionId.TryParse(
                    seedIdText,
                    out seedId))
                    continue;

                farmingManager.PlantFarmPlot(
                    farmPlot,
                    seedId
                );
            }
        }

        private string GetSeedId(int crop)
        {
            switch (crop)
            {
                case 1:
                    return "MyObjectBuilder_SeedItem/Grain";

                case 2:
                    return "MyObjectBuilder_SeedItem/Vegetables";

                case 3:
                    return "MyObjectBuilder_SeedItem/Mushrooms";

                case 4:
                    return "MyObjectBuilder_SeedItem/Fruit";

                default:
                    return null;
            }
        }
    }
}