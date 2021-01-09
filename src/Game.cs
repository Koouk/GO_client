using System;
using SFML.Graphics;
using SFML.System;

namespace GOclient
{
    class Game
    {
        private RenderWindow _window;
        private Networking _net;
        private Button _passButton, _resignButton;
        private RectangleShape _sidePanel;
        private Board _board;
        private PlayerColor _color;
        private PlayerColor _currentTurn;
        private Tuple<int,int> _lastMove;
        private int _inputStatus;
        public bool InGame { get; private set; }
        public Game(RenderWindow window, Networking net, PlayerColor Pcolor)
        {
            _window = window;
            _net = net;
            _color = Pcolor;
            _currentTurn = PlayerColor.white;
            _inputStatus = 0;
            InGame = true;

            _board = new Board(9, (float)_window.Size.Y, _window.Size.X * 0.75f);
            _passButton = new Button(40, new Vector2f(_window.Size.X * 0.80f, _window.Size.Y * 0.33f ), "PASS" );
            _resignButton = new Button(40, new Vector2f(_window.Size.X * 0.80f, _window.Size.Y * 0.66f), "RESIGN");

            _sidePanel = new RectangleShape
            {
                Size = new Vector2f(_window.Size.X * 0.25f, _window.Size.Y),
                Position = new Vector2f(_window.Size.X * 0.75f, 0),
                FillColor = Color.Yellow
            };
        }

        public void Update()
        {
            if(_currentTurn == _color)
            {
                MakeMove();
            }
            else
            {
                WaitForOpponent();
            }


        }

        public void HandleInput(Vector2f mousePosition)
        {
            if (_currentTurn == _color && _net.Status == ConnectionStatus.free)
            {
                if (_passButton.PointInside(mousePosition))
                    _inputStatus = 1;
                else if (_resignButton.PointInside(mousePosition))
                    _inputStatus = 2;
                else
                {
                    var res = _board.TryPlaceStone(mousePosition);
                    if (res.Item1)
                    {
                        _inputStatus = 3;
                        _lastMove = res.Item2;
                    }
                        
                }
            }
        }

        public void Draw()
        {
            DrawPanel();
            _window.Draw(_board);
        }

        private void DrawPanel()
        {

           
            _window.Draw(_sidePanel);
            _window.Draw(_passButton);
            _window.Draw(_resignButton);
        }

        private void MakeMove()
        {
            if(_net.IsConnected && _net.Status == ConnectionStatus.free)
            {
                if(_inputStatus == 1)
                {
                    _net.Send("button", "pass");
                }
                else if(_inputStatus == 2)
                {
                    _net.Send("button", "resign");
                }
                else if(_inputStatus == 3)
                {
                    var data = _lastMove.Item1.ToString() + " " + _lastMove.Item2.ToString();
                    _net.Send("move", data);

                }
            }
            else if (_net.Status == ConnectionStatus.sent)
            {
                _net.Receive();

            }
            else if(_net.Status == ConnectionStatus.recieved)
            {
                var rec = _net.GetData();
                if (rec.Type == "move" || rec.Type == "button")
                {
                    if(rec.Data == "accepted")
                    {
                        
                        if (_inputStatus == 3)
                            _board.Move(_lastMove, _currentTurn);
                        _currentTurn = (PlayerColor)((int)_currentTurn ^ 1);
                    }
                    
                }
                else if(rec.Type == "end")
                {
                    InGame = false;
                }
                _inputStatus = 0;
            }

        }

        private void WaitForOpponent()
        {
            if (_net.IsConnected && _net.Status == ConnectionStatus.free)
            {
                _net.Receive();
            }
            else if(_net.Status == ConnectionStatus.recieved)
            {
                var data = _net.GetData();
                if(data.Type == "move")
                {
                    var subs = data.Type.Split(' ');
                    var move = new Tuple<int, int>(int.Parse(subs[0]), int.Parse(subs[1]));
                    _board.Move(move, _currentTurn);
                    _currentTurn = (PlayerColor)((int)_currentTurn ^ 1);
                }
                else if (data.Type == "button")
                {
                    _currentTurn = (PlayerColor)((int)_currentTurn ^ 1);
                }
                else if(data.Type == "end")
                {
                    InGame = false;
                }

            }
        }
    }
}
