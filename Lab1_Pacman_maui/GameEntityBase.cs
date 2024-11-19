using SkiaSharp;

namespace Lab1_Pacman_maui
{
    public abstract class GameEntityBase
    {
        protected GameManager _gameManager;

        public int X { get; set; }
        public int Y { get; set; }
        public int Speed { get; set; } = 1;

        public int LastTargetX { get; set; }
        public int LastTargetY { get; set; }


        protected GameEntityBase(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public abstract void Move();

        public abstract void Draw(SKCanvas canvas);
    }

    public class Pacman : GameEntityBase
    {

        private Queue<(int x, int y)> _path;
        private AStarPathfinding _pathfinding;

        public Pacman(GameManager gameManager) : base(gameManager)
        {
            _pathfinding = new AStarPathfinding(gameManager);
            _path = new Queue<(int x, int y)>();
        }

        public override void Move()
        {
            if(_path.Count == 0)
            {
                var target = FindNearestDot();
                if(target != (-1, -1))
                {
                    _path = new Queue<(int x, int y)>(_pathfinding.FindPath(X, Y, target.x, target.y));
                }

                LastTargetX = target.x;
                LastTargetY = target.y;
            }

            if(_path.Count > 0)
            {
                var nextStep = _path.Dequeue();
                if(nextStep.x == X && nextStep.y == Y && _path.Count > 0)
                {
                    nextStep = _path.Dequeue();
                }

                X = nextStep.x;
                Y = nextStep.y;

                if(_gameManager.Dots[X, Y])
                {
                    _gameManager.Dots[X, Y] = false;
                    _gameManager.Score++;
                }
            }
        }

        private (int x, int y) FindNearestDot()
        {
            var heatMap = new int[_gameManager.MapGenerator.Width, _gameManager.MapGenerator.Height];

            var distanceMap = CalculateDistanceMap(_gameManager.Pacman.X, _gameManager.Pacman.Y);

            var penaltyMap = CalculateGhostPenalties();

            for(int i = 0; i < _gameManager.MapGenerator.Width; i++)
            {
                for(int j = 0; j < _gameManager.MapGenerator.Height; j++)
                {
                    if(_gameManager.Dots[i, j])
                    {
                        heatMap[i, j] = distanceMap[i, j] + penaltyMap[i, j];
                    }
                    else
                    {
                        heatMap[i, j] = int.MaxValue;
                    }
                }
            }

            int minHeat = int.MaxValue;
            (int x, int y) nearestDot = (-1, -1);

            for(int i = 0; i < _gameManager.MapGenerator.Width; i++)
            {
                for(int j = 0; j < _gameManager.MapGenerator.Height; j++)
                {
                    if(_gameManager.Dots[i, j] && heatMap[i, j] < minHeat)
                    {
                        bool isCloseToGhost = _gameManager.gameEntities.OfType<Ghost>()
                            .Any(ghost => Math.Abs(ghost.X - i) + Math.Abs(ghost.Y - j) < 3);

                        if(!isCloseToGhost)
                        {
                            minHeat = heatMap[i, j];
                            nearestDot = (i, j);
                        }
                    }
                }
            }

            return nearestDot;
        }

        private int[,] CalculateDistanceMap(int startX, int startY)
        {
            var distanceMap = new int[_gameManager.MapGenerator.Width, _gameManager.MapGenerator.Height];
            var visited = new bool[_gameManager.MapGenerator.Width, _gameManager.MapGenerator.Height];
            var stack = new Stack<(int x, int y, int distance)>();

            stack.Push((startX, startY, 0));
            visited[startX, startY] = true;

            while(stack.Any())
            {
                var (x, y, dist) = stack.Pop();

                distanceMap[x, y] = dist;

                var neighbors = new (int dx, int dy)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };

                foreach(var (dx, dy) in neighbors)
                {
                    var newX = x + dx;
                    var newY = y + dy;

                    if(newX >= 0 && newY >= 0 && newX < _gameManager.MapGenerator.Width && newY < _gameManager.MapGenerator.Height)
                    {
                        if(!(_gameManager.MapGenerator.maze[newX, newY] == 1) || visited[newX, newY]) continue;

                        visited[newX, newY] = true;
                        stack.Push((newX, newY, dist + 1));
                    }
                }
            }

            return distanceMap;
        }

        private int[,] CalculateGhostPenalties()
        {
            var penaltyMap = new int[_gameManager.MapGenerator.Width, _gameManager.MapGenerator.Height];

            
            for(int i = 0; i < _gameManager.MapGenerator.Width; i++)
            {
                for(int j = 0; j < _gameManager.MapGenerator.Height; j++)
                {
                    int penalty = 0;
                    foreach(var ghost in _gameManager.gameEntities.OfType<Ghost>())
                    {
                        int distToGhost = Math.Abs(ghost.X - i) + Math.Abs(ghost.Y - j);
                        if(distToGhost < 5)
                        {
                            penalty += 50;
                        }
                    }
                    penaltyMap[i, j] = penalty;
                }
            }

            return penaltyMap;
        }

        public override void Draw(SKCanvas canvas)
        {
            var paint = new SKPaint
            {
                Color = SKColors.Yellow,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawCircle(X * TilesModel.Size + TilesModel.Size / 2, Y * TilesModel.Size + TilesModel.Size / 2, TilesModel.Size / 2, paint);
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 3;
            canvas.DrawCircle(LastTargetX * TilesModel.Size + TilesModel.Size / 4, LastTargetY * TilesModel.Size + TilesModel.Size / 4, TilesModel.Size / 4, paint);
        }
    }

    public class Ghost : GameEntityBase
    {
        private SKColor _color;
        protected AStarPathfinding _pathfinding;
        protected Queue<(int x, int y)> _path;
        private (int dx, int dy) _previousDirection = (0, 0);

        public Ghost(GameManager gameManager, SKColor color) : base(gameManager)
        {
            _color = color;
            _pathfinding = new AStarPathfinding(gameManager);
            _path = new Queue<(int x, int y)>();
        }

        public override void Move()
        {

        }

        public override void Draw(SKCanvas canvas)
        {
            var paint = new SKPaint
            {
                Color = _color,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRect(X * TilesModel.Size, Y * TilesModel.Size, TilesModel.Size, TilesModel.Size, paint);
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 3;
            canvas.DrawRect(LastTargetX * TilesModel.Size, LastTargetY * TilesModel.Size, TilesModel.Size, TilesModel.Size, paint);

        }

        protected void MoveTowardsTarget(int targetX, int targetY)
        {
            var possibleDirections = new List<(int dx, int dy)>
            {
                (0, 1),  // Вниз
                (0, -1), // Вгору
                (1, 0),  // Вправо
                (-1, 0)  // Вліво
            };

            possibleDirections.Remove((-_previousDirection.dx, -_previousDirection.dy));

            var validDirections = possibleDirections
                .Where(dir =>
                {
                    int newX = X + dir.dx;
                    int newY = Y + dir.dy;

                    return IsWalkable(newX, newY);
                })
                .ToList();

            if(validDirections.Count > 0)
            {
                var bestDirection = validDirections
                    .OrderBy(dir =>
                    {
                        int newX = X + dir.dx;
                        int newY = Y + dir.dy;
                        return ManhattanDistance(newX, newY, targetX, targetY);
                    })
                    .First();

                X += bestDirection.dx;
                Y += bestDirection.dy;

                _previousDirection = bestDirection;
            }
        }

        private bool IsWalkable(int x, int y)
        {
            return x >= 0 && x < _gameManager.MapGenerator.Width &&
                   y >= 0 && y < _gameManager.MapGenerator.Height &&
                   _gameManager.MapGenerator.maze[x, y] == 1;
        }

        private int ManhattanDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }
    }

    public class Blinky : Ghost
    {
        public Blinky(GameManager gameManager) : base(gameManager, SKColors.Red) { }

        public override void Move()
        {
            // Завжди переслідує Пакмена
            LastTargetX = _gameManager.Pacman.X;
            LastTargetY = _gameManager.Pacman.Y;
            MoveTowardsTarget(_gameManager.Pacman.X, _gameManager.Pacman.Y);
        }
    }

    public class Pinky : Ghost
    {
        public Pinky(GameManager gameManager) : base(gameManager, SKColors.Pink) { }

        public override void Move()
        {
            var pacman = _gameManager.Pacman;

            // Цілиться на 4 клітини вперед від Пакмена
            var targetX = pacman.X + GetDirectionalOffsetX(pacman);
            var targetY = pacman.Y + GetDirectionalOffsetY(pacman);

            LastTargetX = targetX;
            LastTargetY = targetY;

            MoveTowardsTarget(targetX, targetY);
        }

        private int GetDirectionalOffsetX(Pacman pacman) => pacman.X + pacman.Speed * 4;
        private int GetDirectionalOffsetY(Pacman pacman) => pacman.Y + pacman.Speed * 4;
    }

    public class Inky : Ghost
    {
        public Inky(GameManager gameManager) : base(gameManager, SKColors.Cyan) { }

        public override void Move()
        {
            var pacman = _gameManager.Pacman;
            var blinky = _gameManager.gameEntities.OfType<Blinky>().FirstOrDefault();

            if(blinky != null)
            {
                var vectorX = pacman.X - blinky.X;
                var vectorY = pacman.Y - blinky.Y;

                var targetX = pacman.X + vectorX;
                var targetY = pacman.Y + vectorY;

                LastTargetX = targetX;
                LastTargetY = targetY;

                MoveTowardsTarget(targetX, targetY);
            }
        }
    }

    public class Clyde : Ghost
    {
        public Clyde(GameManager gameManager) : base(gameManager, SKColors.Orange) { }

        public override void Move()
        {
            var pacman = _gameManager.Pacman;

            var distance = Math.Sqrt(Math.Pow(X - pacman.X, 2) + Math.Pow(Y - pacman.Y, 2));

            if(distance > 8)
            {
                LastTargetX = pacman.X;
                LastTargetY = pacman.Y;

                MoveTowardsTarget(pacman.X, pacman.Y);
            }
            else
            {
                LastTargetX = 0;
                LastTargetY = 0;
                MoveTowardsTarget(0, 0);
            }
        }
    }

}
