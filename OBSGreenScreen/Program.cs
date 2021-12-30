namespace OBSGreenScreen
{
    public class Program
    {
        private static Game _game;
        private static void Main(string[] args)
        {
            _game = new Game();
            _game.Run();
        }
    }
}
