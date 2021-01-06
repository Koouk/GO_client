using System;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;
using SFML.System;



namespace GOclient 
{
    class Button : Drawable
    {
        private Font _font;
        private Text _text;
        private RectangleShape _button;
        public bool Clicked { get; set; }

        public Button(int size, Vector2f position, string text)
        {

             _font = new Font(@"content\fonts\arial.ttf");
            _text = new Text(text, _font)
            {
                CharacterSize = (uint)size
            };
            float textWidth = _text.GetGlobalBounds().Width;
            float textHeight = _text.GetGlobalBounds().Height;
            float xOffset = _text.GetLocalBounds().Left;
            float yOffset = _text.GetLocalBounds().Top;
           // _text.Origin = new Vector2f(textWidth / 2f + xOffset, textHeight / 2f + yOffset);
            _text.Position = position;


            _button = new RectangleShape
            {
                Size = new Vector2f(textWidth * 1.1f, textHeight * 2),
                Position = position,
                FillColor = Color.Blue
            };

        }

        public bool PointInside(Vector2f point)
        {
            
            return _button.Position.X < point.X && point.X < _button.Position.X + _button.Size.X
                && _button.Position.Y < point.Y && point.Y < _button.Position.Y + _button.Size.Y;

        }


        public void Draw(RenderTarget target, RenderStates states)
        {

            target.Draw(_button);
            target.Draw(_text);
        }

        public void ChangeText(string t)
        {

            _text = new Text(t, _font);
        }
    }
}
