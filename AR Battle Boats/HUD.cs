using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoblinXNA.UI.UI2D;
using Microsoft.Xna.Framework;
using GoblinXNA;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace AR_Battle_Boats
{
    class HUD : G2DPanel
    {
        G2DLabel ingameHUD;

        int arm = 100;
        int heal = 100;

        G2DLabel armourLabel;
        G2DLabel healthLabel;

        public HUD(SpriteFont uiFont,Texture2D armour, Texture2D health)
        {
            Border = GoblinEnums.BorderFactory.LineBorder;
            Transparency = 0.1f;  // Ranges from 0 (fully transparent) to 1 (fully opaque)
            BackgroundColor = Color.Gray;

            UI2DRenderer.FillRectangle(new Rectangle(5, 20, 50, 50), armour, Color.White);
            UI2DRenderer.FillRectangle(new Rectangle(125, 20, 50, 50), health, Color.White);

            armourLabel = new G2DLabel();
            armourLabel.Bounds = new Rectangle(50, 15, 50, 50);
            armourLabel.TextTransparency = 0.5f;
            armourLabel.TextFont = textFont;
            armourLabel.TextColor = Color.Green;
            AddChild(armourLabel);

            healthLabel = new G2DLabel();
            healthLabel.Bounds = new Rectangle(175, 15, 50, 50);
            healthLabel.TextTransparency = 0.5f;
            healthLabel.TextFont = textFont;
            healthLabel.TextColor = Color.Green;
            AddChild(healthLabel);
        }
    }
}
