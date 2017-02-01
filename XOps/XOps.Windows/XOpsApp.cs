using SiliconStudio.Xenko.Engine;

namespace XOps
{
    class XOpsApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
