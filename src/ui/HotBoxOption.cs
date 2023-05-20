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
	private NodePath buttonPath;

	private Button button;

	public void Setup(String partName)
	{
		button = (Button)GetNode(buttonPath);

		this.partName = partName;

		SetButtonText();
		SetCallable();
	}

	private void SetButtonText()
	{
		button.Text = partName;
	}

	private async void SetCallable()
	{
		callable = Callable.From(await GetPartOptionAction());
	}

	public void Select()
	{
		callable.Call();
	}

	public void ConnectHovered(Action<HotBoxOption> action)
	{
		hoveredAction = action;
		button.MouseEntered += () => { HoveredHandler(); };
	}

	private void HoveredHandler()
	{
		Callable.From(hoveredAction).Call(this);
	}

	private async Task<Action> GetPartOptionAction()
	{
		World world = (World)GetTree().CurrentScene;
		await ToSignal(world, Node.SignalName.Ready);

		PartsList partsList = world.GetInterfaceNode().GetPartsListNode();
		PartOption partOption = partsList.GetPartOption(partName);

		return partsList.GetPartOptionClickedAction(partOption);
	}
}
