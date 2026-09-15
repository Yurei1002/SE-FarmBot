using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;



namespace FarmBot
{
    public class FarmPlotManager
    {
        private Dictionary<long, FarmPlot> farmPlots = new Dictionary<long, FarmPlot>();
        private Dictionary<long, IMyCubeGrid> trackedGrids = new Dictionary<long, IMyCubeGrid>();

        public void Initialize(List<FarmPlot> plots)
        {   //MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmplotManager wird initialisiert. Plots zu verarbeiten: {plots.Count}");
            //farmPlots = new Dictionary<long, FarmPlot>();
            //trackedGrids = new Dictionary<long, IMyCubeGrid>();

            foreach (FarmPlot plot in plots)
            {
                farmPlots.Add(plot.EntityID, plot);
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Registriere FarmPlot. ID: {plot.EntityID}, Grid ID: {plot.GridID}, Grid: {plot.GridName}");

                TrackGrid(plot.GridID);
            }
            //MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmPlotManager initialisiert. Plots verwaltet: {farmPlots.Count}");
            MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
        }

        private void TrackGrid(long GridID)
        {   //MyLog.Default.WriteLineAndConsole($"[FarmBot] TrackGrid aufgerufen. Grid ID: {GridID}");
            if (trackedGrids.ContainsKey(GridID))
            {
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Grid bereits im Tracking. Grid ID: {GridID}");
                return;
            }

            HashSet<IMyEntity> entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities);
            //MyLog.Default.WriteLineAndConsole($"[FarmBot] Suche Grid in der Welt. Grid ID: {GridID}");

            foreach (IMyEntity entity in entities)
            {   
                IMyCubeGrid grid = entity as IMyCubeGrid;

                if (grid == null)
                {
                    continue;
                }

                if (grid.EntityId != GridID)
                {
                    continue;
                }

                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Grid gefunden. ID: {grid.EntityId}, Name: {grid.CustomName}");
                trackedGrids.Add(GridID, grid);
                grid.OnBlockAdded += OnBlockAdded;
                grid.OnBlockRemoved += OnBlockRemoved;
                break;
            }
        }

        private void OnBlockAdded(IMySlimBlock block)
        {
            //MyLog.Default.WriteLineAndConsole($"[FarmBot] OnBlockAdded wurde aufgerufen.");

            string subtype = block.BlockDefinition.Id.SubtypeName;

            //MyLog.Default.WriteLineAndConsole($"[FarmBot] Neuer Block hinzugefügt. Subtype: {subtype}");

            if (subtype != "LargeBlockFarmPlot")
                return;

            //MyLog.Default.WriteLineAndConsole($"[FarmBot] Neuer FarmPlot erkannt.");

            IMyCubeBlock farmPlotBlock = block.FatBlock;

            if (farmPlotBlock == null)
            {
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmPlot besitzt keinen FatBlock.");
                return;
            }

                /*MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmPlot EntityID: {farmPlotBlock.EntityId}");
                MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmPlot GridID: {block.CubeGrid.EntityId}");
                MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmPlot Grid: {block.CubeGrid.CustomName}");*/

                FarmPlot farmPlot = new FarmPlot();

                farmPlot.EntityID = farmPlotBlock.EntityId;
                farmPlot.GridID = block.CubeGrid.EntityId;
                farmPlot.GridName = block.CubeGrid.CustomName;

                AddFarmPlot(farmPlot);
        }

        private void OnBlockRemoved(IMySlimBlock block)
        {
            //MyLog.Default.WriteLineAndConsole($"[FarmBot] OnBlockRemoved wurde aufgerufen.");

            string subtype = block.BlockDefinition.Id.SubtypeName;

            //MyLog.Default.WriteLineAndConsole($"[FarmBot] Block entfernt. Subtype: {subtype}");

            if (subtype != "LargeBlockFarmPlot")
                return;

            IMyCubeBlock farmPlotBlock = block.FatBlock;

            if (farmPlotBlock == null)
            {
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Entferntes FarmPlot besitzt keinen FatBlock.");
                return;
            }

            //MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmPlot entfernt. EntityID: {farmPlotBlock.EntityId}");
            RemoveFarmPlot(farmPlotBlock.EntityId);
        }

        public void AddFarmPlot(FarmPlot farmPlot)
        {
            //MyLog.Default.WriteLineAndConsole($"[FarmBot] AddFarmPlot wurde aufgerufen.");

            if (farmPlots.ContainsKey(farmPlot.EntityID))
            {
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmPlot bereits vorhanden. EntityID: {farmPlot.EntityID}");
                return;
            }

            farmPlots.Add(farmPlot.EntityID, farmPlot);
            //MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmPlot zum Dictionary hinzugefügt. EntityID: {farmPlot.EntityID}, Verwaltete Plots: {farmPlots.Count}");
        }

        public void RemoveFarmPlot(long EntityID)
        {
            //MyLog.Default.WriteLineAndConsole($"[FarmBot] RemoveFarmPlot wurde aufgerufen.");

            if (!farmPlots.ContainsKey(EntityID))
            {
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmPlot nicht vorhanden. EntityID: {EntityID}");
                return;
            }

            farmPlots.Remove(EntityID);
            //MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmPlot aus dem Dictionary entfernt. EntityID: {EntityID}, Verwaltete Plots: {farmPlots.Count}");
        }

        public FarmPlot GetFarmPlot(long EntityId)
        {
            FarmPlot farmPlot;

            if (farmPlots.TryGetValue(EntityId, out farmPlot))
            {
                return farmPlot;
            }

            return null;
        }

        private void OnEntityAdd(IMyEntity entity)
        {
            //MyLog.Default.WriteLineAndConsole($"[FarmBot] OnEntityAdd wurde aufgerufen.");

            IMyCubeGrid grid = entity as IMyCubeGrid;

            if (grid == null)
                return;

            //MyLog.Default.WriteLineAndConsole($"[FarmBot] Neues Grid erkannt. ID: {grid.EntityId}, Name: {grid.CustomName}");
            TrackGrid(grid);
        }

        private void TrackGrid(IMyCubeGrid grid)
        {
            //MyLog.Default.WriteLineAndConsole($"[FarmBot] TrackGrid für neues Grid aufgerufen. Grid ID: {grid.EntityId}");

            if (trackedGrids.ContainsKey(grid.EntityId))
            {
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Grid bereits im Tracking. Grid ID: {grid.EntityId}");
                return;
            }

                trackedGrids.Add(grid.EntityId, grid);

                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Grid zum Tracking hinzugefügt. Grid ID: {grid.EntityId}, Name: {grid.CustomName}");

                grid.OnBlockAdded += OnBlockAdded;
                grid.OnBlockRemoved += OnBlockRemoved;

                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Grid-Events abonniert. Grid ID: {grid.EntityId}");

        }

        public List<FarmPlot> GetFarmPlots()
        {
            return new List<FarmPlot>(farmPlots.Values);
        }
    }
}
