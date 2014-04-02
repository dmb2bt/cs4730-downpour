using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Downpour
{
    abstract public class Screen
    {
        public string Type;
        public virtual void LoadContent() { }
        public virtual void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState) { }
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) { }
    }
}
