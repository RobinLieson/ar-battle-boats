using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

//Networking
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace AR_Battle_Boats
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        PlayerInfo info;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Components.Add(new GamerServicesComponent(this));

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (SignedInGamer.SignedInGamers.Count < 1)
            {
                if (!Guide.IsVisible)
                {
                    Guide.ShowSignIn(1, true);
                }
            }
            else
            {
                if (info == null)
                {
                    info = GetPlayerInfo(SignedInGamer.SignedInGamers[0].Gamertag);
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        /// <summary>
        /// Gets the player info from the server
        /// </summary>
        /// <param name="gamerTag">The XBL GamerTag of a signed-in gamer</param>
        /// <returns>PlayerInfo for a player</returns>
        private PlayerInfo GetPlayerInfo(string gamerTag)
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
                return null;
            }
            Console.WriteLine("Connected!");
            
            Console.Write("Sending data to server...");
            //Write the GamerTag to the server

            String tag = "thenewzerov";
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
                return null;
            }

            info = new PlayerInfo();
            info.CreateFromString(message);

            Console.WriteLine(info.ToString());
            return info;
        }
    }
}
