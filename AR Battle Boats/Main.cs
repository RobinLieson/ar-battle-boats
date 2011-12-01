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
        G2DPanel frame2;
        Lobby lob;
        List<GameObject> ActiveGameObjects;
        Model missileModel;
        AudioEngine audioEngine;
        SoundBank soundBank;
        WaveBank waveBank;
        Cue backgroundMusic;
        Cue explosionSound;
        Cue shootSound;
        HUD hud;

        int playerIndex = 0;
        int packetBuffer = 0;
        

        //Marker Node
        MarkerNode MarkerNode1;
        MarkerNode MarkerNode2;
        MarkerNode MarkerNode3;
        MarkerNode MarkerNode4;
        MarkerNode MarkerNode5;
        MarkerNode MarkerNode6;
        MarkerNode MarkerNode7;

        G2DLabel armourLevel;
        G2DLabel speedLevel;
        G2DLabel missleLevel;

        G2DLabel moneyLevel;

        G2DLabel speedCost;
        G2DLabel armourCost;
        G2DLabel missleCost;

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
            scene.BackgroundTexture = Content.Load<Texture2D>("Images\\two");

            activePlayers = new List<PlayerInfo>();

            this.IsMouseVisible = true; //Set Mouse Visible   

            CreateCamera();

            audioEngine = new AudioEngine("Content\\Sound\\arbattleboatssounds.xgs");
            soundBank = new SoundBank(audioEngine, "Content\\Sound\\BattleBoatsSoundBank.xsb");
            waveBank = new WaveBank(audioEngine, "Content\\Sound\\BattleBoatsWaveBank.xwb");
            backgroundMusic = soundBank.GetCue("Boom Music");
            explosionSound = soundBank.GetCue("explosion");
            shootSound = soundBank.GetCue("missile");

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
            else if (activePlayers.Count < 1)
            {
                GetPlayerInfo();
            }
            //If joining a game, make sure you have all your markers
            if (gameState == GameState.Calibrating)
            {
                CheckReady();
            }

            //Code for playing a match
            if (gameState == GameState.In_Game)
            {
                foreach (GameObject obj in ActiveGameObjects)
                {
                    //Update missle positions, remove any out of bounds ones
                    if (obj.Name == "Missile")
                    {
                        obj.MoveObjectForward(10);
                        if (OutOfBounds(obj))
                        {
                            obj.flagForRemoval = true;
                        }
                    }

                    //Update the player ships
                    if (obj.Name == "Player Ship")
                    {
                        obj.Cool_Down--;

                        if (OutOfBounds(obj) == true)
                        {
                            RotateAnimation(obj);
                        }
                        obj.MoveObjectForward(obj.Player_Information.Speed_Level);
                    }
                }

                //Update for the local player, his shooting, moving, etc...
                if (MarkerNode1.MarkerFound)
                {                   
                    UpdateRotation(ActiveGameObjects[playerIndex], MarkerNode1.WorldTransformation.Translation);
                }

                if (!MarkerNode2.MarkerFound)
                {
                    if (ActiveGameObjects[playerIndex].CanFire)
                    {
                        if (gameMode == GameMode.Network_Multiplayer)
                            SendAttack();
                        Shoot(ActiveGameObjects[playerIndex]);
                        ActiveGameObjects[playerIndex].Cool_Down = 200;
                    }                    
                }

                //If this is a local game, update for player 2
                if (gameMode == GameMode.Local_Multiplayer)
                {
                    if (MarkerNode4.MarkerFound)
                    {
                        UpdateRotation(ActiveGameObjects[1], MarkerNode4.WorldTransformation.Translation);
                    }

                    if (!MarkerNode5.MarkerFound)
                    {
                        if (ActiveGameObjects[1].CanFire)
                        {
                            Shoot(ActiveGameObjects[1]);
                            ActiveGameObjects[1].Cool_Down = 200;
                        }
                    }
                }

                //Update the network if this is a network game
                if (gameMode == GameMode.Network_Multiplayer)
                    UpdateNetwork();


                RemoveInactiveObjects();
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
            missileModel = (Model)loader.Load("Models//", "missile");

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
                AddLobbyToScene();

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
                    if (gameMode == GameMode.Network_Multiplayer)
                    {
                        lob.RemovePlayerFromLobby(e.Gamer.Gamertag);
                        activePlayers.Remove(player);
                    }
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
            if (gameMode == GameMode.Network_Multiplayer)
            {
                lob.AddPlayerToLobby(e.Gamer.Gamertag);
            }
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

            if (gameMode == GameMode.Network_Multiplayer && e.Gamer.Gamertag != SignedInGamer.SignedInGamers[0].Gamertag)
                player.PlayerLocation = Player_Location.Remote;
            else
                player.PlayerLocation = Player_Location.Local;

            activePlayers.Add(player);

            if (player.PlayerName == SignedInGamer.SignedInGamers[0].Gamertag)
            {
                playerIndex = activePlayers.Count - 1;
            }
        }

        /// <summary>
        /// Called when a Game has ended
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void session_GameEnded(object sender, GameEndedEventArgs e)
        {
            Exit();
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
            CreateHUD();
            AddCollisionCallbackShips(ActiveGameObjects[0], ActiveGameObjects[1]);
            HideMainMenu();

            backgroundMusic.Play();

            gameState = GameState.In_Game;
        }

        /// <summary>
        /// Read and write all the network data stuff
        /// </summary>
        private void UpdateNetwork()
        {
            //Write data
            if (packetBuffer <= 0)
            {
                LocalNetworkGamer gamer = session.LocalGamers[0];
                GameObject obj = ActiveGameObjects[playerIndex];

                packetWriter.Write("Position");
                packetWriter.Write(obj.Rotation);
                packetWriter.Write(obj.Translation);

                // Send it to everyone.
                gamer.SendData(packetWriter, SendDataOptions.None);

                packetBuffer = 3;
            }
            else
                packetBuffer--;


            //Read data
            NetworkGamer remoteGamer;
            foreach (LocalNetworkGamer localPlayer in session.LocalGamers)
            {
                while (localPlayer.IsDataAvailable)
                {
                    string messageType;
                    Quaternion rotation = new Quaternion();
                    Vector3 translation = new Vector3();
                    string winner;

                    localPlayer.ReceiveData(packetReader, out remoteGamer);
                    if (!remoteGamer.IsLocal)
                    {

                        messageType = packetReader.ReadString();

                        if (messageType == "Attack" || messageType == "Position")
                        {
                            rotation = packetReader.ReadQuaternion();
                            translation = packetReader.ReadVector3();

                            Console.WriteLine("Recieved message from " + remoteGamer.Gamertag);
                            Console.WriteLine("Rotation = " + rotation.ToString());
                            Console.WriteLine("Translation = " + translation.ToString());

                            GameObject obj = null;
                            foreach (GameObject obj1 in ActiveGameObjects)
                            {
                                if (obj1.Name != "Missile")
                                {
                                    if (obj1.Player_Information.PlayerName == remoteGamer.Gamertag)
                                    {
                                        obj = obj1;
                                        obj1.Rotation = rotation;
                                        obj1.Translation = translation;
                                    }
                                }
                            }
                            if (messageType == "Attack")
                            {
                                Shoot(obj);
                            }
                        }
                        else if (messageType == "Game Over")
                        {
                            winner = packetReader.ReadString();
                            Console.WriteLine(winner + " won the game!");
                        }
                    
                    }
                }
            }
        }

        /// <summary>
        /// Sends a message to other game members that you are attacking
        /// </summary>
        private void SendAttack()
        {
            LocalNetworkGamer gamer = session.LocalGamers[0];
            GameObject obj = ActiveGameObjects[playerIndex];

            packetWriter.Write("Attack");
            packetWriter.Write(obj.Rotation);
            packetWriter.Write(obj.Translation);

            // Send it to everyone.
            gamer.SendData(packetWriter, SendDataOptions.None);
        }

        /// <summary>
        /// Send a message to game clients that game is over
        /// </summary>
        private void SendGameOver(string winner)
        {
            if (gameMode == GameMode.Network_Multiplayer)
            {
                LocalNetworkGamer gamer = session.LocalGamers[0];

                packetWriter.Write("Game Over");
                packetWriter.Write(winner);

                // Send it to everyone.
                gamer.SendData(packetWriter, SendDataOptions.None);
            }
            Console.WriteLine(winner + " won the game!");
        }


        //Menu Functions

        /// <summary>
        /// Creates the Menus
        /// </summary>
        private void CreateMainMenu()
        {
            // Create the main panel which holds all other GUI components
            frame = new G2DPanel();
            frame.Bounds = new Rectangle(337, 175, 350, 210);
            frame.Border = GoblinEnums.BorderFactory.LineBorder;
            frame.Transparency = 0.7f;  // Ranges from 0 (fully transparent) to 1 (fully opaque)

            frame2 = new G2DPanel();
            frame2.Bounds = new Rectangle(262, 150, 500, 300);
            frame2.Border = GoblinEnums.BorderFactory.LineBorder;
            frame2.Transparency = 0.7f;  // Ranges from 0 (fully transparent) to 1 (fully opaque)


            G2DButton localPlay = new G2DButton("Local Play");
            localPlay.Bounds = new Rectangle(120, 30, 100, 30);
            localPlay.Name = "localPlay";
            localPlay.TextFont = textFont;
            localPlay.Texture = Content.Load<Texture2D>("Images\\three");
            localPlay.HighlightColor = Color.Red;
            localPlay.ActionPerformedEvent += new ActionPerformed(HandleLocalPlay);

            G2DButton networkPlay = new G2DButton("Network Play");
            networkPlay.Bounds = new Rectangle(120, 70, 100, 30);
            networkPlay.Name = "networkPlay";
            networkPlay.TextFont = textFont;
            networkPlay.Texture = Content.Load<Texture2D>("Images\\three");
            networkPlay.HighlightColor = Color.Red;
            networkPlay.ActionPerformedEvent += new ActionPerformed(HandleNetworkPlay);

            G2DButton store = new G2DButton("Store");
            store.Bounds = new Rectangle(120, 110, 100, 30);
            store.Name = "store";
            store.TextFont = textFont;
            store.Texture = Content.Load<Texture2D>("Images\\three");
            store.HighlightColor = Color.Red;
            store.ActionPerformedEvent += new ActionPerformed(HandleStore);
            /////////////////////////////////////////////////////////////////////////////////

            G2DButton startGame = new G2DButton("Start Game");
            startGame.Bounds = new Rectangle(120, 30, 100, 30);
            startGame.Name = "startGame";
            startGame.TextFont = textFont;
            startGame.Texture = Content.Load<Texture2D>("Images\\three");
            startGame.HighlightColor = Color.Red;
            startGame.ActionPerformedEvent += new ActionPerformed(HandleStartGame);

            G2DButton hostGame = new G2DButton("Host Game");
            hostGame.Bounds = new Rectangle(120, 30, 100, 30);
            hostGame.Name = "hostGame";
            hostGame.TextFont = textFont;
            hostGame.Texture = Content.Load<Texture2D>("Images\\three");
            hostGame.HighlightColor = Color.Red;
            hostGame.ActionPerformedEvent += new ActionPerformed(HandleHostGame);

            G2DButton joinGame = new G2DButton("Join Game");
            joinGame.Bounds = new Rectangle(120, 70, 100, 30);
            joinGame.Name = "joinGame";
            joinGame.TextFont = textFont;
            joinGame.Texture = Content.Load<Texture2D>("Images\\three");
            joinGame.HighlightColor = Color.Red;
            joinGame.ActionPerformedEvent += new ActionPerformed(HandleJoinGame);

            G2DButton buyUpgrades = new G2DButton("Buy Upgrades");
            buyUpgrades.Bounds = new Rectangle(120, 30, 100, 30);
            buyUpgrades.Name = "buyUpgrades";
            buyUpgrades.TextFont = textFont;
            buyUpgrades.Texture = Content.Load<Texture2D>("Images\\three");
            buyUpgrades.HighlightColor = Color.Red;
            buyUpgrades.ActionPerformedEvent += new ActionPerformed(HandleBuyUpgrades);

            G2DButton back = new G2DButton("Back");
            back.Bounds = new Rectangle(120, 150, 100, 30);
            back.Name = "back";
            back.TextFont = textFont;
            back.Texture = Content.Load<Texture2D>("Images\\three");
            back.HighlightColor = Color.Red;
            back.ActionPerformedEvent += new ActionPerformed(HandleBack);

            G2DButton back2 = new G2DButton("Back");
            back2.Bounds = new Rectangle(110, 160, 100, 30);
            back2.Name = "back2";
            back2.TextFont = textFont;
            back2.Texture = Content.Load<Texture2D>("Images\\three");
            back2.HighlightColor = Color.Red;
            back2.ActionPerformedEvent += new ActionPerformed(HandleBack);

            G2DButton save = new G2DButton("Save");
            save.Bounds = new Rectangle(230, 160, 100, 30);
            save.Name = "save";
            save.TextFont = textFont;
            save.Texture = Content.Load<Texture2D>("Images\\three");
            save.HighlightColor = Color.Red;
            save.ActionPerformedEvent += new ActionPerformed(HandleSave);

            G2DButton quit = new G2DButton("Quit");
            quit.Bounds = new Rectangle(120, 190, 100, 30);
            quit.Name = "quit";
            quit.TextFont = textFont;
            quit.Texture = Content.Load<Texture2D>("Images\\three");
            quit.HighlightColor = Color.Red;
            quit.ActionPerformedEvent += new ActionPerformed(HandleQuit);

            ////////////////////////////*****Store****//////////////////////////////////

            G2DButton upgradeSpeed = new G2DButton("Upgrade Speed");
            upgradeSpeed.Bounds = new Rectangle(20, 30, 130, 30);
            upgradeSpeed.Name = "upgradeSpeed";
            upgradeSpeed.TextFont = textFont;
            upgradeSpeed.Texture = Content.Load<Texture2D>("Images\\three");
            upgradeSpeed.HighlightColor = Color.Red;
            upgradeSpeed.ActionPerformedEvent += new ActionPerformed(HandleBuyUpgrades);

            G2DButton upgradeArmour = new G2DButton("Upgrade Armour");
            upgradeArmour.Bounds = new Rectangle(20, 70, 130, 30);
            upgradeArmour.Name = "upgradeArmour";
            upgradeArmour.TextFont = textFont;
            upgradeArmour.Texture = Content.Load<Texture2D>("Images\\three");
            upgradeArmour.HighlightColor = Color.Red;
            upgradeArmour.ActionPerformedEvent += new ActionPerformed(HandleBuyUpgrades);

            G2DButton upgradeMissle = new G2DButton("Upgrade Missle");
            upgradeMissle.Bounds = new Rectangle(20, 110, 130, 30);
            upgradeMissle.Name = "upgradeMissle";
            upgradeMissle.TextFont = textFont;
            upgradeMissle.Texture = Content.Load<Texture2D>("Images\\three");
            upgradeMissle.HighlightColor = Color.Red;
            upgradeMissle.ActionPerformedEvent += new ActionPerformed(HandleBuyUpgrades);


            missleLevel = new G2DLabel();
            missleLevel.Bounds = new Rectangle(170, 115, 130, 30);
            missleLevel.Name = "missleLevel";
            missleLevel.TextFont = textFont;

            armourLevel = new G2DLabel();
            armourLevel.Bounds = new Rectangle(170, 75, 130, 30);
            armourLevel.Name = "armourLevel";
            armourLevel.TextFont = textFont;

            speedLevel = new G2DLabel();
            speedLevel.Bounds = new Rectangle(170, 35, 130, 30);
            speedLevel.Name = "speedLevel";
            speedLevel.TextFont = textFont;


            missleCost = new G2DLabel();
            missleCost.Bounds = new Rectangle(270, 115, 130, 30);
            missleCost.Name = "missleCost";
            missleCost.TextFont = textFont;

            armourCost = new G2DLabel();
            armourCost.Bounds = new Rectangle(270, 75, 130, 30);
            armourCost.Name = "armourCost";
            armourCost.TextFont = textFont;

            speedCost = new G2DLabel();
            speedCost.Bounds = new Rectangle(270, 35, 130, 30);
            speedCost.Name = "speedCost";
            speedCost.TextFont = textFont;


            moneyLevel = new G2DLabel();
            moneyLevel.Bounds = new Rectangle(370, 35, 130, 30);
            moneyLevel.Name = "moneyLevel";
            moneyLevel.TextFont = textFont;
            moneyLevel.TextColor = Color.YellowGreen;

            //////////////////////////////////////////////////////////////////////////////

            scene.UIRenderer.Add2DComponent(frame);
            scene.UIRenderer.Add2DComponent(frame2);

            frame.AddChild(localPlay);
            frame.AddChild(networkPlay);
            frame.AddChild(store);
            frame.AddChild(startGame);
            frame.AddChild(joinGame);
            frame.AddChild(hostGame);
            frame.AddChild(buyUpgrades);
            frame.AddChild(back);

            frame2.AddChild(upgradeSpeed);
            frame2.AddChild(upgradeArmour);
            frame2.AddChild(upgradeMissle);
            frame2.AddChild(back2);
            frame2.AddChild(save);

            frame2.AddChild(speedLevel);
            frame2.AddChild(armourLevel);
            frame2.AddChild(missleLevel);

            frame2.AddChild(moneyLevel);

            frame2.AddChild(missleCost);
            frame2.AddChild(speedCost);
            frame2.AddChild(armourCost);

            localPlay.Enabled = true;
            networkPlay.Enabled = true;
            store.Enabled = true;
            startGame.Enabled = false;
            joinGame.Enabled = false;
            hostGame.Enabled = false;
            buyUpgrades.Enabled = false;
            back.Enabled = false;
            upgradeSpeed.Enabled = false;
            upgradeArmour.Enabled = false;
            upgradeMissle.Enabled = false;

            frame2.Enabled = false;
            frame2.Visible = false;

            startGame.Visible = false;
            joinGame.Visible = false;
            hostGame.Visible = false;
            buyUpgrades.Visible = false;
            back.Visible = false;
            upgradeSpeed.Visible = false;
            upgradeArmour.Visible = false;
            upgradeMissle.Visible = false;
        }

        /// <summary>
        /// Creates lobby
        /// </summary>
        private void AddLobbyToScene()
        {
            lob = new Lobby();
            lob.Bounds = new Rectangle(120, 70, 100, 70);
            lob.TextFont = textFont;
            frame.AddChild(lob);            
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
            if (gameState == GameState.In_Game)
                return;

            if (gameMode == GameMode.Network_Multiplayer)
            {
                if (session.IsEveryoneReady || !session.IsEveryoneReady)
                {
                    session.StartGame();
                    session.Update();
                    HideMainMenu();
                }
            }
            else
            {
                PlayerInfo info = new PlayerInfo(activePlayers[0].ToString());
                info.PlayerName = info.PlayerName + " Guest";
                info.Player_Ship = AvailableShips[0];
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
            activePlayers.Clear();
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

            string ammoLevelMsg = "Ammo Level: " + activePlayers[0].Ammo_Level;
            string armourLevelMsg = "Armour Level: " + activePlayers[0].Armour_Level;
            string speedLevelMsg = "Speed Level: " + activePlayers[0].Speed_Level;

            string ammoCostMsg = "Ammo Cost: " + (activePlayers[0].Ammo_Level * 100 * 1.5);
            string armourCostMsg = "Armour Cost: " + (activePlayers[0].Armour_Level * 100 * 1.5);
            string speedCostMsg = "Speed Cost: " + (activePlayers[0].Speed_Level * 100 * 1.5);

            string moneyMsg = "Money: " + activePlayers[0].Money;

            speedLevel.Text = speedLevelMsg;
            armourLevel.Text = armourLevelMsg;
            missleLevel.Text = ammoLevelMsg;

            moneyLevel.Text = moneyMsg;

            speedCost.Text = speedCostMsg;
            armourCost.Text = armourCostMsg;
            missleCost.Text = ammoCostMsg;

            G2DComponent comp = (G2DComponent)source;

            foreach (G2DButton button in frame.Children)
            {
                frame2.Visible = true;
                frame2.Enabled = true;
                frame.Enabled = false;
                frame.Visible = false;


                if (button.Name != "back2" && button.Name != "upgradeMissle1" && button.Name != "upgradeMissle2"
                    && button.Name != "upgradeDefense1" && button.Name != "upgradeDefense2")
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
        /// Handles saving the store data
        /// </summary>
        /// <param name="source"></param>
        private void HandleSave(object source)
        {

            G2DComponent comp = (G2DComponent)source;


            if (comp.Name == "save")
            {
                activePlayers[0].UpdateInfoOnServer(SERVER_IP, SERVER_PORT_NUM);
            }

        }

        /// <summary>
        /// Return to Main Menu
        /// </summary>
        /// <param name="source"></param>
        private void HandleBack(object source)
        {
            frame.RemoveChild(lob);

            G2DComponent comp = (G2DComponent)source;

            if (comp.Name == "back2")
            {

                comp.Visible = true;
                comp.Enabled = true;
            }

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
                    frame.Visible = true;
                    frame.Enabled = true;
                    frame2.Enabled = false;
                    frame2.Visible = false;
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

        /// <summary>
        /// Handler for when the Quit button is pressed
        /// </summary>
        /// <param name="source"></param>
        private void HandleQuit(object source)
        {
            Exit();
        }

        /// <summary>
        /// Creates the HUD for gameplay
        /// </summary>
        private void CreateHUD()
        {
            Texture2D armour = Content.Load<Texture2D>("Images\\armorHUD");
            Texture2D health = Content.Load<Texture2D>("Images\\hHUD");

            hud = new HUD(textFont,armour,health);
            hud.Bounds = new Rectangle(0, 688, 250, 80);
            scene.UIRenderer.Add2DComponent(hud);
        }

        /// <summary>
        /// Update the HUD values
        /// </summary>
        private void updateHUD()
        {
           
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
            try
            {
                scene.AddVideoCaptureDevice(captureDevice);
            }
            catch
            {

            }

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

        //Physics Functions

        /// <summary>
        /// Called whenever a collision occurs
        /// </summary>
        /// <param name="pair"></param>
        private void CollisionOccuredShips(NewtonPhysics.CollisionPair pair)
        {
            //ActiveGameObjects[0].Health -= 10 - ActiveGameObjects[0].Player_Information.Armour_Level;
            //ActiveGameObjects[1].Health -= 10 - ActiveGameObjects[1].Player_Information.Armour_Level;
            Console.WriteLine("Collission betwen the Ships!");
        }

        /// <summary>
        /// Adds a collision callback to a pair of GameObjects
        /// </summary>
        /// <param name="ob1">The first object to add to the collision</param>
        /// <param name="ob2">The second object to add to the collision</param>
        private void AddCollisionCallbackShips(GameObject ob1, GameObject ob2)
        {
            NewtonPhysics.CollisionPair pair = new NewtonPhysics.CollisionPair(ob1.Geometry.Physics, ob2.Geometry.Physics);
            ((NewtonPhysics)scene.PhysicsEngine).AddCollisionCallback(pair, CollisionOccuredShips);
        }

        /// <summary>
        /// Called whenever a collision occurs
        /// </summary>
        /// <param name="pair"></param>
        private void CollisionOccuredPlayer1(NewtonPhysics.CollisionPair pair)
        {
            explosionSound.Play();
            explosionSound = soundBank.GetCue("explosion");

            scene.PhysicsEngine.RemovePhysicsObject(pair.CollisionObject2);
            ActiveGameObjects[0].Health -= 10 - ActiveGameObjects[0].Player_Information.Armour_Level;
            if (ActiveGameObjects[0].Health <= 0)
            {
                if (session.IsHost)
                {
                    SendGameOver(ActiveGameObjects[1].Player_Information.PlayerName);
                    session.EndGame();
                }
            }
            Console.WriteLine("Player 1 Hit!  Health is at " + ActiveGameObjects[0].Health);
        }

        /// <summary>
        /// Adds a collision callback to a pair of GameObjects
        /// </summary>
        /// <param name="ob1">The first object to add to the collision</param>
        /// <param name="ob2">The second object to add to the collision</param>
        private void AddCollisionCallbackPlayer1(GameObject ob1, GameObject ob2)
        {
            NewtonPhysics.CollisionPair pair = new NewtonPhysics.CollisionPair(ob1.Geometry.Physics, ob2.Geometry.Physics);
            ((NewtonPhysics)scene.PhysicsEngine).AddCollisionCallback(pair, CollisionOccuredPlayer1);
        }

        /// <summary>
        /// Called whenever a collision occurs
        /// </summary>
        /// <param name="pair"></param>
        private void CollisionOccuredPlayer2(NewtonPhysics.CollisionPair pair)
        {
            explosionSound.Play();
            explosionSound = soundBank.GetCue("explosion");

            scene.PhysicsEngine.RemovePhysicsObject(pair.CollisionObject2);
            ActiveGameObjects[1].Health -= 10 - ActiveGameObjects[1].Player_Information.Armour_Level;
            if (ActiveGameObjects[1].Health <= 0)
            {
                if (session.IsHost)
                {
                    SendGameOver(ActiveGameObjects[0].Player_Information.PlayerName);
                    session.EndGame();
                }
            }
            Console.WriteLine("Player 2 Hit!  Health is at " + ActiveGameObjects[1].Health);
        }

        /// <summary>
        /// Adds a collision callback to a pair of GameObjects
        /// </summary>
        /// <param name="ob1">The first object to add to the collision</param>
        /// <param name="ob2">The second object to add to the collision</param>
        private void AddCollisionCallbackPlayer2(GameObject ob1, GameObject ob2)
        {
            NewtonPhysics.CollisionPair pair = new NewtonPhysics.CollisionPair(ob1.Geometry.Physics, ob2.Geometry.Physics);
            ((NewtonPhysics)scene.PhysicsEngine).AddCollisionCallback(pair, CollisionOccuredPlayer2);
        }


        //********************Game Logic Functions********************************//

        /// <summary>
        /// Checks to see if a GameObject is out of bounds
        /// </summary>
        /// <param name="player">The GameObjecct to be checked</param>
        /// <returns>True if the object is out of bounds, false otherwise</returns>
        private bool OutOfBounds(GameObject player)
        {
            
            if (player.Translation.X > 40 || player.Translation.X < -40)
            {
                return true;
            }
            else if (player.Translation.Y > 24 || player.Translation.Y < -36)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
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
                playerShip = new GameObject();
                playerShip.Scale = new Vector3(0.25f, 0.25f, 0.25f);
                playerShip.Yaw = 1.5f;
                playerShip.Pitch = 0f;
                playerShip.Roll = 1.5f;
                playerShip.Health = 100;
                playerShip.UpdateRotationByYawPitchRoll();
                playerShip.Player_Information = player;
                playerShip.Name = "Player Ship";
                playerShip.shootingSound = soundBank.GetCue("missile");
                playerShip.explosionSound = soundBank.GetCue("explosion");
                Material shipMaterial = new Material();
                shipMaterial.Diffuse = new Vector4(0, 0, 0, 1);
                shipMaterial.SpecularPower = 10;
                if (index == 0)
                {
                    shipMaterial.Specular = Color.Red.ToVector4();
                }
                else
                {
                    shipMaterial.Specular = Color.Green.ToVector4();
                }

                GeometryNode playerShipNode = new GeometryNode("Player Ship");
                playerShipNode.Model = player.Player_Ship.Player_Ship_Model;
                playerShipNode.Material = shipMaterial;
                playerShip.Geometry = playerShipNode;

                playerShipNode.AddToPhysicsEngine = true;
                playerShipNode.Physics.Shape = ShapeType.Box;

                scene.RootNode.AddChild(playerShip);                
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
                player.turnCounter = 0;
                return;
            }
            else
            {
                if (player.turnCounter == 60 || player.turnCounter == -60)
                {
                    player.turnCounter = 0;
                }

                if (player.turnCounter > 0)
                {
                    player.Pitch += 0.1f;
                    player.turnCounter++;
                }
                else if ( player.turnCounter < 0)
                {
                    player.Pitch -= 0.1f;
                    player.turnCounter--;
                }
                else if ( Math.Abs(angleDirection - angleTarget) <= 180.0f)
                {   
                    player.Pitch += 0.1f;
                    player.turnCounter++;
                }
                else
                {
                    player.Pitch -= 0.1f;
                    player.turnCounter--;
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

        /// <summary>
        /// Creates a new missle in front of owner with their rotation
        /// </summary>
        /// <param name="owner">The owner of the object</param>
        private void Shoot(GameObject owner)
        {
            if (!owner.shootingSound.IsPlaying)
                owner.shootingSound.Play();
            owner.shootingSound = soundBank.GetCue("missile");
                

            GameObject missile = new GameObject();
            missile.Rotation = owner.Rotation;
            missile.Name = "Missile";

            Matrix rotation = Matrix.CreateFromYawPitchRoll(owner.Yaw, owner.Pitch, owner.Roll);
            missile.Translation = owner.Translation + (rotation.Backward * 6f);
            missile.Scale = new Vector3(0.025f, 0.025f, 0.025f);
            missile.Yaw = owner.Yaw;
            missile.Pitch = owner.Pitch;
            missile.Roll = owner.Roll;
            missile.Type = GameObjectType.Missle;

            missile.Player_Information = owner.Player_Information;
            missile.Player_Information.Speed_Level = 7;

            GeometryNode missileNode = new GeometryNode("Missile");
            missileNode.Model = missileModel;

            missile.Geometry = missileNode;

            missile.Geometry.AddToPhysicsEngine = true;
            missile.Geometry.Physics.Shape = ShapeType.Box;
            missile.Geometry.Material = owner.Geometry.Material;

            foreach (GameObject obj in ActiveGameObjects)
            {
                if (obj.Name != "Missile")
                {
                    if (owner.Player_Information.PlayerName != obj.Player_Information.PlayerName)
                    {
                        if (ActiveGameObjects[0].Player_Information.PlayerName == obj.Player_Information.PlayerName)
                        {
                            //Console.WriteLine("Added collision callback player 1");
                            AddCollisionCallbackPlayer1(obj, missile);
                        }
                        else if (ActiveGameObjects[1].Player_Information.PlayerName == obj.Player_Information.PlayerName)
                        {
                            //Console.WriteLine("Added collision callback player 2");
                            AddCollisionCallbackPlayer2(obj, missile);
                        }
                    }
                }
            }
                
            scene.RootNode.AddChild(missile);
            ActiveGameObjects.Add(missile);
        }

        /// <summary>
        /// Removes all inactive objects from ActiveGameObjects
        /// </summary>
        private void RemoveInactiveObjects()
        {
            bool objRemoved;

            do
            {
                objRemoved = false;
                foreach (GameObject obj in ActiveGameObjects)
                {
                    if (obj.flagForRemoval)
                    {
                        Console.WriteLine("Removing Object. ActiveObjects: " + ActiveGameObjects.Count +
                            "  Scene objects: " + scene.RootNode.Children.Count);
                        scene.RootNode.RemoveChild(obj);
                        ActiveGameObjects.Remove(obj);
                        objRemoved = true;
                        Console.WriteLine("Object removed.  ActiveObjects: " + ActiveGameObjects.Count +
                            "  Scene objects: " + scene.RootNode.Children.Count);
                        break;
                    }
                }
            } while (objRemoved == true);
        }

        /// <summary>
        /// Rotates the ship when it goes out of bounds
        /// </summary>
        /// <param name="player"></param>
        private void RotateAnimation(GameObject player)
        {

            Matrix rotation = Matrix.CreateFromYawPitchRoll(player.Yaw, player.Pitch, player.Roll);
            Vector3 pos = player.Translation + rotation.Backward;
            double slope = findSlope(player.Translation.X, player.Translation.Y, pos.X, pos.Y);

            float angleDirection = (float)Math.Atan(slope);
            angleDirection = MathHelper.ToDegrees(angleDirection);

            if (OutOfBounds(player) == true)
            {
                angleDirection += 180;
                player.Pitch += .1f;

                player.UpdateRotationByYawPitchRoll();
            }        
        }
    }
}
