using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.Input;

namespace OBSGreenScreen
{
    public class Mouse
    {
        private List<(MouseButton button, bool isDown)> _buttonStates = new();
        private int _x;
        private int _y;

        public Mouse()
        {
            var buttons = Enum.GetValues<MouseButton>();

            foreach (var button in buttons)
            {
                _buttonStates.Add((button, false));
            }
        }

        public MouseState GetState()
        {
            return new MouseState(_buttonStates.ToArray(), _x, _y);
        }

        public void ButtonDown(IMouse mouse, MouseButton button)
        {
            for (var i = 0; i < _buttonStates.Count; i++)
            {
                if (_buttonStates[i].button != button) continue;

                _buttonStates[i] = (_buttonStates[i].button, true);
                return;
            }
        }

        public void ButtonUp(IMouse mouse, MouseButton button)
        {
            for (var i = 0; i < _buttonStates.Count; i++)
            {
                if (_buttonStates[i].button != button) continue;

                _buttonStates[i] = (_buttonStates[i].button, false);
                return;
            }
        }

        public void MouseMove(IMouse mouse, Vector2 position)
        {
            _x = (int)position.X;
            _y = (int)position.Y;
        }
    }
}
