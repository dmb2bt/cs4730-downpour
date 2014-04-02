using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Downpour
{
    public class ControlInvertFruit : PowerUp
    {
        protected Texture2D texture;
        private Vector2 origin;
        private Vector2 basePosition;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        private float bounce;
        public Vector2 Position
        {
            get { return basePosition + new Vector2(0.0f, bounce); }
        }

        public Circle BoundingCircle()
        {
            return new Circle(Position, Tile.Width / 3.0f);
        }

        private const string TEXTURE = "Tiles/controlinvertfruit";

        public ControlInvertFruit(Level level, Vector2 position)
        {
            this.level = level;
            this.basePosition = position;

            LoadContent();
        }

        public void LoadContent()
        {
            texture = Level.Content.Load<Texture2D>(TEXTURE);
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
        }

        public void Update(GameTime gameTime)
        {
            const float BounceHeight = 0.1f;
            const float BounceRate = 3.0f;
            const float BounceSync = -0.75f;

            double t = gameTime.TotalGameTime.TotalSeconds * BounceRate + Position.X * BounceSync;
            bounce = (float)Math.Sin(t) * BounceHeight * texture.Height;
        }

        public void OnCollected(Player collectedBy)
        {
            collectedBy.InvertControls();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }            
    }
}
