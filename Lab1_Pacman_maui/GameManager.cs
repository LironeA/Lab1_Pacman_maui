using SkiaSharp;

namespace Lab1_Pacman_maui
{
    public class GameManager
    {

        public int Score { get; set; } = 0;
        public Action DrawAction { get; set; }
        public MapGenerator MapGenerator { get; set; }
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        private Task _gameLoopTask;

        public List<GameEntityBase> gameEntities;
        public bool[,] Dots;
        public HashSet<(int x, int y)> DangerMap { get; private set; }

        public Pacman Pacman;

        public GameManager()
        {
            MapGenerator = new MapGenerator(29, 31);
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            gameEntities = new List<GameEntityBase>();
            Dots = new bool[MapGenerator.Width, MapGenerator.Height];
            DangerMap = new HashSet<(int x, int y)>();
        }

        public void Restart()
        {
            Score = 0;
            MapGenerator.GenerateMaze();
            Dots = new bool[MapGenerator.Width, MapGenerator.Height];
            _cancellationTokenSource.Cancel();

            for(int i = 0; i < MapGenerator.Width; i++)
            {
                for(int j = 0; j < MapGenerator.Height; j++)
                {
                    if(MapGenerator.maze[i, j] == 1)
                    {
                        Dots[i, j] = true;
                    }
                }
            }

            SpawnEntities();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _gameLoopTask = Task.Run(GameLoop);
        }

        private void SpawnEntities()
        {
            gameEntities.Clear();

            var pacmanPosition = FindNearestEmptyTile(MapGenerator.Width / 2, MapGenerator.Height / 2);
            Pacman = new Pacman(this) { X = pacmanPosition.x, Y = pacmanPosition.y };
            gameEntities.Add(Pacman);
            Dots[Pacman.X, Pacman.Y] = false;

            var ghostTypes = new Ghost[]
            {
                new Blinky(this),
                new Inky(this),
                new Pinky(this),
                new Clyde(this)
            };

            var ghostPositions = new (int, int)[]
            {
                FindNearestEmptyTile(0, 0), // Кут 1
                FindNearestEmptyTile(MapGenerator.Width - 1, 0), // Кут 2
                FindNearestEmptyTile(0, MapGenerator.Height - 1), // Кут 3
                FindNearestEmptyTile(MapGenerator.Width - 1, MapGenerator.Height - 1) // Кут 4
            };

            for(int i = 0; i < ghostTypes.Length; i++)
            {
                ghostTypes[i].X = ghostPositions[i].Item1;
                ghostTypes[i].Y = ghostPositions[i].Item2;
                gameEntities.Add(ghostTypes[i]);
            }
        }

        private (int x, int y) FindNearestEmptyTile(int startX, int startY)
        {
            for(int radius = 0; radius < Math.Max(MapGenerator.Width, MapGenerator.Height); radius++)
            {
                for(int x = Math.Max(0, startX - radius); x <= Math.Min(MapGenerator.Width - 1, startX + radius); x++)
                {
                    for(int y = Math.Max(0, startY - radius); y <= Math.Min(MapGenerator.Height - 1, startY + radius); y++)
                    {
                        if(MapGenerator.maze[x, y] == 1)
                        {
                            return (x, y);
                        }
                    }
                }
            }
            return (startX, startY);
        }

        public async Task GameLoop()
        {
            while(!_cancellationToken.IsCancellationRequested)
            {

                var previousPacmanPosition = (X: Pacman.X, Y: Pacman.Y);
                var previousGhostPositions = gameEntities
                    .OfType<Ghost>()
                    .ToDictionary(ghost => ghost, ghost => (X: ghost.X, Y: ghost.Y));

                foreach(var entity in gameEntities)
                {
                    entity.Move();

                    if(_cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if(entity is Ghost ghost)
                    {
                        if(IsCollision(ghost, Pacman, previousGhostPositions[ghost], previousPacmanPosition))
                        {
                            OnGameOver();
                            return;
                        }
                    }
                }

                if(AreAllDotsEaten())
                {
                    OnGameOver();
                    return;
                }

                DrawAction.Invoke();

                await Task.Delay(100);
            }
        }

        private bool IsCollision(Ghost ghost, Pacman pacman, (int X, int Y) previousGhostPosition, (int X, int Y) previousPacmanPosition)
        {
            if(ghost.X == pacman.X && ghost.Y == pacman.Y)
            {
                return true;
            }

            if(previousGhostPosition.X == pacman.X && previousGhostPosition.Y == pacman.Y &&
                ghost.X == previousPacmanPosition.X && ghost.Y == previousPacmanPosition.Y)
            {
                return true;
            }

            return false;
        }

        private bool AreAllDotsEaten()
        {
            for(int i = 0; i < MapGenerator.Width; i++)
            {
                for(int j = 0; j < MapGenerator.Height; j++)
                {
                    if(Dots[i, j]) return false;
                }
            }
            return true;
        }

        private void OnGameOver()
        {
            Console.WriteLine("Game Over!");
            Restart();
        }

        public void DrawMap(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            for(int i = 0; i < MapGenerator.maze.GetLength(0); i++)
            {
                for(int j = 0; j < MapGenerator.maze.GetLength(1); j++)
                {
                    var tile = MapGenerator.maze[i, j];
                    SKPaint paint = tile == 1 ? TilesModel.PathPaint : TilesModel.WallPaint;
                    canvas.DrawRect(i * TilesModel.Size, j * TilesModel.Size, TilesModel.Size, TilesModel.Size, paint);

                    if(Dots[i, j])
                    {
                        var dotPaint = new SKPaint
                        {
                            Color = SKColors.Yellow,
                            Style = SKPaintStyle.Fill
                        };
                        canvas.DrawCircle(i * TilesModel.Size + TilesModel.Size / 2, j * TilesModel.Size + TilesModel.Size / 2, TilesModel.Size / 6, dotPaint);
                    }
                }
            }

            foreach(var entity in gameEntities)
            {
                entity.Draw(canvas);
            }


            canvas.DrawText($"Score: {Score}", new SKPoint((MapGenerator.maze.GetLength(1) + 2) * TilesModel.Size, 100), new SKPaint() { Color = SKColors.Black,
                TextSize = 50
            });
        }

        public int CalculateDistanceToGhosts(int x, int y)
        {
            int minDistance = int.MaxValue;
            foreach(var ghost in gameEntities.Where(x => x is Ghost))
            {
                int distance = Math.Abs(ghost.X - x) + Math.Abs(ghost.Y - y);
                if(distance < minDistance)
                {
                    minDistance = distance;
                }
            }
            return minDistance;
        }
    }
}
