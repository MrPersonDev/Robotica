using Godot;
using System;

public partial class Window : PanelContainer
{
    [ExportGroup("Node Paths")]
    [Export]
    private NodePath closeButtonPath;

    private Button closeButton;

    public override void _Ready()
    {
        closeButton = (Button)GetNode(closeButtonPath);

        SetCloseButtonPressed();
    }
    
    private async void SetCloseButtonPressed()
    {
        World world = (World)GetTree().CurrentScene;
        if (world.GetInterfaceNode() == null)
            await ToSignal(world, Node.SignalName.Ready);

        Interface ui = world.GetInterfaceNode();
        closeButton.Pressed += () => { ui.ClosePanels(); };
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (IsOpen() && inputEvent is InputEventMouse && !GetRect().HasPoint(((InputEventMouse)inputEvent).GlobalPosition))
            GetViewport().SetInputAsHandled();
    }
    
    public virtual void Open()
    {
        Visible = true;
    }

    public virtual void Close()
    {
        Visible = false;
    }

    public virtual bool IsOpen()
    {
        return Visible;
    }
}
