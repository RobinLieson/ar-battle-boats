using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

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
        private float yaw;
        private float pitch;
        private float roll;


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

        /// <summary>
        /// Get the Yaw of the rotation of this object
        /// </summary>
        public float Yaw
        {
            get
            {
                return yaw;
            }
            set
            {
                yaw = value;
            }
        }

        /// <summary>
        /// Get the Pitch of the rotation of this object
        /// </summary>
        public float Pitch
        {
            get
            {
                return pitch;
            }
            set
            {
                pitch = value;
            }
        }

        /// <summary>
        /// Get the Yaw of the rotation of this object
        /// </summary>
        public float Roll
        {
            get
            {
                return roll;
            }
            set
            {
                roll = value;
            }
        }

        /// <summary>
        /// Updates the rotation of the object based on the yaw, pitch, and roll
        /// </summary>
        public void UpdateRotation()
        {
            Rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
        }
    }
}
