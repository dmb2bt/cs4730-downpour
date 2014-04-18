#region File Description
//-----------------------------------------------------------------------------
// Game1.cs
//  
// Resources from PlatformerGame.cs
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using System.IO;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace Downpour
{
    // This is the main type for your game
    public class Game1 : Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // The HUD (currently displays health and play-testing info)
        private HUD hud;
        private FontRenderer fontRenderer;

        // Pause screen bool
        private bool paused = false;
        private bool pausePressing = false;

        // A boolean for the play-testing mode
        private bool playTesting;
        private bool playTestingKeyPressing;
        private bool minusPressing;
        private bool plusPressing;

        // Meta-level game state.
        private int levelIndex = -1;
        public static Screen currentScreen;
        private Level level;
        private TitleScreen titleScreen;
        private CreditScreen creditsScreen;
        private bool wasContinuePressed;
        private bool wasRestartLevelPressed;

        // We store our input states so that we only poll once per frame,
        // then we use the same input state wherever needed
        private GamePadState gamePadState;
        private GamePadState oldGamePadState;
        private KeyboardState keyboardState;
        private KeyboardState oldKeyboardState;
        private Texture2D winOverlay;
        private Texture2D diedOverlay;
        private Texture2D winOverallOverlay;
        private Texture2D pauseScreen;

        // The number of levels in the Levels directory of our content. We assume that
        // levels in our content are 0-based and that all numbers under this constant
        // have a level file present. This allows us to not need to check for the file
        // or handle exceptions, both of which can add unnecessary time to level loading.
        private const int numberOfLevels = 10;

        // Rain audio must be started here to avoid multiple instances
        public bool hasRainStarted = false;

        private bool muted = false;
        private bool mReleased;

        public AudioManager soundPlayer;

        // Constructor--note that all content should be in the Content directory
        public Game1()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 640;
            graphics.PreferredBackBufferWidth = 800;
            Content.RootDirectory = "Content";

        }

        // Allows the game to perform any initialization it needs to before starting to run.
        // This is where it can query for any required services and load any non-graphic
        // related content.  Calling base.Initialize will enumerate through any components
        // and initialize them as well.
        protected override void Initialize()
        {
            base.Initialize();
        }

        // LoadContent will be called once per game and is the place to load
        // all of your content.
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            soundPlayer = new AudioManager(Content);

            LoadTitleScreen();
            LoadCreditScreen();
            LoadNextLevel();
            currentScreen = titleScreen;
            winOverlay = Content.Load<Texture2D>("you_win");
            winOverallOverlay = Content.Load<Texture2D>("you_win_overall");
            diedOverlay = Content.Load<Texture2D>("you_died");
            pauseScreen = Content.Load<Texture2D>("pause_screen");

            hud = new HUD();
            var fontFilePath = Path.Combine(Content.RootDirectory, "theFont.fnt");
            var fontFile = FontLoader.Load(fontFilePath);
            var fontTexture = Content.Load<Texture2D>("theFont_0.png");
            fontRenderer = new FontRenderer(fontFile, fontTexture);
            //rainSound = Content.Load<SoundEffect>("Sound/rain"); //*.wav
        }

        // UnloadContent will be called once per game and is the place to unload
        // all content.
        protected override void UnloadContent()
        {
            // Unload any non ContentManager content here
            soundPlayer.stopLevelSong();
            soundPlayer.stopRainSound();
            level.Dispose();
            titleScreen.Dispose();
            hasRainStarted = true;
        }

        public void Restart()
        {
            UnloadContent();
            LoadContent();
        }

        // Allows the game to run logic such as updating the world,
        // checking for collisions, gathering input, and playing audio.
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            switch (currentScreen.Type)
            {
                case "TitleScreen":
                    currentScreen = titleScreen;
                    break;
                //case "LevelSelectScreen":
                //    currentScreen = new LevelSelectScreen();
                //    break;
                case "Level":
                    currentScreen = level;
                    break;
                case "CreditScreen":
                    currentScreen = creditsScreen;
                    break;
            }

            HandleInput();

            if (level.ReachedExit)
            {
                level.Player.OnReachedExit();
            }

            if (!paused && !level.ReachedExit)
            {
                // Update our level, passing down the GameTime along with all of our input states
                currentScreen.Update(gameTime, keyboardState, gamePadState);

                base.Update(gameTime);

                if (!hasRainStarted && currentScreen == level)
                {
                    soundPlayer.playRainSound();
                    hasRainStarted = true;
                }
            }
        }

        // Takes in input from controller/keyboard
        private void HandleInput()
        {
            // Get all of our input states
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            // Press Left Shift to bring up the play-testing screen
            if (keyboardState.IsKeyDown(Keys.LeftShift)&&!playTestingKeyPressing)
            {
                playTesting = !playTesting;
                playTestingKeyPressing = true;
            }

            if (keyboardState.IsKeyUp(Keys.LeftShift))
            {
                playTestingKeyPressing = false;
            }

            // Hold R shift and then hit right or left to move through levels
            if (keyboardState.IsKeyDown(Keys.RightShift))
            {
                if (keyboardState.IsKeyDown(Keys.Right))
                    LoadNextLevel();
                if (keyboardState.IsKeyDown(Keys.Left))
                    LoadPreviousLevel();
            }

            // Press a number along with the "-" key to decrease the variable that
            // corresponds to that number
            if (keyboardState.IsKeyDown(Keys.OemMinus)&&!minusPressing)
            {
                minusPressing = true;
                if (keyboardState.IsKeyDown(Keys.D1))
                    level.Player.MoveAcceleration -= 100;
                if (keyboardState.IsKeyDown(Keys.D2))
                    level.Player.MaxMoveSpeed-= 100;
                if (keyboardState.IsKeyDown(Keys.D3))
                    level.Player.GroundDragFactor -= 0.05f;
                if (keyboardState.IsKeyDown(Keys.D4))
                    level.Player.AirDragFactor -= 0.05f;
                if (keyboardState.IsKeyDown(Keys.D5))
                    level.Player.MaxJumpTime -= 0.1f;
                if (keyboardState.IsKeyDown(Keys.D6))
                    level.Player.JumpLaunchVelocity -= 100;
                if (keyboardState.IsKeyDown(Keys.D7))
                    level.Player.GravityAcceleration -= 100;
                if (keyboardState.IsKeyDown(Keys.D8))
                    level.Player.MaxFallSpeed -= 100;
                if (keyboardState.IsKeyDown(Keys.D9))
                    level.Player.JumpControlPower -= 0.05f;
                if (keyboardState.IsKeyDown(Keys.D0))
                    level.Player.groundSpeedMultiplierStep -= 0.05f;
            }

            // Press a number along with the "+" key to decrease the variable that
            // corresponds to that number
            if (keyboardState.IsKeyDown(Keys.OemPlus)&&!plusPressing)
            {
                plusPressing = true;
                if (keyboardState.IsKeyDown(Keys.D1))
                    level.Player.MoveAcceleration += 100;
                if (keyboardState.IsKeyDown(Keys.D2))
                    level.Player.MaxMoveSpeed += 100;
                if (keyboardState.IsKeyDown(Keys.D3))
                    level.Player.GroundDragFactor += 0.05f;
                if (keyboardState.IsKeyDown(Keys.D4))
                    level.Player.AirDragFactor += 0.05f;
                if (keyboardState.IsKeyDown(Keys.D5))
                    level.Player.MaxJumpTime += 0.1f;
                if (keyboardState.IsKeyDown(Keys.D6))
                    level.Player.JumpLaunchVelocity += 100;
                if (keyboardState.IsKeyDown(Keys.D7))
                    level.Player.GravityAcceleration += 100;
                if (keyboardState.IsKeyDown(Keys.D8))
                    level.Player.MaxFallSpeed += 100;
                if (keyboardState.IsKeyDown(Keys.D9))
                    level.Player.JumpControlPower += 0.05f;
                if (keyboardState.IsKeyDown(Keys.D0))
                    level.Player.groundSpeedMultiplierStep += 0.05f;
            }

            if (keyboardState.IsKeyUp(Keys.OemMinus))
                minusPressing = false;
            if (keyboardState.IsKeyUp(Keys.OemPlus))
                plusPressing = false;

            // Pause or unpause the game if the P or start buttons are pressed 
            if ((oldKeyboardState.IsKeyUp(Keys.P)&&keyboardState.IsKeyDown(Keys.P))||
                (oldGamePadState.IsButtonUp(Buttons.Start)&&gamePadState.IsButtonDown(Buttons.Start))&&
                !currentScreen.Type.Equals("TitleScreen") && !currentScreen.Type.Equals("CreditScreen"))
            {
                paused = !paused;
            }

            // Exit the game when back button or escape key is pressed.
            if ((gamePadState.Buttons.Back == ButtonState.Pressed) || (keyboardState.IsKeyDown(Keys.Escape)))
            {
                Exit();
            }

            bool restartLevelPressed = keyboardState.IsKeyDown(Keys.R) || gamePadState.IsButtonDown(Buttons.B);
            if (!wasRestartLevelPressed && restartLevelPressed)
            {
                if (level.Player.IsAlive && !level.ReachedExit)
                    ReloadCurrentLevel();
            }

            bool continuePressed =
             keyboardState.IsKeyDown(Keys.Space) ||
                gamePadState.IsButtonDown(Buttons.Y);

            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)
                {
                    ReloadCurrentLevel();
                }
                else if (level.ReachedExit)
                        LoadNextLevel();
            }

            wasContinuePressed = continuePressed;
            wasRestartLevelPressed = restartLevelPressed;

            if (keyboardState.IsKeyDown(Keys.M) && mReleased)
            {
                if (!muted)
                {
                    MediaPlayer.Pause();
                    SoundEffect.MasterVolume = 0.0f;
                    muted = true;
                }
                else
                {
                    MediaPlayer.Resume();
                    SoundEffect.MasterVolume = 1.0f;
                    muted = false;
                }

                mReleased = false;
            }

            if (keyboardState.IsKeyUp(Keys.M))
            {
                mReleased = true;
            }

            oldGamePadState = gamePadState;
            oldKeyboardState = keyboardState;
        }

        private void LoadTitleScreen()
        {
            titleScreen = new TitleScreen(Services, graphics, Content, soundPlayer);
        }

        private void LoadCreditScreen()
        {
            creditsScreen = new CreditScreen(this, Services, graphics, Content, soundPlayer);
        }

        // Loads the next screen
        private void LoadNextLevel()
        {
            // Move to the next level
            levelIndex++;

            if (levelIndex == numberOfLevels || levelIndex < -1)
            {
                levelIndex = -1;
                soundPlayer.stopLevelSong();
                soundPlayer.stopRainSound();
                soundPlayer.playMenuSong();
                currentScreen.Type = "CreditScreen";

            }

            // Unloads the content for the current level before loading the next one.
            if (level != null)
                level.Dispose();

            if (levelIndex >= 0)
            {
                // Load the level.
                string levelPath = string.Format("{0}/{1}/{2}.json", Content.RootDirectory, "Levels", levelIndex);
                using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                    level = new Level(Services, fileStream, levelIndex, soundPlayer);
            }
        }

        // Starts level over if player hits spacebar.  This feature should probably be removed at some point.
        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }

        private void LoadPreviousLevel()
        {
            levelIndex -= 2;

            if (levelIndex < -1)
                levelIndex = numberOfLevels - 2;

            LoadNextLevel();
        }

        // This is called when the game should draw itself.
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            
            currentScreen.Draw(gameTime, spriteBatch);
            DrawHud();

            if (paused)
            {
                DrawPauseScreen();
            }

            base.Draw(gameTime);
        }

        // Draws the overlay if player reaches exit, wins, or dies
        private void DrawHud()
        {
            spriteBatch.Begin();

            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                        titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            Texture2D status = null;

            // Choose the appropriate overlay
            if (level.ReachedExit)
            {
                if (levelIndex == numberOfLevels - 1) status = winOverallOverlay;
                else status = winOverlay;
            }

            else if (!level.Player.IsAlive)
            {
                status = diedOverlay;
            }

            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }

            if (playTesting&&!paused)
            {
                fontRenderer.DrawText(spriteBatch, 50, 50, "(1) MoveAcceleration:");
                fontRenderer.DrawText(spriteBatch, 50, 80, "(2) MaxMoveSpeed:");
                fontRenderer.DrawText(spriteBatch, 50, 110, "(3) GroundDragFactor:");
                fontRenderer.DrawText(spriteBatch, 50, 140, "(4) AirDragFactor:");
                fontRenderer.DrawText(spriteBatch, 50, 170, "(5) MaxJumpTime:");
                fontRenderer.DrawText(spriteBatch, 50, 200, "(6) JumpLaunchVelocity:");
                fontRenderer.DrawText(spriteBatch, 50, 230, "(7) GravityAcceleration:");
                fontRenderer.DrawText(spriteBatch, 50, 260, "(8) MaxFallSpeed:");
                fontRenderer.DrawText(spriteBatch, 50, 290, "(9) JumpControlPower:");
                fontRenderer.DrawText(spriteBatch, 50, 320, "(0) SpeedMultiplierStep:");

                int offset = 350;

                fontRenderer.DrawText(spriteBatch, offset, 50, level.Player.MoveAcceleration.ToString());
                fontRenderer.DrawText(spriteBatch, offset, 80, level.Player.MaxMoveSpeed.ToString());
                fontRenderer.DrawText(spriteBatch, offset, 110, level.Player.GroundDragFactor.ToString());
                fontRenderer.DrawText(spriteBatch, offset, 140, level.Player.AirDragFactor.ToString());
                fontRenderer.DrawText(spriteBatch, offset, 170, level.Player.MaxJumpTime.ToString());
                fontRenderer.DrawText(spriteBatch, offset, 200, level.Player.JumpLaunchVelocity.ToString());
                fontRenderer.DrawText(spriteBatch, offset, 230, level.Player.GravityAcceleration.ToString());
                fontRenderer.DrawText(spriteBatch, offset, 260, level.Player.MaxFallSpeed.ToString());
                fontRenderer.DrawText(spriteBatch, offset, 290, level.Player.JumpControlPower.ToString());
                fontRenderer.DrawText(spriteBatch, offset, 320, level.Player.groundSpeedMultiplierStep.ToString());
            }

            spriteBatch.End();
        }

        private void DrawPauseScreen()
        {
            spriteBatch.Begin();

            spriteBatch.Draw(pauseScreen, new Vector2(80, 128), Color.White);
            //fontRenderer.DrawText(spriteBatch, 350, 275, "PAUSED");
            //fontRenderer.DrawText(spriteBatch, 225, 300, "press (p) or (start) to continue");

            spriteBatch.End();
        }
    }
}
