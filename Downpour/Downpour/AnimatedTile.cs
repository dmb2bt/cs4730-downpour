﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Downpour
{
    public class AnimatedTile : Tile
    {
        public Animation Animation
        {
            get { return animation; }
            set { animation = value; }
        }
        Animation animation;
        // Gets the index of the current frame in the animation.
        public int FrameIndex
        {
            get { return frameIndex; }
        }
        int frameIndex;

        // The amount of time in seconds that the current frame has been shown for.
        private float time;

        // Gets a texture origin at the bottom center of each frame.
        public Vector2 Origin
        {
            get { return new Vector2(Animation.FrameWidth / 2.0f, Animation.FrameHeight); }
        }

        public AnimatedTile(Animation animation, TileCollision collision, bool rain, bool water)
            : base(collision, rain, water)
        {
            this.animation = animation;
            frameIndex = 0;
            time = 0.0f;
        }

        public void ChangeAnimation(Animation animation)
        {
            // If this animation is already running, do not restart it.
            if (Animation == animation)
                return;

            // Start the new animation.
            this.animation = animation;
            this.frameIndex = 0;
            this.time = 0.0f;
        }


        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position)
        {
            time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (time > Animation.FrameTime)
            {
                time -= Animation.FrameTime;

                // Advance the frame index; looping or clamping as appropriate.
                if (Animation.IsLooping)
                {
                    frameIndex = (frameIndex + 1) % Animation.FrameCount;
                }
                else
                {
                    frameIndex = Math.Min(frameIndex + 1, Animation.FrameCount - 1);
                }
            }

            // Calculate the source rectangle of the current frame.
            Rectangle source = new Rectangle(FrameIndex * Animation.Texture.Height, 0, Animation.Texture.Height, Animation.Texture.Height);

            // Draw the current frame.
            spriteBatch.Draw(Animation.Texture, position, source, Color.White, 0.0f, Origin, 1.0f, SpriteEffects.None, 0.0f);
        }

    }
}
