using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Downpour
{
    public class StaticTile : Tile
    {
        Texture2D Texture;

        public StaticTile(Texture2D texture, TileCollision collision, bool rain, bool water)
            : base(collision, rain, water)
        {
            Texture = texture;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Draw(Texture, position, Color.White);
        }
    }
}
