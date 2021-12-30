using Silk.NET.Input;

namespace OBSGreenScreen
{
    public struct KeyboardState
    {
        private readonly (Key key, bool isDown)[]? _states;

        public KeyboardState((Key, bool)[] states = null)
        {
            _states = states;
        }

        public bool IsDown(Key key)
        {
            if (_states is null)
            {
                return false;
            }

            foreach (var state in _states)
            {
                if (state.key == key)
                {
                    return state.isDown;
                }
            }

            return false;
        }

        public bool IsUp(Key key) => !IsDown(key);
    }
}
