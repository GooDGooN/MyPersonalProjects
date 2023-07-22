using MapGenTest;
using System.Runtime.Intrinsics.X86;

namespace Program
{
    class Program
    {
        private static void Main(string[] args) 
        {
            MapGenerator mapGenerator = new MapGenerator(6, 4);
            mapGenerator.MapGen();
            Console.WriteLine();
            mapGenerator.DrawCurrentMap();
        }

    }
}