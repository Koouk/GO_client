using System;
using System.Collections.Generic;
using System.Text;
using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace GOclient
{
    class Game
    {
        private RenderWindow _window;
        private Networking _net;
        Button tmp;
        public Game(RenderWindow window, Networking net)
        {
            _window = window;
            _net = net;
            tmp = new Button(50, (SFML.System.Vector2f)_window.Size /2, "INGAME" );
            

        }

        public void Update()
        {



        }

        public void HandleInput(Vector2f mousePosition)
        {
            
        }
        public void Draw()
        {
            
            _window.Draw(tmp);
        }
    }
}
