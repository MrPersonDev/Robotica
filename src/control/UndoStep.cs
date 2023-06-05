using Godot;
using System;
using System.Collections.Generic;

public class UndoStep
{
    private Callable callable;
    private bool isDeleting;
    private List<Part> parts;

    public UndoStep(Callable callable, bool isDeleting, List<Part> parts)
    {
        this.callable = callable;
        this.isDeleting = isDeleting;
        this.parts = parts;
    }

    public UndoStep(Callable callable)
    {
        this.callable = callable;
        this.isDeleting = false;
        this.parts = null;
    }

    public Callable GetCallable()
    {
        return callable;
    }

    public bool IsDeleting()
    {
        return isDeleting;
    }

    public List<Part> GetParts()
    {
        return parts;
    }
}
