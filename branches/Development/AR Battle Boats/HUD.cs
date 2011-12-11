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
        int heal = 100;
        G2DLabel healthLabel;
        G2DPanel healthImage;
        Texture2D healthPicture;


        public HUD(SpriteFont uiFont,Texture2D health)
        {
            Border = GoblinEnums.BorderFactory.LineBorder;
            Transparency = 0.5f;  // Ranges from 0 (fully transparent) to 1 (fully opaque)
            BackgroundColor = Color.Gray;
            healthPicture = health;
            DrawBackground = true;

            healthImage = new G2DPanel();
            healthImage.Texture = healthPicture;
            healthImage.Bounds = new Rectangle(0, 0, 80, 80);
            healthImage.Transparency = 1.0f;
            healthImage.Border = GoblinEnums.BorderFactory.LineBorder;
            healthImage.DrawBackground = true;
            healthImage.Visible = true;
            healthImage.DrawBorder = true;
            AddChild(healthImage);

            healthLabel = new G2DLabel();
            healthLabel.Bounds = new Rectangle(100, 0, 50, 80);
            healthLabel.TextTransparency = 0.5f;
            healthLabel.TextFont = uiFont;
            healthLabel.VerticalAlignment = GoblinEnums.VerticalAlignment.Center;
            AddChild(healthLabel);

            TextFont = uiFont;
        }

        /// <summary>
        /// Gets or sets the value of Health displayed by the HUD
        /// </summary>
        public int Health
        {
            get
            {
                return heal;
            }
            set
            {
                heal = value;
                healthLabel.Text = heal.ToString();
            }
        }

        public void AddMessage(string text)
        {
            FadingMessage message = new FadingMessage(text, 180);
            message.Bounds = new Rectangle(150, 0, 100, 80);
            message.TextFont = TextFont;

            if (Children.Count > 2)
            {
                Children.RemoveAt(2);
            }

            AddChild(message);
        }

        public void Update()
        {
            if (Children.Count > 2)
            {
                if (((FadingMessage)Children[2]).Update())
                {
                    Children.RemoveAt(2);
                }
            }
        }

        public override Color TextColor
        {
            get
            {
                return base.TextColor;
            }
            set
            {
                base.TextColor = value;
                healthLabel.TextColor = value;
            }
        }
    }

    class FadingMessage : G2DLabel
    {
        private int time;
        private int totalTime;

        /// <summary>
        /// Creates a new Panel with the given text message.
        /// </summary>
        /// <param name="Text">Text for the message</param>
        /// <param name="Time_To_Fade">Number of times update must be called to fade to zero</param>
        public FadingMessage(string text, int Time_To_Fade)
            : base()
        {
            Text = text;
            time = Time_To_Fade;
            totalTime = Time_To_Fade;
            Transparency = 1.0f;
        }

        /// <summary>
        /// Updates the message.  Every time Update is called, the transparency
        /// is increased.  The object should be removed from the scene when
        /// update returns false.
        /// </summary>
        /// <returns>False if transparency is equal to zero.</returns>
        public bool Update()
        {
            time--;
            float trans = (float)(time / totalTime);

            if (trans <= 0.0f)
            {
                trans = 0.0f;
                Transparency = trans;
                return false;
            }

            Transparency = trans;
            return true;
        }
    }
}
