using Godot;
using System;
using System.Collections.Generic;

public partial class Settings : PanelContainer
{
	[ExportGroup("Node Paths")]
	[Export]
	private NodePath closeButtonPath, panelOptionsPath, settingsPanelContainerPath;

	private Button closeButton;
	private VBoxContainer panelOptions;
	private Godot.Control settingsPanelContainer;

	public override void _Ready()
	{
		closeButton = (Button)GetNode(closeButtonPath);
		panelOptions = (VBoxContainer)GetNode(panelOptionsPath);
		settingsPanelContainer = (Godot.Control)GetNode(settingsPanelContainerPath);

		SetCloseButtonPressed();
		ConnectSettingsPanelOptions();
		LoadSettings();
	}

	private async void SetCloseButtonPressed()
	{
		World world = (World)GetTree().CurrentScene;
		await ToSignal(world, Node.SignalName.Ready);

		Interface ui = world.GetInterfaceNode();
		closeButton.Pressed += () => { ui.CloseSettings(); };
	}

	private void ConnectSettingsPanelOptions()
	{
		Action<PackedScene, String> pressedAction = (PackedScene panelScene, String panelName) => { SetPanel(panelScene, panelName); };

		foreach (SettingsPanelOption panelOption in panelOptions.GetChildren())
			panelOption.ConnectPressed(pressedAction);
	}

	private async void LoadSettings()
	{
		World world = (World)GetTree().CurrentScene;
		await ToSignal(world, Node.SignalName.Ready);

		Interface ui = world.GetInterfaceNode();
		SettingsManager.LoadSettings(ui);
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (IsOpen() && inputEvent is InputEventMouse && !GetRect().HasPoint(((InputEventMouse)inputEvent).GlobalPosition))
			GetViewport().SetInputAsHandled();
	}

	public void Open()
	{
		Visible = true;
		SetInputFocus(FocusModeEnum.None);

		SetPanel(0);
	}

	public void Close()
	{
		SettingsManager.ApplySettings(this, ((World)GetTree().CurrentScene).GetInterfaceNode());

		Visible = false;
		SetInputFocus(FocusModeEnum.All);
	}

	private void SetPanelsPressed(int curIndex)
	{
		for (int i = 0; i < panelOptions.GetChildCount(); i++)
		{
			Button curButton = (Button)panelOptions.GetChildren()[i];
			curButton.ButtonPressed = i == curIndex;
		}
	}

	private void SetPanel(int index)
	{
		SettingsPanelOption panelOption = (SettingsPanelOption)panelOptions.GetChildren()[0];
		panelOption.EmitSignal(Button.SignalName.Pressed);
		SetPanelsPressed(index);
	}

	public void SetPanel(PackedScene panelScene, String panelName)
	{
		if (GetCurrentPanel() != null)
			SettingsManager.ApplySettings(this, ((World)GetTree().CurrentScene).GetInterfaceNode());

		Godot.Collections.Array<Node> children = settingsPanelContainer.GetChildren();
		foreach (Node child in children)
		{
			settingsPanelContainer.RemoveChild(child);
			child.QueueFree();
		}

		SettingsPanel panel = (SettingsPanel)panelScene.Instantiate();
		panel.SetName(panelName);
		settingsPanelContainer.AddChild(panel);

		for (int i = 0; i < panelOptions.GetChildCount(); i++)
		{
			if (((SettingsPanelOption)panelOptions.GetChildren()[i]).GetPanel() == panelScene)
			{
				SetPanelsPressed(i);
				break;
			}
		}

		panel.UpdateSettingEdits(SettingsManager.GetPanelSettings(panelName));
	}

	private void SetInputFocus(FocusModeEnum focusMode)
	{
        List<Node> inputs = new List<Node>(GetTree().GetNodesInGroup("INPUT"));
        foreach (Godot.Control input in inputs)
        {
			input.FocusMode = focusMode;
        }
	}

	public bool IsOpen()
	{
		return Visible;
	}

	private SettingsPanel GetCurrentPanel()
	{
		if (settingsPanelContainer.GetChildCount() == 0)
			return null;

		return (SettingsPanel)settingsPanelContainer.GetChild(0);
	}

	public String GetCurrentPanelName()
	{
		return GetCurrentPanel().GetName();
	}	

	public Dictionary<String, Variant> GetCurrentPanelSettings()
	{
		return GetCurrentPanel().GetCurrentSettings();
	}
}
