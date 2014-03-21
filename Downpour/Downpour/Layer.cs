using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Downpour
{
    class Layer
    {
        public Texture2D Texture { get; set; }
        public float ScrollRate { get; private set; }

        public Layer(ContentManager content, string basePath, float scrollRate)
        {
            Texture = content.Load<Texture2D>(basePath);
            ScrollRate = scrollRate;
        }

        public void Draw(SpriteBatch spriteBatch, float cameraPosition)
        {
            // Assume each segment is the same width
            int segmentWidth = Texture.Width;

            // Calculate which segments to draw and how much to offset them
            float x = cameraPosition * ScrollRate;
            int leftSegment = (int)Math.Floor(x / segmentWidth);
            int rightSegment = leftSegment + 1;
            x = (x / segmentWidth - leftSegment) * -segmentWidth;

            spriteBatch.Draw(Texture, new Vector2(x, 0.0f), Color.White);
            spriteBatch.Draw(Texture, new Vector2(x + segmentWidth, 0.0f), Color.White);
        }
    }
}
