using System.Collections.Generic;
using Sandbox.Game.Screens;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace FarmBot
{
    
    public class FarmingManager
    {
        public void PlantFarmPlot(FarmPlot farmPlot, MyDefinitionId desiredSeed)
        {
            //MyLog.Default.WriteLineAndConsole($"[FarmBot] PlantFarmPlot wurde aufgerufen. EntityID: {farmPlot.EntityID}");

            IMyEntity entity = MyAPIGateway.Entities.GetEntityById(farmPlot.EntityID);

            if (entity == null)
            {
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmPlot Entity nicht gefunden. EntityID: {farmPlot.EntityID}");
                return;
            }

            IMyFunctionalBlock farmPlotBlock = entity as IMyFunctionalBlock;

            if (farmPlotBlock == null)
            {
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Entity ist kein IMyFunctionalBlock. EntityID: {farmPlot.EntityID}");
                return;
            }

            IMyFarmPlotLogic logic;

            if (!farmPlotBlock.Components.TryGet(out logic) || logic == null)
            {
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Keine IMyFarmPlotLogic gefunden. EntityID: {farmPlot.EntityID}");
                return;
            }

            if (logic.IsPlantPlanted)
            {
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmPlot ist bereits bepflanzt. EntityID: {farmPlot.EntityID}");
                return;
            }

            IMyCubeGrid grid = farmPlotBlock.CubeGrid;

            if (grid == null)
            {
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Kein Grid zum FarmPlot gefunden. EntityID: {farmPlot.EntityID}");
                return;
            }

           SeedInfo seedInfo = findseed(grid, desiredSeed);

            if (seedInfo == null)
            {
                MyLog.Default.WriteLineAndConsole($"[FarmBot] Kein Seed verfügbar. FarmPlot EntityID: {farmPlot.EntityID}");
                return;
            }

            MyDefinitionId actualSeedId = seedInfo.SeedId;
            IMyInventory inventory = seedInfo.Inventory;

            //MyLog.Default.WriteLineAndConsole($"[FarmBot] Seed zum Pflanzen ausgewählt. Seed: {actualSeedId}, FarmPlot EntityID: {farmPlot.EntityID}");

            logic.PlantSeed(actualSeedId);

            if (logic.IsPlantPlanted)
            {
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Seed erfolgreich gepflanzt. FarmPlot EntityID: {farmPlot.EntityID}");

                IMyInventoryItem seedItem = inventory.FindItem(actualSeedId);

                if (seedItem == null)
                {
                    MyLog.Default.WriteLineAndConsole($"[FarmBot] Seed nach dem Pflanzen nicht mehr im Inventar gefunden. FarmPlot EntityID: {farmPlot.EntityID}");
                    return;
                }

                inventory.RemoveItemAmount(seedItem, 1);
            }
            else
            {
                MyLog.Default.WriteLineAndConsole($"[FarmBot] Seed konnte nicht gepflanzt werden. FarmPlot EntityID: {farmPlot.EntityID}");
            }

        }
        private SeedInfo findseed(IMyCubeGrid grid, MyDefinitionId desiredSeed)
        {
            List<IMySlimBlock> blocks = new List<IMySlimBlock>();

            grid.GetBlocks(blocks);

            List<MyDefinitionId> seedIDs = new List<MyDefinitionId>();

            MyDefinitionId parsedSeedId;


                if (MyDefinitionId.TryParse("MyObjectBuilder_SeedItem/Grain", out parsedSeedId))
                {
                    seedIDs.Add(parsedSeedId);
                }

                if (MyDefinitionId.TryParse("MyObjectBuilder_SeedItem/Vegetables", out parsedSeedId))
                {
                    seedIDs.Add(parsedSeedId);
                }

                if (MyDefinitionId.TryParse("MyObjectBuilder_SeedItem/Mushrooms", out parsedSeedId))
                {
                    seedIDs.Add(parsedSeedId);
                }

                if (MyDefinitionId.TryParse("MyObjectBuilder_SeedItem/Fruit", out parsedSeedId))
                {
                    seedIDs.Add(parsedSeedId);
                }

            foreach (IMySlimBlock block in blocks)
            {
                IMyCubeBlock cubeBlock = block.FatBlock;

                if (cubeBlock == null)
                    continue;

                    IMyInventory inventory = cubeBlock.GetInventory();

                    if (inventory == null)
                        continue;

                IMyInventoryItem seedItem = inventory.FindItem(desiredSeed);

                if (seedItem == null)
                continue;

                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Desired seed found: {seedItem}, Grid ID: {grid.EntityId}");

                SeedInfo seedInfo = new SeedInfo();
                seedInfo.SeedId = desiredSeed;
                seedInfo.Inventory = inventory;

                return seedInfo;
            }
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Kein Seed auf dem Grid gefunden. GridID: {grid.EntityId}");
                return null;
        }        
    }

    public class SeedInfo
    {
        public MyDefinitionId SeedId;
        public IMyInventory Inventory;
    }
}