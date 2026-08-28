using System.Collections.Generic;
using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class NavigationGrid2D : MonoBehaviour
    {
        [SerializeField] private Vector2 worldSize = new Vector2(40f, 28f);
        [SerializeField] private float cellSize = 0.75f;
        [SerializeField] private float agentRadius = 0.28f;
        [SerializeField] private LayerMask obstacleMask;

        private Node[,] _nodes;
        private int _width;
        private int _height;
        private Vector2 _origin;
        private int _searchId;

        public float CellSize => cellSize;

        public sealed class Node
        {
            public int X;
            public int Y;
            public Vector2 World;
            public bool Walkable;
            public float G;
            public float H;
            public Node Parent;
            public int SearchId;
            public bool Closed;
            public float F => G + H;
        }

        private void Awake()
        {
            Build();
        }

        public void Configure(Vector2 size, float newCellSize, float radius, LayerMask mask)
        {
            worldSize = size;
            cellSize = newCellSize;
            agentRadius = radius;
            obstacleMask = mask;
            Build();
        }

        public void Build()
        {
            _width = Mathf.Max(2, Mathf.RoundToInt(worldSize.x / cellSize));
            _height = Mathf.Max(2, Mathf.RoundToInt(worldSize.y / cellSize));
            _origin = (Vector2)transform.position - worldSize * 0.5f;

            _nodes = new Node[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Vector2 world = _origin + new Vector2((x + 0.5f) * cellSize, (y + 0.5f) * cellSize);
                    bool blocked = Physics2D.OverlapCircle(world, agentRadius, obstacleMask) != null;

                    _nodes[x, y] = new Node
                    {
                        X = x,
                        Y = y,
                        World = world,
                        Walkable = !blocked
                    };
                }
            }
        }

        public List<Vector2> FindPath(Vector2 startWorld, Vector2 endWorld)
        {
            if (_nodes == null) Build();

            Node start = ClosestNode(startWorld);
            Node goal = ClosestNode(endWorld);

            if (start == null || goal == null || !goal.Walkable)
                return new List<Vector2>();

            _searchId++;

            if (_searchId == int.MaxValue)
            {
                _searchId = 1;

                for (int x = 0;
                     x < _width;
                     x++)
                {
                    for (int y = 0;
                         y < _height;
                         y++)
                    {
                        _nodes[x, y].SearchId = 0;
                    }
                }
            }

            var open =
                new List<Node>(128);

            PrepareNode(start);

            start.G = 0f;
            start.H = Heuristic(start, goal);
            open.Add(start);

            while (open.Count > 0)
            {
                int currentIndex = 0;
                Node current = open[0];

                for (int i = 1;
                     i < open.Count;
                     i++)
                {
                    Node candidate =
                        open[i];

                    if (candidate.F < current.F ||
                        (Mathf.Approximately(
                             candidate.F,
                             current.F) &&
                         candidate.H < current.H))
                    {
                        current = candidate;
                        currentIndex = i;
                    }
                }

                open.RemoveAt(
                    currentIndex);

                current.Closed = true;

                if (current == goal)
                    return Retrace(start, goal);

                foreach (Node next in
                         Neighbours(current))
                {
                    if (!next.Walkable)
                        continue;

                    PrepareNode(next);

                    if (next.Closed)
                        continue;

                    float tentative =
                        current.G +
                        Vector2.Distance(
                            current.World,
                            next.World);

                    bool inOpen =
                        open.Contains(next);

                    if (!inOpen ||
                        tentative < next.G)
                    {
                        next.G = tentative;
                        next.H = Heuristic(next, goal);
                        next.Parent = current;

                        if (!inOpen)
                            open.Add(next);
                    }
                }
            }

            return new List<Vector2>();
        }

        private Node ClosestNode(Vector2 world)
        {
            int x = Mathf.Clamp(Mathf.FloorToInt((world.x - _origin.x) / cellSize), 0, _width - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt((world.y - _origin.y) / cellSize), 0, _height - 1);

            Node direct = _nodes[x, y];
            if (direct.Walkable) return direct;

            for (int radius = 1; radius <= 3; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= _width || ny >= _height) continue;
                        if (_nodes[nx, ny].Walkable) return _nodes[nx, ny];
                    }
                }
            }

            return direct;
        }

        private IEnumerable<Node> Neighbours(Node node)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int x = node.X + dx;
                    int y = node.Y + dy;
                    if (x < 0 || y < 0 || x >= _width || y >= _height) continue;

                    if (dx != 0 && dy != 0)
                    {
                        Node a = _nodes[node.X + dx, node.Y];
                        Node b = _nodes[node.X, node.Y + dy];
                        if (!a.Walkable || !b.Walkable) continue;
                    }

                    yield return _nodes[x, y];
                }
            }
        }

        private void PrepareNode(
            Node node)
        {
            if (node == null ||
                node.SearchId == _searchId)
            {
                return;
            }

            node.SearchId =
                _searchId;

            node.G =
                float.PositiveInfinity;

            node.H = 0f;
            node.Parent = null;
            node.Closed = false;
        }

        private float Heuristic(Node a, Node b)
        {
            return Vector2.Distance(a.World, b.World);
        }

        private List<Vector2> Retrace(Node start, Node end)
        {
            var result = new List<Vector2>();
            Node current = end;

            while (current != null && current != start)
            {
                result.Add(current.World);
                current = current.Parent;
            }

            result.Reverse();
            return result;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, worldSize);
        }
    }
}
