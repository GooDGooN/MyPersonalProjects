using MapGenTest;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Intrinsics.X86;

namespace Program
{
    class Program
    {
        private static void Main(string[] args) 
        {
            MapGenerator mapGenerator = new MapGenerator(6, 4);
            while (true) 
            {
                Console.Clear();
                mapGenerator.MapGen();
                Console.WriteLine();
                mapGenerator.DrawCurrentMapForTest();

                bool quit = false;
                while(true)
                {
                    Console.WriteLine("R로 재생성, Q로 종료");
                    var input = Console.ReadKey();
                    {
                        if (input.Key == ConsoleKey.R)
                        {
                            break;
                        }
                        else if (input.Key == ConsoleKey.Q)
                        {
                            quit = true;
                            break;
                        }
                    }
                }
                if (quit) { break; }
            }
        }

    }
}