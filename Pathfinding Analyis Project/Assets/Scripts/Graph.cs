using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph<T>
{
    public class Node
    {
        public class Edge 
        {
            public Node neighbor;
            public float weight;
            public Edge(Node _neighbor, float _weight) {
                neighbor = _neighbor; weight = _weight;
            }
        }

        public ArrayList edges;
        private T value;
        public Node(T _value) {
            edges = new ArrayList();
            value = _value;
        }
        public T GetValue() { return value; }
    }

    private ArrayList nodes = new ArrayList();

    public Graph(ArrayList startingNodes) {
        nodes = startingNodes;
    }

    public void Add(Node node) {
        nodes.Add(node);
    }

    /**
     * <returns>-1 if there was no valid path found</returns>
     */
    public float GreedyFirstSearch(Node start, Node end, out Stack<Node> path) {
        //create OPEN and CLOSED list
        Dictionary<Node, Node> parents = new Dictionary<Node, Node>();
        Dictionary<Node, float> open = new Dictionary<Node, float>();
        Dictionary<Node, float> closed = new Dictionary<Node, float>();
        
        path = new Stack<Node>();

        open.Add(start, 0);
        //loop while end node isn't in the open list
        while (open.Count != 0)
        {
            //remove node n with lowest cost and place it in close list
            KeyValuePair<Node, float> minNode = new KeyValuePair<Node, float>(null, -1);
            foreach (KeyValuePair<Node, float> pair in open) {
                if (minNode.Value == -1 || minNode.Value > pair.Value) minNode = pair;
            }
            //expand the node n and add successors to open list
            //look to see if any node is the end node
            //updating any better values
            if (GreedyRelax(minNode.Key, end, open, closed, parents))
            {
                closed.Add(minNode.Key, minNode.Value);
                closed.Add(end, 0);
                return GreedyRetrace(parents, closed, start, end, path);
            }
            else
            {
                Debug.Log("YAH!");
                GreedyRelax(minNode.Key, end, open, closed, parents);
                closed.Add(minNode.Key, minNode.Value);
                open.Remove(minNode.Key);
            }
        }
        return -1;
    }

    /**
     * @returns true when the end node is an edge
     */
    private bool GreedyRelax(Node vertex, Node end, Dictionary<Node, float> open, Dictionary<Node, float> closed, Dictionary<Node, Node> parents) {
        foreach (Node.Edge edge in vertex.edges) {
            if (edge.neighbor == end) {
                parents.Add(end, vertex);
                return true;
            }
            if (!closed.ContainsKey(edge.neighbor)) {
                float cost;
                bool isInOpen = open.TryGetValue(edge.neighbor, out cost);
                if (!isInOpen){
                    open.Add(edge.neighbor, edge.weight);
                    parents.Add(edge.neighbor, vertex);
                }
                else if (edge.weight < cost) {
                    open[edge.neighbor] = edge.weight;
                    parents[edge.neighbor] = vertex;
                }
            }
        }
        return false;
    }

    /**
     * <summary> Creates the path that was taken during the search </summary>
     * <returns> The total cost of the path </returns>
     */
    private float GreedyRetrace(Dictionary<Node, Node> parents, Dictionary<Node, float> cost, Node start, Node end, Stack<Node> path) {
        float totalCost = 0;
        totalCost += cost[end];
        path.Push(end);
        Node next = parents[end];
        if (!parents.ContainsKey(end)) Debug.Log("shit...");
        int i = 0;
        while (next != null) {
            try
            {
                if (next != start) totalCost += cost[next];
            }
            catch (KeyNotFoundException e) {
                Debug.LogError("i: " + i + " closed count: " + cost.Count);
            }
            path.Push(next);
            next = parents.ContainsKey(next) ? parents[next] : null;
            i++;
        }

        return totalCost;
    }

    /**
     * <param name="path">an out variable for storing the chosen path.</param>
     * <returns>the cost, or -1 if the path is not possible</returns>
     */
    public float Dijkstras(Node start, Node end, out Stack<Node> path) {
        //TODO: Replace hashtables with min heaps
        if (start == end) { path = new Stack<Node>(); path.Push(start); return 0; }
        //Initialize the hashmap of parents key = child, value = parent
        Dictionary<Node, Node> parents = new Dictionary<Node, Node>();
        //initialize correct shortest path set (empty)
        Dictionary<Node, float> shortP = new Dictionary<Node, float>();
        //initialize estimated shortest path set (empty)
        Dictionary<Node, float> estP = new Dictionary<Node, float>();
        //initialize start to have length 0 and add it to correct
        shortP.Add(start, 0);
        Relax(start, 0, shortP, estP, parents);
        //loops until cost is determined for end node
        while (!shortP.ContainsKey(end)) {
            //find an element t with the least estimate in the estimate set
            KeyValuePair<Node, float> smallestCost = new KeyValuePair<Node, float>(null, -1);
            foreach (KeyValuePair<Node, float> v in estP) {
                if (smallestCost.Key == null || v.Value < smallestCost.Value) smallestCost = v;
            }
            //add t to correct shortest path
            shortP.Add(smallestCost.Key, smallestCost.Value);
            estP.Remove(smallestCost.Key);
            //Relax(t)
            Relax(smallestCost.Key, smallestCost.Value, shortP, estP, parents);
        }
        float rValue;
        bool success = shortP.TryGetValue(end, out rValue);
        path = RetracePath(parents, end);
        return rValue;
    }

    private Stack<Node> RetracePath(Dictionary<Node, Node> parents, Node end) {
        Stack<Node> path = new Stack<Node>();
        path.Push(end);
        Node next = parents[end];
        while (next != null) {
            path.Push(next);
            next = parents.ContainsKey(next) ? parents[next] : null;
        }
        return path;
    }

    private void Relax(Node vertex, float vCost, Dictionary<Node, float> shortP, Dictionary<Node, float> estP, Dictionary<Node, Node> parents) {

        foreach (Node.Edge edge in vertex.edges) {
            if (!shortP.ContainsKey(edge.neighbor)) {
                float estCost = vCost + edge.weight;
                float origCost;
                if (!estP.TryGetValue(edge.neighbor, out origCost)) {
                    estP.Add(edge.neighbor, estCost);
                    parents.Add(edge.neighbor, vertex);
                } else if(origCost > estCost){
                    estP[edge.neighbor] = estCost;
                    parents[edge.neighbor] = vertex;
                }
            }
        }
    }
}
