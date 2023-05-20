using Godot;
using System;
using System.Collections.Generic;

public class EdgeGroup
{
	public HashSet<int> Vertices = new HashSet<int>();
	public LinkedList<Edge> Edges = new LinkedList<Edge>();

	public void AddVertex(int vertex)
	{
		Vertices.Add(vertex);
	}

	public void AddVertices(IEnumerable<int> vertices)
	{
		foreach (int vertex in vertices)
			AddVertex(vertex);
	}

	public void AddEdge(Edge edge)
	{
		if (Edges.Count == 0)
		{
			Edges.AddFirst(edge);
			return;
		}

		if (Head().DistanceTo(edge) <= MeshCutter.MAX_VERTEX_MERGE_DIST)
			Edges.AddFirst(edge);
		else if (Tail().DistanceTo(edge) <= MeshCutter.MAX_VERTEX_MERGE_DIST)
			Edges.AddLast(edge);
	}

	public void AddEdges(LinkedList<Edge> edges)
	{
		foreach (Edge edge in edges)
			AddEdge(edge);
	}

	public float DistanceTo(EdgeGroup edgeGroup)
	{
		float minDistance = edgeGroup.Head().DistanceTo(Head());
		minDistance = Math.Min(minDistance, edgeGroup.Tail().DistanceTo(Head()));
		minDistance = Math.Min(minDistance, edgeGroup.Head().DistanceTo(Tail()));
		minDistance = Math.Min(minDistance, edgeGroup.Tail().DistanceTo(Tail()));

		return minDistance;
	}

	public List<Vector3> GetVertices()
	{
		List<Vector3> vertices = new List<Vector3>();
		
		LinkedListNode<Edge> node = Edges.First;
		if (node == null)
			return vertices;

		if (node.Next.Value.DistanceTo(node.Value.v1) <= MeshCutter.MAX_VERTEX_MERGE_DIST)
			vertices.Add(node.Value.v2);
		else if (node.Next.Value.DistanceTo(node.Value.v2) <= MeshCutter.MAX_VERTEX_MERGE_DIST)
			vertices.Add(node.Value.v1);
		else
			throw new Exception("Nodes are not connected");

		while (node != null)
		{
			if (node.Next == null)
			{
				if (vertices[vertices.Count-1].DistanceTo(node.Value.v1) <= MeshCutter.MAX_VERTEX_MERGE_DIST)
					vertices.Add(node.Value.v2);
				else if (vertices[vertices.Count-1].DistanceTo(node.Value.v2) <= MeshCutter.MAX_VERTEX_MERGE_DIST)	
					vertices.Add(node.Value.v1);
				else
					throw new Exception("Nodes are not connected");
				
				break;
			}

			if (node.Next.Value.DistanceTo(node.Value.v2) <= MeshCutter.MAX_VERTEX_MERGE_DIST)
				vertices.Add(node.Value.v2);
			else if (node.Next.Value.DistanceTo(node.Value.v1) <= MeshCutter.MAX_VERTEX_MERGE_DIST)
				vertices.Add(node.Value.v1);
			else
				throw new Exception("Nodes are not connected");

			node = node.Next;
		}

		return vertices;
	}

	public bool IsClosed()
	{
		return Head().Overlaps(Tail());
	}

	public Edge Head()
	{
		return Edges.First.Value;
	}

	public Edge Tail()
	{
		return Edges.Last.Value;
	}

    public override string ToString()
    {
        String str = "";
		foreach (Edge edge in Edges)
		{
			str += edge.ToString() + " ";
		}

		return str;
    }
}
