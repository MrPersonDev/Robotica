using Godot;
using System;
using System.Threading.Tasks;

public partial class HotBoxOption : Godot.Control
{
    private String partName;
    private Action<HotBoxOption> hoveredAction;
    private Callable callable;

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath labelPath, indicatorPanelPath;

    private Label label;
    private Panel indicatorPanel;

    public void Setup(String partName)
    {
        label = (Label)GetNode(labelPath);
        indicatorPanel = (Panel)GetNode(indicatorPanelPath);

        this.partName = partName;

        SetLabelText();
        SetCallable();
    }

    private void SetLabelText()
    {
        label.Text = partName;
    }

    private async void SetCallable()
    {
        callable = Callable.From(await GetPartOptionAction());
    }

    public void ShowIndicator()
    {
        indicatorPanel.SelfModulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }

    public void HideIndicator()
    {
        indicatorPanel.SelfModulate = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }

    public void Select()
    {
        callable.Call();
    }

    private async Task<Action> GetPartOptionAction()
    {
        World world = (World)GetTree().CurrentScene;
        if (world.GetInterfaceNode() == null)
            await ToSignal(world, Node.SignalName.Ready);

        PartsList partsList = world.GetInterfaceNode().GetPartsListNode();
        PartOption partOption = partsList.GetPartOption(partName);

        return partsList.GetPartOptionClickedAction(partOption);
    }
}
