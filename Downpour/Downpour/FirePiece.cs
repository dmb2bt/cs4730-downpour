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
        public string textureName;
        private const string LOGTEXTURE = "log";
        private const string FLINTTEXTURE = "flint";
        private const string TINDERTEXTURE = "tinder";
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

        public Circle BoundingCircle
        {
            get
            {
                return new Circle(Position, Tile.Width / 3.0f);
            }
        }

        public FirePiece(Level level, Vector2 position, int textureNumber)
        {
            this.level = level;
            this.basePosition = position;
            switch (textureNumber)
            {
                case 0:
                    textureName = LOGTEXTURE;
                    break;
                case 1:
                    textureName = FLINTTEXTURE;
                    break;
                case 2:
                    textureName = TINDERTEXTURE;
                    break;
                default:
                    textureName = "FirePiece";
                    break;
            }
            LoadContent();
        }

        public void LoadContent()
        {
            texture = Level.Content.Load<Texture2D>("FirePieces/" + textureName);
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
        }

        public void Update(GameTime gameTime)
        {
            const float BounceHeight = 0.15f;
            const float BounceRate = 3.0f;
            const float BounceSync = -0.75f;

            double t = gameTime.TotalGameTime.TotalSeconds * BounceRate + Position.X * BounceSync;
            bounce = (float)Math.Sin(t) * BounceHeight * texture.Height;
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
