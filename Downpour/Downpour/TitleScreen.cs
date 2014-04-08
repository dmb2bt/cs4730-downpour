using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace Downpour
{
    public class TitleScreen : Screen, IDisposable
    {
        private GraphicsDeviceManager graphics;
        private Texture2D texture;
        private SoundEffect menuMusic;
        private SoundEffectInstance music;
        private bool hasMusicStarted;

        public Vector2 Position
        {
            get { return Vector2.Zero; }
        }

        // TitleScreen content.
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        public TitleScreen(IServiceProvider gameServiceProvider, GraphicsDeviceManager gameGraphics, ContentManager content)
            : base()
        {
            graphics = gameGraphics;
            graphics.PreferredBackBufferHeight = 640;
            this.content = content;
            hasMusicStarted = false;

            LoadContent();
        }

        public override void LoadContent()
        {
            Type = "TitleScreen";
            texture = Content.Load<Texture2D>("Backgrounds/MainMenu");

            menuMusic = content.Load<SoundEffect>("Sound/mainmenu");
        }

        public override void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState)
        {
            GetInput(keyboardState, gamePadState);

            if (!hasMusicStarted)
            {
                music = menuMusic.CreateInstance();
                music.IsLooped = true;
                music.Play();
                hasMusicStarted = true;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //spriteBatch.Begin();
            //spriteBatch.End();
            //graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(texture, Position, Color.White);
            spriteBatch.End();
        }

        public void GetInput(KeyboardState keyboardState, GamePadState gamePadState)
        {
            if (keyboardState.IsKeyDown(Keys.Enter) || gamePadState.Buttons.Start == ButtonState.Pressed)
            {
                //music.Stop();
                Game1.currentScreen.Type = "Level";
            }
        }

        public void Dispose()
        {
            Content.Unload();
        }
    }
}
