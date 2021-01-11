using SFML.Window;
using SFML.Graphics;
using SFML.System;
using System.Diagnostics;

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
        public bool GameStatus { get; set; }

        private RenderWindow _window;
        private Networking _net;

        private Game _game;
        private Lobby _lobby;
        private Vector2f _currentMousePosition;

        private bool temp = false; 
        DataTemplate _finalResult;
        private void Initialize()
        {
            var size = VideoMode.DesktopMode;

            _window = new RenderWindow(new VideoMode(size.Width / 2 , size.Width * 3 / 8), "SFML.NET");
            _net = new Networking("192.168.1.12", 10024);
            _net.Connect();

            _game = new Game(_window,_net, this);
            _lobby = new Lobby(_window, _net, this);


            GameStatus = false;
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
                        if (!GameStatus)
                            _lobby.HandleInput(_currentMousePosition);
                        else
                            _game.HandleInput(_currentMousePosition);
                    }

            };

        }

        private void ResultsScreen()
        {
            if (_net.Status == ConnectionStatus.free && temp == false)
            {
                
                _net.Send("request", "results");

            }
            else if (_net.Status == ConnectionStatus.sent)
            {
                _net.Receive();
            }
            else if (_net.Status == ConnectionStatus.recieved)
            {
                _finalResult = _net.GetData();
                temp = true;
            }

            if (temp)
            {
                Debug.WriteLine(" Result: " + _finalResult.Type + " " + _finalResult.Data);
                _window.Draw(new RectangleShape() { Size = (Vector2f)_window.Size, FillColor = new Color(230, 255, 255) });
                if (_finalResult.Type == "error")
                {
                    _window.Draw(new Text("Connection error... Check logs or contact administrator to find issue.", _lobby.font)
                    { CharacterSize = 10, FillColor = SFML.Graphics.Color.White });

                }
                else if(_finalResult.Type == "victory")
                {
                    var subs = _finalResult.Data.Split(' ');
                    _window.Draw(new Text("VICTORY!", _lobby.font)
                    { CharacterSize = _window.Size.Y / 5, FillColor = new Color(255,234,153), Position = new Vector2f(_window.Size.X / 2 - 50, _window.Size.Y / 2 -50) });
                    _window.Draw(new Text("Black player points: !" + subs[0], _lobby.font)
                    { CharacterSize = _window.Size.Y / 15, FillColor = new Color(255, 234, 153), Position = new Vector2f(_window.Size.X / 20, _window.Size.Y / 20 ) });

                    _window.Draw(new Text("White player points: !" + subs[1], _lobby.font)
                    { CharacterSize = _window.Size.Y / 15, FillColor = new Color(255, 234, 153), Position = new Vector2f(_window.Size.X * 3  / 20, _window.Size.Y * 3  / 20) });
                }
                else if(_finalResult.Type == "defeat")
                {
                    var subs = _finalResult.Data.Split(' ');
                    _window.Draw(new Text("DEFEAT!", _lobby.font)
                    { CharacterSize = _window.Size.Y / 5, FillColor = new Color(153, 0, 0), Position = new Vector2f(_window.Size.X / 2 - 50, _window.Size.Y / 2 - 50) });
                    _window.Draw(new Text("Black player points: !" + subs[0], _lobby.font)
                    { CharacterSize = _window.Size.Y /15, FillColor = new Color(255, 234, 153), Position = new Vector2f(_window.Size.X / 20, _window.Size.Y / 20) });

                    _window.Draw(new Text("White player points: !" + subs[1], _lobby.font)
                    { CharacterSize = _window.Size.Y / 15, FillColor = new Color(255, 234, 153), Position = new Vector2f(_window.Size.X * 3 / 20, _window.Size.Y * 3 / 20) });
                }
                else if (_finalResult.Type == "draw")
                {
                    var subs = _finalResult.Data.Split(' ');
                    _window.Draw(new Text("DRAW!", _lobby.font)
                    { CharacterSize = _window.Size.Y / 5, FillColor = new Color(153, 0, 0), Position = new Vector2f(_window.Size.X / 2 - 50, _window.Size.Y / 2 - 50) });
                    _window.Draw(new Text("Player points: !" + subs[0], _lobby.font)
                    { CharacterSize = _window.Size.Y / 15, FillColor = new Color(255, 234, 153), Position = new Vector2f(_window.Size.X / 20, _window.Size.Y / 20) });


                }

            }
        }

        public void Run()
        {
            Initialize();

            while (_window.IsOpen)
            {

                _window.DispatchEvents();
                _window.Clear();


                if (_net.Error == -1)
                {
                    _window.Draw(new Text("Connection error...", _lobby.font)
                    { CharacterSize = 10, Position = new Vector2f(_window.Size.X / 2, _window.Size.Y / 2) , FillColor = SFML.Graphics.Color.White});
                    _window.Display();
                    continue;
                }

                if (!_game.InGame)
                {
                  
                    //pobierz wyniki, narysuj, zakoncz polaczenie i wyjdz
                    ResultsScreen();
                    _window.Display();
                    continue;
                }

                if (!GameStatus)
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
            _net.Close();
        }
    }

    
}
