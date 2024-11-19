namespace Lab1_Pacman_maui;

public class AStarPathfinding
{
    private GameManager _gameManager;

    public AStarPathfinding(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    // A* пошук шляху
    public List<(int x, int y)> FindPath(int startX, int startY, int targetX, int targetY)
    {
        var openSet = new HashSet<(int x, int y)>();
        var cameFrom = new Dictionary<(int x, int y), (int x, int y)>();
        var gScore = new Dictionary<(int x, int y), int>();
        var fScore = new Dictionary<(int x, int y), int>();

        for(int i = 0; i < _gameManager.MapGenerator.Width; i++)
        {
            for(int j = 0; j < _gameManager.MapGenerator.Height; j++)
            {
                gScore[(i, j)] = int.MaxValue;
                fScore[(i, j)] = int.MaxValue;
            }
        }

        gScore[(startX, startY)] = 0;
        fScore[(startX, startY)] = Heuristic(startX, startY, targetX, targetY);

        openSet.Add((startX, startY));

        while(openSet.Count > 0)
        {
            var current = openSet.OrderBy(pos => fScore[pos]).First();

            if(current == (targetX, targetY))
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);

            foreach(var neighbor in GetNeighbors(current.x, current.y))
            {
                var tentativeGScore = gScore[current] + 1;

                if(tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor.x, neighbor.y, targetX, targetY);

                    if(!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return new List<(int x, int y)>();
    }

    public int Heuristic(int x1, int y1, int x2, int y2)
    {
        int baseHeuristic = Math.Abs(x1 - x2) + Math.Abs(y1 - y2);

        foreach(var entity in _gameManager.gameEntities)
        {
            if(entity is Ghost ghost)
            {
                int distanceToGhost = Math.Abs(x1 - ghost.X) + Math.Abs(y1 - ghost.Y);
                if(distanceToGhost <= 2)
                {
                    baseHeuristic += (3 - distanceToGhost) * 10;
                }
            }
        }

        return baseHeuristic;
    }

    private List<(int x, int y)> ReconstructPath(Dictionary<(int x, int y), (int x, int y)> cameFrom, (int x, int y) current)
    {
        var totalPath = new List<(int x, int y)> { current };
        while(cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Add(current);
        }
        totalPath.Reverse();
        return totalPath;
    }

    private List<(int x, int y)> GetNeighbors(int x, int y)
    {
        var neighbors = new List<(int x, int y)>();

        var directions = new (int, int)[]
        {
            (-1, 0), (1, 0), (0, -1), (0, 1)
        };

        foreach(var dir in directions)
        {
            int newX = x + dir.Item1;
            int newY = y + dir.Item2;

            if(newX >= 0 && newY >= 0 && newX < _gameManager.MapGenerator.Width && newY < _gameManager.MapGenerator.Height)
            {
                if(_gameManager.MapGenerator.maze[newX, newY] == 1)
                {
                    neighbors.Add((newX, newY));
                }
            }
        }

        return neighbors;
    }
}
