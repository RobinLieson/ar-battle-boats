using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

//SQL
using System.Data.SqlClient;
using System.Xml;
using System.Configuration;
using System.Data.SqlServerCe;

//Code hijacked from http://www.switchonthecode.com/tutorials/csharp-tutorial-simple-threaded-tcp-server


namespace BattleBoatsServer
{
    class Server
    {
        private TcpListener tcpListener;
        private Thread listenerThread;
        SqlCeConnection conn;
        public bool active;

        public Server()
        {
            conn = new SqlCeConnection(Properties.Settings.Default.BattleBoatsDatabaseConnectionString);
            active = true;

            this.tcpListener = new TcpListener(IPAddress.Any, 3550);
            this.listenerThread = new Thread(new ThreadStart(ListenForClients));
            this.listenerThread.Start(); 
        }

        /// <summary>
        /// This thread listens for incoming connections
        /// </summary>
        private void ListenForClients(){
            this.tcpListener.Start();

            while (active)
            {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                Console.WriteLine("Client Connected");

                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            } 
        }

        /// <summary>
        /// Handles communication with the clients
        /// </summary>
        /// <param name="client"></param>
        private void HandleClientComm(object client)
        {
            String gamerID = "";
            String sentString = "";
            ASCIIEncoding encoder = new ASCIIEncoding();
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            bytesRead = 0;

            try
            {
                //blocks until a client sends a message
                bytesRead = clientStream.Read(message, 0, 4096);
            }
            catch
            {
            }
            //message has successfully been received

            sentString = encoder.GetString(message, 0, bytesRead);
            string[] tokens = sentString.Split(":".ToCharArray());

            if (tokens.Length > 1)//We're getting an update string, so update
            {
                Console.WriteLine("Player Update data for " + tokens[1] + " recieved.");
                PlayerInfo info = new PlayerInfo();
                info.CreateFromString(sentString);
                UpdatePlayerInfo(info);
                gamerID = info.userName;
            }
            else
            {
                gamerID = sentString;
            }

            //If we have an ID, write the data back
            if (gamerID != "")
            {
                Console.WriteLine("GamerTag = " + gamerID);
                PlayerInfo info = GetPlayerInfo(gamerID);
                byte[] infoBytes = encoder.GetBytes(info.ToString());
                clientStream.Write(infoBytes,0,encoder.GetByteCount(info.ToString()));
                clientStream.Flush();
            }

            tcpClient.Close();
        }

        /// <summary>
        /// Get the info for the player out of the database and return in
        /// </summary>
        /// <param name="tag">The GamerTag of the player</param>
        /// <returns>PlayerInfo for the gamer</returns>
        private PlayerInfo GetPlayerInfo(String tag)
        {
            BattleBoatsDataSet dataset = new BattleBoatsDataSet();
            PlayerInfo info = new PlayerInfo();
            String select = "SELECT * FROM PlayerInfo WHERE UserName = \'" + tag + "\'";

            SqlCeDataAdapter adapter = new SqlCeDataAdapter(select, conn);
            adapter.Fill(dataset,"PlayerInfo");

            
            if(dataset.PlayerInfo.Rows.Count == 0)//New user, create table entries for them
            {
                BattleBoatsDataSet.PlayerInfoRow playerrow = dataset.PlayerInfo.NewPlayerInfoRow();
                playerrow.UserName = tag;
                playerrow.AmmoUpgrades = 0;
                playerrow.ArmourUpgrades = 0;
                playerrow.Money = 0;
                playerrow.SpeedUpgrades = 0;
                playerrow.ShipModel = "Basic";
                dataset.PlayerInfo.AddPlayerInfoRow(playerrow);

                SqlCeCommandBuilder builder = new SqlCeCommandBuilder(adapter);
                builder.QuotePrefix = "[";
                builder.QuoteSuffix = "]";

                adapter.Update(dataset, "PlayerInfo");
            }

            info.userName = dataset.PlayerInfo[0].UserName;
            info.ammo = dataset.PlayerInfo[0].AmmoUpgrades;
            info.armour = dataset.PlayerInfo[0].ArmourUpgrades;
            info.money = dataset.PlayerInfo[0].Money;
            info.speed = dataset.PlayerInfo[0].SpeedUpgrades;
            info.ship = dataset.PlayerInfo[0].ShipModel;

            return info;
        }

        /// <summary>
        /// Update the databse for a player
        /// </summary>
        /// <param name="newInfo"></param>
        private void UpdatePlayerInfo(PlayerInfo newInfo)
        {
            BattleBoatsDataSet dataset = new BattleBoatsDataSet();
            PlayerInfo info = new PlayerInfo();
            String select = "SELECT * FROM PlayerInfo WHERE UserName = \'" + newInfo.userName + "\'";

            SqlCeDataAdapter adapter = new SqlCeDataAdapter(select, conn);
            adapter.Fill(dataset, "PlayerInfo");

            if (dataset.PlayerInfo.Rows.Count == 1)//New user, create table entries for them
            {
                dataset.PlayerInfo.Rows[0]["UserName"] = newInfo.userName;
                dataset.PlayerInfo.Rows[0]["AmmoUpgrades"] = newInfo.ammo;
                dataset.PlayerInfo.Rows[0]["ArmourUpgrades"] = newInfo.armour;
                dataset.PlayerInfo.Rows[0]["Money"] = newInfo.money;
                dataset.PlayerInfo.Rows[0]["SpeedUpgrades"] = newInfo.speed;
                dataset.PlayerInfo.Rows[0]["ShipModel"] = newInfo.ship;

                SqlCeCommandBuilder builder = new SqlCeCommandBuilder(adapter);
                builder.QuotePrefix = "[";
                builder.QuoteSuffix = "]";

                adapter.Update(dataset, "PlayerInfo");
            }
        }
    }
}
