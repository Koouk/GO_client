using System;
using System.Collections.Generic;
using System.Text;
using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace GOclient
{
    class Board : Drawable
    {
        Font font;
        Text text;
        public Board()
        {
             font = new Font("C:/Windows/Fonts/arial.ttf");
             text = new Text("Hello World!", font);
        }
        public void Draw(RenderTarget target, RenderStates states)
        {
            
            text.CharacterSize = 40;
            float textWidth = text.GetLocalBounds().Width;
            float textHeight = text.GetLocalBounds().Height;
            float xOffset = text.GetLocalBounds().Left;
            float yOffset = text.GetLocalBounds().Top;
            text.Origin = new Vector2f(textWidth / 2f + xOffset, textHeight / 2f + yOffset);
            text.Position = new Vector2f(target.Size.X / 2f, target.Size.Y / 2f);
            target.Draw(text);
        }
    }
}
