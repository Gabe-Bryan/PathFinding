using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Graph<UnityEngine.Vector3Int>;
using static Graph<UnityEngine.Vector3Int>.Node;
using Debug = UnityEngine.Debug;

public class GridToGraph : MonoBehaviour
{
    //Types vectNode = Graph<Vector3Int>.Node<Vector3Int>;

    private Graph<Vector3Int> myGraph;
    private Tilemap tilemap;
    private Vector2 size;
    private Dictionary<Vector3Int, Node> nodes;

    private LinkedList<Node> processOrder;
    private LinkedListNode<Node> startHere;
    private LinkedListNode<Node> endHere;

    private Stopwatch greedyStopwatch;
    private Stopwatch dijkstraStopwatch;
    private Stack<int> greedyTilesExplored;
    private Stack<int> dijkstraTilesExplored;
    private Stack<float> greedyCosts;
    private Stack<float> dijkstraCosts;

    bool processing = false;

    int count = 0;

    private void Start()
    {
        GenerateGraph();
        SetUpProcessOrder();
        greedyStopwatch = new Stopwatch();
        dijkstraStopwatch = new Stopwatch();
        greedyTilesExplored = new Stack<int>();
        dijkstraTilesExplored = new Stack<int>();
        greedyCosts = new Stack<float>();
        dijkstraCosts = new Stack<float>();
        Debug.Log("Number of Vertices: " + nodes.Count);
        //RunExample();
    }

    private void Update()
    {
        if (processing && greedyStopwatch != null && dijkstraStopwatch != null) for(int i = 1; i < 5; i++)TestNextPath();
        else if (processing && (greedyStopwatch == null || dijkstraStopwatch == null)) Debug.LogError("A stopwatch is set to null!");
    }

    private Graph<Vector3Int> GenerateGraph() {
        tilemap = gameObject.GetComponentInChildren<Tilemap>();
        size = tilemap.cellSize;

        myGraph = new Graph<Vector3Int>(new ArrayList());

        GenerateNodes(tilemap);

        return myGraph;
    }
    private void SetUpProcessOrder() {
        if (nodes == null) { Debug.LogError("The nodes list has not been initializied"); return; }
        
        processOrder = new LinkedList<Node>();

        foreach (KeyValuePair<Vector3Int, Node> keyNNode in nodes) {
            processOrder.AddFirst(keyNNode.Value);
        }
        startHere = processOrder.First;
        endHere = startHere.Next;

        processing = true;
    }
    private void TestNextPath() {
        //finished case
        if (startHere == null) {
            Debug.Log("Dijkstra's took " + dijkstraStopwatch.ElapsedMilliseconds + " milis, had an average cost of " + FindAverage(dijkstraCosts) + ", and had an average tiles explored of " + FindIntAverage(dijkstraTilesExplored));
            Debug.Log("Greedy first search took " + greedyStopwatch.ElapsedMilliseconds + " milis, had an average cost of " + FindAverage(greedyCosts) + ", and had an average tiles explored of " + FindIntAverage(greedyTilesExplored));
            count++;
            processing = count < 1;
            if (processing) Start();
        }
        //finished with start node case
        if (endHere == null) {
            startHere = startHere.Next;
            endHere = processOrder.First;
        }
        if (endHere != null && startHere != null) {
            if (tilemap.GetTile(startHere.Value.GetValue()).name == "Mountain" || tilemap.GetTile(endHere.Value.GetValue()).name == "Mountain") { Debug.Log("Damn mountain!"); endHere = endHere.Next; if (endHere != null && startHere.Value == endHere.Value) endHere = endHere.Next; return; }

            Debug.Log("Testing...");

            Stack<Node> path;

            dijkstraStopwatch.Start();
            int dijkstraTiles;
            dijkstraCosts.Push(myGraph.Dijkstras(startHere.Value, endHere.Value, out path, out dijkstraTiles));
            dijkstraTilesExplored.Push(dijkstraTiles);
            dijkstraStopwatch.Stop();

            greedyStopwatch.Start();
            int greedyTiles;
            greedyCosts.Push(myGraph.GBS(startHere.Value, endHere.Value, out path, out greedyTiles));
            greedyTilesExplored.Push(greedyTiles);
            greedyStopwatch.Stop();

            endHere = endHere.Next;
            //ensure endhere and starthere are not the same node
            if (endHere != null && startHere.Value == endHere.Value) endHere = endHere.Next;
        }
    }
    
    private void GenerateNodes(Tilemap tilemap) {
        nodes = new Dictionary<Vector3Int, Node>();
        Vector3Int tilePos = FindTopLeftTile(tilemap);
        int farLeft = tilePos.y;
        do {
            Vector3Int thisTilePos = new Vector3Int(tilePos.x, tilePos.y, 0);
            //if (thisTilePos.x != 0 && thisTilePos.y != 0) Debug.Log("thisTilePos: " + thisTilePos);
            nodes.Add(thisTilePos, new Node(thisTilePos));
            tilePos.y++;
            if (!tilemap.HasTile(tilePos)) 
            {
                Vector3Int extraRight = new Vector3Int(tilePos.x, tilePos.y + 1, 0);
                if (!tilemap.HasTile(extraRight)) { tilePos.x--; tilePos.y = farLeft; }
                else { tilePos = extraRight; }
            }
        } while (tilemap.HasTile(tilePos));
        //Generate edges
        foreach (KeyValuePair<Vector3Int, Node> keyNNode in nodes)
        {
            GenerateNodeEdges(keyNNode.Key, tilemap);
            myGraph.Add(keyNNode.Value);
        }
    }

    private void GenerateNodeEdges(Vector3Int tilePos, Tilemap tilemap) {
        TileBase currTile = tilemap.GetTile(tilePos);
        Vector3Int[] neighbors = FindNeighbors(tilePos, tilemap);
        for (int i = 0; i < neighbors.Length; i++) {
            if (nodes.ContainsKey(neighbors[i]))
            {
                Edge edge = new Edge(nodes[neighbors[i]], TileCost(tilePos, neighbors[i], tilemap));
                nodes[tilePos].edges.Add(edge);
            }
            else if (tilePos.x == 0 && tilePos.y == 0) {
                Debug.Log("Failure: " + i);
            }
        }
    }

    private Vector3Int[] FindNeighbors(Vector3Int tilePos, Tilemap tilemap) {
        bool evenColumn = tilePos.y % 2 == 0;
        Vector3Int[] neighbors = new Vector3Int[6];
        //Top Left
        neighbors[0] = evenColumn ? new Vector3Int(tilePos.x, tilePos.y - 1, 0) :
            new Vector3Int(tilePos.x + 1, tilePos.y - 1, 0);
        //Top
        neighbors[1] = new Vector3Int(tilePos.x + 1, tilePos.y, 0);
        //Top Right
        neighbors[2] = evenColumn ? new Vector3Int(tilePos.x, tilePos.y + 1, 0) :
            new Vector3Int(tilePos.x + 1, tilePos.y + 1, 0);
        //Bottom Right
        neighbors[3] = evenColumn ? new Vector3Int(tilePos.x - 1, tilePos.y + 1, 0) :
            new Vector3Int(tilePos.x, tilePos.y + 1, 0);
        //Bottom
        neighbors[4] = new Vector3Int(tilePos.x - 1, tilePos.y, 0);
        //Bottom Left
        neighbors[5] = evenColumn ? new Vector3Int(tilePos.x - 1, tilePos.y - 1, 0) :
            new Vector3Int(tilePos.x, tilePos.y - 1, 0);

        return neighbors;
    }

    private Vector3Int FindTopLeftTile(Tilemap tilemap) {
        bool notFinished = true;

        Vector3Int tilePos = new Vector3Int(0, 0, 0);
        TileBase selected = tilemap.GetTile(tilePos);


        //Moves Left as much as possible
        while (notFinished)
        {
            tilePos.y--;
            TileBase newSel = tilemap.GetTile(tilePos);
            notFinished = newSel != null;

            if (notFinished) selected = newSel;
            else tilePos.y++;
        }

        //Moves up as much as possible
        notFinished = true;
        while (notFinished)
        {
            tilePos.x++;
            TileBase newSel = tilemap.GetTile(tilePos);
            notFinished = newSel != null;

            if (notFinished) selected = newSel;
            else tilePos.x--;
        }
        Debug.Log("Selcted tile is pos: " + tilePos);

        return tilePos;
    }

    private float BaseTileCost(TileBase tile) {
        switch (tile.name) {
            case "Water": return 0.5f;
            case "Road": return 0.5f;
            case "Grass": return 1.0f;
            case "Desert": return 3.0f;
            case "Forest": return 5.0f;
            case "Mountain": return float.MaxValue;
            case "Random": return Random.value * 100;
            default: return 1000000000000;
        }
    }

    private float TileCost(Vector3Int start, Vector3Int end, Tilemap tilemap) {
        float startCost = BaseTileCost(tilemap.GetTile(start));
        float endCost = BaseTileCost(tilemap.GetTile(end));
        string startName = tilemap.GetTile(start).name;
        string endName = tilemap.GetTile(end).name;
        if (startName.Equals("Water") ^ endName.Equals("Water")) return Mathf.Max(startCost, endCost) * 3;
        else return endCost;
    }

    private void RunExample() {
        Stack<Node> path;
        int dTilesExplored;
        float dijkstraCost = myGraph.Dijkstras(nodes[new Vector3Int(1, -9, 0)], nodes[new Vector3Int(1, 9, 0)], out path, out dTilesExplored);
        string pathString = path.Count != 0 ? path.Pop().GetValue().ToString() : "";
        while (path.Count != 0)
        {
            pathString += ", " + path.Pop().GetValue().ToString();
        }
        Debug.Log("Dijkstra got a total cost of: " + dijkstraCost + "\nPath Taken: " + pathString);

        int gTilesExplored;
        float greedyCost = myGraph.GBS(nodes[new Vector3Int(1, -9, 0)], nodes[new Vector3Int(1, 9, 0)], out path, out gTilesExplored);
        pathString = path.Count != 0 ? path.Pop().GetValue().ToString() : "";
        while (path.Count != 0)
        {
            pathString += ", " + path.Pop().GetValue().ToString();
        }
        Debug.Log("GFS got a total cost of: " + greedyCost + "\nPath Taken: " + pathString);
    }

    private float FindAverage(Stack<float> items) {
        int count = items.Count;
        float total = 0;
        while (items.Count > 0) {
            float next = items.Pop();
            if (next == float.MaxValue) Debug.LogError("An infinite cost was found");
            total += next;
        }
        return total / count;
    }

    private float FindIntAverage(Stack<int> items) {
        int count = items.Count;
        float total = 0;
        while (items.Count > 0)
            total += items.Pop();
        return total / count;
    }
}
