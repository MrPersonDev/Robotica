using Godot;
using System;
using System.Collections.Generic;

public abstract partial class Moveable : Selectable
{
    private bool initialized = false;

    public override void _Process(double delta)
    {
        // Run this once the part has been initialized
        if (!initialized)
        {
            Initialize();
            initialized = true;
        }
    }

    public virtual void Initialize() { }

    public abstract void UpdateCollisionTransform();
    public abstract void MoveTo(Vector3 pos, bool meshOnly = false);
    public abstract void RotateCenter(Vector3 axis, float rotation, bool meshOnly = false);
    public abstract void RotatePos(Vector3 axis, float rotation, Vector3 pos, bool meshOnly = false);
    public abstract void EnableMeshCollider();
    public abstract void DisableMeshCollider();
    public abstract void EnableColliders();
    public abstract void DisableColliders(bool disableInserts);
    public abstract void PrepareToDelete();
    public abstract Vector3 GetCenter();
    public abstract bool HasDefaultHole();
    public abstract HoleBody GetDefaultHoleBody();
    public abstract bool HasInsert();
    public abstract bool HasFasteners();
    public abstract List<Part> GetParts();
}
