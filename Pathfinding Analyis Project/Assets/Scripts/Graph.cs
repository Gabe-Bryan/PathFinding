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
        Dictionary<Node, KeyValuePair<Node, float>> parents = new Dictionary<Node, KeyValuePair<Node, float>>();
        PriorityQueue<Node, float> open = new PriorityQueue<Node, float>();
        Dictionary<Node, float> closed = new Dictionary<Node, float>();
        
        path = new Stack<Node>();

        open.Enqueue(start, 0);
        //loop while end node isn't in the open list
        while (open.Count != 0)
        {
            float priority;
            Node element;
            //remove node n with lowest cost and place it in close list
            do {
                open.TryDequeue(out element, out priority);
            } while (closed.ContainsKey(element));
            KeyValuePair<Node, float> minNode = new KeyValuePair<Node, float>(element, priority);
            //expand the node n and add successors to open list
            //look to see if any node is the end node
            //updating any better values
            if (GreedyRelax(minNode.Key, end, open, closed, parents))
            {
                closed.Add(minNode.Key, minNode.Value);
                return GreedyRetrace(parents, closed, start, end, path);
            }
            else
            {
                GreedyRelax(minNode.Key, end, open, closed, parents);
                closed.Add(minNode.Key, minNode.Value);
            }
        }
        return -1;
    }

    /**
     * @returns true when the end node is an edge
     */
    private bool GreedyRelax(Node vertex, Node end, PriorityQueue<Node, float> open, Dictionary<Node, float> closed, Dictionary<Node, KeyValuePair<Node, float>> parents) {
        foreach (Node.Edge edge in vertex.edges) {
            if (edge.neighbor == end) {
                parents.Add(end, new KeyValuePair<Node, float>(vertex, edge.weight));
                closed.Add(end, edge.weight);
                return true;
            }
            if (!closed.ContainsKey(edge.neighbor)) {
                bool alreadyProblem = closed.ContainsKey(end);

                open.Enqueue(edge.neighbor, edge.weight);
                bool containsKey = parents.ContainsKey(edge.neighbor);
                if (containsKey && parents[edge.neighbor].Value > edge.weight) parents[edge.neighbor] = new KeyValuePair<Node, float>(vertex, edge.weight);
                else if (!containsKey) parents.Add(edge.neighbor, new KeyValuePair<Node, float>(vertex, edge.weight));

                if (closed.ContainsKey(end)) Debug.Log("hello problem! Edge.neighbor == end: " + (edge.neighbor == end) + " was it a problem before? " + alreadyProblem);
            }
        }
        return false;
    }

    /**
     * <summary> Creates the path that was taken during the search </summary>
     * <returns> The total cost of the path </returns>
     */
    private float GreedyRetrace(Dictionary<Node, KeyValuePair<Node, float>> parents, Dictionary<Node, float> cost, Node start, Node end, Stack<Node> path) {
        float totalCost = 0;
        totalCost += cost[end];
        path.Push(end);
        Node next = parents[end].Key;
        if (!parents.ContainsKey(end)) Debug.Log("shit...");
        int i = 0;
        while (next != null) {
            try
            {
                if (next != start) totalCost += parents[next].Value;
            }
            catch (KeyNotFoundException e) {
                Debug.LogError("i: " + i + " closed count: " + cost.Count);
            }
            path.Push(next);
            next = parents.ContainsKey(next) ? parents[next].Key : null;
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
        Dictionary<Node, KeyValuePair<Node, float>> parents = new Dictionary<Node, KeyValuePair<Node, float>>();
        //initialize correct shortest path set (empty)
        Dictionary<Node, float> correctP = new Dictionary<Node, float>();
        //initialize estimated shortest path set (empty)
        PriorityQueue<Node, float> estP = new PriorityQueue<Node, float>();
        //initialize start to have length 0 and add it to correct
        correctP.Add(start, 0);
        Relax(start, 0, correctP, estP, parents);
        //loops until cost is determined for end node
        while (!correctP.ContainsKey(end)) {
            //find an element t with the least estimate in the estimate set
            float priority;
            Node element;
            do
            {
                estP.TryDequeue(out element, out priority);
            } while (correctP.ContainsKey(element));
            KeyValuePair<Node, float> smallestCost = new KeyValuePair<Node, float>(element, priority);
            //add t to correct shortest path
            correctP.Add(smallestCost.Key, smallestCost.Value);
            //Relax(t)
            Relax(smallestCost.Key, smallestCost.Value, correctP, estP, parents);
        }
        float rValue;
        bool success = correctP.TryGetValue(end, out rValue);
        path = RetracePath(parents, end);
        return rValue;
    }

    private Stack<Node> RetracePath(Dictionary<Node, KeyValuePair<Node, float>> parents, Node end) {
        Stack<Node> path = new Stack<Node>();
        path.Push(end);
        Node next = parents[end].Key;
        while (next != null) {
            path.Push(next);
            next = parents.ContainsKey(next) ? parents[next].Key : null;
        }
        return path;
    }

    private void Relax(Node vertex, float vCost, Dictionary<Node, float> correctP, PriorityQueue<Node, float> estP, Dictionary<Node, KeyValuePair<Node, float>> parents) {

        foreach (Node.Edge edge in vertex.edges) {
            if (!correctP.ContainsKey(edge.neighbor)) {
                float estCost = vCost + edge.weight;

                estP.Enqueue(edge.neighbor, estCost);
                bool containsKey = parents.ContainsKey(edge.neighbor);
                if (containsKey && parents[edge.neighbor].Value > estCost) parents[edge.neighbor] = new KeyValuePair<Node, float>(vertex, estCost);
                else if (!containsKey) parents.Add(edge.neighbor, new KeyValuePair<Node, float>(vertex, estCost));
            }
        }
    }
}
