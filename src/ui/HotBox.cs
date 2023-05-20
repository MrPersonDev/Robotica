using Godot;
using System;

public partial class HotBox : Godot.Control
{
	private HotBoxOption currentHovering = null;

	[ExportGroup("Properties")]
	[Export]
	private float dist = 120.0f;
	[Export]
	private Godot.Collections.Array<String> partNames = new Godot.Collections.Array<String>();
	[Export]
	private PackedScene hotBoxOptionScene;

	public override void _Ready()
	{
		SetHotBoxOptions();
	}

	private void SetHotBoxOptions()
	{
		float angleBetween = (float)((360.0f / partNames.Count) * (Math.PI / 180.0f));
		float angle = 0.0f;
		for (int i = 0; i < partNames.Count; i++)
		{
			float dx = (float)Math.Sin(angle) * dist;
			float dy = (float)Math.Cos(angle) * dist;

			HotBoxOption newHotBoxOption = (HotBoxOption)hotBoxOptionScene.Instantiate();
			AddChild(newHotBoxOption);
			newHotBoxOption.Setup(partNames[i]);
			newHotBoxOption.ConnectHovered((HotBoxOption hotBoxOption) => { HotBoxOptionHovered(hotBoxOption); });

			newHotBoxOption.Position += new Vector2(dx, dy);

			angle += angleBetween;
		}
	}

	private void HotBoxOptionHovered(HotBoxOption hotBoxOption)
	{
		currentHovering = hotBoxOption;
	}

	public void Start()
	{
		Show();
	}	

	public void End()
	{
		if (currentHovering != null)
			currentHovering.Select();

		currentHovering = null;
		Hide();
	}
}
