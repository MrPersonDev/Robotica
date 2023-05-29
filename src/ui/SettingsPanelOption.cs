using Godot;
using System;
using System.Collections.Generic;

public partial class SettingsPanelOption : Button
{
	private Action<PackedScene, String> pressedAction;

	[ExportGroup("Properties")]
	[Export]
	private PackedScene settingsPanel;

	public void ConnectPressed(Action<PackedScene, String> pressedAction)
	{
		this.pressedAction = pressedAction;
		Pressed += () => { PressedHandler(); };
	}

	private void PressedHandler()
	{
		Callable.From(pressedAction).Call(settingsPanel, GetName());
	}

	public PackedScene GetPanel()
	{
		return settingsPanel;
	}

	public Dictionary<String, Variant> GetCurrentSettings()
	{
		Dictionary<String, Variant> currentSettings = new Dictionary<String, Variant>();



		return currentSettings;
	}

	public String GetName()
	{
		return Text;
	}
}
