/************************************************************************************ 
 * Copyright (c) 2008-2011, Columbia University
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Columbia University nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY COLUMBIA UNIVERSITY ''AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL <copyright holder> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 * 
 * ===================================================================================
 * Author: Ohan Oda (ohan@cs.columbia.edu)
 * 
 *************************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Util;
using GoblinXNA.Physics;
using GoblinXNA.Physics.Newton1;
using GoblinXNA.Helpers;

namespace final_proj
{
    /// <summary>
    /// This tutorial demonstrates how to use both the ALVAR and ARTag optical marker tracker 
    /// library with our Goblin XNA framework. Please read the README.pdf included with this 
    /// project before running this tutorial. If you're using ARTag, please remove calib.xml
    /// from the solution explorer so that you can successfully build the project. 
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        Scene scene;

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

        public Game1()
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
            base.Initialize();

            // Initialize the GoblinXNA framework
            State.InitGoblin(graphics, Content, "");

            // Initialize the scene graph
            scene = new Scene(this);

            //scene.BackgroundColor = Color.CornflowerBlue;

            // Use the newton physics engine to perform collision detection
            scene.PhysicsEngine = new NewtonPhysics();

            // Multi-thread the marker tracking process
            State.ThreadOption = (ushort)ThreadOptions.MarkerTracking;


            // Set up optical marker tracking
            // Note that we don't create our own camera when we use optical marker
            // tracking. It'll be created automatically
            SetupMarkerTracking();

            // Set up the lights used in the scene
            CreateLights();

            // Create 3D objects
            CreateObjects();

            // Create the ground that represents the physical ground marker array
            CreateGround();

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

            scene.RootNode.AddChild(lightNode);
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

        /// <summary>
        /// A callback function that will be called when the box and sphere model collides
        /// </summary>
        /// <param name="pair"></param>
        private void BoxSphereCollision(NewtonPhysics.CollisionPair pair)
        {
            Console.WriteLine("Box and Sphere has collided");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
        }

        void UpdateDirections() {

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

        void updateAttack() {

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
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            UpdateDirections();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
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

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
