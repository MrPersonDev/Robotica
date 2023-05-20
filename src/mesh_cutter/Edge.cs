using Godot;
using System;

public class Edge
{
    public Vector3 v1, v2;

    public Edge(Vector3 v1, Vector3 v2)
    {
        this.v1 = v1;
        this.v2 = v2;
    }

    public bool Contains(Vector3 vertex)
    {
        return vertex.DistanceTo(v1) <= MeshCutter.VERTEX_MATCH_THRESHOLD || vertex.DistanceTo(v2) <= MeshCutter.VERTEX_MATCH_THRESHOLD;
    }

    public bool Overlaps(Edge edge)
    {
        return Contains(edge.v1) || Contains(edge.v2);
    }

    public float DistanceTo(Edge edge)
    {
        float minDistance = edge.v1.DistanceTo(v1);
        minDistance = Math.Min(minDistance, edge.v1.DistanceTo(v1));
        minDistance = Math.Min(minDistance, edge.v1.DistanceTo(v2));
        minDistance = Math.Min(minDistance, edge.v2.DistanceTo(v1));
        minDistance = Math.Min(minDistance, edge.v2.DistanceTo(v2));

        return minDistance;
    }

    public float DistanceTo(Vector3 vertex)
    {
        float minDistance = v1.DistanceTo(vertex);
        minDistance = Math.Min(minDistance, v2.DistanceTo(vertex));

        return minDistance;
    }

    public override string ToString()
    {
        return '(' + v1.ToString() + ", " + v2.ToString() + ')';
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Edge))
            return false;

        Edge edge = (Edge)obj;
        return (v1.Equals(edge.v1) && v2.Equals(edge.v2)) || (v1.Equals(edge.v2) && v2.Equals(edge.v1));
    }

    public override int GetHashCode()
    {
        return v1.GetHashCode() ^ v2.GetHashCode();
    }
}