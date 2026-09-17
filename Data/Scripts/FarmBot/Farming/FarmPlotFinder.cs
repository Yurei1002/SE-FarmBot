using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using ParallelTasks;
using Sandbox.Game;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.VoiceChat;
using Sandbox.ModAPI;
using VRage;
using VRage.Game.Components.Interfaces;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace FarmBot
{
    public static class FarmPlotFinder
    {
        public static List<FarmPlot> FindFarmPlots()
        {

        //MyLog.Default.WriteLineAndConsole("[FarmBot] FindFarmPlots wurde aufgerufen");

        List<FarmPlot> farmPlots = new List<FarmPlot>();
        HashSet<IMyEntity> entities = new HashSet<IMyEntity>();

        MyAPIGateway.Entities.GetEntities(entities);
            int gridCount = 0;
            foreach (VRage.ModAPI.IMyEntity entity in entities)
            {
                IMyCubeGrid grid = entity as VRage.Game.ModAPI.IMyCubeGrid;

                if (grid == null)
                continue;

                gridCount++;

                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Grid gefunden: {grid.CustomName}");

                foreach (VRage.Game.ModAPI.IMyCubeBlock block in grid.GetFatBlocks<VRage.Game.ModAPI.IMyCubeBlock>())
                {
                    if (block.BlockDefinition.SubtypeId != "LargeBlockFarmPlot")
                    continue;
                    
                    FarmPlot farmPlot = new FarmPlot();

                    farmPlot.EntityID = block.EntityId;
                    farmPlot.GridID = grid.EntityId;
                    farmPlot.GridName = grid.CustomName;

                    IMyTerminalBlock terminalBlock = block as IMyTerminalBlock;

                    if(terminalBlock != null)
                        farmPlot.CustomName = terminalBlock.CustomName;

                    //MyLog.Default.WriteLineAndConsole($"[FarmBot] Es wurde ein Farmplot mit folgenden Daten gefunden: ID: {farmPlot.EntityID}, Grid ID: {farmPlot.GridID}, Grid: {farmPlot.GridName}");

                    farmPlots.Add(farmPlot);

                }
            }

            //MyLog.Default.WriteLineAndConsole($"[FarmBot] Grid gefunden: {gridCount}");
            //MyLog.Default.WriteLineAndConsole($"[FarmBot] FindFarmPlots beendet: Gefundene Plots: {farmPlots.Count}");
            return farmPlots;
        }
    }
}

