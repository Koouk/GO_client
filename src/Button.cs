using SFML.Graphics;
using SFML.System;



namespace GOclient 
{
    class Button : Drawable
    {
        private Font _font;
        private Text _text;
        public RectangleShape ButtonShape { get; set; }
        public bool Clicked { get; set; }

        public Button(int size, Vector2f position, string text, Color color)
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
            
            _text.Position = new Vector2f(position.X - textWidth * 1.5f / 2, position.Y - textHeight);

            Clicked = false;
            ButtonShape = new RectangleShape
            {
                Size = new Vector2f(textWidth * 1.5f, textHeight * 2f),
                Position = new Vector2f(position.X - textWidth * 1.5f / 2, position.Y - textHeight),
                FillColor = color
            };

            _text.Position = new Vector2f(ButtonShape.Position.X + textWidth / 4f - xOffset / 2, ButtonShape.Position.Y + textHeight / 4f - yOffset / 2);

        }

        public bool PointInside(Vector2f point)
        {
            
            return ButtonShape.Position.X < point.X && point.X < ButtonShape.Position.X + ButtonShape.Size.X
                && ButtonShape.Position.Y < point.Y && point.Y < ButtonShape.Position.Y + ButtonShape.Size.Y;

        }


        public void Draw(RenderTarget target, RenderStates states)
        {

            target.Draw(ButtonShape);
            target.Draw(_text);
        }

        public void ChangeText(string t)
        {

            _text = new Text(t, _font);
        }
    }
}
