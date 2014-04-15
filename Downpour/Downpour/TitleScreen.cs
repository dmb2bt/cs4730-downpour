using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Downpour
{
    public class TitleScreen : Screen, IDisposable
    {
        private GraphicsDeviceManager graphics;
        private Texture2D texture;

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

        AudioManager audio;

        public TitleScreen(IServiceProvider gameServiceProvider, GraphicsDeviceManager gameGraphics, ContentManager content, AudioManager audio)
            : base()
        {
            graphics = gameGraphics;
            graphics.PreferredBackBufferHeight = 640;
            this.content = content;
            this.audio = audio;
            audio.playMenuSong();

            LoadContent();
        }

        public override void LoadContent()
        {
            Type = "TitleScreen";
            texture = Content.Load<Texture2D>("Backgrounds/MainMenu");
        }

        public override void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState)
        {
            GetInput(keyboardState, gamePadState);
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
                audio.stopMenuSong();

                Game1.currentScreen.Type = "Level";
                audio.playLevelSong();
            }
        }

        public void Dispose()
        {
            Content.Unload();
        }
    }
}
