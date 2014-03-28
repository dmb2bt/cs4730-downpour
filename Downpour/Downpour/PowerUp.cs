using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Downpour
{
    public interface PowerUp
    {
        Circle BoundingCircle();

        void Update(GameTime gameTime);

        void OnCollected(Player collectedBy);

        void Draw(GameTime gameTime, SpriteBatch spriteBatch);

    }
}
