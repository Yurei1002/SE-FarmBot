using Draygo.API;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using VRage.Game.ModAPI.Ingame;
using VRage.Utils;

namespace FarmBot
{
    public class FarmBotMenu
    {
        private HudAPIv2 hud;
        private HudAPIv2.MenuRootCategory rootMenu;
        private readonly Dictionary<long, HudAPIv2.MenuSubCategory> gridMenus = new Dictionary<long, HudAPIv2.MenuSubCategory>();
        private HudAPIv2.MenuItem automaticItem;
        private bool automaticPlanting;

        private readonly List<HudAPIv2.MenuItem> plotItems = new List<HudAPIv2.MenuItem>();
        private readonly List<long> menuPlotIds = new List<long>();
        private readonly Dictionary<long, int> cropByPlot =
            new Dictionary<long, int>();

        private List<FarmPlot> currentPlots;
        private bool hudReady;
        private Networking networking;
        private FarmPlotManager farmPlotManager;

        public FarmBotMenu(FarmPlotManager farmPlotManager)
        {
            this.farmPlotManager = farmPlotManager;
            hud = new HudAPIv2(OnHudRegistered);
        }

        private void OnHudRegistered()
        {
            if (hud == null || !hud.Heartbeat)
                return;

            hudReady = true;

            MyLog.Default.WriteLineAndConsole("[FarmBot] Text HUD API initialized.");

            if (MyAPIGateway.Multiplayer == null)
            {
                if (currentPlots != null)
                    BuildMenuFromPlots(currentPlots);
                return;
            }

            if (MyAPIGateway.Multiplayer.IsServer)
            {
                if (currentPlots != null)
                    BuildMenuFromPlots(currentPlots);
                return;
            }

            if (networking != null)
            {
                networking.RequestFarmPlots();
            }
        }

        private void ToggleAutomaticPlanting()
        {
            automaticPlanting = !automaticPlanting;

            automaticItem.Text =
                "Automatic Planting: " +
                (automaticPlanting ? "On" : "Off");

            //MyLog.Default.WriteLineAndConsole($"[FarmBot] Automatic Planting: {(automaticPlanting ? "On" : "Off")}");
        }

        private Dictionary<long, List<FarmPlot>> GroupPlotsByGrid(
            List<FarmPlot> plots)
        {
            Dictionary<long, List<FarmPlot>> groupedPlots =
                new Dictionary<long, List<FarmPlot>>();

            foreach (FarmPlot plot in plots)
            {
                if (!groupedPlots.ContainsKey(plot.GridID))
                {
                    groupedPlots.Add(
                        plot.GridID,
                        new List<FarmPlot>()
                    );
                }

                groupedPlots[plot.GridID].Add(plot);
            }

            return groupedPlots;
        }

        public void BuildMenuFromPlots(List<FarmPlot> plots)
        {
            //MyLog.Default.WriteLineAndConsole($"[FarmBot] BuildMenuFromPlots called. Plotcount: {plots.Count}");

            currentPlots = plots;

            if (!hudReady)
                return;

            if (rootMenu != null)
                return;

            rootMenu = new HudAPIv2.MenuRootCategory(
                "FarmBot",
                HudAPIv2.MenuRootCategory.MenuFlag.AdminMenu,
                "FarmBot Settings"
            );

            automaticItem = new HudAPIv2.MenuItem(
                "Automatic Planting: " +
                (automaticPlanting ? "ON" : "OFF"),
                rootMenu,
                ToggleAutomaticPlanting
            );

            Dictionary<long, List<FarmPlot>> groupedPlots =
                GroupPlotsByGrid(plots);

            //MyLog.Default.WriteLineAndConsole($"[FarmBot] RootMenu exists: {rootMenu != null}");

            foreach (
                KeyValuePair<long, List<FarmPlot>> grid
                in groupedPlots)
            {
                FarmPlot firstPlot = grid.Value[0];

                HudAPIv2.MenuSubCategory gridMenu =
                    new HudAPIv2.MenuSubCategory(
                        firstPlot.GridName,
                        rootMenu,
                        firstPlot.GridName
                    );

                gridMenus.Add(
                    grid.Key,
                    gridMenu
                );

                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Grid menu created. GridID: {grid.Key}, Name: {firstPlot.GridName}, FarmPlots: {grid.Value.Count}");

                for (int i = 0; i < grid.Value.Count; i++)
                {
                    FarmPlot plot = grid.Value[i];

                    long entityId =
                        plot.EntityID;

                    if (!cropByPlot.ContainsKey(entityId))
                        cropByPlot[entityId] = 0;

                    int menuIndex =
                        plotItems.Count;

                    HudAPIv2.MenuItem plotItem =
                        new HudAPIv2.MenuItem(
                            GetPlotText(
                                menuIndex,
                                entityId
                            ),
                            gridMenu,
                            delegate
                            {
                                CyclePlot(
                                    entityId,
                                    menuIndex
                                );
                            }
                        );

                    plotItems.Add(plotItem);
                    menuPlotIds.Add(entityId);
                }
            }
        }

        private void CyclePlot(
            long entityId,
            int menuIndex)
        {
            int value;

            if (!cropByPlot.TryGetValue(
                entityId,
                out value))
            {
                value = 0;
            }

            value++;

            if (value > 4)
                value = 0;

            cropByPlot[entityId] =
                value;

            if (menuIndex >= 0 &&
                menuIndex < plotItems.Count)
            {
                plotItems[menuIndex].Text =
                    GetPlotText(
                        menuIndex,
                        entityId
                    );
            }

            //MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmPlot {entityId} changed to {CropName(value)}");

            if (networking != null)
            {
                networking.SetCropSelection(
                    entityId,
                    value
                );
            }
        }

        private string GetPlotText(
            int index,
            long entityId)
        {
            int value;

            if (!cropByPlot.TryGetValue(
                entityId,
                out value))
            {
                value = 0;
            }

            return "Feld " +
                   (index + 1) +
                   " -> " +
                   CropName(value);
        }

        private static string CropName(
            int value)
        {
            switch (value)
            {
                case 1:
                    return "Grain";

                case 2:
                    return "Vegetable";

                case 3:
                    return "Mushrooms";

                case 4:
                    return "Fruit";

                default:
                    return "Disabled";
            }
        }

        private static string CropSeedId(
            int value)
        {
            switch (value)
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

        public void ApplyServerState(
            List<FarmPlot> plots,
            Dictionary<long, int> cropConfiguration)
        {
            currentPlots = plots;

            cropByPlot.Clear();

            foreach (
                KeyValuePair<long, int> entry
                in cropConfiguration)
            {
                cropByPlot[entry.Key] =
                    entry.Value;
            }

            //MyLog.Default.WriteLineAndConsole($"[FarmBot] Applied server crop configuration. Entries: {cropByPlot.Count}");

            BuildMenuFromPlots(plots);

            for (
                int i = 0;
                i < plotItems.Count &&
                i < menuPlotIds.Count;
                i++)
            {
                plotItems[i].Text =
                    GetPlotText(
                        i,
                        menuPlotIds[i]
                    );
            }
        }

        public void SetNetworking(
            Networking networking)
        {
            this.networking = networking;
        }

        public void RefreshFarmPlots()
        {
            BuildMenuFromPlots(farmPlotManager.GetFarmPlots());
        }
    }
}