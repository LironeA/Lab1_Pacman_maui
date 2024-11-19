using SkiaSharp;

namespace Lab1_Pacman_maui
{
    public class MapGenerator
    {
        private static readonly int[] dx = { 0, 2, 0, -2 };
        private static readonly int[] dy = { -2, 0, 2, 0 };
        private static readonly Random random = new Random();

        public int Width { get; set; }
        public int Height { get; set; }
        public int[,] maze;
        private const int MAX_CORRIDOR_LENGTH = 2;

        public MapGenerator(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            maze = new int[width, height];
        }

        public void GenerateMaze()
        {
            for(int y = 0; y < Height; y++)
            {
                for(int x = 0; x < Width; x++)
                {
                    maze[x, y] = 0;
                }
            }

            DFS(1, 1);
            RemoveDeadEnds();
        }

        private void DFS(int x, int y, int prevDir = -1, int corridorLength = 0)
        {
            maze[x, y] = 1;
            List<int> directions = new List<int> { 0, 1, 2, 3 };
            Shuffle(directions);

            foreach(int dir in directions)
            {
                if(prevDir != -1 && dir == prevDir && corridorLength >= MAX_CORRIDOR_LENGTH)
                {
                    continue;
                }

                int nx = x + dx[dir];
                int ny = y + dy[dir];

                if(IsInBounds(nx, ny) && maze[nx, ny] == 0 && HasNoNearbyJunctions(nx, ny))
                {
                    maze[ x + dx[dir] / 2, y + dy[dir] / 2] = 1;
                    maze[nx, ny] = 1;
                    DFS(nx, ny, dir, (dir == prevDir) ? corridorLength + 1 : 1);
                }
            }
        }

        private void RemoveDeadEnds()
        {
            for(int y = 1; y < Height - 1; y++)
            {
                for(int x = 1; x < Width - 1; x++)
                {
                    if(maze[x, y] == 1 && CountExits(x, y) == 1)
                    {
                        List<int> directions = new List<int>();

                        for(int i = 0; i < 4; i++)
                        {
                            int nx = x + dx[i] / 2;
                            int ny = y + dy[i] / 2;

                            if(IsInBounds(nx, ny) && maze[nx, ny] == 0)
                            {
                                directions.Add(i);
                            }
                        }

                        if(directions.Count > 0)
                        {
                            int dir = directions[random.Next(directions.Count)];
                            maze[x + dx[dir] / 2, y + dy[dir] / 2] = 1;
                        }
                    }
                }
            }
        }

        private bool HasNoNearbyJunctions(int x, int y)
        {
            for(int i = 0; i < 4; i++)
            {
                int nx = x + dx[i] / 2;
                int ny = y + dy[i] / 2;
                if(IsInBounds(nx, ny) && CountExits(nx, ny) > 1)
                {
                    return false;
                }
            }
            return true;
        }

        private int CountExits(int x, int y)
        {
            int exits = 0;
            for(int i = 0; i < 4; i++)
            {
                int nx = x + dx[i] / 2;
                int ny = y + dy[i] / 2;

                if(IsInBounds(nx, ny) && maze[nx, ny] == 1)
                {
                    exits++;
                }
            }
            return exits;
        }

        private bool IsInBounds(int x, int y)
        {
            return x > 0 && x < Width - 1 && y > 0 && y < Height - 1;
        }

        private void Shuffle(List<int> list)
        {
            for(int i = 0; i < list.Count; i++)
            {
                int j = random.Next(i, list.Count);
                int temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        internal void DrawMap(SKCanvas canvas)
        {
            for (int i = 0; i < Width; i++)
            {
                for(int j = 0; j < Height; j++)
                {
                    if(maze[i, j] == 0)
                    {
                        canvas.DrawRect(i * 10, j * 10, 10, 10, TilesModel.WallPaint);
                    }
                    else
                    {
                        canvas.DrawRect(i * 10, j * 10, 10, 10, TilesModel.PathPaint);
                    }
                }
            }
        }

        public bool IsWall(int x, int y)
        {
            if(x < 0 || x >= Width || y < 0 || y >= Height) return true;
            return maze[x, y] == 0;
        }
    }
}
