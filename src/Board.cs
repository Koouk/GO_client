using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;

namespace GOclient
{
    class Board : Drawable
    {
        private uint _size;
        private float _windowHeight, _windowWidth, _wDistance, _hDistance;
        private PlayerColor[,] _gameBoard;
        private List<Tuple<int, int>> _blackStones, _whiteStones;


        public Board(uint size, float height, float width)
        {
            _gameBoard = new PlayerColor[size, size];
            for (int i = 0; i < _gameBoard.GetLength(0); i++)
            {
                for (int j = 0; j < _gameBoard.GetLength(1); j++)
                {
                    _gameBoard[i, j] = PlayerColor.none;
                }
            }

            _blackStones = new List<Tuple<int, int>>();
            _whiteStones = new List<Tuple<int, int>>();

            _size = size;
            _windowHeight = height;
            _windowWidth = width;
            _wDistance = _windowWidth / (_size + 1);
            _hDistance = _windowHeight / (_size + 1);

        }

        public Tuple<bool, Tuple<int, int>> TryPlaceStone(Vector2f mousePosition)
        {

            float x = mousePosition.X - _wDistance;
            float y = mousePosition.Y - _hDistance;
            float col = x / _wDistance;
            float row = y / _hDistance;
            int c = (int)Math.Round(col, MidpointRounding.AwayFromZero);
            int r = (int)Math.Round(row, MidpointRounding.AwayFromZero);

            var data = new Tuple<bool, Tuple<int, int>>(false, new Tuple<int, int>(c, r));
            if ((c - col) * (c - col) > 1f / 16)
                return data;
            if ((r - row) * (r - row) > 1f / 16)
                return data;
            if (r < 0 || c < 0 || r >= _size || c >= _size)
                return data;

            return new Tuple<bool, Tuple<int, int>>(true, new Tuple<int, int>(r, c));
        }


        public void Move(Tuple<int, int> move, PlayerColor color)
        {
            _gameBoard[move.Item1, move.Item2] = color;
            SolveCaptures(move, color);
            if (color == PlayerColor.black)
            {
                _blackStones.Add(move);
            }
            else
            {
                _whiteStones.Add(move);
            }

        }
        private void SolveCaptures(Tuple<int, int> move, PlayerColor color)
        {
            var directions = new List<(int, int)> { (0, 1), (0, -1), (1, 0), (-1, 0) };
            foreach (var dir in directions)
            {
                int row = move.Item1 + dir.Item1;
                int col = move.Item2 + dir.Item2;
                if (CheckIndex(row, col) && _gameBoard[row, col] != PlayerColor.none && _gameBoard[row, col] != color)
                {

                    Captures(row, col);
                }
            }

        }

        private void Captures(int row, int col)
        {
            var visited = new HashSet<int>();
            var captured = new List<Tuple<int, int>>();
            var toVisit = new Stack<Tuple<int, int>>();
            var color = _gameBoard[row, col];

            visited.Add((int)(row * _size + col));
            captured.Add(new Tuple<int, int>(row, col));
            toVisit.Push(new Tuple<int, int>(row, col));

            while (toVisit.Count > 0)
            {
                var point = toVisit.Pop();
                var directions = new List<(int, int)> { (0, 1), (0, -1), (1, 0), (-1, 0) };
                foreach (var dir in directions)
                {
                    int r = point.Item1 + dir.Item1;
                    int c = point.Item2 + dir.Item2;
                    if (CheckIndex(r, c) && !visited.Contains((int)((r * _size) + c)) && _gameBoard[r, c] == color)
                    {
                        toVisit.Push(new Tuple<int, int>(r, c));
                        visited.Add((int)(r * _size + c));
                        captured.Add(new Tuple<int, int>(r, c));
                    }
                    else if (CheckIndex(r, c) && _gameBoard[r, c] == PlayerColor.none)
                        return;
                }
            }

            foreach (var point in captured)
            {
                _gameBoard[point.Item1, point.Item2] = PlayerColor.none;
                if (color == PlayerColor.black)
                {

                    _blackStones.Remove(point);
                }
                else if (color == PlayerColor.white)
                {
                    _whiteStones.Contains(point);
                }
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            DrawBoard(target, states);
            DrawStones(target, states);

        }
        private void DrawBoard(RenderTarget target, RenderStates states)
        {
            var background = new RectangleShape
            {
                Size = new Vector2f(_windowWidth, _windowHeight),
                FillColor = Color.White
            };
            target.Draw(background);

            var horizontalLine = new RectangleShape
            {
                FillColor = Color.Black,
                Size = new Vector2f((_size - 1) * _wDistance, 5f)
            };
            var verticalLine = new RectangleShape
            {
                FillColor = Color.Black,
                Size = new Vector2f(5f, (_size - 1) * _hDistance)
            };
            for (int i = 0; i < _size; i++)
            {

                horizontalLine.Position = new Vector2f(_wDistance, _hDistance + _hDistance * i);
                verticalLine.Position = new Vector2f(_wDistance + _wDistance * i, _hDistance);
                target.Draw(horizontalLine);
                target.Draw(verticalLine);

            }
        }

        private void DrawStones(RenderTarget target, RenderStates states)
        {
            var stone = new CircleShape
            {
                Radius = _wDistance / 4f,
                OutlineColor = Color.Black,
                OutlineThickness = 5f,
                FillColor = Color.White
            };

            foreach (var pos in _whiteStones)
            {
                stone.Position = new Vector2f(_wDistance * 0.75f + _wDistance * pos.Item2, _hDistance * 0.75f + _hDistance * pos.Item1);
                target.Draw(stone);
            }
            stone.FillColor = Color.Black;
            foreach (var pos in _blackStones)
            {
                stone.Position = new Vector2f(_wDistance * 0.75f + _wDistance * pos.Item2, _hDistance * 0.75f + _hDistance * pos.Item1);
                target.Draw(stone);
            }
        }

        private bool CheckIndex(int row, int col)
        {
            return row >= 0 && row < _size && col >= 0 && col < _size;

        }
    }
}
