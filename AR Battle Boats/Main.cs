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
        Microsoft.Xna.Framework.Graphics.Model shipModel;

        GeometryNode playerGeometryNode;
        TransformNode playerTransformNode;


        //Markers initialization starts here

        MarkerNode groundMarkerNode, toolbarMarkerNode;

        MarkerNode MarkerNode1;
        MarkerNode MarkerNode2;
        MarkerNode MarkerNode3;
        MarkerNode MarkerNode4;
        MarkerNode MarkerNode5;
        MarkerNode MarkerNode6;
        MarkerNode MarkerNode7;

        GeometryNode boxNode;

        GeometryNode cylinderNode1;
        GeometryNode cylinderNode2;
        GeometryNode cylinderNode3;
        GeometryNode cylinderNode4;
        GeometryNode cylinderNode5;
        GeometryNode cylinderNode6;
        GeometryNode cylinderNode7;

        GeometryNode sphereNode;

        bool useStaticImage = false;

        GeometryNode allShapesNode;
        TransformNode allShapesTransNode;
        Material allShapesMat;

        int[] ids1;
        int[] ids2;
        int[] ids3;
        int[] ids4;
        int[] ids5;
        int[] ids6;
        int[] ids7;
        
        // Markers ini ends here


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


            // Use the newton physics engine to perform collision detection
            scene.PhysicsEngine = new NewtonPhysics();

            // Multi-thread the marker tracking process
            State.ThreadOption = (ushort)ThreadOptions.MarkerTracking;




            // Set up optical marker tracking
            // Note that we don't create our own camera when we use optical marker
            // tracking. It'll be created automatically
            SetupMarkerTracking();

          
            //scene.PhysicsEngine = new NewtonPhysics();
            CreateLights();
            // Create 3D objects
            CreateObjects();
            // Create the ground that represents the physical ground marker array
            CreateGround();

            //CreateCamera();



            // Use per pixel lighting for better quality (If you using non NVidia graphics card,
            // setting this to true may reduce the performance significantly)
            scene.PreferPerPixelLighting = true;

            // Enable shadow mapping
            // NOTE: In order to use shadow mapping, you will need to add 'PostScreenShadowBlur.fx'
            // and 'ShadowMap.fx' shader files as well as 'ShadowDistanceFadeoutMap.dds' texture file
            // to your 'Content' directory
            scene.EnableShadowMapping = true;

            // Show Frames-Per-Second on the screen for debugging
            State.ShowFPS = true;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            textFont = Content.Load<SpriteFont>("UIFont");
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
                        //Guide.ShowSignIn(1, true);
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
            }

            if (gameState == GameState.In_Game)
            {
                //Code for actually playing a match
                UpdateNetwork();
            }

            if (session != null)
                session.Update();

            //Marker start update
            UpdateDirections();

            //marker ends update
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
            DrawMarkerUpdate();
            
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
            camera.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0,1,0), MathHelper.ToRadians(180));
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
            sailBoat.Boat_Name = "Sailboat";
            sailBoat.Health = 100;
            sailBoat.Speed = 0;
            sailBoat.Position = Vector3.Zero;

            ModelLoader loader = new ModelLoader();
            //sailBoat.Player_Ship_Model = (Model)loader.Load("Models//", "Ship");


            AvailableShips.Add(sailBoat);

        }

        /// <summary>
        /// Add the ship objects to the screen
        /// </summary>
        private void AddShipsToScene()
        {
          

            foreach (PlayerInfo player in activePlayers)
            {
                playerGeometryNode = new GeometryNode(player.PlayerName);
                playerGeometryNode.Model = player.Player_Ship.Player_Ship_Model;
                playerTransformNode = new TransformNode();
                //-5,0,-6
              
                playerTransformNode.Translation = new Vector3(0, 0, 650);
                playerTransformNode.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(1,0,0), MathHelper.ToRadians(90));
                playerGeometryNode.Physics.Shape = GoblinXNA.Physics.ShapeType.Box;
                playerGeometryNode.AddToPhysicsEngine = true;// Add this sailBoat model to the physics engine
                playerTransformNode.AddChild(playerGeometryNode);
               // scene.RootNode.AddChild(playerGeometryNode);
                scene.RootNode.AddChild(playerTransformNode);
                //playerGeometryNode.AddChild(playerTransformNode);
               
            

               // playerGeometryNode.AddChild(playerTransformNode);
               
           //   playerTransformNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(-30))
            
            
            }

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
                session = NetworkSession.Create(NetworkSessionType.SystemLink, 1, 10);
                session.AllowJoinInProgress = true;
            }
            else if (gameState == GameState.Joining)
            {
                Console.WriteLine("Looking for a game to join...");
                AvailableNetworkSessionCollection availableSessions;

                availableSessions = NetworkSession.Find(NetworkSessionType.SystemLink, 2, null);
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
            if (session.AllGamers.Count >= 1 && session.IsHost)
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
                player.Player_Ship = AvailableShips[0];

                CreateCamera();
                CreateLights();
                AddShipsToScene();
            }

            gameState = GameState.In_Game;
        }


        //Markers Functions Sarts 


        private void CreateGround()
        {
            GeometryNode groundNode = new GeometryNode("Ground");

            groundNode.Model = new Box(95, 59, 0.1f);

            // Set this ground model to act as an occluder so that it appears transparent
            //groundNode.IsOccluder = true;

            // Make the ground model to receive shadow casted by other objects with
            // CastShadows set to true
            groundNode.Model.ReceiveShadows = true;



            Material groundMaterial = new Material();

            groundMaterial.Diffuse = Color.Gray.ToVector4();

            groundMaterial.Diffuse = Color.MediumBlue.ToVector4();
            groundMaterial.Specular = Color.White.ToVector4();
            groundMaterial.SpecularPower = 20;

            //groundMaterial.Texture = Content.Load<Texture2D>("sea3");
            groundNode.Material = groundMaterial;
            groundMarkerNode.AddChild(groundNode);
        }

        private void SetupMarkerTracking()
        {
            IVideoCapture captureDevice = null;

            if (useStaticImage)
            {
                captureDevice = new NullCapture();
                captureDevice.InitVideoCapture(0, FrameRate._60Hz, Resolution._640x480,
                    ImageFormat.R8G8B8_24, false);
                //((NullCapture)captureDevice).StaticImageFile = "testImage800x600.jpg";
            }
            else
            {
                // Create our video capture device that uses DirectShow library. Note that 
                // the combinations of resolution and frame rate that are allowed depend on 
                // the particular video capture device. Thus, setting incorrect resolution 
                // and frame rate values may cause exceptions or simply be ignored, depending 
                // on the device driver.  The values set here will work for a Microsoft VX 6000, 
                // and many other webcams.
                captureDevice = new DirectShowCapture2();
                captureDevice.InitVideoCapture(0, FrameRate._30Hz, Resolution._640x480,
                    ImageFormat.R8G8B8_24, false);
            }

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
            //scene.BackgroundColor = Color.CornflowerBlue;
        }

        /// <summary>
        /// A callback function that will be called when the box and sphere model collides
        /// </summary>
        /// <param name="pair"></param>
        private void BoxSphereCollision(NewtonPhysics.CollisionPair pair)
        {
            Console.WriteLine("Box and Sphere has collided");
        }

        void UpdateDirections()
        {

            if (groundMarkerNode.MarkerFound)
            {
                if ((MarkerNode1.MarkerFound && MarkerNode2.MarkerFound) || (MarkerNode4.MarkerFound && MarkerNode3.MarkerFound))
                {
                    allShapesNode = (GeometryNode)sphereNode.Physics.Container;
                    // ((TransformNode)allShapesNode.Parent).Translation += new Vector3(0f, 0f, 0f);
                }
                else if (MarkerNode3.MarkerFound && MarkerNode1.MarkerFound)
                {
                    allShapesNode = (GeometryNode)sphereNode.Physics.Container;
                    ((TransformNode)allShapesNode.Parent).Translation -= new Vector3(.10f, .10f, 0f);
                }
                else if (MarkerNode3.MarkerFound && MarkerNode2.MarkerFound)
                {
                    allShapesNode = (GeometryNode)sphereNode.Physics.Container;
                    ((TransformNode)allShapesNode.Parent).Translation -= new Vector3(-.10f, .10f, 0f);
                }
                else if (MarkerNode4.MarkerFound && MarkerNode1.MarkerFound)
                {
                    allShapesNode = (GeometryNode)sphereNode.Physics.Container;
                    ((TransformNode)allShapesNode.Parent).Translation += new Vector3(.10f, .10f, 0f);
                }
                else if (MarkerNode4.MarkerFound && MarkerNode2.MarkerFound)
                {
                    allShapesNode = (GeometryNode)sphereNode.Physics.Container;
                    ((TransformNode)allShapesNode.Parent).Translation += new Vector3(-.10f, .10f, 0f);
                }
                else if (MarkerNode1.MarkerFound)
                {
                    allShapesNode = (GeometryNode)sphereNode.Physics.Container;
                    //allShapesNode= ((GeometryNode)((NewtonPhysics)scene.).GetPhysicsObject);
                    //((TransformNode)allShapesNode.Parent).Translation = (new Vector3((MarkerNode2.WorldTransformation.Translation.X * -1f), ((TransformNode)allShapesNode.Parent).Translation.Y,((TransformNode)allShapesNode.Parent).Translation.Z));
                    ((TransformNode)allShapesNode.Parent).Translation -= new Vector3(.10f, 0f, 0f);
                }
                else if (MarkerNode2.MarkerFound)
                {
                    allShapesNode = (GeometryNode)sphereNode.Physics.Container;
                    ((TransformNode)allShapesNode.Parent).Translation += new Vector3(.10f, 0f, 0f);
                }
                else if (MarkerNode3.MarkerFound)
                {
                    allShapesNode = (GeometryNode)sphereNode.Physics.Container;
                    ((TransformNode)allShapesNode.Parent).Translation -= new Vector3(0f, .10f, 0f);
                }
                else if (MarkerNode4.MarkerFound)
                {
                    allShapesNode = (GeometryNode)sphereNode.Physics.Container;
                    ((TransformNode)allShapesNode.Parent).Translation += new Vector3(0f, .10f, 0f);
                }
            }
        }

        void updateAttack()
        {

            //Vector3 close2Box = new Vector3(3, 3, 3);
            //Vector3 close2Sphere = new Vector3(3, 3, 3);
            //((NewtonPhysics)scene.PhysicsEngine).GetClosestPoint(boxNode.Physics, sphereNode.Physics, close2Box, close2Sphere);


            if (groundMarkerNode.MarkerFound)
            {

                if (MarkerNode5.MarkerFound) // attack
                {
                    allShapesNode = (GeometryNode)sphereNode.Physics.Container;
                    ((TransformNode)allShapesNode.Parent).Translation += new Vector3(0f, .10f, 0f);
                }

                else if (MarkerNode6.MarkerFound) // defend
                {
                    allShapesNode = (GeometryNode)sphereNode.Physics.Container;
                    ((TransformNode)allShapesNode.Parent).Translation += new Vector3(0f, .10f, 0f);
                }

            }


        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void DrawMarkerUpdate()
        {
            // If ground marker array is detected
            if (groundMarkerNode.MarkerFound)
            {

                // If the toolbar marker array is detected, then overlay the box model on top
                // of the toolbar marker array; otherwise, overlay the box model on top of
                // the ground marker array
                if (toolbarMarkerNode.MarkerFound)
                {
                    // The box model is overlaid on the ground marker array, so in order to
                    // make the box model appear overlaid on the toolbar marker array, we need
                    // to offset the ground marker array's transformation. Thus, we multiply
                    // the toolbar marker array's transformation with the inverse of the ground marker
                    // array's transformation, which becomes T*G(inv)*G = T*I = T as a result, 
                    // where T is the transformation of the toolbar marker array, G is the 
                    // transformation of the ground marker array, and I is the identity matrix. 
                    // The Vector3(4, 4, 4) is a shift translation to make the box overlaid right 
                    // on top of the toolbar marker. The top-left corner of the left marker of the 
                    // toolbar marker array is defined as (0, 0, 0), so in order to make the box model
                    // appear right on top of the left marker of the toolbar marker array, we shift by
                    // half of each dimension of the 8x8x8 box model.  The approach used here requires that
                    // the ground marker array remains visible at all times.
                    Vector3 shiftVector = new Vector3(4, -4, 4);
                    Matrix mat = Matrix.CreateTranslation(shiftVector) *
                        toolbarMarkerNode.WorldTransformation *
                        Matrix.Invert(groundMarkerNode.WorldTransformation);

                    // Modify the transformation in the physics engine
                    ((NewtonPhysics)scene.PhysicsEngine).SetTransform(boxNode.Physics, mat);
                }
                else
                {
                    ((NewtonPhysics)scene.PhysicsEngine).SetTransform(boxNode.Physics,
                        Matrix.CreateTranslation(Vector3.One * 4));
                }

            }
        }


        private void CreateObjects()
         {
             allShapesNode = new GeometryNode();
             allShapesMat = new Material();
             allShapesTransNode = new TransformNode();

             // Create a geometry node with a model of a sphere that will be overlaid on
             // top of the ground marker array
             sphereNode = new GeometryNode("Sphere");
             sphereNode.Model = new Sphere(3, 20, 20);


             // Add this sphere model to the physics engine for collision detection
             sphereNode.AddToPhysicsEngine = true;
             sphereNode.Physics.Shape = ShapeType.Sphere;
             // Make this sphere model cast and receive shadows
             sphereNode.Model.CastShadows = true;
             sphereNode.Model.ReceiveShadows = true;

             // Create a marker node to track a ground marker array.
             groundMarkerNode = new MarkerNode(scene.MarkerTracker, "ALVARGroundArray.xml");

             // Since the ground marker's size is 80x52 ARTag units, in order to move the sphere model
             // to the center of the ground marker, we shift it by 40x26 units and also make it
             // float from the ground marker's center
             TransformNode sphereTransNode = new TransformNode();
             sphereTransNode.Translation = new Vector3(40, 26, 10);

             // Create a material to apply to the sphere model
             Material sphereMaterial = new Material();
             sphereMaterial.Diffuse = new Vector4(0, 0.5f, 0, 1);
             sphereMaterial.Specular = Color.White.ToVector4();
             sphereMaterial.SpecularPower = 10;

             sphereNode.Material = sphereMaterial;

             // Now add the above nodes to the scene graph in the appropriate order.
             // Note that only the nodes added below the marker node are affected by 
             // the marker transformation.
             scene.RootNode.AddChild(groundMarkerNode);
             groundMarkerNode.AddChild(sphereTransNode);
             sphereTransNode.AddChild(sphereNode);

             // Create a geometry node with a model of a box that will be overlaid on
             // top of the ground marker array initially. (When the toolbar marker array is
             // detected, it will be overlaid on top of the toolbar marker array.)
             boxNode = new GeometryNode("Box");
             boxNode.Model = new Box(8);

             // Add this box model to the physics engine for collision detection
             boxNode.AddToPhysicsEngine = true;
             boxNode.Physics.Shape = ShapeType.Box;
             // Make this box model cast and receive shadows
             boxNode.Model.CastShadows = true;
             boxNode.Model.ReceiveShadows = true;

             // Create a marker node to track a toolbar marker array.
             toolbarMarkerNode = new MarkerNode(scene.MarkerTracker, "Toolbar.txt");

             scene.RootNode.AddChild(toolbarMarkerNode);

             // Create a material to apply to the box model
             Material boxMaterial = new Material();
             boxMaterial.Diffuse = new Vector4(0.5f, 0, 0, 1);
             boxMaterial.Specular = Color.White.ToVector4();
             boxMaterial.SpecularPower = 10;

             boxNode.Material = boxMaterial;

             // Add this box model node to the ground marker node
             groundMarkerNode.AddChild(boxNode);

             // Create a collision pair and add a collision callback function that will be
             // called when the pair collides
             NewtonPhysics.CollisionPair pair = new NewtonPhysics.CollisionPair(boxNode.Physics, sphereNode.Physics);
             ((NewtonPhysics)scene.PhysicsEngine).AddCollisionCallback(pair, BoxSphereCollision);

             //NewtonPhysics.CollisionPair tmp = new NewtonPhysics.CollisionPair

             // NewtonMaterial.ContactBegin startWar = new NewtonMaterial.ContactBegin(boxNode.Physics, sphereNode.Physics);


             ids1 = new int[4];
             ids1[0] = 70;
             ids1[1] = 71;
             ids1[2] = 72;
             ids1[3] = 73;
             MarkerNode1 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML1.xml", ids1);
             cylinderNode1 = new GeometryNode("ENEMY's SHIP");
             cylinderNode1.Model = new Cylinder(3, 3, 6, 10);
             cylinderNode1.Material = sphereMaterial;
             TransformNode cylinderTransNode = new TransformNode();
             cylinderTransNode.Translation = new Vector3(0, 0, 3);
             MarkerNode1.AddChild(cylinderTransNode);
             cylinderTransNode.AddChild(cylinderNode1);
             scene.RootNode.AddChild(MarkerNode1);

             ids2 = new int[4];
             ids2[0] = 80;
             ids2[1] = 81;
             ids2[2] = 82;
             ids2[3] = 83;

             MarkerNode2 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML2.xml", ids2);

             cylinderNode2 = new GeometryNode("PLAYER's SHIP EAST");
             cylinderNode2.Model = new Cylinder(3, 3, 6, 10);
             cylinderNode2.Material = boxMaterial;
             TransformNode cylinderTransNode2 = new TransformNode();
             cylinderTransNode2.Translation = new Vector3(20, 5, 10);
             MarkerNode2.AddChild(cylinderTransNode2);
             cylinderTransNode2.AddChild(cylinderNode2);
             scene.RootNode.AddChild(MarkerNode2);


             ids3 = new int[4];
             ids3[0] = 90;
             ids3[1] = 91;
             ids3[2] = 92;
             ids3[3] = 93;

             MarkerNode3 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML3.xml", ids3);

             cylinderNode3 = new GeometryNode("PLAYER's SHIP WEST");
             cylinderNode3.Model = new Cylinder(3, 3, 6, 10);
             cylinderNode3.Material = sphereMaterial;
             TransformNode cylinderTransNode3 = new TransformNode();
             cylinderTransNode3.Translation = new Vector3(0, 5, 0);
             MarkerNode3.AddChild(cylinderTransNode3);
             cylinderTransNode3.AddChild(cylinderNode3);
             scene.RootNode.AddChild(MarkerNode3);


             ids4 = new int[4];
             ids4[0] = 100;
             ids4[1] = 101;
             ids4[2] = 102;
             ids4[3] = 103;

             MarkerNode4 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML4.xml", ids4);

             cylinderNode4 = new GeometryNode("PLAYER's NORTH");
             cylinderNode4.Model = new Cylinder(3, 3, 6, 10);
             cylinderNode4.Material = boxMaterial;
             TransformNode cylinderTransNode4 = new TransformNode();
             cylinderTransNode4.Translation = new Vector3(0, 0, 0);
             MarkerNode4.AddChild(cylinderTransNode4);
             cylinderTransNode4.AddChild(cylinderNode4);
             scene.RootNode.AddChild(MarkerNode4);

             ids5 = new int[4];
             ids5[0] = 110;
             ids5[1] = 111;
             ids5[2] = 112;
             ids5[3] = 113;
             MarkerNode5 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML5.xml", ids5);

             ids6 = new int[4];
             ids6[0] = 120;
             ids6[1] = 121;
             ids6[2] = 122;
             ids6[3] = 123;
             MarkerNode6 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML6.xml", ids6);

             ids7 = new int[4];
             ids7[0] = 130;
             ids7[1] = 131;
             ids7[2] = 132;
             ids7[3] = 133;
             MarkerNode7 = new MarkerNode(scene.MarkerTracker, "ALVARConfigFromXML7.xml", ids7);

             scene.RootNode.AddChild(MarkerNode5);
             scene.RootNode.AddChild(MarkerNode6);
             scene.RootNode.AddChild(MarkerNode7);


         }

        // Marker Functions Ends

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
