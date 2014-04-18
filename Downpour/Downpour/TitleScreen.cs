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
        private Texture2D background;
        private Texture2D titleDown;
        private Texture2D titlePour;
        private Texture2D titlePause;
        private Texture2D titleStart;
        private Vector2 startPosition = new Vector2(270, 400);
        private Vector2 pausePosition = new Vector2(270, 500);

        private Vector2 downBasePosition = new Vector2(109, 219);
        public Vector2 DownPosition
        {
            get { return downBasePosition + new Vector2(0, downBounce); }
        }
        private float downBounce;
        private Vector2 pourBasePosition = new Vector2(390, 221);
        public Vector2 PourPosition
        {
            get { return pourBasePosition + new Vector2(0, pourBounce); }
        }
        private float pourBounce;
        float logoFadeValue;      // Current Value of the Fade for the Logo
        float logoFadeSpeed = 60.0f;

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
            background = Content.Load<Texture2D>("Backgrounds/levelbackground");
            titleDown = Content.Load<Texture2D>("Backgrounds/down");
            titlePour = Content.Load<Texture2D>("Backgrounds/pour");
            titlePause = Content.Load<Texture2D>("Backgrounds/menuPause");
            titleStart = Content.Load<Texture2D>("Backgrounds/pressEnter");
        }

        public override void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState)
        {
            GetInput(keyboardState, gamePadState);

            const float BounceHeight = 0.2f;
            const float BounceRate = 2.0f;
            const float BounceSync = -0.75f;

            double downT = gameTime.TotalGameTime.TotalSeconds * BounceRate + DownPosition.X * BounceSync;
            downBounce = (float)Math.Sin(downT) * BounceHeight * titleDown.Height;
            double pourT = gameTime.TotalGameTime.TotalSeconds * BounceRate + PourPosition.X * BounceSync;
            pourBounce = (float)Math.Sin(pourT) * BounceHeight * titlePour.Height;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(background, Position, Color.White);
            spriteBatch.Draw(titleDown, DownPosition, Color.White);
            spriteBatch.Draw(titlePour, PourPosition, Color.White);
            spriteBatch.Draw(titlePause, pausePosition, Color.White);
            spriteBatch.Draw(titleStart, startPosition, Color.White);
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

        public Color FadeColor(Color baseColor, float FadeValue)
        {
            Color tempColor;
            tempColor = new Color(baseColor.R, baseColor.G, baseColor.B, (byte)FadeValue);
            return tempColor;
        }
    }
}
