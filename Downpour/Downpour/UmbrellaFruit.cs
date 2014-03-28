using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Downpour
{
    class UmbrellaFruit : PowerUp
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

        public Circle BoundingCircle()
        {
            return new Circle(Position, Tile.Width / 3.0f);
        }

        private const string TEXTURE = "Tiles/Fruit2";

        public UmbrellaFruit(Level level, Vector2 position)
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

        }

        public void OnCollected(Player collectedBy)
        {
            collectedBy.setUmbrella();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }        
    }
}
