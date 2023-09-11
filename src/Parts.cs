using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

public partial class Parts : Node3D
{
    private const int MAX_CONCURRENT_HOLE_THREADS = 1;
    private const int MAX_CONCURRENT_DELETE_THREADS = 1;

    private List<List<Part>> manualPartGroupings = new List<List<Part>>();
    private List<PartGroup> partGroups = new List<PartGroup>();
    private HashSet<Part> partsToUpdate = new HashSet<Part>();
    private SemaphoreSlim holeSemaphore = new SemaphoreSlim(MAX_CONCURRENT_HOLE_THREADS);
    private SemaphoreSlim deleteSemaphore = new SemaphoreSlim(MAX_CONCURRENT_DELETE_THREADS);

    public static Part AbductChild(PartGroup partGroup, Part part, bool changeGroup)
    {
        partGroup.RemovePart(part, changeGroup);

        return part;
    }

    public static List<Part> AbductChildren(PartGroup partGroup, bool changeGroup)
    {
        List<Part> parts = new List<Part>();
        List<Part> groupedParts = partGroup.GetParts();
        while (groupedParts.Count > 0)
        {
            parts.Add(AbductChild(partGroup, groupedParts[0], changeGroup));
            groupedParts = partGroup.GetParts();
        }

        return parts;
    }

    public static void AdoptChild(PartGroup partGroup, Part part, bool changeGroup)
    {
        partGroup.AddPart(part, changeGroup);
    }

    public static void AdoptChildren(PartGroup partGroup, List<Part> parts, bool changeGroup)
    {
        foreach (Part part in parts)
            partGroup.AddPart(part, changeGroup);
    }

    public void RequireUpdate(Part part)
    {
        partsToUpdate.Add(part);
        part.SetRequiresUpdate(true);
    }

    public void RequireUpdate(PartGroup partGroup)
    {
        foreach (Part part in partGroup.GetParts())
            RequireUpdate(part);
    }

    public void CreatePartGroups()
    {
        HashSet<Part> skipped = new HashSet<Part>();

        foreach (Part part in partsToUpdate)
        {
            bool shouldUpdatePart = part.HasInsert() && part.HasFasteners();
            if (shouldUpdatePart)
                UpdatePart(part);
            else
                skipped.Add(part);
        }

        CleanUpGroups();
        RestoreManualPartGroups();

        partsToUpdate = skipped;
    }

    private void UpdatePart(Part insert)
    {
        List<Part> fastenedParts = insert.GetCollidingFastenedParts();
        GroupParts(fastenedParts, insert);
        insert.SetRequiresUpdate(false);
    }

    private void GroupParts(List<Part> parts, Part originalPart)
    {
        List<PartGroup> overlappingPartGroups = new List<PartGroup>();

        foreach (Part part in parts)
        {
            if (part.GetPartGroup() != null)
                overlappingPartGroups.Add(part.GetPartGroup());
        }

        if (originalPart != null && !parts.Contains(originalPart))
            parts.Add(originalPart);

        PartGroup partGroup;
        if (overlappingPartGroups.Count > 0)
            partGroup = overlappingPartGroups[0];
        else
        {
            partGroup = new PartGroup();
            partGroups.Add(partGroup);
        }

        for (int i = 1; i < overlappingPartGroups.Count; i++)
            parts.AddRange(AbductChildren(overlappingPartGroups[i], true));

        List<Part> partsWithoutDeleted = new List<Part>();
        foreach (Part part in parts)
        {
            if (IsInstanceValid(part))
                partsWithoutDeleted.Add(part);
        }

        AdoptChildren(partGroup, partsWithoutDeleted, true);
    }

    public void CreateManualPartGroup(List<Part> parts, bool changeGroup)
    {
        manualPartGroupings.Add(new List<Part>(parts));
    }

    private void RestoreManualPartGroups()
    {
        foreach (List<Part> manualPartGrouping in manualPartGroupings)
            GroupParts(new List<Part>(manualPartGrouping), null);
    }

    private void CleanUpGroups()
    {
        int i = 0;
        while (i < partGroups.Count)
        {
            List<Part> curParts = partGroups[i].GetParts();
            bool hasParts = curParts.Count > 0;
            bool isValidGroup = hasParts && partGroups[i].HasInsert() && partGroups[i].HasFasteners();

            if (isValidGroup)
            {
                i++;
                continue;
            }

            if (hasParts)
                AbductChildren(partGroups[i], true);

            partGroups.RemoveAt(i);
        }

        CleanUpManualGroups();
    }

    private void CleanUpManualGroups()
    {
        Dictionary<Part, List<Part>> existingGroupings = new Dictionary<Part, List<Part>>();

        foreach (List<Part> grouping in manualPartGroupings)
        {
            Part overlappingPart = null;
            foreach (Part part in grouping)
            {
                if (existingGroupings.ContainsKey(part))
                {
                    overlappingPart = part;
                    break;
                }

                existingGroupings[part] = grouping;
            }

            if (overlappingPart == null)
                continue;

            List<Part> overlappingGrouping = existingGroupings[overlappingPart];
            HashSet<Part> newGroupingSet = new HashSet<Part>();
            foreach (Part part in overlappingGrouping)
                newGroupingSet.Add(part);
            foreach (Part part in grouping)
                newGroupingSet.Add(part);

            List<Part> newGrouping = new List<Part>();
            foreach (Part part in newGroupingSet)
            {
                if (!IsInstanceValid(part))
                    continue;

                newGrouping.Add(part);
                existingGroupings[part] = newGrouping;
            }
        }

        HashSet<List<Part>> newGroupingsSet = new HashSet<List<Part>>();
        foreach (List<Part> grouping in existingGroupings.Values)
        {
            if (grouping.Count < 2)
                continue;

            newGroupingsSet.Add(grouping);
        }

        manualPartGroupings = new List<List<Part>>(newGroupingsSet);
    }

    public void OverlappingFix()
    {
        foreach (Part part in GetAllParts())
            part.OverlappingFix();
    }

    public async void CreateHoles(Part part, Godot.Collections.Array<Hole> holeList)
    {
        await holeSemaphore.WaitAsync();

        try
        {
            await part.CreateHoles(new List<Hole>(holeList));
        }
        finally
        {
            holeSemaphore.Release();
        }
    }

    public async void DeletePart(Part part)
    {
        // await Task.Run(() => DeletePartTask(part));
        DeletePartTask(part);
    }

    public async void DeletePartTask(Part part)
    {
        if (!IsInstanceValid(part))
            return;

        try
        {
            part.PretendDelete();
        }
        catch (System.ObjectDisposedException) { return; }

        await holeSemaphore.WaitAsync();

        try
        {
            part.Delete();
        }
        finally
        {
            holeSemaphore.Release();
        }
    }

    public Part CreatePart(PartObject partObject, List<Hole> holeList, bool loaded)
    {
        Part part = (Part)partObject.Object.Instantiate();
        part.Setup(loaded);

        // AddChild(part);
        CallDeferred("add_child", part);
        // CreateHoles(part, new Godot.Collections.Array<Hole>(holeList));
        CallDeferred("CreateHoles", part, new Godot.Collections.Array<Hole>(holeList));
        part.CallDeferred("call_deferred", "SetAdditionalMeshColliders");

        return part;
    }

    public void DuplicatePart(Part part)
    {
        PartOption partOption = part.GetPartOption();
        Dictionary<String, Variant> parameters = part.GetParameters();
        Transform3D transform = part.GlobalTransform;

        PartObject partObject = partOption.GetPartObject(parameters);
        List<Hole> holeList = partOption.GetHoles(partObject);

        Part newPart = CreatePart(partObject, holeList, true);
        partOption.Setup(newPart, parameters);
        newPart.GlobalTransform = transform;
    }

    public void DeselectAll()
    {
        foreach (Part part in GetAllParts())
            part.Deselect();
    }

    public List<Part> EnableColliders(List<Part> parts)
    {
        List<Part> previouslyDisabled = new List<Part>();
        foreach (Part part in parts)
        {
            if (!part.IsEnabled())
            {
                previouslyDisabled.Add(part);
                part.EnableColliders();
            }
        }

        return previouslyDisabled;
    }

    public void DisableColliders(List<Part> parts, bool disableInserts)
    {
        foreach (Part part in parts)
            part.DisableColliders(disableInserts);
    }

    public void SetOwner(Node parent)
    {
        foreach (Part part in GetAllParts())
            part.Owner = parent;
    }

    public List<Part> GetAllParts()
    {
        List<Part> List = new List<Part>();
        AddPartsRecursive(List, this);
        return List;
    }

    public static void AddPartsRecursive(List<Part> List, Node3D cur)
    {
        if (cur is Part && !((Part)cur).IsDeleted())
            List.Add((Part)cur);
        else if (cur is PartGroup || cur is Parts)
        {
            foreach (Node3D child in cur.GetChildren())
                AddPartsRecursive(List, child);
        }
    }

    public void Clear()
    {
        ClearParts();
        manualPartGroupings = new List<List<Part>>();
        partGroups = new List<PartGroup>();
        holeSemaphore = new SemaphoreSlim(MAX_CONCURRENT_HOLE_THREADS);
    }

    public void ClearParts()
    {
        List<Part> children = GetAllParts();
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].GetPartGroup() != null)
                AbductChild(children[i].GetPartGroup(), children[i], true);
            RemoveChild(children[i]);
            children[i].QueueFree();
        }
    }

    public List<Dictionary<String, Variant>> GetSaveInfo()
    {
        List<Dictionary<String, Variant>> saveInfo = new List<Dictionary<String, Variant>>();

        foreach (Part part in GetAllParts())
            saveInfo.Add(part.GetSaveInfo());

        return saveInfo;
    }

    public void LoadPartGroups(List<PartGroup> partGroups)
    {
        this.partGroups.AddRange(partGroups);
    }

    public List<List<Part>> GetManualPartGroupings()
    {
        return this.manualPartGroupings;
    }

    public void LoadManualPartGroupings(List<List<Part>> manualPartGroupings)
    {
        this.manualPartGroupings.AddRange(manualPartGroupings);
    }
}
