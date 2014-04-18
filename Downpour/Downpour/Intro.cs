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
    class Intro : Screen, IDisposable
    {
        private GraphicsDeviceManager graphics;
        private Texture2D texture;
        private Game1 game;

        public Vector2 Position
        {
            get { return Vector2.Zero; }
        }
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        AudioManager audio;

        public Intro(Game1 game, IServiceProvider gameServiceProvider, GraphicsDeviceManager gameGraphics, ContentManager content, AudioManager audio)
            : base()
        {
            this.game = game;
            graphics = gameGraphics;
            graphics.PreferredBackBufferHeight = 640;
            this.content = content;
            this.audio = audio;

            LoadContent();
        }

        public override void LoadContent()
        {
            Type = "Intro";
            texture = Content.Load<Texture2D>("Backgrounds/intro");
        }
        public override void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState)
        {
            GetInput(keyboardState, gamePadState);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texture, Position, Color.White);
            spriteBatch.End();
        }

        public void GetInput(KeyboardState keyboardState, GamePadState gamePadState)
        {
            if (keyboardState.IsKeyDown(Keys.Space) || gamePadState.Buttons.A == ButtonState.Pressed)
            {
                audio.stopMenuSong();
                audio.playThunderSound();
                audio.playLevelSong();

                Game1.currentScreen.Type = "Level";
                
            }
        }

        public void Dispose()
        {
            Content.Unload();
        }
    }
}
