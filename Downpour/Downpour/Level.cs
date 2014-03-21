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
using System.IO;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;


namespace Downpour
{
    // A uniform grid of tiles.
    // The level owns the player and controls the game's win and lose
    // conditions as well as scoring.
    class Level : IDisposable
    {
        public static ContentManager LibContent;

        // Physical structure of the level.
        private Tile[,] tiles;
        private Layer[] layers;
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 1;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        static Player player;

        private List<FirePiece> firePieces = new List<FirePiece>();
        private List<SpeedFruit> powerups = new List<SpeedFruit>();
        
        // Key locations in the level.
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);
        
        // Rain levels (1-3)
        int lastRain;

        // Level game state.
        private float cameraPosition;
        private Random random = new Random(354668); // Arbitrary, but constant seed

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        // Level content.
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;


        #region Loading

        
        // Constructs a new level.
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            // All levels begin with a rain level of 2
            lastRain = 2;

            // Load background layer textures. All levels must
            // use the same backgrounds and only use the left-most part of them.
            // Currently, the background has two layers, blank white behind rain2
            layers = new Layer[2];
            layers[0] = new Layer(Content, "clear", 1.0f);
            layers[1] = new Layer(Content, "rain2", 1.0f);

            // Initialize the player with 2000 life if this is the first level
            if (player == null)
            {
                player = new Player(this, new Vector2(), 2000);
            }
            
            LoadTiles(fileStream);
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

            using (StreamReader reader = new StreamReader(fileStream))
            {
                String levelDataString = reader.ReadToEnd();

                LevelData.RootObject levelData = JsonConvert.DeserializeObject<LevelData.RootObject>(levelDataString);

                width = levelData.width;
                height = levelData.height;
                foreach (Downpour.LevelData.Layer layer in levelData.layers){
                    tileNums = layer.data;
                }

                System.Diagnostics.Debug.WriteLine("width is: " + width);
                System.Diagnostics.Debug.WriteLine("height is: " + height);

            }

            // Allocate the tile grid.
            tiles = new Tile[width, height];

            // Counter to go through the 
            int count = 0;

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    int tileType = tileNums[count++];

                    tiles[x, y] = LoadTile(tileType, x, y);
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
        private Tile LoadTile(int tileType, int x, int y)
        {
            System.Diagnostics.Debug.WriteLine("tile num: " + tileType);
            switch (tileType)
            {
                // Air without rain
                case 0:
                    return LoadClearTile();

                // Impassable block (standard platform) without rain
                case 1:
                    return LoadTile("BlockA0", TileCollision.Impassable, false);

                // Impassable block (standard platform) with rain
                case 2:
                    return LoadTile("RainBlock", TileCollision.Impassable, false);
                
                // Part of fire -- Not currently used 
                case 3:
                    /*
                     * DAVID!! DAVIDDD!! This will load the tile for the part of the fire, 
                     * but not yet because it's not created yet. So right now it's loading a
                     * blank tile
                     */
                    return LoadFirePieceTile(x, y);
                    //return LoadTile("FirePiece", TileCollision.Passable, false);

                // Power-up -- Not currently used 
                case 4:
                    /*
                     * DAVID!! DAVIDDD!! This will load the tile for the power-up, 
                     * but not yet because it's not created yet. So right now it's loading a
                     * blank tile
                     */
                    //return LoadTile("FirePart", TileCollision.Passable, false);
                    //return LoadClearTile();
                    return LoadFruitTile(x, y);

                // Exit
                case 5:
                    return LoadExitTile(x, y);

                // Air with rain
                case 6:
                    return LoadRainTile();

                // Player start point without rain
                case 7:
                    return LoadStartTile(x, y);

                // Floating platform--Not currently used
                case 8:
                    return LoadTile("Platform", TileCollision.Platform, false);

                // Platform block--Not currently used
                case 9:
                    return LoadTile("BlockA0", TileCollision.Platform, false);

                // Passable block--Not currently used
                case 10:
                    return LoadTile("BlockA0", TileCollision.Passable, false);
                
                // Player start point with rain
                case 11:
                    return LoadStartTileRain(x, y);

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        // Air with rain
        private Tile LoadRainTile()
        {
            return LoadTile("rain2", TileCollision.Passable, true);
        }

        // Air without rain
        private Tile LoadClearTile()
        {
            return LoadTile("rain0", TileCollision.Passable, false);
        }

        private Tile LoadTile(string name, TileCollision collision, bool rain)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision, rain);
        }

        // Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        private Tile LoadStartTile(int x, int y)
        {
            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));

            // If this isn't the first level, initialize the player with the life he had left
            int tempLife = 2000;
            if (player != null) tempLife = player.Life;
            player = new Player(this, start, tempLife);

            return LoadClearTile();
        }

        // Redundant method that does the same thing except starts the player in the rain
        private Tile LoadStartTileRain(int x, int y)
        {
            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));

            // If this isn't the first level, initialize the player with the life he had left
            int tempLife;
            if (player != null) tempLife = player.Life;
            else tempLife = 2000;
            player = new Player(this, start, tempLife);

            return LoadRainTile();
        }

        // Remembers the location of the level's exit.
        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable, false);
        }

        private Tile LoadFirePieceTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            firePieces.Add(new FirePiece(this, new Vector2(position.X, position.Y)));

            return LoadTile("rain0", TileCollision.Passable, false);
        }

        private Tile LoadFruitTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            powerups.Add(new SpeedFruit(this, new Vector2(position.X, position.Y)));

            return LoadRainTile();
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

        // Gets whether a tile a tile will damage the player
        public bool GetRain(int x, int y)
        {
            // Can't be damaged by tiles off the screen.
            if (x < 0 || x >= Width) return false;
            if (y < 0 || y >= Height) return false;

            return tiles[x, y].rain;
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
        public void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState)
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
                SpeedFruit powerup = powerups[i];

                powerup.Update(gameTime);

                if (powerup.BoundingCircle.Intersects(Player.BoundingRectangle))
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
                Player.OnReachedExit();
                reachedExit = true;
            }
            
        }

        private void OnFirePieceCollected(FirePiece firepiece, Player collectedBy)
        {
            // TODO: Need to show piece collected
            firepiece.OnCollected(collectedBy);
        }

        private void OnPowerUpCollected(SpeedFruit powerup, Player collectedBy)
        {
            // TODO: Need to do some animation or sound for pick up
            collectedBy.incrementSpeedMultiplier();
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
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            // Update the background to match the rain level
            if (lastRain != player.rainLevel)
            {
               // This should probably not be loading content in the Draw method...
                layers[1].Texture = Content.Load<Texture2D>("rain" + player.rainLevel + ".png");
            }
            lastRain = player.rainLevel;
            
            // Draw background up to entity layer
            for (int i = 0; i <= EntityLayer; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, 0.0f, 0.0f);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, cameraTransform);
            // Draw tiles
            DrawTiles(spriteBatch);

            // Draw player
            Player.Draw(gameTime, spriteBatch);

            // Draw firepieces
            foreach (FirePiece piece in firePieces)
                piece.Draw(gameTime, spriteBatch);

            foreach (SpeedFruit powerup in powerups)
                powerup.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin();
            // Draw background past entity layer
            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);

            // Draw life bar
            int life = player.Life;
            Rectangle lifeBar = new Rectangle(20, 20, life / 4, 20);
            spriteBatch.Draw(layers[0].Texture, lifeBar, Color.Red);

            spriteBatch.End();
        }

        public void DrawTiles(SpriteBatch spriteBatch)
        {
            // Calculate the visible range of tiles
            int left = (int) Math.Floor(cameraPosition / Tile.Width);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
            right = Math.Min(right, Width - 1);
            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = left; x < right; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
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