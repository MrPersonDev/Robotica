using Godot;
using System;
using System.Collections.Generic;

public partial class PartsList : PanelContainer
{
	Dictionary<String, PartOption> partOptionNames = new Dictionary<String, PartOption>();

    [ExportGroup("Node Paths")]
	[Export]
	private NodePath partOptionsPath;
	[Export]
	private NodePath partSettingsPath;

	private VBoxContainer partOptions;
	private PartSettings partSettings;
	
	public override void _Ready()
	{
		partOptions = (VBoxContainer)GetNode(partOptionsPath);
		partSettings = (PartSettings)GetNode(partSettingsPath);
		
		GetHoleArraysFromList();
		StorePartOptionNames();
	}
	
	private void GetHoleArraysFromList()
	{
		foreach (PartOption partOption in partOptions.GetChildren())
			GetHoleArrayFromPartOption(partOption);
	}

	private void GetHoleArrayFromPartOption(PartOption partOption)
	{
		List<PartObject> partObjects = partOption.GetPartObjects();

		foreach (PartObject partObject in partObjects)
			GetHoleArrayFromPartObject(partOption, partObject);

		Action buttonPressedAction = GetPartOptionClickedAction(partOption);
		partOption.Connect(Button.SignalName.Pressed, Callable.From(buttonPressedAction));
	}

	private void GetHoleArrayFromPartObject(PartOption partOption, PartObject partObject)
	{
		Part part = (Part)partObject.Object.Instantiate();		

		part.Setup();
		part.RemoveCollider();
		part.DisableColliders(true);

		List<Hole> holeArray = new List<Hole>(part.GetHoles());
		
		part.RemoveHoles();
		part.HidePartMesh();

		PackedScene partObjectWithoutHoles = new PackedScene();
		partObjectWithoutHoles.Pack(part);
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

	public PartOption GetPartOption(String name)
	{
		return partOptionNames[name];
	}

	public Action GetPartOptionClickedAction(PartOption partOption)
	{
		Action partOptionClickedAction = () => { PartOptionClicked(partOption); };
		return partOptionClickedAction;
	}
}
