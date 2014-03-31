#region File Description
//-----------------------------------------------------------------------------
// Animation.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Downpour
{
    // Represents an animated texture.
    // Currently, this class assumes that each frame of animation is
    // as wide as each animation is tall. The number of frames in the
    // animation are inferred from this.
    class Animation
    {
        // All frames in the animation arranged horizontally.
        public Texture2D Texture
        {
            get { return texture; }
        }
        Texture2D texture;

        // Duration of time to show each frame.
        public float FrameTime
        {
            get { return frameTime; }
            set { frameTime = value; }
        }
        float frameTime;

        // When the end of the animation is reached, should it
        // continue playing from the beginning?
        public bool IsLooping
        {
            get { return isLooping; }
        }
        bool isLooping;

        // Gets the number of frames in the animation.
        public int FrameCount
        {
            get { return Texture.Width / FrameWidth; }
        }

        // Gets the width of a frame in the animation.
        public int FrameWidth
        {
            // Assume square frames.
            get { return Texture.Height; }
        }

        // Gets the height of a frame in the animation.
        public int FrameHeight
        {
            get { return Texture.Height; }
        }

        // Constructs a new animation.
        public Animation(Texture2D texture, float frameTime, bool isLooping)
        {
            this.texture = texture;
            this.frameTime = frameTime;
            this.isLooping = isLooping;
        }
    }
}