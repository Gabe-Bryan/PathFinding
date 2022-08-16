using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Graph<UnityEngine.Vector3Int>;
using static Graph<UnityEngine.Vector3Int>.Node;

public class PathFinder : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CreateAGraph();
    }

    private Graph<Vector3Int> CreateAGraph() {
        Node[] nodes = new Node[13];
        for (int i = 0; i < nodes.Length; i++) {
            nodes[i] = new Node(new Vector3Int(0,0,0));
        }
        //s=0, a=1, b=2, ... , l = 12
        ArrayList nodeList = new ArrayList(nodes);
        Graph<Vector3Int> graph = new Graph<Vector3Int>(nodeList);

        //S edges
        nodes[0].edges.Add(new Edge(nodes[1], 7));
        nodes[0].edges.Add(new Edge(nodes[2], 2));
        nodes[0].edges.Add(new Edge(nodes[3], 3));

        //A Edges
        nodes[1].edges.Add(new Edge(nodes[2], 3));
        nodes[1].edges.Add(new Edge(nodes[0], 7));
        nodes[1].edges.Add(new Edge(nodes[4], 4));

        //B Edges
        nodes[2].edges.Add(new Edge(nodes[0], 2));
        nodes[2].edges.Add(new Edge(nodes[1], 3));
        nodes[2].edges.Add(new Edge(nodes[4], 4));
        nodes[2].edges.Add(new Edge(nodes[8], 1));

        //C Edges
        nodes[3].edges.Add(new Edge(nodes[0], 3));
        nodes[3].edges.Add(new Edge(nodes[12], 2));

        //D Edges
        nodes[4].edges.Add(new Edge(nodes[1], 4));
        nodes[4].edges.Add(new Edge(nodes[2], 4));
        nodes[4].edges.Add(new Edge(nodes[6], 5));

        //E Edges
        nodes[5].edges.Add(new Edge(nodes[11], 5));
        nodes[5].edges.Add(new Edge(nodes[7], 2));

        //F Edges
        nodes[6].edges.Add(new Edge(nodes[4], 5));
        nodes[6].edges.Add(new Edge(nodes[8], 3));

        //G Edges
        nodes[7].edges.Add(new Edge(nodes[5], 2));
        nodes[7].edges.Add(new Edge(nodes[8], 2));

        //H Edges
        nodes[8].edges.Add(new Edge(nodes[2], 1));
        nodes[8].edges.Add(new Edge(nodes[6], 3));
        nodes[8].edges.Add(new Edge(nodes[7], 2));

        //I Edges
        nodes[9].edges.Add(new Edge(nodes[10], 6));
        nodes[9].edges.Add(new Edge(nodes[11], 4));
        nodes[9].edges.Add(new Edge(nodes[12], 4));

        //J Edges
        nodes[10].edges.Add(new Edge(nodes[9], 6));
        nodes[10].edges.Add(new Edge(nodes[11], 4));
        nodes[10].edges.Add(new Edge(nodes[12], 4));

        //K Edges
        nodes[11].edges.Add(new Edge(nodes[5], 5));
        nodes[11].edges.Add(new Edge(nodes[9], 4));
        nodes[11].edges.Add(new Edge(nodes[10], 4));

        //L Edges
        nodes[12].edges.Add(new Edge(nodes[3], 2));
        nodes[12].edges.Add(new Edge(nodes[9], 4));
        nodes[12].edges.Add(new Edge(nodes[10], 4));

        //Debug.Log(((Node)nodeList[0]).edges.Count);
        Stack<Node> pathChosen;
        Debug.Log("cost: " + graph.Dijkstras(nodes[0], nodes[5], out pathChosen));
        Debug.Log("greedy cost: " + graph.GreedyFirstSearch(nodes[0], nodes[5], out pathChosen));

        return graph;
    }
}
