using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class Insert : Area3D
{
    private const float MAX_SNAPPING_DIST = 0.4f;
    private const float MAX_INCLUDED_DIST = 0.01f;
    private const float CHECKING_WIDTH = 0.25f;

    private HashSet<HoleBody> potentiallyCollidingHoleBodies = new HashSet<HoleBody>();
    private HashSet<HoleBody> actuallyCollidingHoleBodies = new HashSet<HoleBody>();

    [ExportGroup("Properties")]
    [Export]
    private float length = 0.263f;
    [Export]
    private float width = 0.5f;

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath colliderPath;

    private CylinderShape3D colliderShape;
    private CollisionShape3D collider;

    public override void _Ready()
    {
        collider = (CollisionShape3D)GetNode(colliderPath);

        SetColliderShape();
        SetEvents();
    }

    private void SetColliderShape()
    {
        colliderShape = new CylinderShape3D();
        colliderShape.Radius = width;
        colliderShape.Height = length;
        collider.Shape = colliderShape;
    }

    private void SetEvents()
    {
        BodyEntered += (Node3D body) => EnteredHandler(body);
        BodyExited += (Node3D body) => ExitedHandler(body);
    }

    public void SetLength(float length)
    {
        CylinderShape3D shape = (CylinderShape3D)collider.Shape;
        shape.Height = length;
    }

    private void EnteredHandler(Node3D body)
    {
        if (!body.IsInGroup("HOLE_COLLIDER"))
            return;

        HoleBody holeBody = (HoleBody)body.GetParent();
        potentiallyCollidingHoleBodies.Add(holeBody);
        actuallyCollidingHoleBodies.Add(holeBody);
    }

    private void ExitedHandler(Node3D body)
    {
        if (!body.IsInGroup("HOLE_COLLIDER"))
            return;

        HoleBody holeBody = (HoleBody)body.GetParent();
        actuallyCollidingHoleBodies.Remove(holeBody);
    }

    public void EnableColliders()
    {
        if (!IsInstanceValid(collider))
            return;

        collider.Disabled = false;
    }

    public void DisableColliders()
    {
        if (!IsInstanceValid(collider))
            return;

        collider.Disabled = true;
    }

    public void IncreaseWidth()
    {
        colliderShape.Radius = CHECKING_WIDTH;
    }

    public void ResetWidth()
    {
        colliderShape.Radius = width;
    }

    public List<Part> GetCurCollidingParts()
    {
        List<Part> collidingParts = new List<Part>();
        foreach (Node3D node in GetOverlappingBodies())
        {
            if (!node.IsInGroup("PART_COLLIDER"))
                continue;

            Part part = (Part)node.GetParent();
            collidingParts.Add(part);
        }

        return collidingParts;
    }

    public List<HoleBody> GetCollidingHoleBodies()
    {
        // List<HoleBody> holeBodies = new List<HoleBody>();
        // foreach (Node3D node in GetOverlappingBodies())
        // {
        //     if (!(node is HoleBody))
        //         continue;

        //     HoleBody holeBody = (HoleBody)node;
        //     if (ShouldIncludeHoleBody(holeBody))
        //         holeBodies.Add(holeBody);
        // }

        List<HoleBody> holeBodies = new List<HoleBody>(actuallyCollidingHoleBodies);

        return holeBodies;
    }

    public void UpdatePotentiallyCollidingHoleBodies()
    {
        List<HoleBody> curHoleBodies = new List<HoleBody>();

        foreach (HoleBody holeBody in potentiallyCollidingHoleBodies)
        {
            if (!IsInstanceValid(holeBody) || holeBody.GetHole().GetPart().IsDeleted())
                continue;

            if (ShouldIncludeHoleBody(holeBody))
                curHoleBodies.Add(holeBody);
        }

        potentiallyCollidingHoleBodies = new HashSet<HoleBody>(curHoleBodies);
    }

    private bool ShouldIncludeHoleBody(HoleBody holeBody)
    {
        Vector3 pointOnInsert = GetPoint(holeBody, holeBody.GetPos());
        float distToInsert = pointOnInsert.DistanceTo(holeBody.GetPos());

        return distToInsert <= MAX_INCLUDED_DIST;
    }

    public List<Part> GetCollidingParts()
    {
        HashSet<Part> partsSet = new HashSet<Part>();

        foreach (HoleBody holeBody in GetCollidingHoleBodies())
        {
            Part part = holeBody.GetHole().GetPart();
            partsSet.Add(part);
        }

        List<Part> parts = new List<Part>(partsSet);
        return parts;
    }

    public List<Part> GetCollidingFastenedParts()
    {
        Vector3 negativeEnd = (Vector3)GetEnd(-1);
        Vector3 positiveEnd = (Vector3)GetEnd(1);
        Vector3 negativeFastenerPos = GetClosestFastenerPos(negativeEnd);
        Vector3 positiveFastenerPos = GetClosestFastenerPos(positiveEnd);

        HashSet<Part> partsSet = new HashSet<Part>();

        foreach (HoleBody holeBody in GetCollidingHoleBodies())
        {
            Vector3 pos = holeBody.GetPos();

            bool afterNegative = pos.DistanceTo(negativeEnd) >= negativeFastenerPos.DistanceTo(negativeEnd);
            bool beforePositive = pos.DistanceTo(positiveEnd) >= positiveFastenerPos.DistanceTo(positiveEnd);
            bool isFastened = afterNegative && beforePositive;
            if (!isFastened)
                continue;

            Part part = holeBody.GetHole().GetPart();
            if (!partsSet.Contains(part))
                partsSet.Add(part);
        }

        Part negativeFastener = GetClosestFastener(negativeEnd).GetHole().GetPart();
        Part positiveFastener = GetClosestFastener(positiveEnd).GetHole().GetPart();
        partsSet.Add(negativeFastener);
        partsSet.Add(positiveFastener);

        List<Part> parts = new List<Part>(partsSet);
        return parts;
    }

    public HoleBody GetClosestFastener(Vector3 pos)
    {
        HoleBody closestFastenerHoleBody = null;
        float minDist = float.MaxValue;

        foreach (HoleBody holeBody in GetCollidingHoleBodies())
        {
            Hole hole = holeBody.GetHole();
            if (!hole.IsFastener())
                continue;

            float dist = holeBody.GlobalPosition.DistanceTo(pos);
            if (dist < minDist)
            {
                closestFastenerHoleBody = holeBody;
                minDist = dist;
            }
        }

        return closestFastenerHoleBody;
    }

    public Vector3 GetClosestFastenerPos(Vector3 pos)
    {
        return GetClosestFastener(pos).GetPos();
    }

    public bool HasFasteners()
    {
        Vector3? negativeEnd = GetEnd(-1);
        Vector3? positiveEnd = GetEnd(1);
        if (negativeEnd == null || positiveEnd == null)
            return false;

        HoleBody negativeFastenerHoleBody = GetClosestFastener((Vector3)negativeEnd);
        HoleBody positiveFastenerHoleBody = GetClosestFastener((Vector3)positiveEnd);

        bool hasBoth = negativeFastenerHoleBody != null && positiveFastenerHoleBody != null;
        if (!hasBoth)
            return false;

        bool notSame = negativeFastenerHoleBody.GetHole() != positiveFastenerHoleBody.GetHole();
        return hasBoth && notSame;
    }

    public Vector3 GetPoint(HoleBody holeBody, Vector3 point)
    {
        Vector3 origin = GlobalPosition;
        Vector3 direction = GlobalTransform.Basis.Z;
        Vector3 difference = point - origin;
        float dotP = difference.Dot(direction);
        Vector3 closestPointOnAxis = origin + direction * dotP;

        return closestPointOnAxis;
    }

    public HoleBody GetSnappingHoleBody(Vector3 point)
    {
        UpdatePotentiallyCollidingHoleBodies();

        HoleBody closestHoleBody = null;
        float minDist = float.MaxValue;

        foreach (HoleBody collidingHoleBody in potentiallyCollidingHoleBodies)
        {
            if (!IsInstanceValid(collidingHoleBody) || collidingHoleBody.GetHole().GetPart().IsDeleted())
                continue;

            bool isSamePart = collidingHoleBody.GetHole().GetPart() == GetPart();
            if (isSamePart)
                continue;

            Vector3 holeBodyPos = collidingHoleBody.GetPos();
            float dist = holeBodyPos.DistanceTo(point);

            if (dist <= MAX_SNAPPING_DIST && dist < minDist)
            {
                closestHoleBody = collidingHoleBody;
                minDist = dist;
            }
        }

        return closestHoleBody;
    }

    public List<Part> GetPotentiallyCollidingParts()
    {
        HashSet<Part> potentiallyCollidingParts = new HashSet<Part>();
        foreach (HoleBody holeBody in potentiallyCollidingHoleBodies)
        {
            if (!IsInstanceValid(holeBody))
                continue;
            potentiallyCollidingParts.Add(holeBody.GetHole().GetPart());
        }

        return new List<Part>(potentiallyCollidingParts);
    }

    public Vector3 GetPos()
    {
        return GlobalPosition;
    }

    public Vector3? GetEnd(int direction)
    {
        if (!IsInsideTree())
            return null;

        return GetPos() + length / 2 * GlobalTransform.Basis.Z * direction;
    }

    public Part GetPart()
    {
        if (!IsInstanceValid(this) || GetParent() == null)
            return null;
        return (Part)GetParent().GetParent();
    }
}