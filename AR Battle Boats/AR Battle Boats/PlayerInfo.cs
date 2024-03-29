﻿using System.Text;
using System.Net.Sockets;
using System.Net;
using System;
using System.Collections.Generic;

namespace AR_Battle_Boats
{
    /// <summary>
    /// The info for a server, 
    /// </summary>
    public class PlayerInfo
    {
        private string userName;
        private int money;
        private int armour;
        private int ammo;
        private int speed;
        private Ship ship;
        public Player_Location PlayerLocation;

        /// <summary>
        /// Create a new PlayerInfo class
        /// </summary>
        public PlayerInfo()
        {

        }

        /// <summary>
        /// Will create a new PlayerInfo object from the string given
        /// </summary>
        /// <param name="Creation_String">String used to populat the PlayerInfo object</param>
        public PlayerInfo(string Creation_String)
        {
            CreateFromString(Creation_String);
        }

        /// <summary>
        /// Get or Set the Player Name associated with this PlayerInfo
        /// </summary>
        public string PlayerName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
            }

        }

        /// <summary>
        /// Get or Set the Money for this PlayerInfo
        /// </summary>
        public int Money
        {
            get
            {
                return money;
            }
            set
            {
                money = value;
            }
        }

        /// <summary>
        /// Get or Set the Armour Research Level for this PlayerInfo
        /// </summary>
        public int Armour_Level
        {
            get
            {
                return armour;
            }
            set
            {
                armour = value;
            }
        }

        /// <summary>
        /// Get or Set the Ammo Research Level for this PlayerInfo
        /// </summary>
        public int Ammo_Level
        {
            get
            {
                return ammo;
            }
            set
            {
                ammo = value;
            }
        }

        /// <summary>
        /// Get or Set the Speed Research Level for this PlayerInfo
        /// </summary>
        public int Speed_Level
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }


        /// <summary>
        /// Get or Set the Ship Model Info for this PlayerInfo
        /// </summary>
        public Ship Player_Ship
        {
            get
            {
                return ship;
            }
            set
            {
                ship = value;
                ship.Ammo = ammo;
                ship.Speed = speed;
                ship.Health = 100;
                ship.Armour = armour;
            }
        }

        /// <summary>
        /// Fills in all the data from a string sent to it
        /// </summary>
        /// <param name="data"></param>
        private void CreateFromString(string data)
        {
            string[] info = data.Split("\t".ToCharArray());

            foreach (string label in info)
            {
                string[] items = label.Split(":".ToCharArray());

                if (items[0] == "userName")
                    userName = items[1];
                if (items[0] == "money")
                    money = int.Parse(items[1]);
                if (items[0] == "armour")
                    armour = int.Parse(items[1]);
                if (items[0] == "ammo")
                    ammo = int.Parse(items[1]);
                if (items[0] == "speed")
                    speed = int.Parse(items[1]);
            }
        }

        /// <summary>
        /// Returns a string representing the PlayerInfo object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("userName:" + userName + "\t");
            builder.Append("money:" + money + "\t");
            builder.Append("armour:" + armour + "\t");
            builder.Append("ammo:" + ammo + "\t");
            builder.Append("speed:" + speed + "\t");
            builder.Append("ship:Basic");

            return builder.ToString();
        }

        /// <summary>
        /// Retrieves the PlayerInfo from the given server
        /// </summary>
        /// <param name="PlayerName">PlayerName of the info to retriver</param>
        /// <param name="Server_Address">Address of the remoter server</param>
        /// <param name="Port_Num">Port on the remote server</param>
        /// <returns>True if the retrieval was successful, False otherwise</returns>
        public bool GetPlayerInfoFromServer(string PlayerName, string Server_Address, int Port_Num)
        {
            NetworkStream stream; //Stream to write and read data to

            ASCIIEncoding encoder = new ASCIIEncoding();

            TcpClient client = new TcpClient();
            IPEndPoint serverEndPoint;


            try
            {
                serverEndPoint = new IPEndPoint(IPAddress.Parse(Server_Address), Port_Num);
                Console.Write("Connecting to server...");
                client.Connect(serverEndPoint);
            }
            catch
            {
                try
                {
                    Console.Write("Connecting to server...");
                    client.Connect(Server_Address, Port_Num);
                }
                catch
                {
                    Console.WriteLine("ERROR:  Could not connect to server!");
                    return false;
                }
            }
            try
            {

                Console.WriteLine("Connected!");

                Console.Write("Sending data to server...");
                //Write the GamerTag to the server

                String tag = PlayerName;
                stream = client.GetStream();
                stream.Write(encoder.GetBytes(tag), 0, encoder.GetByteCount(tag));
                stream.Flush();

                Console.WriteLine("Data sent!");

                Console.Write("Reading return data...");
                byte[] msg = new byte[4096];
                int read = stream.Read(msg, 0, 4096);
                string message = "";

                if (read > 0)
                {
                    Console.WriteLine("Data returned!");
                    message = encoder.GetString(msg, 0, read);
                }
                else
                {
                    Console.WriteLine("ERROR: No Data returned!");
                    return false;
                }

                CreateFromString(message);

                Console.WriteLine(ToString());
                return true;
            }
            catch
            {
                Console.WriteLine("ERROR:  Could not connect to server!");
                return false;
            }
        }

        /// <summary>
        /// Connects to the Remote Server and Updates the Player Info on it
        /// </summary>
        /// <param name="Server_address">Address of the Remote Server</param>
        /// <param name="Port_Num">Port Number on the Remote Server</param>
        /// <returns>True if update was successful, false otherwise</returns>
        public bool UpdateInfoOnServer(string Server_Address, int Port_Num)
        {
            NetworkStream stream; //Stream to write and read data to

            ASCIIEncoding encoder = new ASCIIEncoding();

            TcpClient client = new TcpClient();
            IPEndPoint serverEndPoint;


            try
            {
                serverEndPoint = new IPEndPoint(IPAddress.Parse(Server_Address), Port_Num);
                Console.Write("Connecting to server...");
                client.Connect(serverEndPoint);
            }
            catch
            {
                try
                {
                    Console.Write("Connecting to server...");
                    client.Connect(Server_Address, Port_Num);
                }
                catch
                {
                    Console.WriteLine("ERROR:  Could not connect to server!");
                    return false;
                }
            }
            try
            {
                Console.WriteLine("Connected!");

                Console.Write("Sending data to server...");
                //Write the GamerTag to the server

                stream = client.GetStream();
                stream.Write(encoder.GetBytes(ToString()), 0, encoder.GetByteCount(ToString()));
                stream.Flush();

                Console.WriteLine("Data sent!");

                Console.Write("Reading return data...");
                byte[] msg = new byte[4096];
                int read = stream.Read(msg, 0, 4096);
                string message = "";

                if (read > 0)
                {
                    Console.WriteLine("Data returned!");
                    message = encoder.GetString(msg, 0, read);
                }
                else
                {
                    Console.WriteLine("ERROR: No Data returned!");
                    return false;
                }

            }
            catch
            {
                Console.WriteLine("ERROR:  Could not connect to server!");
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Class used to compare two player info together
    /// </summary>
    public class PlayerInfoEqualityComprarer : IEqualityComparer<PlayerInfo>
    {
        /// <summary>
        /// Returns true if two PlayerInfo objects are the same
        /// </summary>
        /// <param name="p1">PlayerInfo 1</param>
        /// <param name="p2">PlayerInfo 2</param>
        /// <returns></returns>
        public bool Equals(PlayerInfo p1, PlayerInfo p2)
        {
            if (p1.PlayerName == p2.PlayerName && p1.Ammo_Level == p2.Ammo_Level && p1.Armour_Level == p2.Armour_Level &&
                p1.Money == p2.Money && p1.Player_Ship == p2.Player_Ship && p1.PlayerLocation == p2.PlayerLocation &&
                p1.Speed_Level == p2.Speed_Level)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if the two playerInfo's have the same name
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool HasName(PlayerInfo p1, PlayerInfo p2)
        {
            if (p1.PlayerName == p2.PlayerName)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region IEqualityComparer<PlayerInfo> Members
        
        public int GetHashCode(PlayerInfo obj)
        {
            return obj.PlayerName.GetHashCode();
        }

        #endregion
    }
}
