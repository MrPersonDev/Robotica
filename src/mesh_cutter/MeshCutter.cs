using Godot;
using System;
using System.Collections.Generic;

public partial class MeshCutter : MeshInstance3D
{
    public static readonly int NO_CUT = -1;
    public static readonly float VERTEX_MATCH_THRESHOLD = 0.0001f;
    public static readonly float MAX_VERTEX_MERGE_DIST = 0.01f;
    const int ROUND_DIGITS = 4;

    private Mesh prevMesh;
    private MeshDataTool prevMdt;
    private MeshDataTool mdt;
    private Dictionary<Vector3, int> indexes;
    private float magnitude;
    private Vector3 axis;
    private bool cutting = false;
    private bool queued = false;
    private float queuedLength, queuedHeight, queuedWidth;

    public void SetMeshSize(float width, float height, float length)
    {
        try
        {
            if (prevMesh == null)
                prevMesh = Mesh;
            else
                Mesh = prevMesh;

            if (width != NO_CUT)
                SetMeshSize(width, Vector3.Right);
            if (height != NO_CUT)
                SetMeshSize(height, Vector3.Up);
            if (length != NO_CUT)
                SetMeshSize(length, Vector3.Forward);
        }
        catch (System.ObjectDisposedException) { }
    }

    private void SetMeshSize(float magnitude, Vector3 axis)
    {
        this.magnitude = magnitude;
        this.axis = axis;

        ArrayMesh arrayMesh = (ArrayMesh)Mesh;
        mdt = prevMdt;
        if (prevMdt == null)
        {
            mdt = new MeshDataTool();
            mdt.CreateFromSurface(arrayMesh, 0);
            prevMdt = mdt;
        }

        SurfaceTool st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);

        RemoveExcludedFaces(mdt, st);

        arrayMesh = st.Commit();

        mdt = new MeshDataTool();
        mdt.CreateFromSurface(arrayMesh, 0);
        LoadIndexesDictionary(mdt);

        FixMeshHoles(mdt, st, arrayMesh);

        arrayMesh = st.Commit();
        CallDeferred("set_mesh", arrayMesh);
    }

    public void SetMaterial(StandardMaterial3D material)
    {
        if (!IsInstanceValid(this))
            return;

        MaterialOverride = material;
    }

    private void RemoveExcludedFaces(MeshDataTool mdt, SurfaceTool st)
    {
        for (int face = 0; face < mdt.GetFaceCount(); face++)
        {
            if (IsFaceInBounds(mdt, face))
                AddFace(mdt, st, face);
        }
    }

    private bool IsFaceInBounds(MeshDataTool mdt, int face)
    {
        for (int v = 0; v < 3; v++)
        {
            Vector3 vertex = mdt.GetVertex(mdt.GetFaceVertex(face, v));
            if (!OutsideBounds(vertex))
                return true;
        }

        return false;
    }

    private void AddFace(MeshDataTool mdt, SurfaceTool st, int face)
    {
        for (int v = 0; v < 3; v++)
        {
            int idx = mdt.GetFaceVertex(face, v);
            st.SetUV(mdt.GetVertexUV(idx));
            st.SetNormal(mdt.GetVertexNormal(idx));
            st.AddVertex(LimitToBounds(mdt.GetVertex(idx)));
        }
    }

    private void FixMeshHoles(MeshDataTool mdt, SurfaceTool st, ArrayMesh mesh)
    {
        List<Vector3> vertices = new List<Vector3>(mesh.GetFaces());
        List<Edge> boundaryEdges = GetBoundaryEdges(vertices);
        List<EdgeGroup> edgeGroups = GetEdgeGroups(boundaryEdges);
        FillEdgeGroups(mdt, st, edgeGroups);
    }

    private Dictionary<Edge, int> GetEdgeCounts(List<Vector3> vertices)
    {
        Dictionary<Edge, int> edgeCounts = new Dictionary<Edge, int>();
        for (int i = 0; i < vertices.Count; i += 3)
        {
            Vector3 a = vertices[i], b = vertices[i + 1], c = vertices[i + 2];
            Edge ab = new Edge(a, b), bc = new Edge(b, c), ac = new Edge(a, c);

            edgeCounts[ab] = edgeCounts.ContainsKey(ab) ? edgeCounts[ab] + 1 : 1;
            edgeCounts[bc] = edgeCounts.ContainsKey(bc) ? edgeCounts[bc] + 1 : 1;
            edgeCounts[ac] = edgeCounts.ContainsKey(ac) ? edgeCounts[ac] + 1 : 1;
        }

        return edgeCounts;
    }

    private List<Edge> GetBoundaryEdges(List<Vector3> vertices)
    {
        Dictionary<Edge, int> edgeCounts = GetEdgeCounts(vertices);
        List<Edge> boundaryEdges = new List<Edge>();
        foreach (KeyValuePair<Edge, int> pair in edgeCounts)
        {
            // in a manifold mesh, all edges have 2 faces
            bool edgeHasOneFace = pair.Value == 1;
            if (edgeHasOneFace)
                boundaryEdges.Add(pair.Key);
        }

        return boundaryEdges;
    }

    private List<EdgeGroup> GetEdgeGroups(List<Edge> boundaryEdges)
    {
        Dictionary<int, List<Edge>> vertexEdges = new Dictionary<int, List<Edge>>();
        foreach (Edge edge in boundaryEdges)
        {
            int idx1 = GetIdx(edge.v1), idx2 = GetIdx(edge.v2);

            if (!vertexEdges.ContainsKey(idx1))
                vertexEdges[idx1] = new List<Edge>();
            if (!vertexEdges.ContainsKey(idx2))
                vertexEdges[idx2] = new List<Edge>();

            vertexEdges[idx1].Add(edge);
            vertexEdges[idx2].Add(edge);
        }

        HashSet<int> visited = new HashSet<int>();
        List<EdgeGroup> connectedEdges = new List<EdgeGroup>();
        foreach (int vertex in vertexEdges.Keys)
        {
            if (visited.Contains(vertex))
                continue;

            EdgeGroup group = new EdgeGroup();
            FindConnectedEdges(boundaryEdges, vertexEdges, vertex, visited, group);
            connectedEdges.Add(group);
        }

        return connectedEdges;
    }

    private void FindConnectedEdges(List<Edge> boundaryEdges, Dictionary<int, List<Edge>> vertexEdges,
                                    int vertex, HashSet<int> visited, EdgeGroup group)
    {
        visited.Add(vertex);

        if (vertexEdges[vertex].Count == 1)
        {
            Edge originEdge = vertexEdges[vertex][0];

            float minDist = float.MaxValue;
            Edge closestEdge = null;
            foreach (Edge edge in boundaryEdges)
            {
                if (edge == originEdge || edge.Overlaps(originEdge))
                    continue;

                float distToEdge = originEdge.DistanceTo(edge);
                if (distToEdge < minDist)
                {
                    minDist = distToEdge;
                    closestEdge = edge;
                }
            }

            if (minDist > MAX_VERTEX_MERGE_DIST)
                return;

            int idx1 = GetIdx(closestEdge.v1), idx2 = GetIdx(closestEdge.v2);
            int fartherVertex;
            if (vertexEdges[idx1].Count == 1)
                fartherVertex = idx2;
            else if (vertexEdges[idx2].Count == 1)
                fartherVertex = idx1;
            else
                return;

            group.AddEdge(closestEdge);
            visited.Add(idx1);
            visited.Add(idx2);

            FindConnectedEdges(boundaryEdges, vertexEdges, fartherVertex, visited, group);
            return;
        }

        foreach (Edge edge in vertexEdges[vertex])
        {
            int idx1 = GetIdx(edge.v1), idx2 = GetIdx(edge.v2);
            int neighbor = idx1 == vertex ? idx2 : idx1;

            if (!visited.Contains(neighbor))
            {
                group.AddEdge(edge);
                FindConnectedEdges(boundaryEdges, vertexEdges, neighbor, visited, group);
            }
        }
    }

    private void FillEdgeGroups(MeshDataTool mdt, SurfaceTool st, List<EdgeGroup> edgeGroups)
    {
        foreach (EdgeGroup edgeGroup in edgeGroups)
            FillEdgeGroup(mdt, st, edgeGroup);
    }

    private void FillEdgeGroup(MeshDataTool mdt, SurfaceTool st, EdgeGroup edgeGroup)
    {


        Vector2[] polygon = GetPolygonFromEdgeGroup(mdt, edgeGroup);
        int[] triangles = Geometry2D.TriangulatePolygon(polygon);

        for (int i = 0; i < triangles.Length; i += 3)
        {
            List<Vector3> verticesToAdd = new List<Vector3>();
            verticesToAdd.Add(Vector2To3(polygon[triangles[i]]));
            verticesToAdd.Add(Vector2To3(polygon[triangles[i + 1]]));
            verticesToAdd.Add(Vector2To3(polygon[triangles[i + 2]]));

            Vector3 triangleNormal = (verticesToAdd[2] - verticesToAdd[0]).Cross(verticesToAdd[1] - verticesToAdd[0]);

            AddTriangle(st, verticesToAdd, triangleNormal);
        }
    }

    private Vector2[] GetPolygonFromEdgeGroup(MeshDataTool mdt, EdgeGroup edgeGroup)
    {
        List<Vector2> polygon = new List<Vector2>();
        List<Vector3> vertices = edgeGroup.GetVertices();
        foreach (Vector3 vertex in vertices)
        {
            Vector2 vertex2D = Vector3To2(vertex);
            polygon.Add(vertex2D);
        }

        return polygon.ToArray();
    }

    private void AddTriangle(SurfaceTool st, List<Vector3> vertices, Vector3 normal)
    {
        foreach (Vector3 vertex in vertices)
        {
            st.SetNormal(normal);
            st.AddVertex(vertex);
        }
    }

    private void LoadIndexesDictionary(MeshDataTool mdt)
    {
        indexes = new Dictionary<Vector3, int>();
        for (int i = 0; i < mdt.GetVertexCount(); i++)
        {
            Vector3 vertex = mdt.GetVertex(i);
            Vector3 roundedVertex = RoundVector3(vertex);

            indexes[roundedVertex] = i;
        }
    }

    private int GetIdx(Vector3 vertex)
    {
        Vector3 vertexRounded = RoundVector3(vertex);

        if (indexes.ContainsKey(vertexRounded))
            return indexes[vertexRounded];

        return GetIdxLinear(vertex);
    }

    private int GetIdxLinear(Vector3 vertex)
    {
        for (int i = 0; i < mdt.GetVertexCount(); i++)
        {
            if (mdt.GetVertex(i).DistanceTo(vertex) < VERTEX_MATCH_THRESHOLD)
                return i;
        }

        throw new KeyNotFoundException(vertex.ToString());
    }

    private Vector2 Vector3To2(Vector3 vector)
    {
        if (axis == Vector3.Right)
            return new Vector2(vector.Z, vector.Y);
        else if (axis == Vector3.Up)
            return new Vector2(vector.X, vector.Z);
        else if (axis == Vector3.Forward)
            return new Vector2(vector.X, vector.Y);

        throw new Exception("Unknown axis");
    }

    private Vector3 Vector2To3(Vector2 vector)
    {
        Vector3 newVector;
        if (axis == Vector3.Right)
            newVector = new Vector3(0, vector.Y, vector.X);
        else if (axis == Vector3.Up)
            newVector = new Vector3(vector.X, 0, vector.Y);
        else if (axis == Vector3.Forward)
            newVector = new Vector3(vector.X, vector.Y, 0);
        else
            throw new Exception("Unknown axis");

        return newVector + magnitude * axis;
    }

    private Vector3 RoundVector3(Vector3 vector)
    {
        return new Vector3(RoundAxis(vector.X), RoundAxis(vector.Y), RoundAxis(vector.Z));
    }

    private float RoundAxis(float value)
    {
        return (float)Math.Round(value, ROUND_DIGITS);
    }

    private bool OutsideBounds(Vector3 vertex)
    {
        float length = (vertex * axis).Length();
        return length > magnitude;
    }

    private Vector3 LimitToBounds(Vector3 vertex)
    {
        if (!OutsideBounds(vertex))
            return vertex;

        vertex *= (Vector3.One - (axis.Abs()));

        Vector3 limit = axis * magnitude;

        return vertex + limit;
    }
}
