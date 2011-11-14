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

using System.Net;
using System.Net.Sockets;
using GoblinXNA.UI;

namespace AR_Battle_Boats
{
    public class Lobby : G2DPanel
    {
        public G2DButton button;
        int count;

        public Lobby()
        {
            Border = GoblinEnums.BorderFactory.LineBorder;
            Transparency = 0.7f;
            count = 0;
        }

        public void AddPlayerToLobby(string Player)
        {
            G2DLabel label = new G2DLabel(Player);
            label.Text = Player;
            label.TextFont = textFont;
            label.Bounds = new Rectangle(10, (count * 15 + 5), 100, 15);
            this.AddChild(label);
            count++;
        }

        public void RemovePlayerFromLobby(string Gamertag)
        {
            foreach (G2DLabel label in this.Children)
            {
                if (label.Text == Gamertag)
                {
                    this.RemoveChild(label);
                    return;
                }
            }
        }
    }


}
