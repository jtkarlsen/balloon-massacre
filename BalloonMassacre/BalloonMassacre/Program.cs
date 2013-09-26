using System;

namespace BalloonMassacre
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (BalloonMassacre game = new BalloonMassacre())
            {
                game.Run();
            }
        }
    }
#endif
}

