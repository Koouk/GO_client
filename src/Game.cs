﻿using System;
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
        private CircleShape _colorCircleC, _colorCircleM ;
        private Board _board;
        private Engine _engine;
        private PlayerColor _currentTurn;
        private Tuple<int,int> _lastMove;
        private int _inputStatus;
        public bool InGame { get; private set; }
        public Game(RenderWindow window, Networking net, Engine engine)
        {
            _window = window;
            _net = net;
            _engine = engine;
            _currentTurn = PlayerColor.black;
            _inputStatus = 0;
            InGame = true;

            _board = new Board(9, (float)_window.Size.Y, _window.Size.X * 0.75f);
            _passButton = new Button((int)(_window.Size.X/ 25f), new Vector2f(_window.Size.X * 0.875f, _window.Size.Y * 0.5f ), "PASS", Color.Black);
            _resignButton = new Button((int)(_window.Size.X / 25f), new Vector2f(_window.Size.X * 0.875f, _window.Size.Y * 0.7f), "RESIGN", Color.Black);
            _colorCircleC = new CircleShape()
            {
                Radius = _window.Size.Y * 0.05f,
                OutlineColor = Color.Black,
                OutlineThickness = 5f,
                FillColor = Color.White,
                Position = new Vector2f(_window.Size.X * 0.875f, _window.Size.Y * 0.2f)
            };

            _colorCircleM = new CircleShape()
            {
                Radius = _window.Size.Y * 0.05f,
                OutlineColor = Color.Black,
                OutlineThickness = 5f,
                Position = new Vector2f(_window.Size.X * 0.875f, _window.Size.Y * 0.8f)
            };


            _sidePanel = new RectangleShape
            {
                Size = new Vector2f(_window.Size.X * 0.25f, _window.Size.Y),
                Position = new Vector2f(_window.Size.X * 0.75f, 0),
                FillColor = new Color(111, 79, 40)
            };
        }

        public void Update()
        {
            if(_currentTurn == _engine.Color)
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
            if (_currentTurn == _engine.Color && _net.Status == ConnectionStatus.free)
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

            if (_currentTurn == PlayerColor.white)
                _colorCircleC.FillColor = Color.White;
            else
                _colorCircleC.FillColor = Color.Black;

            if (_engine.Color == PlayerColor.white)
                _colorCircleM.FillColor = Color.White;
            else
                _colorCircleM.FillColor = Color.Black;
            _window.Draw(_colorCircleC);
            _window.Draw(_colorCircleM);
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
                else if(rec.Type == "end" || rec.Type =="error")
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
                    var subs = data.Data.Split(' ');
                    var move = new Tuple<int, int>(int.Parse(subs[0]), int.Parse(subs[1]));
                    _board.Move(move, _currentTurn);
                    _currentTurn = (PlayerColor)((int)_currentTurn ^ 1);
                }
                else if (data.Type == "button")
                {
                    _currentTurn = (PlayerColor)((int)_currentTurn ^ 1);
                }
                else if(data.Type == "end" || data.Type == "error")
                {
                    InGame = false;
                }

            }
        }
    }
}
