using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace WindowsGame2
{
    public struct Square
    {
        public int x;
        public int y;
        public int colourValue;
        public static Texture2D blocks;
        public bool hasBlock;
        public Square(int x1, int y1)
        {
            x = x1;
            y = y1;
            hasBlock = false;
            colourValue = 0;
        }
        public void Update(int colVal)
        {
            colourValue = colVal;
            if (colVal != 0)
                hasBlock = true;
            else
                hasBlock = false;
        }
    }
}
