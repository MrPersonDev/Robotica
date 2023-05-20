using Godot;
using System;
using System.Collections.Generic;

public partial class PartGroup : Moveable
{
    private HashSet<Part> children = new HashSet<Part>();
    private Vector3 center = Vector3.Zero;

    [ExportGroup("Properties")]
    [Export]
    private Material material;

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath centerPath;

    public void UpdateCenter()
    {
        Vector3 sumOfPositions = new Vector3(0.0f, 0.0f, 0.0f);
        foreach (Part part in GetParts())
        {
            sumOfPositions += part.GetCenter();
        }

        int numParts = Math.Max(GetParts().Count, 1);
        Vector3 averagePos = sumOfPositions /= numParts;
        center = averagePos;
    }

    public override void UpdateCollisionTransform()
    {
        foreach (Part part in GetParts())
            part.UpdateCollisionTransform();
    }

    public override void MoveTo(Vector3 pos, bool meshOnly = false)
    {
        Vector3 displacement = pos - GetCenter();

        List<Part> parts = new List<Part>(GetParts());        
        foreach (Part part in parts)
            part.MoveTo(part.GetCenter() + displacement, meshOnly);
    }

    public override void RotateCenter(Vector3 axis, float rotation, bool meshOnly = false)
    {
        Vector3 center = GetCenter();

        foreach (Part part in GetParts())
            part.RotatePos(axis, rotation, center, meshOnly);
    }

    public override void RotatePos(Vector3 axis, float rotation, Vector3 pos, bool meshOnly = false)
    {
        foreach (Part part in GetParts())
            part.RotatePos(axis, rotation, pos, meshOnly);
    }

    public override void EnableMeshCollider()
    {
        foreach (Part part in GetParts())
            part.EnableMeshCollider();
    }

    public override void DisableMeshCollider()
    {
        foreach (Part part in GetParts())
            part.DisableMeshCollider();
    }

    public override void EnableColliders()
    {
        foreach (Part part in GetParts())
            part.EnableColliders();
    }

    public override void DisableColliders(bool disableInserts)
    {
        foreach (Part part in GetParts())
            part.DisableColliders(disableInserts);
    }

    public override void Select()
    {
        foreach (Part part in GetParts())
            part.Select();
    }

    public override void Deselect()
    {
        foreach (Part part in GetParts())
            part.Deselect();
    }

    public override void Hover()
    {
        foreach (Part part in GetParts())
            part.Hover();
    }

    public override void Unhover()
    {
        foreach (Part part in GetParts())
            part.Unhover();
    }

    public void AddPart(Part child, bool changeGroup)
    {
        if (GetParts().Contains(child))
            return;

        children.Add(child);
        if (changeGroup)
            child.SetPartGroup(this);
    }

    public void RemovePart(Part child, bool changeGroup)
    {
        children.Remove(child);
        if (changeGroup)
            child.SetPartGroup(null);
    }

    public override void PrepareToDelete()
    {
        foreach (Part part in GetParts())
            part.PrepareToDelete();
    }

    public override Vector3 GetCenter()
    {
        UpdateCenter();
        return center;
    }

    public bool HasParts()
    {
        return GetParts().Count > 0;
    }

    public override bool HasDefaultHole()
    {
        return false;
    }

    public override HoleBody GetDefaultHoleBody()
    {
        return null;
    }

    public override bool HasInsert()
    {
        foreach (Part part in GetParts())
        {
            if (part.HasInsert())
                return true;
        }

        return false;
    }

    public override bool HasFasteners()
    {
        foreach (Part part in GetParts())
        {
            if (part.HasFastenerHoles())
                return true;
        }

        return false;
    }

    public override List<Part> GetParts()
    {
        return new List<Part>(children);
    }
}
