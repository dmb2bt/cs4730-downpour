using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Downpour
{
    class SpeedFruit : PowerUp
    {
        private const string TEXTURE = "Tiles/Fruit0";

        public SpeedFruit(Level level, Vector2 position) : base(level, position, TEXTURE)
        {
        }


        public override void Update(GameTime gameTime)
        {
            // Does nothing for now but can be used for animation
        }

        public override void OnCollected(Player collectedBy)
        {
            collectedBy.incrementSpeedMultiplier();
        }
   }
}
