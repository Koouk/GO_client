using System;
using System.Collections.Generic;
using System.Text;
using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace GOclient
{


    class Engine
    {

        private RenderWindow _window;
        private Networking _net;

        private Game _game;
        private Lobby _lobby;
        private Vector2f _currentMousePosition;

        private bool _gameStatus;
        private void Initialize()
        {

            _window = new RenderWindow(new VideoMode(500, 500), "SFML.NET");
            Networking _net = new Networking("192.168.1.12", 1024);
            _net.Connect();

            _game = new Game(_window,_net);
            _lobby = new Lobby(_window, _net);

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
                _gameStatus = _lobby.Status;
                _window.DispatchEvents();
                _window.Clear();

                if (!_gameStatus)
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
