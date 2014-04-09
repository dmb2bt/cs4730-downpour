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

        Song menuSong;
        Song levelSong;

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
            //hasMenuMusicStarted = false;

            LoadContent();
        }

        public override void LoadContent()
        {
            Type = "TitleScreen";
            texture = Content.Load<Texture2D>("Backgrounds/MainMenu");

            menuSong = Content.Load<Song>("Sound/mainmenu.wav");
            MediaPlayer.Play(menuSong);
            MediaPlayer.IsRepeating = true;
            levelSong = Content.Load<Song>("Sound/level.wav");
        }

        public override void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState)
        {
            GetInput(keyboardState, gamePadState);

            //if (!hasMenuMusicStarted)
            //{
            //    menuMusicInstance = menuMusic.CreateInstance();
            //    menuMusicInstance.IsLooped = true;
            //    menuMusic.Play();
            //    hasMenuMusicStarted = true;
            //}
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
                MediaPlayer.Volume = 0.25f;
                MediaPlayer.Play(levelSong);

                Game1.currentScreen.Type = "Level";
            }
        }

        public void Dispose()
        {
            Content.Unload();
        }
    }
}
