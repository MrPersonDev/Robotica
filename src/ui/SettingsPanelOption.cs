using Godot;
using System;

public partial class SettingsPanelOption : Button
{
	private Action<PackedScene> pressedAction;

	[ExportGroup("Properties")]
	[Export]
	private PackedScene settingsPanel;

	public void ConnectPressed(Action<PackedScene> pressedAction)
	{
		this.pressedAction = pressedAction;
		Pressed += () => { PressedHandler(); };
	}

	private void PressedHandler()
	{
		Callable.From(pressedAction).Call(settingsPanel);
	}

	public PackedScene GetPanel()
	{
		return settingsPanel;
	}
}
