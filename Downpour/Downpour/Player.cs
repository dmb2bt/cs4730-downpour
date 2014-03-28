#region File Description
//-----------------------------------------------------------------------------
// Player.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Downpour 
{

    public class Player
    {
        // Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;

        // Current level
        public Level Level
        {
            get { return level; }
        }
        Level level;

        public bool IsAlive
        {
            get { return isAlive; }
        }
        bool isAlive;

        // Physics state
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        private float previousBottom;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;

        public float speedMultiplier = 1.0f;
        public float speedMultiplierStep = 1.0f;

        // Constants for controlling horizontal movement
        public float MoveAcceleration = 11500.0f;
        public float MaxMoveSpeed = 500.0f;
        public float GroundDragFactor = 0.60f;
        public float AirDragFactor = 0.50f;

        // Constants for controlling vertical movement
        public float MaxJumpTime = 3.0f;
        public float JumpLaunchVelocity = -1100.0f;
        public float GravityAcceleration = 3000.0f;
        public float MaxFallSpeed = 600.0f;
        public float JumpControlPower = 0.05f;

        // Gamepad input configuration
        private float MoveStickScale = 1.0f;
        private const Buttons JumpButton = Buttons.A;

        private const bool DEBUG_NO_RAIN_DAMAGE = false;

        // boolean for inverting keys 
        private bool controlsInverted = false;

        // Default starting life is set in Level.cs at 2000
        public int Life
        {
            get { return life; }
            set { life = value; }
        }
        private int life;

        // Creates an Umbrella shield that decreases before player life
        public int UmbrellaLife {
            get { return umbrellaLife; }
        }
        private int umbrellaLife = 0;
        private const int UMBRELLA_LIFE_MAX = 100;

        // Rain values
        bool rainedOn;
        public int rainLevel;

        // Counter to update rain level periodically.
        int count;

        // Gets whether or not the player's feet are on the ground.
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        // Current user movement input.
        private float movement;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        // Rectangle which bounds this player in world space.
        private Rectangle localBounds;
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        // Constructs a new player.
        public Player(Level level, Vector2 position, int life)
        {
            this.level = level;
            this.life = life;
            this.rainedOn = false;
            LoadContent();
            this.rainLevel = 2;
            Reset(position);
            count = 0;
        }

        // Loads the player sprite sheet and sounds.
        public void LoadContent()
        {
            // Load animated textures. (.1f is frame time)
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Idle"), 0.1f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Run"), 0.1f, true);

            // Calculate bounds within texture size.
            int width = (int)(idleAnimation.FrameWidth * 0.35);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.6);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

        }

        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState)
        {
            GetInput(keyboardState, gamePadState);
            ApplyPhysics(gameTime);
            changeRain(gameTime);

            // Choose the animation
            if (IsAlive)
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                {
                    sprite.PlayAnimation(runAnimation);
                }
                else
                {
                    sprite.PlayAnimation(idleAnimation);
                }
            }

            // Clear input.
            movement = 0.0f;
            isJumping = false;
        }

        // Updates the rain level every 50 updates.
        public void changeRain(GameTime gameTime)
        {
            if (count > 50)
            {
                count = 0;

                // 50% chance rain goes up/down, staying within 1-3 range
                Random random = new Random();
                int up = random.Next(2);
                if (up == 1 && rainLevel != 3) rainLevel++;
                else if (rainLevel != 1) rainLevel--;
            }
            else count++;
        }

        // Gets the current input from keyboard/controller
        // Arrow keys, thumbstick, or WASD should work.
        // Spacebar currently both tries to jump and restarts level.
        private void GetInput(KeyboardState keyboardState, GamePadState gamePadState)
        {
            // Get analog horizontal movement from controller.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // If any digital horizontal movement input is found, override the analog movement.
            // Move left
            if ((gamePadState.ThumbSticks.Left.X < 0 && !controlsInverted) ||
                (gamePadState.ThumbSticks.Left.X > 0 && controlsInverted) ||
                (keyboardState.IsKeyDown(Keys.Left) && !controlsInverted) ||
                (keyboardState.IsKeyDown(Keys.Right) && controlsInverted) ||
                keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0f * speedMultiplier;
            }

            // Move right
            else if ((gamePadState.ThumbSticks.Left.X > 0 && !controlsInverted) ||
                     (gamePadState.ThumbSticks.Left.X < 0 && controlsInverted) ||
                     (keyboardState.IsKeyDown(Keys.Right) && !controlsInverted) ||
                     (keyboardState.IsKeyDown(Keys.Left) && controlsInverted) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f * speedMultiplier;
            }

            // Check if the player wants to jump.
            if (!isJumping && !wasJumping && !pressed)
            {
                isJumping =
                    gamePadState.IsButtonDown(JumpButton) ||
                    keyboardState.IsKeyDown(Keys.Space) ||
                    keyboardState.IsKeyDown(Keys.Up) ||
                    keyboardState.IsKeyDown(Keys.W);
                pressed = isJumping;

            }
            else
            {
                pressed = gamePadState.IsButtonDown(JumpButton) ||
                    keyboardState.IsKeyDown(Keys.Space) ||
                    keyboardState.IsKeyDown(Keys.Up) ||
                    keyboardState.IsKeyDown(Keys.W);
            }

        }
        bool pressed;

        // Bug: horizontal collisions zero out vertical movement?
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, JumpLaunchVelocity, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }

        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping || wasJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    if (jumpTime == 0.0f)

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * 3 * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }

        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision & rain collision.
            isOnGround = false;
            this.rainedOn = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // Check for rain.
                    if (Level.GetRain(x, y)) rainedOn = true;

                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        isJumping = false;
                        wasJumping = false;
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            // Take damage from rain.
            if (rainedOn && !DEBUG_NO_RAIN_DAMAGE)
            {
                if (umbrellaLife > 0)
                {
                    umbrellaLife -= this.rainLevel;
                }
                else
                {
                    if (umbrellaLife < 0)
                    {
                        this.life += umbrellaLife;
                        umbrellaLife = 0;
                    }
                    this.life -= this.rainLevel;
                    if (this.life <= 0)
                    {
                        OnKilled();
                    }
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }

        public void OnKilled()
        {
            isAlive = false;
        }

        public void OnReachedExit()
        {
            // Should the player freeze here?
            // Right now they can continue to move after finishing the level.
        }

        public void incrementSpeedMultiplier()
        {
            speedMultiplier += speedMultiplierStep;
        }

        public void invertControls()
        {
            controlsInverted = !controlsInverted;
        }

        public void setUmbrella()
        {
            umbrellaLife = UMBRELLA_LIFE_MAX;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Determine orientation of sprite.
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

            // Draw that sprite.
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }

    }
}