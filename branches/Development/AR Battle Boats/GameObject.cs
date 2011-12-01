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
using Microsoft.Xna.Framework.Audio;

namespace AR_Battle_Boats
{
    public enum GameObjectType { PlayerShip, Missle}

    public class GameObject : TransformNode
    {
        private PlayerInfo info;
        private NetworkGamer netPlayer;
        private float yaw;
        private float pitch;
        private float roll;
        private float angle;
        private GeometryNode geometry;
        private int coolDown;
        private int health;
        public int turnCounter;
        public bool flagForRemoval = false;
        public Cue shootingSound;
        public Cue explosionSound;
        
        public GameObjectType Type;

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

        /// <summary>
        /// Moves the game object forward by a factor of the given speed
        /// </summary>
        /// <param name="speed"></param>
        public void MoveObjectForward(int speed)
        {
            Matrix rotate = Matrix.CreateFromYawPitchRoll(yaw,pitch, roll);
            Translation += (rotate.Backward * ((speed + 1)* 0.05f));          
        }

        /// <summary>
        /// Gets the geometry node associated with this game object
        /// </summary>
        public GeometryNode Geometry
        {
            get
            {
                return geometry;
            }
            set
            {
                geometry = value;
                if (this.children.Count <= 0)
                {
                    AddChild(geometry);
                }                
            }
        }

        /// <summary>
        /// Gets if object is available to fire
        /// </summary>
        public bool CanFire
        {
            get
            {
                if (coolDown > 0)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Gets the cycles remaining before this ship can fire again
        /// </summary>
        public int Cool_Down
        {
            get
            {
                return coolDown;
            }
            set
            {
                coolDown = value;
            }
        }

        /// <summary>
        /// Gets the health of the current game object
        /// </summary>
        public int Health
        {
            get
            {
                return health;
            }
            set
            {
                health = value;
            }
        }

        public override bool Equals(object obj)
        {
            GameObject gameObj = (GameObject)obj;

            if (info != gameObj.info)
                return false;
            /*
            if (yaw != gameObj.Yaw)
                return false;

            if (pitch != gameObj.Pitch)
                return false;

            if (roll != gameObj.Roll)
                return false;

            if (angle != gameObj.Angle)
                return false;
            */
            if (Type != gameObj.Type)
                return false;

            if (flagForRemoval != gameObj.flagForRemoval)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
