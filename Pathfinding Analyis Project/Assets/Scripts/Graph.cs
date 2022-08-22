using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VNode = Graph<UnityEngine.Vector3Int>.Node;

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

    public float GBS(VNode start, VNode end, out Stack<VNode> path, out int tilesExplored) {
        Dictionary<VNode, KeyValuePair<VNode, float>> parents = new Dictionary<VNode, KeyValuePair<VNode, float>>();
        //first VNode is the node, 
        PriorityQueue<VNode, float> open = new PriorityQueue<VNode, float>();
        Dictionary<VNode, float> closed = new Dictionary<VNode, float>();

        open.Enqueue(start, 0);
        parents.Add(start, new KeyValuePair<VNode, float>(null, 0));
        int i = 0;
        while (open.Count > 0) {
            VNode currentNode;
            float currentHeuristic;
            //continues to remove nodes from the queue until one that isnt already closed is found
            do{ 
                open.TryDequeue(out currentNode, out currentHeuristic);
                if (currentHeuristic == float.MaxValue) Debug.LogError("GBS added an infinite cost to closed");
            } while (closed.ContainsKey(currentNode));
            //Next two lines marks the node as visited and adds neighbors to the open priority queue
            closed.Add(currentNode, currentHeuristic);
            int newTilesExplored;
            if(MarkNode(currentNode, end, parents, closed, open)) break;
            i++;
        }
        //if (open.Count <= 0) Debug.LogError("You made it throw " + i + " full iterations");
        path = new Stack<VNode>();
        tilesExplored = parents.Count;
        return RetraceGBS(path, end, parents);
    }

    private bool MarkNode(VNode node, VNode end, Dictionary<VNode, KeyValuePair<VNode, float>> parents, Dictionary<VNode, float> closed, PriorityQueue<VNode, float> open) {
        foreach (VNode.Edge edge in node.edges) {
            if (!closed.ContainsKey(edge.neighbor)) {
                float cost = Vect3Heuristic(edge.neighbor, end, edge);
                if (cost > 1_000_000) continue;
                if (edge.neighbor == end) {
                    parents.Add(end, new KeyValuePair<VNode, float>(node, cost));
                    return true;
                } else if (!parents.ContainsKey(edge.neighbor)){
                    parents.Add(edge.neighbor, new KeyValuePair<VNode, float>(node, cost));
                } else if (parents[edge.neighbor].Value > cost){
                    parents[edge.neighbor] = new KeyValuePair<VNode, float>(node, cost);
                } else{
                    continue;
                }
                open.Enqueue(edge.neighbor, cost);
            }
        }
        //Debug.Log("Queue is loaded with " + open.Count);
        return false;
    }

    private float GetWeight(VNode a, VNode b) {
        int i = 0;
        foreach (VNode.Edge edge in a.edges) { 
            if(edge.neighbor == b) return edge.weight;
        }
        throw new Exception("Error: No connection found between a = " + a.GetValue() + " and b = " + b.GetValue());
    }

    private float Vect3Heuristic(VNode node, VNode end, VNode.Edge edgeToStart) {
        Vector3Int startPos = node.GetValue();
        Vector3Int endPos = end.GetValue();
        return Vector3Int.Distance(startPos, endPos) * 2 + edgeToStart.weight;
    }

    private float RetraceGBS(Stack<VNode> path, VNode end, Dictionary<VNode, KeyValuePair<VNode, float>> parents) {
        if (!parents.ContainsKey(end)) throw new Exception("There is no end node in the parents dictionary!!!");
        float total = 0;
        VNode prev = end;
        KeyValuePair<VNode, float> currentPair = parents[end];
        while (currentPair.Key != null){
            if (prev == currentPair.Key) Debug.LogError("Parents has self cycle");
            if (currentPair.Value >= 100_000) Debug.LogError("An edge is infinite");
            path.Push(prev);
            total += GetWeight(currentPair.Key, prev);
            prev = currentPair.Key;
            currentPair = parents[currentPair.Key];
        }
        path.Push(prev);

        return total;
    }

    /**
     * <param name="path">an out variable for storing the chosen path.</param>
     * <returns>the cost, or -1 if the path is not possible</returns>
     */
    public float Dijkstras(Node start, Node end, out Stack<Node> path, out int tilesExplored) {
        if (start == end) { path = new Stack<Node>(); path.Push(start); tilesExplored = 1; return 0; }
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
        tilesExplored = parents.Count;
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
