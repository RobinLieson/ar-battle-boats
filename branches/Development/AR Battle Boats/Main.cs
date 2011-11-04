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
        SpriteFont textFont;
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
        /*------------------------------*/
        GeometryNode boxNode;//creates object
        GeometryNode sphereNode;
        GeometryNode cylinderNode;
        TransformNode boxTransNode;
        TransformNode sphereTransNode;
        TransformNode cylinderTransNode;
        Material boxMat;
        Material sphereMat;
        Material cylinderMat;
       



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

           
            DisplayMainMenu();

            //scene.PhysicsEngine = new NewtonPhysics();
            CreateLights();
            CreateCamera();
            CreateShips();
            base.Initialize();
        }

        private void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(1, -1, -1);
            lightSource.Diffuse = Color.White.ToVector4();
            lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);

            // Create a light node to hold the light source
            LightNode lightNode = new LightNode();
            lightNode.LightSource = lightSource;

            // Add this light node to the root node
            scene.RootNode.AddChild(lightNode);
        }



        private void CreateCamera()
        {
            // Create a camera
            Camera camera = new Camera();
            // Put the camera at (0, 0, 10)
            camera.Translation = new Vector3(0, 4, 10);
            // Rotate the camera -20 degrees about the X axis
            camera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(0));
            // Set the vertical field of view to be 45 degrees
            camera.FieldOfViewY = MathHelper.ToRadians(90);
            // Set the near clipping plane to be 0.1f unit away from the camera
            camera.ZNearPlane = 0.1f;
            // Set the far clipping plane to be 1000 units away from the camera
            camera.ZFarPlane = 1000;

            // Now assign this camera to a camera node, and add this camera node to our scene graph
            CameraNode cameraNode = new CameraNode(camera);
            scene.RootNode.AddChild(cameraNode);

            // Assign the camera node to be our scene graph's current camera node
            scene.CameraNode = cameraNode;
        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            textFont = Content.Load<SpriteFont>("SpriteFont1");
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
<<<<<<< .mine
                //gameMode = GameMode.Local_Multiplayer;
                //gameState = GameState.Hosting;
                gameState = GameState.Joining;
                StartNetworkSession();
=======
                KeyboardState state = Keyboard.GetState();
                if (state.IsKeyDown(Keys.F1))
                {
                    gameMode = GameMode.Local_Multiplayer;
                    gameState = GameState.Hosting;
                    StartNetworkSession();
                }
                else if (state.IsKeyDown(Keys.F2))
                {
                    gameMode = GameMode.Local_Multiplayer;
                    gameState = GameState.Joining;
                    StartNetworkSession();
                }
>>>>>>> .r33
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
           // UI2DRenderer.WriteText(Vector2.Zero, Color.Black,
             // textFont, GoblinEnums.HorizontalAlignment.Center, GoblinEnums.VerticalAlignment.Top);
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
<<<<<<< .mine
                bool result = playerInfo1.GetPlayerInfoFromServer(SignedInGamer.SignedInGamers[0].Gamertag, "192.168.2.187", 3550);
=======
                bool result = playerInfo1.GetPlayerInfoFromServer(SignedInGamer.SignedInGamers[0].Gamertag, "192.168.1.112", 3550);
>>>>>>> .r33
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
            boxNode = new GeometryNode("Box");
            boxNode.Model = new Box(Vector3.One * 4);
            boxMat = new Material();
            boxMat.Diffuse = Color.Red.ToVector4();
            boxMat.Specular = Color.White.ToVector4();
            boxMat.SpecularPower = 5;
            boxNode.Material = boxMat;
            boxTransNode = new TransformNode();
            boxTransNode.Translation = new Vector3(-5, 0, -6);
            boxNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;          
            boxNode.Physics.Pickable = true;// Set this box model to be pickable
            boxNode.AddToPhysicsEngine = true;// Add this box model to the physics engine
            scene.RootNode.AddChild(boxTransNode);
            boxTransNode.AddChild(boxNode);


            sphereNode = new GeometryNode("Sphere");
            sphereNode.Model = new Sphere(2, 20, 20);
            sphereMat = new Material();
            sphereMat.Diffuse = Color.Blue.ToVector4();
            sphereMat.Specular = Color.White.ToVector4();
            sphereMat.SpecularPower = 10;
            sphereNode.Material = sphereMat;
            sphereTransNode = new TransformNode();
            sphereTransNode.Translation = new Vector3(0, 0, -6);
            sphereNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Sphere;
            sphereNode.Physics.Pickable = true;
            sphereNode.AddToPhysicsEngine = true;
            scene.RootNode.AddChild(sphereTransNode);
            sphereTransNode.AddChild(sphereNode);

            cylinderNode = new GeometryNode("Cylinder");
            cylinderNode.Model = new Cylinder(.5f, .5f, 1f, 20);
            cylinderMat = new Material();
            cylinderMat.Diffuse = Color.Green.ToVector4();
            cylinderMat.Specular = Color.White.ToVector4();
            cylinderMat.SpecularPower = 5;
            cylinderNode.Material = cylinderMat;
            cylinderTransNode = new TransformNode();
            cylinderTransNode.Translation = new Vector3(5, 0, -6);
            cylinderNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;
            cylinderNode.Physics.Pickable = true;
            cylinderNode.AddToPhysicsEngine = true;
            scene.RootNode.AddChild(cylinderTransNode);
            cylinderTransNode.AddChild(cylinderNode);

          

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
                session.GameStarted += new EventHandler<GameStartedEventArgs>(session_GameStarted);
                session.GameEnded += new EventHandler<GameEndedEventArgs>(session_GameEnded);
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
<<<<<<< .mine
                    session.GameStarted += new EventHandler<GameStartedEventArgs>(session_GameStarted);
                    session.GameEnded += new EventHandler<GameEndedEventArgs>(session_GameEnded);
                    gameState = GameState.In_Game;
                    gameMode = GameMode.Local_Multiplayer;
                    Console.WriteLine("Session Joined!");
=======
                    Console.WriteLine("Session Joined!");
>>>>>>> .r33
                }
<<<<<<< .mine
                
=======

>>>>>>> .r33
            }

<<<<<<< .mine
=======
            if (session != null)
            {
                session.GameStarted += new EventHandler<GameStartedEventArgs>(session_GameStarted);
                session.GameEnded += new EventHandler<GameEndedEventArgs>(session_GameEnded);
                session.GamerJoined += new EventHandler<GamerJoinedEventArgs>(session_GamerJoined);
            }
>>>>>>> .r33


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
                //scene.RootNode.AddChild(cylinderNode);
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
