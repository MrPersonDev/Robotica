using Godot;
using System;

public partial class InputSettingEdit : SettingEdit
{
	private bool listening = false;
	private InputEvent action, lastActionPressed;

	[ExportGroup("Node paths")]
	[Export]
	private NodePath buttonPath;

	private Button button;

	public override void _Ready()
	{
		base._Ready();

		button = (Button)GetNode(buttonPath);

		SetAction();
		SetButtonText();
		ConnectButton();
	}

	private void SetAction()
	{
		Godot.Collections.Array<InputEvent> actions = InputMap.ActionGetEvents(GetName());
		action = actions[0];
	}
 
	private void SetButtonText()
	{
		button.Text = InputEventToString(action);
	}

	private void ConnectButton()
	{
		button.Connect(Button.SignalName.Pressed, Callable.From(() => { StartListening(); }));
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (!listening || inputEvent is InputEventMouseMotion)
			return;	
		
		if (inputEvent.IsActionPressed("ui_cancel"))
		{
			listening = false;
			SetButtonText();
			return;
		}

		if (inputEvent.IsPressed())
		{
			GetViewport().SetInputAsHandled();
			lastActionPressed = inputEvent;
			return;
		}

		action = lastActionPressed;
		listening = false;
		SetButtonText();
	}

	private void StartListening()
	{
		button.Text = "...";
		listening = true;
	}

	private String InputEventModifiersToString(InputEventWithModifiers inputEvent)
	{
		if (inputEvent == null)
			return "";
		String str = "";
		if (inputEvent.CtrlPressed)
			str += (OS.GetName().Equals("OSX") ? "Cmd+" : "Ctrl+");
		if (inputEvent.MetaPressed)
			str += "Meta+";
		if (inputEvent.AltPressed)
			str += "Alt+";
		if (inputEvent.ShiftPressed)
			str += "Shift+";
		return str;
	}

	private String InputEventToString(InputEvent inputEvent)
	{
		String str = InputEventModifiersToString((InputEventWithModifiers)inputEvent);
		if (inputEvent is InputEventKey)
		{
			InputEventKey keyEvent = (InputEventKey)inputEvent;
			str += OS.GetKeycodeString(keyEvent.Keycode);
		}
		else if (inputEvent is InputEventMouse)
		{
			InputEventMouseButton mouseEvent = (InputEventMouseButton)inputEvent;
			str += "Mouse " + mouseEvent.ButtonIndex.ToString();
		}

		return str;
	}

	public override void SetValue(Variant value)
	{
		action = (InputEvent)value;
	}

	public override Variant GetValue()
	{
		return action;
	}
}

