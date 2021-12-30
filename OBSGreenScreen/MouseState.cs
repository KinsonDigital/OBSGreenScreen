using Silk.NET.Input;

namespace OBSGreenScreen
{
    public class MouseState
    {
        private readonly (MouseButton button, bool isDown)[]? _states;
        private readonly int _x;
        private readonly int _y;

        public MouseState((MouseButton button, bool isDown)[] states = null, int x = 0, int y = 0)
        {
            _states = states;
            _x = x;
            _y = y;
        }

        public bool IsDown(MouseButton button)
        {
            if (_states is null)
            {
                return false;
            }

            foreach (var state in _states)
            {
                if (state.button == button)
                {
                    return state.isDown;
                }
            }

            return false;
        }

        public bool IsUp(MouseButton button) => !IsDown(button);

        public int X => _x;

        public int Y => _y;
    }
}
