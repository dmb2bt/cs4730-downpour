using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Downpour
{
    class FirePiece
    {
        private Texture2D texture;
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
            get
            {
                return new Circle(Position, Tile.Width / 3.0f);
            }
        }

        public FirePiece(Level level, Vector2 position)
        {
            this.level = level;
            this.basePosition = position;

            LoadContent();
        }

        public void LoadContent()
        {
            texture = Level.Content.Load<Texture2D>("Tiles/FirePiece");
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
        }

        public void Update(GameTime gameTime)
        {
            // Does nothing for now but can be used to animate the piece if we want

        }

        public void OnCollected(Player collectedBy)
        {
            // if we want a sound to play later
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }
    }
}
