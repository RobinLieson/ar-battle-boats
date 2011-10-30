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
        List<PlayerInfo> activePlayers;
        PacketWriter packetWriter; //For writing to the network
        PacketReader packetReader; //For reading from the network


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
            activePlayers = new List<PlayerInfo>();

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

            if (session == null)
            {
                KeyboardState state = Keyboard.GetState();
                if (state.IsKeyDown(Keys.H))
                {
                    gameMode = GameMode.Local_Multiplayer;
                    gameState = GameState.Hosting;
                    StartNetworkSession();
                }
                else if (state.IsKeyDown(Keys.J))
                {
                    gameMode = GameMode.Local_Multiplayer;
                    gameState = GameState.Joining;
                    StartNetworkSession();
                }
            }

            if (gameState == GameState.In_Game)
            {
                //Code for actually playing a match
                UpdateNetwork();
            }

            if(session != null)
                session.Update();

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
                bool result = playerInfo1.GetPlayerInfoFromServer(SignedInGamer.SignedInGamers[0].Gamertag, "192.168.1.112", 3550);
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

            playerInfo1.Player_Ship = AvailableShips[0];
            playerInfo2 = playerInfo1;
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
            //Julio set the model here

            AvailableShips.Add(sailBoat);

        }

        /// <summary>
        /// Setup everything to start a game
        /// </summary>
        private void StartNetworkSession()
        {

            packetReader = new PacketReader();
            packetWriter = new PacketWriter();

            if (gameMode == GameMode.Network_Multiplayer)
            {

            }

            if (gameState == GameState.Hosting)
            {
                Console.WriteLine("Creating a new match");
                session = NetworkSession.Create(NetworkSessionType.SystemLink,1,10);
                session.AllowJoinInProgress = true;
            }
            else if (gameState == GameState.Joining)
            {
                Console.WriteLine("Looking for a game to join...");
                AvailableNetworkSessionCollection availableSessions;
                
                availableSessions = NetworkSession.Find(NetworkSessionType.SystemLink, 2,null);
                Console.WriteLine("Found " + availableSessions.Count + " available sessions");
                if (availableSessions.Count > 0)
                {
                    session = NetworkSession.Join(availableSessions[0]);
                    Console.WriteLine("Session Joined!");
                }

            }

            if (session != null)
            {
                session.GameStarted += new EventHandler<GameStartedEventArgs>(session_GameStarted);
                session.GameEnded += new EventHandler<GameEndedEventArgs>(session_GameEnded);
                session.GamerJoined += new EventHandler<GamerJoinedEventArgs>(session_GamerJoined);
            }

        }

        /// <summary>
        /// Called when a gamer joins the game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void session_GamerJoined(object sender, GamerJoinedEventArgs e)
        {
            Console.WriteLine("A new Gamer, " + e.Gamer.Gamertag + " has joined");
            if (session.AllGamers.Count > 1 && session.IsHost)
            {
                session.StartGame();
                session.Update();
            }
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

            activePlayers.Add(playerInfo1);
            activePlayers.Add(playerInfo2);

            foreach (PlayerInfo player in activePlayers)
            {
                //Julio, add the models to the scene here
                //player.Player_Ship.Player_Ship_Model;
            }

            gameState = GameState.In_Game;
        }

        /// <summary>
        /// Read and write all the network data stuff
        /// </summary>
        private void UpdateNetwork()
        {
            foreach (LocalNetworkGamer gamer in session.LocalGamers)
            {
                // Get the tank associated with this player.
                Ship myShip = activePlayers[0].Player_Ship;
                // Write the data.
                packetWriter.Write(myShip.Position);
                packetWriter.Write(myShip.Health);
                packetWriter.Write(myShip.Firing);

                // Send it to everyone.
                gamer.SendData(packetWriter, SendDataOptions.None);
            }

            NetworkGamer remoteGamer;

            foreach (LocalNetworkGamer localPlayer in session.LocalGamers)
            {
                while (localPlayer.IsDataAvailable)
                {
                    localPlayer.ReceiveData(packetReader, out remoteGamer);
                    if (!remoteGamer.IsLocal)
                    {
                        Vector3 vect = packetReader.ReadVector3();
                        int health = packetReader.ReadInt32();
                        bool shooting = packetReader.ReadBoolean();
                        Console.WriteLine("Recieved message from " + remoteGamer.Gamertag);
                        Console.WriteLine("Pos = " + vect.ToString());
                        Console.WriteLine("Health = " + health.ToString());
                        Console.WriteLine("Shooting = " + shooting.ToString());
                    }
                }
            }            
        }
    }
}
