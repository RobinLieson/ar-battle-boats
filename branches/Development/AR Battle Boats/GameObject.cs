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
        private float angle;


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
        /// Gets the angle of the current game object
        /// </summary>
        public float Angle
        {
            get
            {
                return angle;
            }
            set
            {
                angle = value;
            }
        }

        /// <summary>
        /// Updates the rotation of the object based on the yaw, pitch, and roll
        /// </summary>
        public void UpdateRotationByYawPitchRoll()
        {
            Rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
        }

        /// <summary>
        /// Rotates an object around the Z axis
        /// </summary>
        public void UpdateRotationByAngle()
        {
            //rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle);
            Matrix rotate;
            //Vector3 up = Vector3.foward;
            rotate = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            rotation= Quaternion.CreateFromRotationMatrix(rotate);
        //    rotation = Quaternion.CreateFromRotationMatrix(rotate.foward);
        }

        //Move the ship forward
        public void MoveObjectForward(int speed)
        {
            Vector3 up = Vector3.Backward;
           // up.Normalize();
            Matrix rotate = Matrix.CreateFromYawPitchRoll(yaw,pitch, roll);
            Translation += (rotate.Backward * (speed * 0.05f));
          
        }
    }
}
