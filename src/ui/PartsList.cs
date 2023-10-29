using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public partial class PartsList : PanelContainer
{
    private static String holelessPartsPath = "res://assets/objects/HolelessParts/";
    private static String partHolesPath = "res://assets/objects/PartHoles/";

    Dictionary<String, PartOption> partOptionNames = new Dictionary<String, PartOption>();

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath partOptionsPath, partSettingsPath, searchPath;

    private VBoxContainer partOptions;
    private PartSettings partSettings;
    private LineEdit search;

    public override void _Ready()
    {
        partOptions = (VBoxContainer)GetNode(partOptionsPath);
        partSettings = (PartSettings)GetNode(partSettingsPath);
        search = (LineEdit)GetNode(searchPath);

        GetHoleArraysFromList();
        StorePartOptionNames();
        ConnectSearch();
    }

    private void GetHoleArraysFromList()
    {
        foreach (PartOption partOption in partOptions.GetChildren())
            GetHoleArrayFromPartOption(partOption);
    }

    private void GetHoleArrayFromPartOption(PartOption partOption)
    {
        List<PartObject> partObjects = partOption.GetPartObjects();

        for (int i = 0; i < partObjects.Count; i++)
            GetHoleArrayFromPartObject(partOption, partObjects[i], i);

        Action buttonPressedAction = GetPartOptionClickedAction(partOption);
        partOption.Connect(Button.SignalName.Pressed, Callable.From(buttonPressedAction));
    }

    private String GetHolelessPartPath(PartOption partOption, PartObject partObject, int idx) {
        return holelessPartsPath + partOption.GetName() + idx.ToString() + ".tscn";
    }
    
    private String GetPartHolePath(PartOption partOption, PartObject partObject, int idx) {
        return partHolesPath + partOption.GetName() + idx.ToString() + ".tscn";
    }
    
    private void GenerateHolelessPart(PartOption partOption, PartObject partObject, int idx)
    {
        Part part = (Part)partObject.Object.Instantiate();

        part.Setup();
        part.RemoveCollider();
        part.DisableColliders(true);

        List<Hole> holeArray = new List<Hole>(part.GetHoles());

        part.RemoveHoles();
        part.HidePartMesh();

        PackedScene holeScene = new PackedScene();
        PackedScene partObjectWithoutHoles = new PackedScene();
        Node3D holeParent = new Node3D();
        foreach (Hole hole in holeArray)
        {
            holeParent.AddChild(hole);
            hole.Owner = holeParent;
        }
        holeScene.Pack(holeParent);
        partObjectWithoutHoles.Pack(part);
        
        String holePath = GetPartHolePath(partOption, partObject, idx);
        String holelessPath = GetHolelessPartPath(partOption, partObject, idx);
        
        ResourceSaver.Save(holeScene, holePath);
        ResourceSaver.Save(partObjectWithoutHoles, holelessPath);
    }
    
    private void GetHoleArrayFromPartObject(PartOption partOption, PartObject partObject, int idx)
    {
        String holePath = GetPartHolePath(partOption, partObject, idx);
        String holelessPath = GetHolelessPartPath(partOption, partObject, idx);
        
        if (!ResourceLoader.Exists(holePath) || !ResourceLoader.Exists(holelessPath))
            GenerateHolelessPart(partOption, partObject, idx);

        PackedScene holeScene = (PackedScene)ResourceLoader.Load(holePath);
        PackedScene partObjectWithoutHoles = (PackedScene)ResourceLoader.Load(holelessPath);
        
        Node3D holes = (Node3D)holeScene.Instantiate();
        List<Hole> holeArray = new List<Hole>();
        foreach (Hole hole in holes.GetChildren())
            holeArray.Add(hole);

        partObject.Object = partObjectWithoutHoles;
        partOption.StoreHoles(partObject, holeArray);
    }

    private void PartOptionClicked(PartOption partOption)
    {
        partSettings.ShowSettings(partOption);
    }

    private void StorePartOptionNames()
    {
        foreach (PartOption partOption in partOptions.GetChildren())
            partOptionNames[partOption.GetName()] = partOption;
    }

    private void ConnectSearch()
    {
        search.Connect(LineEdit.SignalName.TextChanged, Callable.From((String value) => { SearchUpdated(value); }));
    }

    private void SearchUpdated(String value)
    {
        String valueLower = value.ToLower();
        foreach (PartOption partOption in partOptions.GetChildren())
            partOption.Visible = partOption.GetName().ToLower().Contains(valueLower);
    }

    public PartOption GetPartOption(String name)
    {
        return partOptionNames[name];
    }

    public Action GetPartOptionClickedAction(PartOption partOption)
    {
        Action partOptionClickedAction = () => { PartOptionClicked(partOption); };
        return partOptionClickedAction;

    }

    public List<String> GetPartsNames()
    {
        return new List<String>(partOptionNames.Keys);
    }
}
