using System;
using System.Collections.Generic;
using Silk.NET.Input;

namespace OBSGreenScreen
{
    public class Keyboard
    {
        private List<(Key key, bool isDown)> _keyStates = new ();

        public Keyboard()
        {
            var keys = Enum.GetValues<Key>();

            foreach (var key in keys)
            {
                _keyStates.Add((key, false));
            }
        }

        public KeyboardState GetState()
        {
            return new KeyboardState(_keyStates.ToArray());
        }

        public void KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            for (var i = 0; i < _keyStates.Count; i++)
            {
                if (_keyStates[i].key != key) continue;

                _keyStates[i] = (_keyStates[i].key, true);
                return;
            }
        }

        public void KeyUp(IKeyboard keyboard, Key key, int arg3)
        {
            for (var i = 0; i < _keyStates.Count; i++)
            {
                if (_keyStates[i].key != key) continue;

                _keyStates[i] = (_keyStates[i].key, false);
                return;
            }
        }
    }
}
