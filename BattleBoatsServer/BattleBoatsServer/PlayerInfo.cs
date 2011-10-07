using System.Text;

namespace BattleBoatsServer
{
    public class PlayerInfo
    {
        public string userName;
        public int money;
        public int armour;
        public int ammo;
        public int speed;
        public string ship;

        /// <summary>
        /// Create a new PlayerInfo class
        /// </summary>
        public PlayerInfo()
        {

        }

        /// <summary>
        /// Fills in all the data from a string sent to it
        /// </summary>
        /// <param name="data"></param>
        public void CreateFromString(string data)
        {
            string[] info = data.Split("\t".ToCharArray());

            foreach (string label in info)
            {
                string[] items = label.Split(":".ToCharArray());

                if (items[0] == "userName")
                    userName = items[1];
                if (items[0] == "money")
                    money = int.Parse(items[1]);
                if (items[0] == "armour")
                    armour = int.Parse(items[1]);
                if (items[0] == "ammo")
                    ammo = int.Parse(items[1]);
                if (items[0] == "speed")
                    speed = int.Parse(items[1]);
                if (items[0] == "ship")
                    ship = items[1];
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("userName:" + userName + "\t");
            builder.Append("money:" + money + "\t");
            builder.Append("armour:" + armour + "\t");
            builder.Append("ammo:" + ammo + "\t");
            builder.Append("speed:" + speed + "\t");
            builder.Append("ship:" + ship);

            return builder.ToString();
        }
    }
}
