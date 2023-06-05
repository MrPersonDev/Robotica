using Godot;
using System;
using System.Collections.Generic;

public partial class RequiredParts : Window
{
    [ExportGroup("Properties")]
    [Export]
    private PackedScene requiredPart;

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath listPath;

    private VBoxContainer list;

    public override void _Ready()
    {
        base._Ready();

        list = (VBoxContainer)GetNode(listPath);
    }

    private void LoadList()
    {
        ClearList();
        Dictionary<PartOption, List<Part>> groupedParts = GetGroupedParts();   
        
        foreach (PartOption partOption in groupedParts.Keys)
            LoadList(partOption, groupedParts[partOption]);
    }
    
    private void LoadList(PartOption partOption, List<Part> parts)
    {
        Dictionary<String, int> partsCount = new Dictionary<String, int>();
        foreach (Part part in parts)
        {
            String specificName;
            try
            {
                specificName = partOption.GetSpecificName(part.GetParameters());
            }
            catch (KeyNotFoundException)
            {
                specificName = partOption.GetName();
                GD.PushWarning("Key error on " + partOption.GetName());
            }
            
            if (!partsCount.ContainsKey(specificName))
                partsCount.Add(specificName, 0);
            partsCount[specificName]++;
        }
        
        foreach (String specificName in partsCount.Keys)
        {
            int count = partsCount[specificName];
            String text = "";
            text += count;
            text += "x ";
            text += specificName;

            RequiredPart item = (RequiredPart)requiredPart.Instantiate();
            item.Setup();
            item.SetText(text);            
            list.AddChild(item);
        }
    }
    
    private void ClearList()
    {
        List<Node> children = new List<Node>(list.GetChildren());
        foreach (Node child in children)
        {
            list.RemoveChild(child);
            child.QueueFree();
        }
    }
    
    private Dictionary<PartOption, List<Part>> GetGroupedParts()
    {
        World world = (World)GetTree().CurrentScene;
        Parts parts = world.GetPartsNode();

        List<Part> partsList = parts.GetAllParts();        
        Dictionary<PartOption, List<Part>> groupedParts = new Dictionary<PartOption, List<Part>>();
        foreach (Part part in partsList)
        {
            PartOption partOption = part.GetPartOption();
            if (!groupedParts.ContainsKey(partOption))
                groupedParts.Add(partOption, new List<Part>());
            groupedParts[partOption].Add(part);
        }
        
        return groupedParts;
    }

    public override void Open()
    {
        base.Open();
        
        LoadList();
    }

    public override void Close()
    {
        base.Close();
    }
}
