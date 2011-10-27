using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AR_Battle_Boats
{
    public class Ship
    {
        private string name;
        private int armour;
        private int health;
        private int ammo;
        private int speed;
        private Vector3 position;
        private bool firing;


        /// <summary>
        /// Creates a new Player Ship Class
        /// </summary>
        public Ship()
        {
        }

        /// <summary>
        /// Gets or Sets the name of the Boat
        /// </summary>
        public string Boat_Name{
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// sets and gets the health of ship
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

        /// <summary>
        /// sets and gets the amount of armour
        /// </summary>
        public int Armour
        {
            get
            {
                return armour;
            }
            set
            {
                armour = value;
            }
        }

        /// <summary>
        /// sets and gets the power of the ammo
        /// </summary>
        public int Ammo
        {
            get
            {
                return ammo;
            }
            set
            {
                ammo = value;
            }
        }

        /// <summary>
        /// sets and gets the speed of each ship
        /// </summary> 
        public int Speed
        {
            get
            {
                return speed;
            }
            set 
            {
                speed = value;
            }
        }

        /// <summary>
        /// Gets or Sets the position of the ship
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        /// <summary>
        /// Gets or Sets if the ship is shooting
        /// </summary>
        public bool Firing
        {
            get
            {
                return firing;
            }
            set
            {
                firing = value;
            }
        }

        /// <summary>
        /// Creates this object from a string
        /// </summary>
        /// <param name="data"></param>
        public void CreateFromString(string data)
        {
            string[] info = data.Split("\t".ToCharArray());

            foreach (string label in info)
            {
                string[] items = label.Split(":".ToCharArray());
                if (items[0] == "Boatname")
                    name = items[1];
                if (items[0] == "Boathealth")
                    health = int.Parse(items[1]);
                if (items[0] == "armour")
                    armour = int.Parse(items[1]);
                if (items[0] == "ammo")
                    ammo = int.Parse(items[1]);
                if (items[0] == "speed")
                    speed = int.Parse(items[1]);
            }
        }

        /// <summary>
        /// Returns a string representing this object
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Boatname:" + name + "\t");
            builder.Append("Boathealth:" +health + "\t");

            builder.Append("armour:" + armour + "\t");
            builder.Append("ammo:" + ammo + "\t");
            builder.Append("speed:" + speed + "\t");


            return builder.ToString();
        }
    }
}
