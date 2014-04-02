using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace Downpour
{
    public class TitleScreen : Screen
    {
        private GraphicsDeviceManager graphics;

        // TitleScreen content.
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        public TitleScreen(IServiceProvider gameServiceProvider, GraphicsDeviceManager gameGraphics)
            : base()
        {
            graphics = gameGraphics;
            graphics.PreferredBackBufferHeight = 640;
            content = new ContentManager(gameServiceProvider, "Content");
        }

        public override void LoadContent()
        {
            Type = "TitleScreen";
        }

        public override void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState)
        {
            GetInput(keyboardState, gamePadState);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //spriteBatch.Begin();
            //spriteBatch.End();
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
        }

        public void GetInput(KeyboardState keyboardState, GamePadState gamePadState)
        {
            if (keyboardState.IsKeyDown(Keys.Enter))
            {
                Game1.currentScreen.Type = "Level";
            }
        }
    }
}
