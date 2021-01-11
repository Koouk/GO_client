using SFML.Graphics;
using SFML.System;

namespace GOclient
{
   

    class Lobby
    {

        private RenderWindow _window;
        private Networking _net;
        private Button _playButton;
        private Engine _engine;

        public Font font { get; private set; }
        public bool Status { get; private set; }
        public Lobby(RenderWindow window, Networking net, Engine engine)
        {
            _window = window;
            _net = net;
            _engine = engine;
            _playButton = new Button((int)(window.Size.X / 10), new SFML.System.Vector2f(window.Size.X/2, window.Size.Y/2), "PLAY", Color.Black)
            {
                Clicked = false
               
            };
            _playButton.ButtonShape.FillColor = new Color(1,1,1);
            _engine.GameStatus = false;


            font = new Font(@"content\fonts\arial.ttf");
        }

        public void HandleInput(Vector2f mousePosition)
        {
            if (!_playButton.Clicked && _playButton.PointInside(mousePosition))
            {
                _playButton.Clicked = true;
            }
        }
        public void Update()
        {

            if (_playButton.Clicked)
                FindGame();
        }

        public void Draw()
        {
            _window.Draw(new RectangleShape() { Size = (Vector2f)_window.Size, FillColor = new Color(220, 179, 92) });
            if(!_playButton.Clicked)
                _window.Draw(_playButton);
            else
            {
                _window.Draw(new Text("Waiting for a game...", font) {CharacterSize = 50 });
            }
        }

        private void FindGame()
        {

            if(_net.Status == ConnectionStatus.free && _net.IsConnected == true)
            {
                _net.Send("lobby", "ready");
            }
            else if(_net.Status == ConnectionStatus.sent)
            {
                _net.Status = ConnectionStatus.free;
                _net.Receive();
            }
            else if(_net.Status == ConnectionStatus.recieved)
            {
                var response = _net.GetData();
                _net.Status = ConnectionStatus.free;
                if (response.Type == "found" )
                {
                    if (response.Data == "white")
                        _engine.Color = PlayerColor.white;
                    else
                        _engine.Color = PlayerColor.black;
                    _engine.GameStatus = true;
                    _playButton.Clicked = false;
                    
                }
                else
                {
                    _net.Receive();
                }
            }
        }

    }
}
