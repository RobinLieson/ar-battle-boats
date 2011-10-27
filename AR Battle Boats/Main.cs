using System;
using System.Collections.Generic;
using System.Linq;

//XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

//Goblin XNA
using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Generic;
using GoblinXNA.UI.UI2D;

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
        PlayerInfo playerInfo1; //Information for Player 1
        PlayerInfo playerInfo2; //Information for Player 2 (Only if we're not playing online)
        Scene scene;
        GameMode gameMode;
        GameState gameState;
        List<Ship> AvailableShips;
        NetworkSession session;

        PacketWriter packetWriter; //For writing to the network

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
            Components.Add(new GamerServicesComponent(this));

            State.InitGoblin(graphics, Content, ""); //Start Goblin XNA
            scene = new Scene(this); //Create a new Scene

            this.IsMouseVisible = true; //Set Mouse Visible         

            gameState = GameState.Main_Menu;
            gameMode = GameMode.Menu;

            CreateShips();

            DisplayMainMenu();

            
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

            if (gameState == GameState.Main_Menu)
            {
                //Check to see if the Gamer is Signed In
                if (SignedInGamer.SignedInGamers.Count < 1)
                {
                    if (!Guide.IsVisible)
                    {
                        Guide.ShowSignIn(1, true);
                    }
                }
                else
                {
                    GetPlayerInfo();
                }
            }

            if (gameMode == GameMode.Menu)
            {
                gameMode = GameMode.Local_Multiplayer;
            }

            if (gameMode == GameMode.Local_Multiplayer)
            {

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
        /// Display the main menu
        /// </summary>
        private void DisplayMainMenu()
        {
            Console.WriteLine("Displaing Main Menu");
        }

        /// <summary>
        /// Hide the main menu
        /// </summary>
        private void HideMainMenu()
        {
            Console.WriteLine("Hiding Main Menu");
        }

        /// <summary>
        /// Get the player info from the Server
        /// </summary>
        private void GetPlayerInfo()
        {
            if (playerInfo1 == null)
            {
                playerInfo1 = new PlayerInfo();
                bool result = playerInfo1.GetPlayerInfoFromServer(SignedInGamer.SignedInGamers[0].Gamertag, "127.0.0.1", 3550);
                if (!result)
                {
                    playerInfo1 = new PlayerInfo();
                    playerInfo1.PlayerName = SignedInGamer.SignedInGamers[0].Gamertag;
                    playerInfo1.Ammo_Level = 0;
                    playerInfo1.Armour_Level = 0;
                    playerInfo1.Money = 0;
                    playerInfo1.Speed_Level = 0;
                    playerInfo1.Player_Ship = AvailableShips[0];
                    Console.WriteLine("Creating new profile");
                    Console.Write(playerInfo1.ToString());
                }
            }
        }

        /// <summary>
        /// Create the different types of ships available in the game
        /// </summary>
        private void CreateShips()
        {
            AvailableShips = new List<Ship>();

            Ship sailBoat = new Ship();
            sailBoat.Ammo = 0;
            sailBoat.Armour = 0;
            sailBoat.Boat_Name = "Sailboat";
            sailBoat.Health = 100;
            sailBoat.Speed = 0;
            sailBoat.Position = Vector3.Zero;

            AvailableShips.Add(sailBoat);

        }

        /// <summary>
        /// Setup everything to start a game
        /// </summary>
        private void StartNetworkSession()
        {
            if (gameMode == GameMode.Network_Multiplayer)
            {
                //Network stuff
                packetWriter = new PacketWriter();
            }

            if (gameState == GameState.Hosting)
            {
                Console.WriteLine("Creating a new match");
                session = NetworkSession.Create(NetworkSessionType.Local, 2, 2);
            }
            else if (gameState == GameState.Joining)
            {
                Console.WriteLine("Looking for a game to join...");
                AvailableNetworkSessionCollection availableSessions;
                availableSessions = NetworkSession.Find(NetworkSessionType.PlayerMatch, 2,null);
                Console.WriteLine("Found " + availableSessions.Count + " available sessions");
                if (availableSessions.Count > 0)
                {
                    session = NetworkSession.Join(availableSessions[0]);
                }
                Console.WriteLine("Session Joined!");
            }

            session.GameStarted += new EventHandler<GameStartedEventArgs>(session_GameStarted);
            session.GameEnded += new EventHandler<GameEndedEventArgs>(session_GameEnded);

        }

        /// <summary>
        /// Called when a Game has ended
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void session_GameEnded(object sender, GameEndedEventArgs e)
        {
            Console.WriteLine("Game has ended...");
        }

        /// <summary>
        /// Called when a game has started
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void session_GameStarted(object sender, GameStartedEventArgs e)
        {
            Console.WriteLine("Game has started...");
        }

        /// <summary>
        /// Read and write all the network data stuff
        /// </summary>
        private void UpdateNetwork()
        {
            foreach (LocalNetworkGamer gamer in session.LocalGamers)
            {
                // Get the tank associated with this player.
                Ship myShip = gamer.Tag as Ship;
                // Write the data.
                packetWriter.Write(myShip.Position);
                packetWriter.Write(myShip.Health);
                packetWriter.Write(myShip.Firing);

                // Send it to everyone.
                gamer.SendData(packetWriter, SendDataOptions.None);

            } 
        }
    }
}
