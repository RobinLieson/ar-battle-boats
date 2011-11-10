using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Goblin XNA
using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Generic;
using GoblinXNA.UI.UI2D;
using Microsoft.Xna.Framework.Net;

namespace AR_Battle_Boats
{
    public class GameObject : TransformNode
    {
        private PlayerInfo info;
        private NetworkGamer netPlayer;


        /// <summary>
        /// Get the Player info associated with this GameObject
        /// </summary>
        public PlayerInfo Player_Information
        {
            get
            {
                return info;
            }
            set
            {
                info = value;
            }
        }

        /// <summary>
        /// Gets or sets the NetworkGamer Info for this game object
        /// </summary>
        public NetworkGamer Network_Player_Information
        {
            get
            {
                return netPlayer;
            }
            set
            {
                netPlayer = value;
            }
        }


    }
}
