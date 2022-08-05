using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Graph.Node;

public class Graph
{
    public class Node 
    {
        public class Edge 
        {
            public Node neighbor;
            public int weight;
            public Edge(Node _neighbor, int _weight) {
                neighbor = _neighbor; weight = _weight;
            }
        }

        public ArrayList edges;
        public Node() {
            edges = new ArrayList();
        }
    }

    ArrayList nodes = new ArrayList();

    public Graph(ArrayList startingNodes) {
        nodes = startingNodes;
    }

    /**
     * @param path is an out variable for storing the chosen path.
     */
    public int FindShortestPath(Node start, Node end, LinkedList<Node> path) {
        //TODO: Replace hashtables with min heaps
        if (start == end) return 0;
        //initialize correct shortest path set (empty)
        Dictionary<Node, int> shortP = new Dictionary<Node, int>();
        //initialize estimated shortest path set (empty)
        Dictionary<Node, int> estP = new Dictionary<Node, int>();
        //initialize start to have length 0 and add it to correct
        shortP.Add(start, 0);
        Relax(start, 0, shortP, estP);
        //loops until cost is determined for end node
        while (!shortP.ContainsKey(end)) {
            Debug.Log("Started while loop");
            //find an element t with the least estimate in the estimate set
            KeyValuePair<Node, int> smallestCost = new KeyValuePair<Node, int>(null, -1);
            foreach (KeyValuePair<Node, int> v in estP) {
                if (smallestCost.Key == null || v.Value < smallestCost.Value) smallestCost = v;
            }
            shortP.Add(smallestCost.Key, smallestCost.Value);
            estP.Remove(smallestCost.Key);
            Relax(smallestCost.Key, smallestCost.Value, shortP, estP);
            //add t to correct shortest path
            //Relax(t)
        }
        int rValue;
        bool success = shortP.TryGetValue(end, out rValue);
        return rValue;
    }

    private void Relax(Node vertex, int vCost, Dictionary<Node, int> shortP, Dictionary<Node, int> estP) {

        foreach (Edge edge in vertex.edges) {
            if (!shortP.ContainsKey(edge.neighbor)) {
                int estCost = vCost + edge.weight;
                int origCost;
                if (!estP.TryGetValue(edge.neighbor, out origCost)) {
                    estP.Add(edge.neighbor, estCost);
                } else if(origCost > estCost){
                    estP[edge.neighbor] = estCost;
                }
            }
        }
    }
}
