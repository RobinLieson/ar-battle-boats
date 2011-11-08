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

//AR Battle Boats
using AR_Battle_Boats;

//Code hijacked from http://www.switchonthecode.com/tutorials/csharp-tutorial-simple-threaded-tcp-server
//http://msdn.microsoft.com/en-us/library/system.net.sockets.tcplistener.beginaccepttcpclient.aspx

namespace BattleBoatsServer
{
    class Server
    {
        private TcpListener tcpListener;
        private Thread listenerThread;
        SqlCeConnection conn;
        public bool active;

        public static ManualResetEvent tcpClientConnected =
            new ManualResetEvent(false);


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
            tcpClientConnected.Reset();
            this.tcpListener.Start();
            while (active)
            {               

                //blocks until a client has connected to the server
                tcpListener.BeginAcceptTcpClient(new AsyncCallback(HandleConnection), tcpListener);

                tcpClientConnected.WaitOne();
            }

            tcpListener.Stop();
        }

        /// <summary>
        /// Callback for tcplistener
        /// </summary>
        /// <param name="ar"></param>
        private void HandleConnection(IAsyncResult ar)
        {
            // Get the listener that handles the client request.
            TcpListener listener = (TcpListener)ar.AsyncState;

            // End the operation and display the received data on 
            // the console.
            if (active == false)
                return;

            TcpClient client = listener.EndAcceptTcpClient(ar);

            Console.WriteLine("Client Connected");

            //create a thread to handle communication 
            //with connected client
            Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
            clientThread.Start(client);

            tcpClientConnected.Set();
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
                PlayerInfo info = new PlayerInfo(sentString);
                UpdatePlayerInfo(info);
                gamerID = info.PlayerName;
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

            info.PlayerName = dataset.PlayerInfo[0].UserName;
            info.Ammo_Level = dataset.PlayerInfo[0].AmmoUpgrades;
            info.Armour_Level = dataset.PlayerInfo[0].ArmourUpgrades;
            info.Money = dataset.PlayerInfo[0].Money;
            info.Speed_Level = dataset.PlayerInfo[0].SpeedUpgrades;
            info.Ship_Model_Name = dataset.PlayerInfo[0].ShipModel;

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
            String select = "SELECT * FROM PlayerInfo WHERE UserName = \'" + newInfo.PlayerName + "\'";

            SqlCeDataAdapter adapter = new SqlCeDataAdapter(select, conn);
            adapter.Fill(dataset, "PlayerInfo");

            if (dataset.PlayerInfo.Rows.Count == 1)//New user, create table entries for them
            {
                dataset.PlayerInfo.Rows[0]["UserName"] = newInfo.PlayerName;
                dataset.PlayerInfo.Rows[0]["AmmoUpgrades"] = newInfo.Ammo_Level;
                dataset.PlayerInfo.Rows[0]["ArmourUpgrades"] = newInfo.Armour_Level;
                dataset.PlayerInfo.Rows[0]["Money"] = newInfo.Money;
                dataset.PlayerInfo.Rows[0]["SpeedUpgrades"] = newInfo.Speed_Level;
                dataset.PlayerInfo.Rows[0]["ShipModel"] = newInfo.Ship_Model_Name;

                SqlCeCommandBuilder builder = new SqlCeCommandBuilder(adapter);
                builder.QuotePrefix = "[";
                builder.QuoteSuffix = "]";

                adapter.Update(dataset, "PlayerInfo");
            }
        }
    }
}
