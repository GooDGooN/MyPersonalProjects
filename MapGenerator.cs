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
        public readonly int MyLastRoomDirNum;
        public Room(int roomx, int roomy, bool[] enableDir)
        {
            x = roomx;
            y = roomy;
            MyEnableDir = enableDir;
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

        public string GetSymbols(bool[] dirs)
        {
            var bitdirs = default(int);
            foreach (var dir in dirs) 
            {
                bitdirs <<= 1;
                if (dir) { bitdirs |= 1; }
            }
            int.TryParse(Convert.ToString(bitdirs, 2),out bitdirs);
            string result = bitdirs switch
            {
                1000 => "←",
                0100 => "↓",
                0010 => "→",
                0001 => "↑",
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

        public void DrawCurrentMap()
        {
            for (int my = 0; my < MaxY; my++)
            {
                for (int mx = 0; mx < MaxX; mx++)
                {
                    bool skip = false;
                    foreach (Room room in Rooms)
                    {
                        if(room.x == mx && room.y == my)
                        {
                            Console.Write($"[{GetSymbols(room.MyEnableDir)} ] ");
                            skip = true;
                            break;
                        }
                    }
                    if (!skip) { Console.Write($"[  ] "); }
                }
                Console.WriteLine("\n");
            }

            foreach (var room in Rooms)
            {
                /*
                Console.WriteLine($"x : {room.x} | y : {room.y} ");
                Console.WriteLine($"  Right : {room.MyEnableDir[0]}");
                Console.WriteLine($"  Up : {room.MyEnableDir[1]}");
                Console.WriteLine($"  Left : {room.MyEnableDir[2]}");
                Console.WriteLine($"  Down : {room.MyEnableDir[3]}");
                */
            }

            for (int my = 0; my < MaxY; my++)
            {
                for (int mx = 0; mx < MaxX; mx++)
                {
                    bool skip = false;
                    foreach (Room room in Rooms)
                    {
                        if (room.x == mx && room.y == my)
                        {
                            Console.Write($"[1] ");
                            skip = true;
                            break;
                        }
                    }
                    if (!skip) { Console.Write($"[0] "); }
                }
                Console.WriteLine("\n");
            }

            Console.WriteLine($"룸 수 : {Rooms.Count}");
        }

        public bool CheckInsideMap(int x, int y)
        {
            if (x >= 0 && x < MaxX && y >= 0 && y < MaxY)
            {
                return true;
            }
            return false;
        }

        public void MapGen()
        {
            Rooms = new List<Room>();
            var roomAmountLimit = new Tuple<int, int>((int)((MaxX * MaxY) * 0.5f), (int)((MaxX * MaxY) * 0.6f));
            int startPointAmount = Random.Shared.Next(3, 5);
            int maxRepeat = (MaxX + MaxY) - startPointAmount * 2;
            bool[] dir = new bool[4]; // 1r 2u 3l 4d
            var startPointx = ((MaxX) / 2) + Random.Shared.Next(-1, 1);
            var startPointy = ((MaxY) / 2) + Random.Shared.Next(-1, 1);

            List<Tuple<int, int>> roomCreator = new List<Tuple<int, int>> // x,y
            {
                new Tuple<int, int>(startPointx, startPointy)
            };
            //for (int repeat = 0; repeat < MaxX - 1; repeat++)
            for (int repeat = 0; repeat < 2; repeat++)
            {
                List<Tuple<int, int>> newRoom = new List<Tuple<int, int>>();
                foreach (var room in roomCreator)
                {
                    dir = SetRandomDir(room.Item1, room.Item2);
                    if (dir.Count(i => i == true) > 1 || repeat == 0)
                    {
                        if (dir[0]) // r
                        {
                            newRoom.Add(new Tuple<int, int>(room.Item1 + 1, room.Item2));
                        }
                        if (dir[1]) // u
                        {
                            newRoom.Add(new Tuple<int, int>(room.Item1, room.Item2 - 1));
                        }
                        if (dir[2]) // l
                        {
                            newRoom.Add(new Tuple<int, int>(room.Item1 - 1, room.Item2));
                        }
                        if (dir[3]) // d
                        {
                            newRoom.Add(new Tuple<int, int>(room.Item1, room.Item2 + 1));
                        }
                        Rooms.Add(new Room(room.Item1, room.Item2, dir));
                    }
                }
                roomCreator.Clear();
                foreach (var room in newRoom)
                {
                    roomCreator.Add(room);
                }
            }
            /*
            // 검사
            var count = Math.Clamp(Rooms.Count(), roomAmountLimit.Item1, roomAmountLimit.Item2);
            if (count == Rooms.Count())
            {
                for (int i = 0; i < MaxY; i++)
                {
                    if (ActiveRoomCount(-1, i) == 0)
                    {
                        MapGen();
                        return;
                    }
                }
                for (int i = 0; i < MaxX; i++)
                {
                    if (ActiveRoomCount(i) == 0)
                    {
                        MapGen();
                        return;
                    }
                }
            }
            else
            {
                MapGen();
                return;
            }
            */
        }

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
                        if (room.MyEnableDir[0] == true)
                        {
                            connectableDirs[2] = true;
                        }
                        if (room.MyEnableDir[1] == true)
                        {
                            connectableDirs[3] = true;
                        }
                        if (room.MyEnableDir[2] == true)
                        {
                            connectableDirs[0] = true;
                        }
                        if (room.MyEnableDir[3] == true)
                        {
                            connectableDirs[1] = true;
                        }
                    }
                    break;
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
        /*
        public bool IsCanMoveHere(int currentx, int currenty, int nextx, int nexty, bool includeRoom = false)
        {
            foreach (Room room in Rooms)
            {
                if(includeRoom)
                {
                    if (room.x == nextx && room.y == nexty)
                    {
                        //check connectable
                        switch(nextx - currentx)
                        {
                            case -1:
                                if (room.MyEnableDir[0])
                                {
                                    return true;
                                }
                                break;
                            case 1:
                                if (room.MyEnableDir[2])
                                {
                                    return true;
                                }
                                break;
                        }
                        switch (nexty - currenty)
                        {
                            case -1:
                                if (room.MyEnableDir[1])
                                {
                                    return true;
                                }
                                break;
                            case 1:
                                if (room.MyEnableDir[3])
                                {
                                    return true;
                                }
                                break;
                        }
                        return true;
                    }
                }
                if (room.x != nextx && room.y != nexty)
                {
                    return true;
                }
            }
            return false;
        }
        */
        /*
        private Vector2 NextPos(int currentX, int currentY)
        {
            List<Tuple<int, int>> listtuple = new List<Tuple<int, int>>();
            
            for(int xy = 0; xy < 2; xy++)
            {
                for (int delta = -1; delta < 2; delta += 2)
                {
                    if(xy == 0 && (currentX + delta >= 0 && currentX + delta < MaxX))
                    {
                        if (Maps[currentX + delta, currentY] == 0)
                        {
                            listtuple.Add(new Tuple<int, int>(currentX + delta, currentY));
                        }
                    }
                    if (xy == 1 && (currentY + delta >= 0 && currentY + delta < MaxY))
                    {
                        if (Maps[currentX, currentY + delta] == 0)
                        {
                            listtuple.Add(new Tuple<int, int>(currentX, currentY + delta));
                        }
                    }
                }
            }
            if (listtuple.Count > 0)
            {
                var randomPos = listtuple[Random.Shared.Next(listtuple.Count)];
                var result = new Vector2(randomPos.Item1, randomPos.Item2);
                return result;
            }
            return Vector2.Zero;
        }
        public void MapGen()
        {
            Maps = new int[MaxX, MaxY];
            var roomAmountLimit = new Tuple<int, int>((int)((MaxX * MaxY) * 0.2f), (int)((MaxX * MaxY) * 0.5f));
            int startPointAmount = Random.Shared.Next(3, 5);
            int maxRepeat = (MaxX + MaxY) - startPointAmount * 2;

            var startPointx = ((MaxX) / 2);
            var startPointy = ((MaxY) / 2);

            Vector2[] roomCreator = new Vector2[startPointAmount];
            Array.Fill(roomCreator, new Vector2(startPointx, startPointy));
            Maps[startPointx, startPointy] = 1;
            for(int repeat = 0; repeat < maxRepeat; repeat++)
            {
                for (int i = 0; i < startPointAmount; i++)
                {
                    roomCreator[i] = NextPos((int)roomCreator[i].X, (int)roomCreator[i].Y);
                    if (roomCreator[i] == Vector2.Zero)
                    {
                        MapGen();
                        return;
                    }
                    Maps[(int)roomCreator[i].X, (int)roomCreator[i].Y] = 1;
                }
            }

            var count = ActiveRoomCount();
            if (Math.Clamp(count, roomAmountLimit.Item1, roomAmountLimit.Item2) == ActiveRoomCount()) // 검사
            {
                for (int i = 0; i < MaxY; i++)
                {
                    if (ActiveRoomCount(i) == 0)
                    {
                        MapGen();
                    }
                }
                for (int i = 0; i < MaxX; i++)
                {
                    if (ActiveRoomCount(-1, i) == 0)
                    {
                        MapGen();
                    }
                }
            }
            else
            {
                MapGen();
            }
        }
        */

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
                /*
                for(int i = 0; i < MaxX; i++)
                {
                    
                    if(Maps[i, yPos] == 1)
                    {
                        amount++;
                    }
                }
                */
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
                /*
                for (int i = 0; i < MaxY; i++)
                {
                    if (Maps[xPos, i] == 1)
                    {
                        amount++;
                    }
                }*/
            }
            return amount;
        }
    }
}
