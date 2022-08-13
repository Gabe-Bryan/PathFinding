using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Graph<UnityEngine.Vector3Int>;
using static Graph<UnityEngine.Vector3Int>.Node;

public class GridToGraph : MonoBehaviour
{
    //Types vectNode = Graph<Vector3Int>.Node<Vector3Int>;

    private Graph<Vector3Int> myGraph;
    private Vector2 size;
    private Dictionary<Vector3Int, Node> nodes;
    private void Start()
    {
        GenerateGraph();
    }

    private Graph<Vector3Int> GenerateGraph() {
        Tilemap tilemap = gameObject.GetComponentInChildren<Tilemap>();
        size = tilemap.cellSize;
        BoundsInt mapBounds = tilemap.cellBounds;
        Debug.Log("X: " + (mapBounds.xMax - mapBounds.xMin) + " Y: " + (mapBounds.yMax - mapBounds.yMin));

        myGraph = new Graph<Vector3Int>(new ArrayList());

        /*Vector3Int[] neighbors = FindNeighbors(FindTopLeftTile(tilemap), tilemap);
        for (int i = 0; i < neighbors.Length; i++) {
            if(neighbors[i] != null) Debug.Log("Neighbor[" + i + "]: " + tilemap.GetTile(neighbors[i]));// + " and it costs: " + BaseTileCost(neighbors[i]));
        }*/
        GenerateNodes(tilemap);
        foreach (KeyValuePair<Vector3Int, Node> keyNNode in nodes) {
            //Debug.Log(keyNNode.Key + " " + keyNNode.Value);
            GenerateNodeEdges(keyNNode.Key, tilemap);
            myGraph.Add(keyNNode.Value);
        }
        Stack<Node> path;
        float cost = myGraph.Dijkstras(nodes[new Vector3Int(1, -9, 0)], nodes[new Vector3Int(1, 9, 0)], out path);
        string pathChosen = "";
        pathChosen += path.Pop().GetValue();
        while (path.Count != 0) pathChosen += ", " + path.Pop().GetValue();
        Debug.Log("Cost for tilemap path (Dijkstras): " + cost + "\nPath:" + pathChosen);

        //Greedy
        Stack<Node> greedyPath;
        float greedyCost = myGraph.GreedyFirstSearch(nodes[new Vector3Int(1, -9, 0)], nodes[new Vector3Int(1, 9, 0)], out greedyPath);
        Debug.Log("Greedy cost: " + greedyCost);

        return myGraph;
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
        /*string edges = "";
        foreach (Edge edge in nodes[tilePos].edges) {
            edges += edge.weight + "\n";
        }
        Debug.Log("Edges for " + tilePos + " has " + nodes[tilePos].edges.Count + " edges: \n" + edges);*/
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
            case "Desert": return 2.0f;
            case "Forest": return 3.0f;
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
}
