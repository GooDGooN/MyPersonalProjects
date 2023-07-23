using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MapGenTest
{
    public class Room
    {
        public readonly int x;
        public readonly int y;
        public readonly bool[] MyEnableDir;
        public readonly int RoomID;
        public readonly int RoomBitDir;
        public Room(int roomx, int roomy, bool[] enableDir, int roomID, int roomBitDir)
        {
            x = roomx;
            y = roomy;
            MyEnableDir = enableDir;
            RoomID = roomID;
            RoomBitDir = roomBitDir;
        }
    }


    public class MapGenerator
    {
        public int MaxX;
        public int MaxY;
        public List<Room> Rooms = new List<Room>();
        public MapGenerator(int mapSizeX, int mapSizeY)
        {
            MaxX = mapSizeX;
            MaxY = mapSizeY;
        }

        public string GetSymbols(int bitdirs)
        {
            string result = bitdirs switch
            {
                1000 => "→",
                0100 => "↑",
                0010 => "←",
                0001 => "↓",
                1100 => "┗",
                1010 => "━",
                1001 => "┏",
                0110 => "┛",
                0101 => "┃",
                0011 => "┓",
                1110 => "┻",
                1011 => "┳",
                0111 => "┫",
                1101 => "┣",
                1111 => "╋",
                _ => "",

            };
            return result;
        }

        /// <summary>
        /// 테스트용 출력
        /// </summary>
        public void DrawCurrentMapForTest()
        {
            for (int my = 0; my < MaxY; my++)
            {
                for (int mx = 0; mx < MaxX; mx++)
                {
                    bool skip = false;
                    foreach (Room room in Rooms)
                    {
                        if (room.x == mx && room.y == my)
                        {
                            var dirSymbol = GetSymbols(room.RoomBitDir);
                            if (dirSymbol == "→" || dirSymbol == "↑" || dirSymbol == "←" || dirSymbol == "↓")
                            {
                                Console.Write($"[{dirSymbol}] ");
                            }
                            else
                            {
                                Console.Write($"[{dirSymbol} ] ");
                            }
                            skip = true;
                            break;
                        }
                    }
                    if (!skip) { Console.Write($"[  ] "); }
                }
                Console.WriteLine("\n");
            }
            Console.WriteLine("\n");

            for (int my = 0; my < MaxY; my++)
            {
                for (int mx = 0; mx < MaxX; mx++)
                {
                    bool skip = false;
                    foreach (Room room in Rooms)
                    {
                        if (room.x == mx && room.y == my)
                        {
                            if (room.RoomID < 10)
                            {
                                Console.Write($"[0{room.RoomID}] ");
                            }
                            else
                            {
                                Console.Write($"[{room.RoomID}] ");
                            }
                            skip = true;
                            break;
                        }
                    }
                    if (!skip) { Console.Write($"[  ] "); }
                }
                Console.WriteLine("\n");
            }
            Console.WriteLine("\n");
            Console.WriteLine($"룸 수 : {Rooms.Count}");
            Console.WriteLine();
            foreach (var room in Rooms)
            {
                Console.WriteLine($"[{room.RoomID}].bitdir = {room.RoomBitDir}");
            }
        }


        public void MapGen()
        {

            var roomAmountLimit = new Tuple<int, int>((int)((MaxX * MaxY) * 0.6f), (int)((MaxX * MaxY) * 0.8f));
            bool[] dir = new bool[4]; // 1r 2u 3l 4d
            var startPointx = ((MaxX) / 2) + Random.Shared.Next(-1, 2);
            var startPointy = ((MaxY) / 2) + Random.Shared.Next(-1, 2);
            var roomID = 0;
            List<Tuple<int, int>> roomCreator = new List<Tuple<int, int>> // x,y
            {
                new Tuple<int, int>(startPointx, startPointy)
            };
            List<Tuple<int, int>> newRoom = new List<Tuple<int, int>>();

            bool totalLoopbreakable = false;
            while (!totalLoopbreakable) 
            {
                Console.Clear();
                roomID = 0;
                int maxRepeat = ((MaxX + MaxY) / 2) + Random.Shared.Next(0, (MaxX + MaxY) / 2) - 1;
                startPointx = ((MaxX) / 2) + Random.Shared.Next(-1, 2);
                startPointy = ((MaxY) / 2) + Random.Shared.Next(-1, 2);

                roomCreator.Clear();
                roomCreator.Add(new Tuple<int, int>(startPointx, startPointy));
                Rooms.Clear();
                totalLoopbreakable = true;
                for (int repeat = 0; repeat < maxRepeat; repeat++)
                {
                    newRoom.Clear();
                    foreach (var room in roomCreator)
                    {
                        dir = SetRandomDir(room.Item1, room.Item2);

                        if (dir.Count(i => i == true) > 0)
                        {
                            if (dir[0]) // r
                            {
                                if (CheckConnectableDir(room.Item1 + 1, room.Item2).Count(i => i == true) > 0 || repeat == 0)
                                {
                                    newRoom.Add(new Tuple<int, int>(room.Item1 + 1, room.Item2));
                                }
                            }
                            if (dir[1]) // u
                            {
                                if (CheckConnectableDir(room.Item1, room.Item2 - 1).Count(i => i == true) > 0 || repeat == 0)
                                {
                                    newRoom.Add(new Tuple<int, int>(room.Item1, room.Item2 - 1));
                                }
                            }
                            if (dir[2]) // l
                            {
                                if (CheckConnectableDir(room.Item1 - 1, room.Item2).Count(i => i == true) > 0 || repeat == 0)
                                {
                                    newRoom.Add(new Tuple<int, int>(room.Item1 - 1, room.Item2));
                                }
                            }
                            if (dir[3]) // d
                            {
                                if (CheckConnectableDir(room.Item1, room.Item2 + 1).Count(i => i == true) > 0 || repeat == 0)
                                {
                                    newRoom.Add(new Tuple<int, int>(room.Item1, room.Item2 + 1));
                                }
                            }

                            //Console.Write($"room id : {roomID}, x : {room.Item1}, y : {room.Item2}\n");
                            if (!IsRoomHere(room.Item1, room.Item2))
                            {
                                Rooms.Add(new Room(room.Item1, room.Item2, dir, roomID++, ConvertTheDirToInt(dir)));
                            }
                        }
                        //GC.Collect();
                    }
                    roomCreator.Clear();
                    foreach (var room in newRoom)
                    {
                        roomCreator.Add(room);
                    }
                }

                // 맵 마무리
                for(int x = 0; x < MaxX; x++)
                {
                    for(int y = 0; y < MaxY; y++) 
                    {
                        var connectableDir = CheckConnectableDir(x, y);
                        if (!IsRoomHere(x, y) && connectableDir.Count(i => i == true) > 0)
                        {
                            Rooms.Add(new Room(x, y, connectableDir, roomID++, ConvertTheDirToInt(connectableDir)));
                        }
                    }
                }
                if (Rooms.Count >= roomAmountLimit.Item1 && Rooms.Count <= roomAmountLimit.Item2)
                {
                    for (int i = 0; i < MaxY; i++)
                    {
                        if (ActiveRoomCount(-1, i) == 0)
                        {
                            totalLoopbreakable = false;
                            break;
                        }
                    }
                    for (int i = 0; i < MaxX; i++)
                    {
                        if (ActiveRoomCount(i) == 0)
                        {
                            totalLoopbreakable = false;
                            break;
                        }
                    }
                }
                else
                {
                    totalLoopbreakable = false;
                }
                
            }

        }
        
        /// <summary>
        /// 방향을 2진수로 반환, R/U/L/D 
        /// </summary>
        private int ConvertTheDirToInt(bool[] dirs)
        {
            var bitdirs = default(int);
            foreach (var dir in dirs)
            {
                bitdirs <<= 1;
                if (dir) { bitdirs |= 1; }
            }
            int.TryParse(Convert.ToString(bitdirs, 2), out bitdirs);
            return bitdirs;
        }

        /// <summary>
        /// 연결 가능한 룸이 있는지 검색 후 연결이 가능한 현재 룸의 방향을 반환
        /// </summary>
        public bool[] CheckConnectableDir(int currentX, int currentY)
        {
            bool[] connectableDirs = new bool[4];
            Array.Fill(connectableDirs, false);
            for (int i = 1; i >= -2; i--)
            {
                var xdelta = (int)default;
                var ydelta = (int)default;
                if (Math.Abs(i) % 2 == Math.Abs(1))
                {
                    xdelta = (i == 1) ? 1 : -1;
                }
                if (Math.Abs(i) % 2 == 0)
                {
                    ydelta = (i == 0) ? -1 : 1;
                }

                foreach (var room in Rooms)
                {
                    if (room.x == currentX + xdelta && room.y == currentY + ydelta)
                    {
                        if(xdelta == -1 && room.MyEnableDir[0])
                        {
                            connectableDirs[2] = true;
                        }
                        if (ydelta == -1 && room.MyEnableDir[3])
                        {
                            connectableDirs[1] = true;
                        }
                        if (xdelta == 1 && room.MyEnableDir[2])
                        {
                            connectableDirs[0] = true;
                        }
                        if(ydelta == 1 && room.MyEnableDir[1])
                        {
                            connectableDirs[3] = true;
                        }
                        break;
                    }
                }   
            }
            return connectableDirs;
        }
        /// <summary>
        /// 현재 좌표에서 상하좌우를 검색하여 갈 수 있는 위치를 랜덤 설정
        /// </summary>
        public bool[] SetRandomDir(int currentX, int currentY)
        {
            bool[] dirs = new bool[4];
            Array.Fill(dirs, false);
            for(int i = 1; i >= -2; i--)
            {
                var xdelta = (int)default;
                var ydelta = (int)default;

                if (Math.Abs(i) % 2 == Math.Abs(1))
                {
                    xdelta = (i == 1) ? 1 : -1;
                }
                if (Math.Abs(i) % 2 == 0)
                {
                    ydelta = (i == 0) ? -1 : 1;
                }

                if (CheckInsideMap(currentX + xdelta, currentY + ydelta))
                {
                    dirs[Math.Abs(i - 1)] = !IsRoomHere(currentX + xdelta, currentY + ydelta);
                }
            }
            for (int i = 0; i < dirs.Length; i++)
            {
                if(dirs.Count(i => i == true) > 1 && dirs[i] == true)
                {
                    dirs[i] = Convert.ToBoolean(Random.Shared.Next(0, 2));
                }
            }

            var connectableDir = CheckConnectableDir(currentX, currentY);
            for (int i = 0; i < dirs.Length; i++)
            {
                if (connectableDir[i])
                {
                    dirs[i] = connectableDir[i];
                }
            }
            return dirs;
        }

        /// <summary>
        /// 해당 위치가 맵 안에 있는 것인지 확인
        /// </summary>
        public bool CheckInsideMap(int x, int y)
        {
            if (x >= 0 && x < MaxX && y >= 0 && y < MaxY)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 이미 해당 위치에 룸이 존재하는지 검사
        /// </summary>
        public bool IsRoomHere(int nextx, int nexty)
        {
            foreach (Room room in Rooms)
            {
                if (room.x == nextx && room.y == nexty)
                {
                    return true;
                }

            }
            return false;
        }
        /// <summary>
        /// 맵 완성 후 활성화된 룸 체크
        /// </summary>
        public int ActiveRoomCount(int xPos = -1, int yPos = -1)
        {
            int amount = 0;
            if(yPos == -1 && xPos == -1)
            {
                return Rooms.Count;
            }
            else if(yPos != -1)
            {
                foreach(var i in Rooms)
                {
                    if(i.y == yPos)
                    {
                        amount++;
                    }
                }
            }
            else if (xPos != -1)
            {
                foreach (var i in Rooms)
                {
                    if (i.x == xPos)
                    {
                        amount++;
                    }
                }
            }
            return amount;
        }
    }
}
