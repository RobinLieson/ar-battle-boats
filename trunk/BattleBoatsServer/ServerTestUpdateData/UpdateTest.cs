using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using BattleBoatsServer;
using AR_Battle_Boats;

namespace ServerTester
{
    class Program
    {
        static void Main(string[] args)
        {
            PlayerInfo info;

            NetworkStream stream; //Stream to write and read data to

            ASCIIEncoding encoder = new ASCIIEncoding();

            TcpClient client = new TcpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3550);

            try
            {
                Console.Write("Connecting to server...");
                client.Connect(serverEndPoint);
            }
            catch
            {
                Console.WriteLine("ERROR:  Could not connect to server!");
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Connected!");

            Console.Write("Sending data to server...");
            //Write the GamerTag to the server

            PlayerInfo newInfo = new PlayerInfo();
            newInfo.PlayerName = "thenewzerov";
            newInfo.Ammo_Level = 1;
            newInfo.Armour_Level = 3;
            newInfo.Money = 100;
            newInfo.Speed_Level = 0;
            newInfo.Ship_Model_Name = "Galleon";

            stream = client.GetStream();
            stream.Write(encoder.GetBytes(newInfo.ToString()), 0, encoder.GetByteCount(newInfo.ToString()));
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
                Console.ReadLine();
                return;
            }

            info = new PlayerInfo(message);

            Console.WriteLine(info.ToString());
            Console.ReadLine();
        }
    }
}
