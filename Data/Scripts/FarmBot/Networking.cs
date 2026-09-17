using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Utils;

namespace FarmBot
{
    public class Networking
    {
        private const ushort NetworkId = 49996;

        private const byte MessageRequestFarmPlots = 1;
        private const byte MessageFarmPlotsResponse = 2;
        private const byte MessageSetCrop = 3;

        private FarmPlotManager farmPlotManager;
        private Action<List<FarmPlot>, Dictionary<long, int>> farmPlotReceiver;

        private readonly Dictionary<long, int> cropByPlot =
            new Dictionary<long, int>();

        public Networking(FarmPlotManager farmPlotManager)
        {
            this.farmPlotManager = farmPlotManager;

            MyLog.Default.WriteLineAndConsole($"[FarmBot] Networking initialized.");

            if (MyAPIGateway.Multiplayer != null)
            {
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(
                    NetworkId,
                    NetworkHandler
                );
            }
        }

        public void RequestFarmPlots()
        {
            if (MyAPIGateway.Multiplayer == null)
                return;

            byte[] data = new byte[]
            {
                MessageRequestFarmPlots
            };

            MyLog.Default.WriteLineAndConsole($"[FarmBot] Requesting FarmPlots from server.");

            MyAPIGateway.Multiplayer.SendMessageToServer(
                NetworkId,
                data
            );
        }

        public void SetCropSelection(long farmPlotId, int crop)
        {
            if (crop < 0 || crop > 4)
                return;

            if (MyAPIGateway.Multiplayer == null || MyAPIGateway.Multiplayer.IsServer)
            {
                SetServerCropSelection(farmPlotId, crop);
                return;
            }

            byte[] data = new byte[13];

            data[0] = MessageSetCrop;

            WriteLong(
                data,
                1,
                farmPlotId
            );

            WriteInt(
                data,
                9,
                crop
            );

            MyAPIGateway.Multiplayer.SendMessageToServer(
                NetworkId,
                data
            );

            //MyLog.Default.WriteLineAndConsole($"[FarmBot] Sent crop selection. Plot: {farmPlotId}, Crop: {crop}");
        }

        private void NetworkHandler(
            ushort channel,
            byte[] data,
            ulong sender,
            bool fromServer)
        {
            if (data == null || data.Length == 0)
                return;

            byte messageType = data[0];

            if (MyAPIGateway.Multiplayer != null &&
                MyAPIGateway.Multiplayer.IsServer)
            {
                HandleServerMessage(
                    messageType,
                    data,
                    sender
                );

                return;
            }

            HandleClientMessage(
                messageType,
                data
            );
        }

        private void HandleServerMessage(
            byte messageType,
            byte[] data,
            ulong sender)
        {
            if (messageType != MessageRequestFarmPlots &&
                messageType != MessageSetCrop)
                return;

            if (MyAPIGateway.Session == null)
                return;

            if (!MyAPIGateway.Session.IsUserAdmin(sender))
            {
                MyLog.Default.WriteLineAndConsole($"[FarmBot] Rejected network request from non-admin. Sender: {sender}");

                return;
            }

            if (messageType == MessageRequestFarmPlots)
            {
                HandleRequestFarmPlots(sender);
                return;
            }

            if (messageType == MessageSetCrop)
            {
                HandleSetCrop(
                    data,
                    sender
                );

                return;
            }
        }

        private void HandleRequestFarmPlots(ulong sender)
        {
            List<FarmPlot> farmPlots =
                farmPlotManager.GetFarmPlots();

            MyLog.Default.WriteLineAndConsole($"[FarmBot] RequestFarmPlots received. Sender: {sender}");

            MyLog.Default.WriteLineAndConsole($"[FarmBot] Sending FarmPlot state to client. Count: {farmPlots.Count}");

            byte[] response =
                SerializeFarmPlotState(farmPlots);

            MyAPIGateway.Multiplayer.SendMessageTo(
                NetworkId,
                response,
                sender
            );
        }

        private void HandleSetCrop(
            byte[] data,
            ulong sender)
        {
            if (data.Length < 13)
                return;

            long farmPlotId =
                ReadLong(
                    data,
                    1
                );

            int crop =
                ReadInt(
                    data,
                    9
                );

            if (crop < 0 || crop > 4)
            {
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Rejected invalid crop value. Plot: {farmPlotId}, Crop: {crop}");

                return;
            }

            FarmPlot farmPlot =
                FindFarmPlot(
                    farmPlotId
                );

            if (farmPlot == null)
            {
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Rejected crop selection for unknown FarmPlot. Plot: {farmPlotId}");

                return;
            }

            SetServerCropSelection(
                farmPlotId,
                crop
            );

            SendStateToPlayer(sender);
        }

        private FarmPlot FindFarmPlot(long entityId)
        {
            List<FarmPlot> farmPlots =
                farmPlotManager.GetFarmPlots();

            for (int i = 0; i < farmPlots.Count; i++)
            {
                if (farmPlots[i].EntityID == entityId)
                    return farmPlots[i];
            }

            return null;
        }

        private void SetServerCropSelection(
            long farmPlotId,
            int crop)
        {
            if (crop < 0 || crop > 4)
                return;

            if (FindFarmPlot(farmPlotId) == null)
            {
                //MyLog.Default.WriteLineAndConsole($"[FarmBot] Cannot configure unknown FarmPlot. Plot: {farmPlotId}");

                return;
            }

            cropByPlot[farmPlotId] =
                crop;

            //MyLog.Default.WriteLineAndConsole($"[FarmBot] Server crop selection updated. Plot: {farmPlotId}, Crop: {crop}");
        }

        private void SendStateToPlayer(ulong playerId)
        {
            if (MyAPIGateway.Multiplayer == null)
                return;

            List<FarmPlot> farmPlots =
                farmPlotManager.GetFarmPlots();

            byte[] response =
                SerializeFarmPlotState(farmPlots);

            MyAPIGateway.Multiplayer.SendMessageTo(
                NetworkId,
                response,
                playerId
            );
        }

        private byte[] SerializeFarmPlotState(
            List<FarmPlot> farmPlots)
        {
            List<byte> data =
                new List<byte>();

            data.Add(
                MessageFarmPlotsResponse
            );

            AddInt(
                data,
                farmPlots.Count
            );

            foreach (FarmPlot farmPlot in farmPlots)
            {
                AddLong(data, farmPlot.EntityID);

                AddLong(data, farmPlot.GridID);

                AddString(data, farmPlot.GridName ?? "");

                AddString(data, farmPlot.CustomName);

                int crop = 0;

                cropByPlot.TryGetValue(
                    farmPlot.EntityID,
                    out crop
                );

                AddInt(
                    data,
                    crop
                );
            }

            return data.ToArray();
        }

        private void HandleClientMessage(
            byte messageType,
            byte[] data)
        {
            if (messageType != MessageFarmPlotsResponse)
                return;

            if (data.Length < 5)
                return;

            int position = 1;

            int plotCount =
                ReadInt(
                    data,
                    position
                );

            position += 4;

            if (plotCount < 0)
                return;

            List<FarmPlot> farmPlots =
                new List<FarmPlot>();

            Dictionary<long, int> cropConfiguration =
                new Dictionary<long, int>();

            for (int i = 0; i < plotCount; i++)
            {
                if (position + 16 > data.Length)
                    return;

                FarmPlot farmPlot =
                    new FarmPlot();

                farmPlot.EntityID =
                    ReadLong(
                        data,
                        position
                    );

                position += 8;

                farmPlot.GridID =
                    ReadLong(
                        data,
                        position
                    );

                position += 8;

                string gridName;

                if (!ReadString(
                    data,
                    ref position,
                    out gridName))
                    return;

                farmPlot.GridName =
                    gridName;

                if (position + 4 > data.Length)
                    return;

                int crop =
                    ReadInt(
                        data,
                        position
                    );

                position += 4;

                farmPlots.Add(
                    farmPlot
                );

                cropConfiguration[
                    farmPlot.EntityID
                ] = crop;
            }

            MyLog.Default.WriteLineAndConsole($"[FarmBot] FarmPlot state received from server. Count: {farmPlots.Count}");

            if (farmPlotReceiver != null)
            {
                farmPlotReceiver(
                    farmPlots,
                    cropConfiguration
                );
            }
        }

        public Dictionary<long, int> GetCropConfiguration()
        {
            return new Dictionary<long, int>(
                cropByPlot
            );
        }

        public void SetFarmPlotReceiver(
            Action<List<FarmPlot>, Dictionary<long, int>> receiver)
        {
            farmPlotReceiver = receiver;
        }

        public void Unregister()
        {
            if (MyAPIGateway.Multiplayer != null)
            {
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(
                    NetworkId,
                    NetworkHandler
                );
            }

            MyLog.Default.WriteLineAndConsole($"[FarmBot] Networking unregistered.");
        }

        private static void WriteLong(
            byte[] data,
            int position,
            long value)
        {
            byte[] bytes =
                BitConverter.GetBytes(value);

            for (int i = 0; i < 8; i++)
            {
                data[position + i] =
                    bytes[i];
            }
        }

        private static long ReadLong(
            byte[] data,
            int position)
        {
            return BitConverter.ToInt64(
                data,
                position
            );
        }

        private static void WriteInt(
            byte[] data,
            int position,
            int value)
        {
            byte[] bytes =
                BitConverter.GetBytes(value);

            for (int i = 0; i < 4; i++)
            {
                data[position + i] =
                    bytes[i];
            }
        }

        private static int ReadInt(
            byte[] data,
            int position)
        {
            return BitConverter.ToInt32(
                data,
                position
            );
        }

        private static void AddLong(
            List<byte> data,
            long value)
        {
            byte[] bytes =
                BitConverter.GetBytes(value);

            for (int i = 0; i < bytes.Length; i++)
            {
                data.Add(
                    bytes[i]
                );
            }
        }

        private static void AddInt(
            List<byte> data,
            int value)
        {
            byte[] bytes =
                BitConverter.GetBytes(value);

            for (int i = 0; i < bytes.Length; i++)
            {
                data.Add(
                    bytes[i]
                );
            }
        }

        private static void AddString(
            List<byte> data,
            string value)
        {
            byte[] bytes =
                Encoding.UTF8.GetBytes(value);

            AddInt(
                data,
                bytes.Length
            );

            for (int i = 0; i < bytes.Length; i++)
            {
                data.Add(
                    bytes[i]
                );
            }
        }

        private static bool ReadString(
            byte[] data,
            ref int position,
            out string value)
        {
            value = "";

            if (position + 4 > data.Length)
                return false;

            int length =
                ReadInt(
                    data,
                    position
                );

            position += 4;

            if (length < 0 ||
                position + length > data.Length)
                return false;

            value =
                Encoding.UTF8.GetString(
                    data,
                    position,
                    length
                );

            position += length;

            return true;
        }
    }
}