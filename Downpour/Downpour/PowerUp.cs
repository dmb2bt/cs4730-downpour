using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Downpour
{
    class PowerUp
    {
        protected Texture2D texture;
        private Vector2 origin;
        private Vector2 basePosition;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        public Vector2 Position
        {
            get { return basePosition; }
        }

        public Circle BoundingCircle
        {
            get { return new Circle(Position, Tile.Width / 3.0f); }
        }

        public PowerUp(Level level, Vector2 position, string texture)
        {
            this.level = level;
            this.basePosition = position;

            LoadContent(texture);
        }

        public void LoadContent(string image)
        {
            texture = Level.Content.Load<Texture2D>(image);
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void OnCollected(Player collectedBy)
        {

        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }
    }
}
