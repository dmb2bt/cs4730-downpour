#region File Description
//-----------------------------------------------------------------------------
// Level.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.IO;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;


namespace Downpour
{
    // A uniform grid of tiles.
    // The level owns the player and controls the game's win and lose
    // conditions as well as scoring.
    public class Level : Screen, IDisposable
    {
        public static ContentManager LibContent;

        public AudioManager Audio
        {
            get { return audio; }
        }
        AudioManager audio;

        // Physical structure of the level.
        private Tile[,] tiles;
        private Layer[] layers;
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 1;

        private Animation[,] rainAnimations;
        private int[] rainCols;
        private List<AnimatedTile> rainTiles;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        static Player player;

        private List<FirePiece> firePieces = new List<FirePiece>();
        private List<PowerUp> powerups = new List<PowerUp>();
        
        // Key locations in the level.
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);
        
        // Rain levels (1-3)
        int lastRain;

        // Audio
        public Song levelSong;
        public SoundEffect powerupSound;
        public SoundEffect fireSound;
        public SoundEffect firePieceSound;

        // Level game state.
        private float cameraPosition;
        private Random random = new Random(354668); // Arbitrary, but constant seed

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;
        private AnimationPlayer exitAnimation;
        private Animation campfireAnimation;

        // Level content.
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        private FontRenderer fontRenderer;


        #region Loading

        
        // Constructs a new level.
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex, AudioManager audio)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            this.audio = audio;

            var fontFilePath = Path.Combine(Content.RootDirectory, "theFont.fnt");
            var fontFile = FontLoader.Load(fontFilePath);
            var fontTexture = Content.Load<Texture2D>("theFont_0.png");
            fontRenderer = new FontRenderer(fontFile, fontTexture);
            // All levels begin with a rain level of 2
            lastRain = 2;

            // Load background layer textures. All levels must
            // use the same backgrounds and only use the left-most part of them.
            // Currently, the background has two layers, blank white behind rain2
            layers = new Layer[2];
            layers[0] = new Layer(Content, "Backgrounds/levelbackground", 1.0f);
            //layers[1] = new Layer(Content, "rain2", 1.0f);

            // Initialize the player with 2000 life if this is the first level
            if (player == null)
            {
                player = new Player(this, new Vector2(), Audio);
            }
            LoadContent();
            LoadTiles(fileStream);
        }

        public override void LoadContent()
        {
            Type = "Level";
            rainAnimations = new Animation[3, 5];
            rainTiles = new List<AnimatedTile>();
            rainCols = new int[150];
            for (int i = 0; i < 150; i++)
            {
                rainCols[i] = random.Next(1, 6);
            }

            campfireAnimation = new Animation(Content.Load<Texture2D>("Tiles/campfire"), 0.1f, true);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Texture2D texture = Content.Load<Texture2D>(String.Format("Tiles/rain{0}col{1}", (i+1), (j+1)));
                    rainAnimations[i, j] = new Animation(texture, 0.075f, true);
                }
            }
        }

        // Reads the JSON file made from Tiled and creates a new LevelData instance from
        // that. Then loads all of the tiles into the game.
        /// <param name="fileStream">
        /// The Stream for the JSON file for whichever level we're on
        /// </param>
        private void LoadTiles(Stream fileStream)
        {
            int width;
            int height; 
            List<int> tileNums = null;
            List<int> pickupNums = null;

            using (StreamReader reader = new StreamReader(fileStream))
            {
                String levelDataString = reader.ReadToEnd();

                LevelData.RootObject levelData = JsonConvert.DeserializeObject<LevelData.RootObject>(levelDataString);

                width = levelData.width;
                height = levelData.height;
                int layerCount = 0;
                foreach (LevelData.Layer layer in levelData.layers){
                    if (layerCount == 0)
                        tileNums = layer.data;
                    else if (layerCount == 1)
                        pickupNums = layer.data;

                    layerCount += 1;

                }

                System.Diagnostics.Debug.WriteLine("width is: " + width);
                System.Diagnostics.Debug.WriteLine("height is: " + height);

            }

            // Allocate the tile grid.
            tiles = new Tile[width, height];

            // Counter to go through the 
            int count = 0;
            int pickupCount = 0;

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    int tileType = tileNums[count++];

                    tiles[x, y] = LoadTile(tileType, x, y);

                    if (pickupCount < pickupNums.Count)
                    {
                        int pickupableType = pickupNums[pickupCount];
                        LoadPickUpable(pickupableType, x, y);
                    }
                    pickupCount++;
                }
            }

            // Verify that the level has a beginning and an end.
            // Don't think checking for the beginning this way works quite right.
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            if (exit == InvalidPosition)
                throw new NotSupportedException("A level must have an exit.");

        }

        // Loads an individual tile's appearance and behavior.
        // This calls some probably unnecessarily redundant methods, 
        // but I don't want to change them since I can't check that nothing will break.
        // Feel free to clean up if you see bloat.
        /// <param name="tileType">
        /// The character loaded from the structure file which
        /// indicates what should be loaded.
        /// </param>
        /// <param name="x">
        /// The X location of this tile in tile space.
        /// </param>
        /// <param name="y">
        /// The Y location of this tile in tile space.
        /// </param>
        /// <returns>The loaded tile.</returns>
        public Tile LoadTile(int tileType, int x, int y)
        {
            System.Diagnostics.Debug.WriteLine("tile num: " + tileType);
            switch (tileType)
            {
                case 0:
                    return LoadClearTile();

                case 1:
                    return LoadTile("tlcorner", TileCollision.Impassable, false);

                case 2:
                    return LoadTile("tside", TileCollision.Impassable, false);

                case 3:
                    return LoadTile("trcorner", TileCollision.Impassable, false);

                case 4: 
                    return LoadTile("lrtside", TileCollision.Impassable, false);

                case 5:
                    return LoadTile("brindent", TileCollision.Impassable, false);

                case 6:
                    return LoadTile("blindent", TileCollision.Impassable, false);

                case 7:
                    return LoadTile("trbrindent", TileCollision.Impassable, false);

                case 8:
                    return LoadTile("tlblindent", TileCollision.Impassable, false);

                case 9:
                    return LoadTile("lside", TileCollision.Impassable, false);

                case 10:
                    return LoadTile("center", TileCollision.Impassable, false);

                case 11:
                    return LoadTile("rside", TileCollision.Impassable, false);

                case 12:
                    return LoadTile("lrside", TileCollision.Impassable, false);

                case 13:
                    return LoadTile("trindent", TileCollision.Impassable, false);

                case 14:
                    return LoadTile("tlindent", TileCollision.Impassable, false);

                case 15:
                    return LoadTile("tltrindent", TileCollision.Impassable, false);

                case 16:
                    return LoadTile("blbrindent", TileCollision.Impassable, false);

                case 17:
                    return LoadTile("blcorner", TileCollision.Impassable, false);

                case 18:
                    return LoadTile("bside", TileCollision.Impassable, false);

                case 19:
                    return LoadTile("brcorner", TileCollision.Impassable, false);

                case 20:
                    return LoadTile("lbrside", TileCollision.Impassable, false);

                case 21:
                    return LoadTile("moon", TileCollision.Impassable, false);

                case 22:
                    return LoadTile("grass1", TileCollision.Passable, false);

                case 23:
                    return LoadTile("grass2", TileCollision.Passable, false);

                case 24:
                    return LoadTile("vines", TileCollision.Passable, false);

                case 25:
                    return LoadTile("lsidetrindent", TileCollision.Impassable, false);

                case 26:
                    return LoadTile("tlindentrside", TileCollision.Impassable, false);

                case 27:
                    return LoadTile("ltbside", TileCollision.Impassable, false);

                case 28:
                    return LoadTile("tbside", TileCollision.Impassable, false);

                case 29:
                    return LoadTile("tbrside", TileCollision.Impassable, false);

                case 30:
                    return LoadWaterTile();

                case 33:
                    return LoadTile("lsidebrindent", TileCollision.Impassable, false);

                case 34:
                    return LoadTile("blindentrside", TileCollision.Impassable, false);

                case 41:
                    return LoadTile("tsideblindent", TileCollision.Impassable, false);
                
                case 42:
                    return LoadTile("tsidebrindent", TileCollision.Impassable, false);
                
                case 49:
                    return LoadTile("tlindentbside", TileCollision.Impassable, false);
                
                case 50:
                    return LoadTile("trindentbside", TileCollision.Impassable, false);

                case 126:
                    return LoadExitTile(x, y);

                case 127:
                    return LoadRainTile(x, y);

                case 128: 
                    return LoadStartTile(x, y);

                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        // Air with rain
        private Tile LoadRainTile(int x, int y)
        {
            int columnNum = rainCols[x];

            Tile tile = LoadAnimatedTile(String.Format("rain2col{0}", columnNum), TileCollision.Passable, true, false, columnNum);

            rainTiles.Add((AnimatedTile)tile);
            return tile;
        }

        private Tile LoadWaterTile()
        {
            return LoadAnimatedTile("waterAnimation", TileCollision.Passable, false, true);
        }

        // Air without rain
        private Tile LoadClearTile()
        {
            return LoadTile("rain0", TileCollision.Passable, false);
        }

        private Tile LoadTile(string name, TileCollision collision, bool rain)
        {
            return new StaticTile(Content.Load<Texture2D>("Tiles/" + name), collision, rain, false);
        }

        private Tile LoadTile(string name, TileCollision collision, bool rain, bool water)
        {
            return new StaticTile(Content.Load<Texture2D>("Tiles/" + name), collision, rain, water);
        }

        private Tile LoadAnimatedTile(string name, TileCollision collision, bool rain, bool water)
        {
            return new AnimatedTile(new Animation(Content.Load<Texture2D>("Tiles/" + name), 0.075f, true), collision, rain, water);
        }

        private Tile LoadAnimatedTile(string name, TileCollision collision, bool rain, bool water, int columnNum)
        {
            return new AnimatedTile(new Animation(Content.Load<Texture2D>("Tiles/" + name), 0.075f, true), collision, rain, water, columnNum);
        }

        // Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        private Tile LoadStartTile(int x, int y)
        {
            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));

            // If this isn't the first level, initialize the player with the life he had left
            player = new Player(this, start, Audio);

            return LoadClearTile();
        }

        // Redundant method that does the same thing except starts the player in the rain
        private Tile LoadStartTileRain(int x, int y)
        {
            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));

            // If this isn't the first level, initialize the player with the life he had left
            player = new Player(this, start, Audio);

            return LoadRainTile(x, y);
        }

        // Remembers the location of the level's exit.
        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable, false);
        }

        public void LoadPickUpable(int pickupableNum, int x, int y)
        {
            switch (pickupableNum)
            {
                // No item so do nothing
                case 0:
                    break;
                
                case 116:
                    LoadSpeedFruit(x, y);
                    break;

                case 117:
                    LoadControlInvertFruit(x, y);
                    break;

                case 118:
                    LoadInvulnerabilityFruit(x, y);
                    break;

                case 119:
                    LoadJumpBoostFruit(x, y);
                    break;

                case 120:
                    LoadHealthFruit(x, y);
                    break;

                case 123:
                    LoadSuit(x, y);
                    break;

                case 124:
                    LoadFirePiece(x, y);
                    break;

                default:
                    break;
            }
        }

        private void LoadFirePiece(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            firePieces.Add(new FirePiece(this, new Vector2(position.X, position.Y), firePieces.Count));
        }

        private void LoadSpeedFruit(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            powerups.Add(new SpeedFruit(this, new Vector2(position.X, position.Y)));
        }

        private void LoadControlInvertFruit(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            powerups.Add(new ControlInvertFruit(this, new Vector2(position.X, position.Y)));
        }

        private void LoadInvulnerabilityFruit(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            powerups.Add(new InvulnerabilityFruit(this, new Vector2(position.X, position.Y)));
        }

        private void LoadHealthFruit(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            powerups.Add(new HealthFruit(this, new Vector2(position.X, position.Y)));
        }
        
        private void LoadJumpBoostFruit(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            powerups.Add(new JumpBoostFruit(this, new Vector2(position.X, position.Y)));
        }

        private void LoadSuit(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            powerups.Add(new Suit(this, new Vector2(position.X, position.Y)));
        }

        // Unloads the level content.
        public void Dispose()
        {
            Content.Unload();
        }

        #endregion

        #region Bounds and collision

        // Gets the collision mode of the tile at a particular location.
        // This method handles tiles outside of the levels boundaries by making it
        // impossible to escape past the left or right edges, but allowing things
        // to jump beyond the top of the level and fall off the bottom.
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        // Gets whether a tile will damage the player
        public bool GetRain(int x, int y)
        {
            // Can't be damaged by tiles off the screen.
            if (x < 0 || x >= Width) return false;
            if (y < 0 || y >= Height) return false;

            return tiles[x, y].rain;
        }

        // Gets whether a tile is water and will drastically damage the player
        public bool GetWater(int x, int y)
        {
            // Can't be damaged by tiles off the screen.
            if (x < 0 || x >= Width) return false;
            if (y < 0 || y >= Height) return false;

            return tiles[x, y].water;
        }

        // Gets the bounding rectangle of a tile in world space.
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        // Width of level measured in tiles.
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        // Height of the level measured in tiles.
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        #endregion

        #region Update

        // Updates all objects in the world and performs collision between them.
        public override void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState)
        {

            // Pause while the player is dead or time is expired.
            if (!Player.IsAlive)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);
            }
            else
            {
                Player.Update(gameTime,keyboardState,gamePadState);
                UpdateFirePieces(gameTime);
                UpdatePowerUps(gameTime);
                UpdateRainTiles(gameTime);

                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    OnPlayerKilled();

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile.
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }           
        }

        private void UpdateRainTiles(GameTime gameTime)
        {
            foreach (AnimatedTile tile in rainTiles)
            {
                tile.ChangeAnimation(rainAnimations[lastRain - 1, tile.rainCol - 1]);
            }
        }

        private void UpdateFirePieces(GameTime gameTime)
        {
            for (int i = 0; i < firePieces.Count; ++i)
            {
                FirePiece firepiece = firePieces[i];

                firepiece.Update(gameTime);

                if (firepiece.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    firePieces.RemoveAt(i--);
                    OnFirePieceCollected(firepiece, Player);
                }
            }
        }

        private void UpdatePowerUps(GameTime gameTime)
        {
            for (int i = 0; i < powerups.Count; ++i)
            {
                PowerUp powerup = powerups[i];

                powerup.Update(gameTime);

                if (powerup.BoundingCircle().Intersects(Player.BoundingRectangle))
                {
                    powerups.RemoveAt(i--);
                    OnPowerUpCollected(powerup, Player);
                }
            }
        }


        // Called when the player is killed.
        private void OnPlayerKilled()
        {
            Player.OnKilled();
        }

        // Called when the player reaches the level's exit.
        private void OnExitReached()
        {
            if (firePieces.Count == 0)
            {
                if(!reachedExit)
                    audio.playFireSound();

                Player.OnReachedExit();
                reachedExit = true;
                exitAnimation.PlayAnimation(campfireAnimation);
            }
            
        }

        private void OnFirePieceCollected(FirePiece firepiece, Player collectedBy)
        {
            // TODO: Need to show piece collected
            audio.playFirePieceSound();

            firepiece.OnCollected(collectedBy);
        }

        private void OnPowerUpCollected(PowerUp powerup, Player collectedBy)
        {
            if(!(powerup is Suit))
                audio.playPowerupSound();

            powerup.OnCollected(collectedBy);
        }


        // Restores the player to the starting point to try the level again.
        public void StartNewLife()
        {
            Player.Reset(start);
        }

        #endregion

        #region Draw

        // Draw everything in the level from background to foreground.
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            lastRain = player.rainLevel;

            // Draw background up to entity layer
            for (int i = 0; i < EntityLayer; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, 0.0f, 0.0f);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, cameraTransform);
            // Draw tiles
            DrawTiles(gameTime, spriteBatch);

            // Draw player
            Player.Draw(gameTime, spriteBatch);

            // Draw firepieces
            foreach (FirePiece piece in firePieces)
                piece.Draw(gameTime, spriteBatch);

            foreach (PowerUp powerup in powerups)
                powerup.Draw(gameTime, spriteBatch);

            if (reachedExit)
            {
                Vector2 exitPosition = new Vector2(exit.X, exit.Y + 15);
                exitAnimation.Draw(gameTime, spriteBatch, exitPosition, SpriteEffects.None);
            }

            spriteBatch.End();

            spriteBatch.Begin();
            // Draw background past entity layer
            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);

            // Draw life bar
            int life = player.Life;
            fontRenderer.DrawText(spriteBatch, 35, 14, "Life");
            Rectangle lifeBar = new Rectangle(85, 20, life / 4, 20);
            spriteBatch.Draw(layers[0].Texture, lifeBar, Color.Red);
            int shield = player.ShieldLife;
            fontRenderer.DrawText(spriteBatch, 5, 37, "Shield");
            Rectangle shieldBar = new Rectangle(85, 44, shield, 20);
            spriteBatch.Draw(layers[0].Texture, shieldBar, Color.Purple);

            fontRenderer.DrawText(spriteBatch, 575, 600, "Fire Pieces: " + (3 - firePieces.Count) + " / 3"); 

            spriteBatch.End();
        }

        public void DrawTiles(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Calculate the visible range of tiles
            int left = (int) Math.Floor(cameraPosition / Tile.Width);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
            right = Math.Min(right, Width - 1);
            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = left; x <= right; ++x)
                {
                    // If there is a visible tile in that position
                    Tile tile = tiles[x, y];
                    
                    
                    Vector2 position;
                    if (tile.rain || tile.water)
                    {
                        position = new Vector2(x, y) * new Vector2(32, 32);
                        position += new Vector2(16, 32);
                    }
                    else
                    {
                        position = new Vector2(x, y) * Tile.Size;
                    }
                    tile.Draw(gameTime, spriteBatch, position);
                }
            }
        }

        private void ScrollCamera(Viewport viewport) {
            const float ViewMargin = 0.35f;

            // Calculate the edges of the screen
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraPosition + marginWidth;
            float marginRight = cameraPosition + viewport.Width - marginWidth;

            // Calculate how far to scroll when the player is near the edges of the screen.
            float cameraMovement = 0.0f; 
            if (Player.Position.X < marginLeft)
                cameraMovement = Player.Position.X - marginLeft;
            else if (Player.Position.X > marginRight)
                cameraMovement = Player.Position.X - marginRight;

            // Update the camera position, but prevent scrolling off the ends of the level
            float maxCameraPosition = Tile.Width * Width - viewport.Width;
            cameraPosition = MathHelper.Clamp(cameraPosition + cameraMovement, 0.0f, maxCameraPosition);
        }

        #endregion
    }
}