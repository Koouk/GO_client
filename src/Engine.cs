using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace GOclient
{
    public enum PlayerColor
    {
        
        white,
        black,
        none
    }

    class Engine
    {
        public PlayerColor Color { get; set; }

        private RenderWindow _window;
        private Networking _net;

        private Game _game;
        private Lobby _lobby;
        private Vector2f _currentMousePosition;

        private bool _gameStatus;
        private void Initialize()
        {

            _window = new RenderWindow(new VideoMode(1600, 1200), "SFML.NET");
            _net = new Networking("192.168.1.12", 1024);
            _net.Connect();

            _game = new Game(_window,_net, Color);
            _lobby = new Lobby(_window, _net, this);

            _gameStatus = false;
            InitializePollEvents();
        }

        private void InitializePollEvents()
        {
            _window.Closed += (sender, e) => { _window.Close(); };

            _window.KeyPressed +=
                (sender, e) =>
                {
                    Window window = (Window)sender;
                    if (e.Code == Keyboard.Key.Escape)
                    {
                        window.Close();
                    }
                    
                };

            _window.MouseButtonPressed += 
                (sender, e) =>
                {
                Window window = (Window)sender;
                if (e.Button == Mouse.Button.Left)
                {
                        _currentMousePosition = _window.MapPixelToCoords(Mouse.GetPosition(_window));
                        if (!_gameStatus)
                            _lobby.HandleInput(_currentMousePosition);
                        else
                            _game.HandleInput(_currentMousePosition);
                    }

            };

        }

        public void Run()
        {
            Initialize();

            while (_window.IsOpen)
            {
                if(!_game.InGame)
                {
                    //pobierz wyniki, narysuj, zakoncz polaczenie i wyjdz
                }
                if(_net.Error == -1)
                {
                    _window.Close();
                }

                _gameStatus = _lobby.Status;
                _window.DispatchEvents();
                _window.Clear();
                if (!_gameStatus )
                {
                    _lobby.Update();
                    _lobby.Draw();
                }
                else
                {
                    _game.Update();
                    _game.Draw();
                }
                _window.Display();
            }
        }
    }
}
