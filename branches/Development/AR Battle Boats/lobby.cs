using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public class lobby 
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
       public G2DLabel gamerList;
       public G2DPanel framelobby;
       public int spacing = 0;


        public lobby()
        {
          
        }
       
  
        public void createlobbyframe()
        {
            framelobby = new G2DPanel();
            framelobby.Bounds = new Rectangle(220, 0, 350, 175);
            framelobby.Border = GoblinEnums.BorderFactory.LineBorder;
            framelobby.Transparency = 0.7f;

           
            
        }
        /// <summary>
        /// Creates labels
        /// </summary>
        /// <param name="str">name of label</param>
        public void createLabel(string str)
        {
            
            gamerList = new G2DLabel(str);

            spacing += 15;
            framelobby.AddChild(gamerList);
           
        }
    }


}
