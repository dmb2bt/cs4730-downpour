#region File Description
//-----------------------------------------------------------------------------
// Tile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Downpour
{
    // Controls the collision detection and response behavior of a tile.
    public enum TileCollision
    {
        // A passable tile is one which does not hinder player motion at all.
        Passable = 0,

        // An impassable tile is one which does not allow the player to move through
        // it at all. It is completely solid.
        Impassable = 1,

        // A platform tile is one which behaves like a passable tile except when the
        // player is above it. A player can jump up through a platform as well as move
        // past it to the left and right, but can not fall down through the top of it.
        Platform = 2,
    }

    // Stores the appearance and collision behavior of a tile.
    public struct Tile
    {
        public Texture2D Texture;
        public TileCollision Collision;
        public bool rain;

        public const int Width = 32;
        public const int Height = 32;

        public static readonly Vector2 Size = new Vector2(Width, Height);

        // Constructs a new tile.
        public Tile(Texture2D texture, TileCollision collision, bool rain)
        {
            Texture = texture;
            Collision = collision;
            this.rain = rain;
        }
    }
}