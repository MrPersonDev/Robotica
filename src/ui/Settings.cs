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
		Action<PackedScene> pressedAction = (PackedScene panelScene) => { SetPanel(panelScene); };

		foreach (SettingsPanelOption panelOption in panelOptions.GetChildren())
			panelOption.ConnectPressed(pressedAction);
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

	public void SetPanel(PackedScene panelScene)
	{
		Godot.Collections.Array<Node> children = settingsPanelContainer.GetChildren();
		foreach (Node child in children)
		{
			settingsPanelContainer.RemoveChild(child);
			child.QueueFree();
		}

		Node panel = panelScene.Instantiate();
		settingsPanelContainer.AddChild(panel);

		for (int i = 0; i < panelOptions.GetChildCount(); i++)
		{
			if (((SettingsPanelOption)panelOptions.GetChildren()[i]).GetPanel() == panelScene)
			{
				SetPanelsPressed(i);
				break;
			}
		}
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
}
