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
}
