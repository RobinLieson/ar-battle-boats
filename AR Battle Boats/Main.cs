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

// Goblin XNA markers
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Util;
using GoblinXNA.Physics;
using GoblinXNA.Physics.Newton1;
using GoblinXNA.Helpers;


//Networking
using System.Text;
using System.Net;
using System.Net.Sockets;
using GoblinXNA.UI;

namespace AR_Battle_Boats
{

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {
        string SERVER_IP = "www.thenewzerov.com";
        int SERVER_PORT_NUM = 3550;


        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont textFont;
        Scene scene;
        GameMode gameMode;
        GameState gameState;
        List<Ship> AvailableShips;
        NetworkSession session;
        List<PlayerInfo> activePlayers;
        PacketWriter packetWriter; //For writing to the network
        PacketReader packetReader; //For reading from the network
        G2DPanel frame;
        List<GameObject> ActiveGameObjects;
        int turnCounter = 0;

        //Marker Node
        MarkerNode MarkerNode1;
        MarkerNode MarkerNode2;
        MarkerNode MarkerNode3;
        MarkerNode MarkerNode4;
        MarkerNode MarkerNode5;
        MarkerNode MarkerNode6;
        MarkerNode MarkerNode7;

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

            //Init Goblin, Create and setup scene
            State.InitGoblin(graphics, Content, "");
            scene = new Scene(this); 

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();

            scene.BackgroundColor = Color.DarkBlue;

            scene.PhysicsEngine = new NewtonPhysics();
            State.ThreadOption = (ushort)ThreadOptions.MarkerTracking;
            scene.PreferPerPixelLighting = true;

            activePlayers = new List<PlayerInfo>();

            this.IsMouseVisible = true; //Set Mouse Visible   

            CreateCamera();

            base.Initialize();

            gameState = GameState.Main_Menu;
            gameMode = GameMode.Menu;

            DisplayMainMenu();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            textFont = Content.Load<SpriteFont>("Fonts//uiFont");

            CreateShips();

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
            KeyboardState state = Keyboard.GetState();

            //Check to see if the Gamer is Signed In
            if (SignedInGamer.SignedInGamers.Count < 1)
            {
                if (!Guide.IsVisible)
                    Guide.ShowSignIn(1, true);
            }
            else if(activePlayers.Count < 1)
                GetPlayerInfo();

            //If joining a game, make sure you have all your markers
            if (gameState == GameState.Calibrating)
            {
                CheckReady();
            }

            //Code for playing a match
            if (gameState == GameState.In_Game)
            {
                if (gameMode == GameMode.Network_Multiplayer)
                    UpdateNetwork();

                if (MarkerNode1.MarkerFound)
                {                   
                    UpdateRotation(ActiveGameObjects[0], MarkerNode1.WorldTransformation.Translation);
                    ActiveGameObjects[0].MoveObjectForward(4);
                }

                if (!MarkerNode2.MarkerFound)
                {
                    //Console.WriteLine("Player 1 Fire Left!");
                }
                if (!MarkerNode3.MarkerFound)
                {
                    //Console.WriteLine("Player 1 Fire Right!");
                }

                if (gameMode == GameMode.Local_Multiplayer)
                {
                    if (MarkerNode4.MarkerFound)
                    {
                        UpdateRotation(ActiveGameObjects[1], MarkerNode4.WorldTransformation.Translation);
                        ActiveGameObjects[1].MoveObjectForward(4);
                    }

                    if (!MarkerNode5.MarkerFound)
                    {
                        //Console.WriteLine("Player 1 Fire Left!");
                    }
                    if (!MarkerNode6.MarkerFound)
                    {
                        //Console.WriteLine("Player 1 Fire Right!");
                    }
                }
            }

            if (session != null)
                session.Update();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        /// <summary>
        /// Create the Lights for the scene
        /// </summary>
        private void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();
            //lightSource.Direction = new Vector3(1, -1, -1);
            lightSource.Direction = new Vector3(1, -20, -100);
            lightSource.Diffuse = Color.White.ToVector4();
            lightSource.Specular = new Vector4(0.6f, .6f, .6f, 1);

            // Create a light node to hold the light source
            LightNode lightNode = new LightNode();
            lightNode.LightSource = lightSource;

            // Add this light node to the root node
            scene.RootNode.AddChild(lightNode);
        }

        /// <summary>
        /// Setup the Camera
        /// </summary>
        private void CreateCamera()
        {
            // Create a camera
            Camera camera = new Camera();
            // Put the camera at (0, 0, 10)
            camera.Translation = new Vector3(0, 50, 0);
            // Rotate the camera -20 degrees about the X axis
            //camera.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.ToRadians(180));
            // camera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(-10));
            // Set the vertical field of view to be 45 degrees
            camera.FieldOfViewY = MathHelper.ToRadians(90);
            // Set the near clipping plane to be 0.1f unit away from the camera
            // camera.ZNearPlane = 0.1f;
            camera.ZNearPlane = 10f;
            // Set the far clipping plane to be 1000 units away from the camera
            camera.ZFarPlane = 1000;

            // Now assign this camera to a camera node, and add this camera node to our scene graph
            CameraNode cameraNode = new CameraNode(camera);
            scene.RootNode.AddChild(cameraNode);

            // Assign the camera node to be our scene graph's current camera node
            scene.CameraNode = cameraNode;
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
            sailBoat.Boat_Name = "Fighter";
            sailBoat.Health = 100;
            sailBoat.Speed = 0;
            sailBoat.Position = Vector3.Zero;

            ModelLoader loader = new ModelLoader();
            sailBoat.Player_Ship_Model = (Model)loader.Load("Models//", "fighter");


            AvailableShips.Add(sailBoat);

        }


        //Networking

        /// <summary>
        /// Get the player info from the Server
        /// </summary>
        private void GetPlayerInfo()
        {
            PlayerInfo playerInfo1; //Information for Player 1

            playerInfo1 = new PlayerInfo();
            bool result = playerInfo1.GetPlayerInfoFromServer(SignedInGamer.SignedInGamers[0].Gamertag, SERVER_IP, SERVER_PORT_NUM);
            if (!result)
            {
                playerInfo1 = new PlayerInfo();
                playerInfo1.PlayerName = SignedInGamer.SignedInGamers[0].Gamertag;
                playerInfo1.Ammo_Level = 0;
                playerInfo1.Armour_Level = 0;
                playerInfo1.Money = 0;
                playerInfo1.Speed_Level = 0;
                playerInfo1.Player_Ship = AvailableShips[0];
                playerInfo1.PlayerLocation = Player_Location.Local;
                Console.WriteLine("Creating new profile");
                Console.Write(playerInfo1.ToString());
            }

            playerInfo1.Player_Ship = AvailableShips[0];
            activePlayers.Add(playerInfo1);
        }

        /// <summary>
        /// Setup everything to start a game
        /// </summary>
        private void StartNetworkSession()
        {
            if (gameMode == GameMode.Network_Multiplayer)
            {
                packetReader = new PacketReader();
                packetWriter = new PacketWriter();

                if (gameState == GameState.Hosting)
                {
                    Console.WriteLine("Hosting a new Network match");
                    session = NetworkSession.Create(NetworkSessionType.SystemLink, 1,10,0,null);
                    session.AllowJoinInProgress = true;
                }
                else if (gameState == GameState.Joining)
                {
                    Console.WriteLine("Looking for a game to join...");
                    AvailableNetworkSessionCollection availableSessions;

                    availableSessions = NetworkSession.Find(NetworkSessionType.SystemLink, 1, null);
                    Console.WriteLine("Found " + availableSessions.Count + " available sessions");
                    if (availableSessions.Count > 0)
                    {
                        session = NetworkSession.Join(availableSessions[0]);
                        Console.WriteLine("Session Joined!");
                    }
                }
            }

            else if (gameMode == GameMode.Local_Multiplayer || gameMode == GameMode.Single_Player)
            {
                Console.WriteLine("Creating a new Local match");
                session = NetworkSession.Create(NetworkSessionType.Local, 2, 2);
                session.AllowJoinInProgress = true;
            }

            if (session != null)
            {
                session.GameStarted += new EventHandler<GameStartedEventArgs>(session_GameStarted);
                session.GameEnded += new EventHandler<GameEndedEventArgs>(session_GameEnded);
                session.GamerJoined += new EventHandler<GamerJoinedEventArgs>(session_GamerJoined);
                session.GamerLeft += new EventHandler<GamerLeftEventArgs>(session_GamerLeft);

                scene.CameraNode = null;
                SetupMarkerTracking();
                CreateMarkers();
                gameState = GameState.Calibrating;
            }
        }

        /// <summary>
        /// Called when a gamer leaves the session
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void session_GamerLeft(object sender, GamerLeftEventArgs e)
        {
            foreach (PlayerInfo player in activePlayers)
            {
                if (player.PlayerName == e.Gamer.Gamertag)
                {
                    Console.WriteLine(e.Gamer.Gamertag + " has left the match");
                    activePlayers.Remove(player);
                    return;
                }
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

            foreach (PlayerInfo info in activePlayers)
            {
                if (info.PlayerName == e.Gamer.Gamertag)
                    return;
            }

            PlayerInfo player = new PlayerInfo();
            bool result = player.GetPlayerInfoFromServer(e.Gamer.Gamertag, SERVER_IP, SERVER_PORT_NUM);
            if (!result)
            {
                player = new PlayerInfo();
                player.PlayerName = e.Gamer.Gamertag;
                player.Ammo_Level = 0;
                player.Armour_Level = 0;
                player.Money = 0;
                player.Speed_Level = 0;
                Console.WriteLine("Creating new profile");
                Console.WriteLine(player.ToString());
            }
            player.Player_Ship = AvailableShips[0];
            activePlayers.Add(player);
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

            CreateLights();
            CreateGameObjects();

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


        //Menu Functions

        /// <summary>
        /// Creates the Menus
        /// </summary>
        private void CreateMainMenu()
        {
            // Create the main panel which holds all other GUI components
            frame = new G2DPanel();
            frame.Bounds = new Rectangle(220, 120, 350, 210);
            frame.Border = GoblinEnums.BorderFactory.LineBorder;
            frame.Transparency = 0.7f;  // Ranges from 0 (fully transparent) to 1 (fully opaque)

            G2DButton localPlay = new G2DButton("Local Play");
            localPlay.Bounds = new Rectangle(120, 30, 100, 30);
            localPlay.Name = "localPlay";
            localPlay.TextFont = textFont;
            localPlay.ActionPerformedEvent += new ActionPerformed(HandleLocalPlay);

            G2DButton networkPlay = new G2DButton("Network Play");
            networkPlay.Bounds = new Rectangle(120, 70, 100, 30);
            networkPlay.Name = "networkPlay";
            networkPlay.TextFont = textFont;
            networkPlay.ActionPerformedEvent += new ActionPerformed(HandleNetworkPlay);

            G2DButton store = new G2DButton("Store");
            store.Bounds = new Rectangle(120, 110, 100, 30);
            store.Name = "store";
            store.TextFont = textFont;
            store.ActionPerformedEvent += new ActionPerformed(HandleStore);
            /////////////////////////////////////////////////////////////////////////////////

            G2DButton startGame = new G2DButton("Start Game");
            startGame.Bounds = new Rectangle(120, 30, 100, 30);
            startGame.Name = "startGame";
            startGame.TextFont = textFont;
            startGame.ActionPerformedEvent += new ActionPerformed(HandleStartGame);

            G2DButton hostGame = new G2DButton("Host Game");
            hostGame.Bounds = new Rectangle(120, 30, 100, 30);
            hostGame.Name = "hostGame";
            hostGame.TextFont = textFont;
            hostGame.ActionPerformedEvent += new ActionPerformed(HandleHostGame);

            G2DButton joinGame = new G2DButton("Join Game");
            joinGame.Bounds = new Rectangle(120, 70, 100, 30);
            joinGame.Name = "joinGame";
            joinGame.TextFont = textFont;
            joinGame.ActionPerformedEvent += new ActionPerformed(HandleJoinGame);

            G2DButton buyUpgrades = new G2DButton("Buy Upgrades");
            buyUpgrades.Bounds = new Rectangle(120, 30, 100, 30);
            buyUpgrades.Name = "buyUpgrades";
            buyUpgrades.TextFont = textFont;
            buyUpgrades.ActionPerformedEvent += new ActionPerformed(HandleBuyUpgrades);

            G2DButton back = new G2DButton("Back");
            back.Bounds = new Rectangle(120, 150, 100, 30);
            back.Name = "back";
            back.TextFont = textFont;
            back.ActionPerformedEvent += new ActionPerformed(HandleBack);


            scene.UIRenderer.Add2DComponent(frame);


            frame.AddChild(localPlay);
            frame.AddChild(networkPlay);
            frame.AddChild(store);
            frame.AddChild(startGame);
            frame.AddChild(joinGame);
            frame.AddChild(hostGame);
            frame.AddChild(buyUpgrades);
            frame.AddChild(back);

            localPlay.Enabled = true;
            networkPlay.Enabled = true;
            store.Enabled = true;
            startGame.Enabled = false;
            joinGame.Enabled = false;
            hostGame.Enabled = false;
            buyUpgrades.Enabled = false;
            back.Enabled = false;

            startGame.Visible = false;
            joinGame.Visible = false;
            hostGame.Visible = false;
            buyUpgrades.Visible = false;
            back.Visible = false;
        }

        /// <summary>
        /// Display the main menu
        /// </summary>
        private void DisplayMainMenu()
        {
            Console.WriteLine("Displaing Main Menu");
            CreateMainMenu();
        }

        /// <summary>
        /// Hide the main menu
        /// </summary>
        private void HideMainMenu()
        {
            Console.WriteLine("Hiding Main Menu");
            scene.UIRenderer.Remove2DComponent(frame);
        }

        /// <summary>
        /// Menu for entering a local match
        /// </summary>
        /// <param name="source"></param>
        private void HandleLocalPlay(object source)
        {
            G2DComponent comp = (G2DComponent)source;

            foreach (G2DButton button in frame.Children)
            {
                if (button.Name != "startGame" && button.Name != "back")
                {
                    button.Visible = false;
                    button.Enabled = false;
                }
                else
                {
                    button.Visible = true;
                    button.Enabled = true;
                }
            }

            gameState = GameState.Hosting;
            gameMode = GameMode.Local_Multiplayer;
            StartNetworkSession();
        }

        /// <summary>
        /// Handler for starting a network match
        /// </summary>
        /// <param name="source"></param>
        private void HandleNetworkPlay(object source)
        {
            foreach (G2DButton button in frame.Children)
            {
                if (button.Name != "joinGame" && button.Name != "hostGame" && button.Name != "back")
                {
                    button.Visible = false;
                    button.Enabled = false;
                }
                else
                {
                    button.Visible = true;
                    button.Enabled = true;
                }
            }

            gameMode = GameMode.Network_Multiplayer;
        }

        /// <summary>
        /// Handler for entering the store
        /// </summary>
        /// <param name="source"></param>
        private void HandleStore(object source)
        {
            foreach (G2DButton button in frame.Children)
            {
                if (button.Name != "buyUpgrades" && button.Name != "back")
                {
                    button.Visible = false;
                    button.Enabled = false;
                }
                else
                {
                    button.Visible = true;
                    button.Enabled = true;
                }

            }
        }

        /// <summary>
        /// Called when a game is started
        /// </summary>
        /// <param name="source"></param>
        private void HandleStartGame(object source)
        {

            if (gameMode == GameMode.Network_Multiplayer)
            {
                if (session.IsEveryoneReady)
                {
                    session.StartGame();
                    session.Update();
                    HideMainMenu();
                }
            }
            else
            {
                PlayerInfo info = activePlayers[0];
                info.PlayerName = info.PlayerName + " Guest";
                activePlayers.Add(info);
                session.StartGame();
                session.Update();
                HideMainMenu();
            }
        }

        /// <summary>
        /// Join a Network Game
        /// </summary>
        /// <param name="source"></param>
        private void HandleJoinGame(object source)
        {
            G2DComponent comp = (G2DComponent)source;

            foreach (G2DButton button in frame.Children)
            {
                if (button.Name != "back")
                {
                    button.Visible = false;
                    button.Enabled = false;
                }
                else
                {
                    button.Visible = true;
                    button.Enabled = true;
                }
            }

            gameState = GameState.Joining;
            StartNetworkSession();
        }

        /// <summary>
        /// Start Hosting a Network Game
        /// </summary>
        /// <param name="source"></param>
        private void HandleHostGame(object source)
        {
            G2DComponent comp = (G2DComponent)source;

            foreach (G2DButton button in frame.Children)
            {
                if (button.Name != "startGame" && button.Name != "back")
                {
                    button.Visible = false;
                    button.Enabled = false;
                }
                else
                {
                    button.Visible = true;
                    button.Enabled = true;
                }
            }

            gameState = GameState.Hosting;
            StartNetworkSession();
        }

        /// <summary>
        /// Handler for when someone enters the store
        /// </summary>
        /// <param name="source"></param>
        private void HandleBuyUpgrades(object source)
        {
            G2DComponent comp = (G2DComponent)source;

            foreach (G2DButton button in frame.Children)
            {
                if (button.Name != "back")
                {
                    button.Visible = false;
                    button.Enabled = false;
                }
                else
                {
                    button.Visible = true;
                    button.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Return to Main Menu
        /// </summary>
        /// <param name="source"></param>
        private void HandleBack(object source)
        {
            foreach (G2DButton button in frame.Children)
            {
                if (button.Name != "localPlay" && button.Name != "networkPlay" && button.Name != "store")
                {
                    button.Visible = false;
                    button.Enabled = false;
                }
                else
                {
                    button.Visible = true;
                    button.Enabled = true;
                }
            }

            gameMode = GameMode.Menu;
            gameState = GameState.Main_Menu;


            if (session != null)
            {
                session.Dispose();
                session = null;
            }
        }


        //Markers Functions

        /// <summary>
        /// Setup the marker tracking capture devices
        /// </summary>
        private void SetupMarkerTracking()
        {
            IVideoCapture captureDevice = null;

            captureDevice = new DirectShowCapture2();
            captureDevice.InitVideoCapture(0, FrameRate._30Hz, Resolution._640x480,
                ImageFormat.R8G8B8_24, false);

            // Add this video capture device to the scene so that it can be used for
            // the marker tracker
            scene.AddVideoCaptureDevice(captureDevice);

            // Create an optical marker tracker that uses ALVAR library
            ALVARMarkerTracker tracker = new ALVARMarkerTracker();
            tracker.MaxMarkerError = 0.02f;
            tracker.InitTracker(captureDevice.Width, captureDevice.Height, "calib.xml", 9.0);

            // Set the marker tracker to use for our scene
            scene.MarkerTracker = tracker;

            // Display the camera image in the background. Note that this parameter should
            // be set after adding at least one video capture device to the Scene class.
            scene.ShowCameraImage = true;
        }

        /// <summary>
        /// Create the objects for the markers
        /// </summary>
        private void CreateMarkers()
        {
            int[] ids1;
            ids1 = new int[4];
            ids1[0] = 70;
            ids1[1] = 71;
            ids1[2] = 72;
            ids1[3] = 73;

            MarkerNode1 = new MarkerNode(scene.MarkerTracker, "Markers//ALVARConfigFromXML1.xml", ids1);
            scene.RootNode.AddChild(MarkerNode1);

            int[] ids2;
            ids2 = new int[4];
            ids2[0] = 80;
            ids2[1] = 81;
            ids2[2] = 82;
            ids2[3] = 83;

            MarkerNode2 = new MarkerNode(scene.MarkerTracker, "Markers//ALVARConfigFromXML2.xml", ids2);
            scene.RootNode.AddChild(MarkerNode2);

            int[] ids3;
            ids3 = new int[4];
            ids3[0] = 90;
            ids3[1] = 91;
            ids3[2] = 92;
            ids3[3] = 93;

            MarkerNode3 = new MarkerNode(scene.MarkerTracker, "Markers//ALVARConfigFromXML3.xml", ids3);
            scene.RootNode.AddChild(MarkerNode3);


            int[] ids4;
            ids4 = new int[4];
            ids4[0] = 100;
            ids4[1] = 101;
            ids4[2] = 102;
            ids4[3] = 103;

            MarkerNode4 = new MarkerNode(scene.MarkerTracker, "Markers//ALVARConfigFromXML4.xml", ids4);
            scene.RootNode.AddChild(MarkerNode4);


            int[] ids5;
            ids5 = new int[4];
            ids5[0] = 110;
            ids5[1] = 111;
            ids5[2] = 112;
            ids5[3] = 113;

            MarkerNode5 = new MarkerNode(scene.MarkerTracker, "Markers//ALVARConfigFromXML5.xml", ids5);
            scene.RootNode.AddChild(MarkerNode5);

            int[] ids6;
            ids6 = new int[4];
            ids6[0] = 120;
            ids6[1] = 121;
            ids6[2] = 122;
            ids6[3] = 123;

            MarkerNode6 = new MarkerNode(scene.MarkerTracker, "Markers//ALVARConfigFromXML6.xml", ids6);
            scene.RootNode.AddChild(MarkerNode6);

            int[] ids7;
            ids7 = new int[4];
            ids7[0] = 130;
            ids7[1] = 131;
            ids7[2] = 132;
            ids7[3] = 133;

            MarkerNode7 = new MarkerNode(scene.MarkerTracker, "Markers//ALVARConfigFromXML7.xml", ids7);
            scene.RootNode.AddChild(MarkerNode7);
        }


        //********************Game Logic Functions********************************//

        /// <summary>
        /// Creates all the initial game objects
        /// </summary>
        private void CreateGameObjects()
        {
            ActiveGameObjects = new List<GameObject>();
            int index = 0;

            foreach (PlayerInfo player in activePlayers)
            {
                //Create a Game Object for every active player
                GameObject playerShip;
                GeometryNode playerShipNode = new GeometryNode("Player Ship");
                playerShipNode.Model = player.Player_Ship.Player_Ship_Model;
                playerShip = new GameObject();
                playerShip.Scale = new Vector3(0.25f, 0.25f, 0.25f);
                playerShip.Yaw = 1.5f;
                playerShip.Pitch = 0f;
                playerShip.Roll = 1.5f;
                playerShip.UpdateRotationByYawPitchRoll();
                scene.RootNode.AddChild(playerShip);
                playerShip.AddChild(playerShipNode);
                playerShip.Player_Information = player;
                ActiveGameObjects.Add(playerShip);

                if (index == 0)
                {
                    playerShip.Translation = new Vector3(-25, 20, -100);
                }
                else if (index == 1)
                {
                    playerShip.Translation = new Vector3(25,-25, -100);
                    playerShip.Pitch = 9.5f;
                    playerShip.UpdateRotationByYawPitchRoll();
                }

                index++;
            }

        }

        /// <summary>
        /// Updates the rotation of an object based on it's target position
        /// </summary>
        /// <param name="player"></param>
        /// <param name="targetPosition"></param>
        private void UpdateRotation(GameObject player, Vector3 targetPosition)
        {

            Matrix rotation = Matrix.CreateFromYawPitchRoll(player.Yaw, player.Pitch, player.Roll);
            Vector3 pos = player.Translation + rotation.Backward;

            double slope = findSlope(player.Translation.X,player.Translation.Y,pos.X,pos.Y);
            double slopeDiff = findSlope(player.Translation.X, player.Translation.Y, targetPosition.X, targetPosition.Y);


            float angleDirection = (float)Math.Atan(slope);
            angleDirection = MathHelper.ToDegrees(angleDirection);
            
            float angleTarget = (float)Math.Atan(slopeDiff);
            angleTarget = MathHelper.ToDegrees(angleTarget);



            if (pos.X < player.Translation.X && pos.Y > player.Translation.Y)
                angleDirection += 180;
            if (pos.X < player.Translation.X && pos.Y < player.Translation.Y)
                angleDirection += 180;
            if (pos.X > player.Translation.X && pos.Y < player.Translation.Y)
                angleDirection += 360; 

            if (targetPosition.X < player.Translation.X && targetPosition.Y > player.Translation.Y)
                angleTarget += 180;
            if (targetPosition.X < player.Translation.X && targetPosition.Y < player.Translation.Y)
                angleTarget += 180;
            if (targetPosition.X > player.Translation.X && targetPosition.Y < player.Translation.Y)
                angleTarget += 360;


            if (angleTarget < 1)
                angleTarget = 360;
            if (angleDirection < 1)
                angleDirection = 360;


            if ( Math.Abs(angleDirection - angleTarget) < 5)
            {
                turnCounter = 0;
                return;
            }
            else
            {
                if (turnCounter == 60 || turnCounter == -60)
                {
                    turnCounter = 0;
                }

                if (turnCounter > 0)
                {
                    player.Pitch += 0.1f;
                    turnCounter++;
                }
                else if ( turnCounter < 0)
                {
                    player.Pitch -= 0.1f;
                    turnCounter--;
                }
                else if ( Math.Abs(angleDirection - angleTarget) <= 180.0f)
                {   player.Pitch += 0.1f;
                    turnCounter++;
                }
                else
                {
                    player.Pitch -= 0.1f;
                    turnCounter--;
                }
            }


            player.UpdateRotationByYawPitchRoll();
        }

        /// <summary>
        /// Returns the slope of the line from an object
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private float findSlope(float x1, float y1, float x2, float y2){

            float x = x2 - x1;
            float y = y2 - y1;

            if (x == 0)
                return 0;

            return (y) / (x);
        }

        /// <summary>
        /// Checks to see if a player has all their markers
        /// </summary>
        private void CheckReady()
        {
            bool startGame = true;
            if (!MarkerNode1.MarkerFound)
            {
                //Console.WriteLine("Missing Marker 1");
                startGame = false;
            }
            if (!MarkerNode2.MarkerFound)
            {
                //Console.WriteLine("Missing Marker 2");
                startGame = false;
            }
            if (!MarkerNode3.MarkerFound)
            {
                //Console.WriteLine("Missing Marker 3");
                startGame = false;
            }

            if (startGame)
            {
                Console.WriteLine("All markers found, gamer ready.");
                gameState = GameState.Game_Load;
                session.LocalGamers[0].IsReady = true;
            }
        }
    }
}
