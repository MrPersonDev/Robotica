using Godot;
using System;
using System.Collections.Generic;

public partial class Interface : Godot.Control
{
    private bool creatingPart = false;
    private bool inQueue = false;
    private PartOption queuedPartOption;
    private Dictionary<String, Variant> queuedParameters;
    private Dictionary<int, Callable> fileCallables = new Dictionary<int, Callable>();
    private Dictionary<int, Callable> editCallables = new Dictionary<int, Callable>();
    private Dictionary<int, Callable> selectCallables = new Dictionary<int, Callable>();

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath fieldPath, fieldButtonPath, selectionBoxPath, fpsCounterPath, controlPath, partsPath, partsListPath, saveDialogPath, loadDialogPath, errorPath;
    [Export]
    private NodePath settingsPath, overlayPath, hotBoxPath, fileButtonPath, editButtonPath, selectButtonPath;

    private Node3D field;
    private Button fieldButton;
    private Panel selectionBox;
    private Label fpsCounter;
    private Control control;
    private Parts parts;
    private PartsList partsList;
    private Node saveDialog, loadDialog, error;
    private Settings settings;
    private Panel overlay;
    private HotBox hotBox;
    private MenuButton fileButton, editButton, selectButton;

    public override void _Ready()
    {
        field = (Node3D)GetNode(fieldPath);
        fieldButton = (Button)GetNode(fieldButtonPath);
        selectionBox = (Panel)GetNode(selectionBoxPath);
        fpsCounter = (Label)GetNode(fpsCounterPath);
        control = (Control)GetNode(controlPath);
        parts = (Parts)GetNode(partsPath);
        partsList = (PartsList)GetNode(partsListPath);
        saveDialog = GetNode(saveDialogPath);
        loadDialog = GetNode(loadDialogPath);
        error = GetNode(errorPath);
        settings = (Settings)GetNode(settingsPath);
        overlay = (Panel)GetNode(overlayPath);
        hotBox = (HotBox)GetNode(hotBoxPath);
        fileButton = (MenuButton)GetNode(fileButtonPath);
        editButton = (MenuButton)GetNode(editButtonPath);
        selectButton = (MenuButton)GetNode(selectButtonPath);

        SetMenuButtons();
        SetFieldButtonPressed();
        SetupFileDialogs();
    }

    private void SetMenuButtons()
    {
        AddMenuOption(fileButton, "New", "new", fileCallables, () => { control.HandleNewInput(); });
        AddMenuOption(fileButton, "Open", "load", fileCallables, () => { control.HandleLoadInput(); });
        AddMenuSeparator(fileButton, "");
        AddMenuOption(fileButton, "Save", "save", fileCallables, () => { control.HandleSaveInput(); });
        AddMenuOption(fileButton, "Save As", "save_as", fileCallables, () => { control.HandleSaveAsInput(); });
        AddMenuSeparator(fileButton, "");
        AddMenuOption(fileButton, "Exit", null, fileCallables, () => { control.HandleExitInput(); });

        AddMenuOption(editButton, "Undo", "undo", editCallables, () => { control.HandleUndoInput(); });
        AddMenuOption(editButton, "Redo", "redo", editCallables, () => { control.HandleRedoInput(); });
		AddMenuSeparator(editButton, "");
        AddMenuOption(editButton, "Delete", "delete", editCallables, () => { control.HandleDeleteInput(); });
		AddMenuSeparator(editButton, "");
        AddMenuOption(editButton, "Settings", null, editCallables, () => { control.HandleSettingsInput(); });

		AddMenuOption(selectButton, "Select All", "select_all", selectCallables, () => { control.HandleSelectAllInput(); });
		AddMenuOption(selectButton, "Deselect All", "deselect_all", selectCallables, () => { control.HandleDeselectAllInput(); });
		AddMenuSeparator(selectButton, "");
		AddMenuOption(selectButton, "Focus", "focus", selectCallables, () => { control.HandleFocusInput(); });
		AddMenuSeparator(selectButton, "");
		AddMenuOption(selectButton, "Group", "group", selectCallables, () => { control.HandleGroupInput(); });
		AddMenuSeparator(selectButton, "");
		AddMenuOption(selectButton, "Duplicate", "duplicate", selectCallables, () => { control.HandleDuplicateInput(); });

        fileButton.GetPopup().Connect("id_pressed", Callable.From((int id) => { PopupMenuItemSelected(fileCallables, id); }));
        editButton.GetPopup().Connect("id_pressed", Callable.From((int id) => { PopupMenuItemSelected(editCallables, id); }));
        selectButton.GetPopup().Connect("id_pressed", Callable.From((int id) => { PopupMenuItemSelected(selectCallables, id); }));
    }

    private void AddMenuOption(MenuButton menuButton, String name, String shortcutEventName, Dictionary<int, Callable> callables, Action pressedAction)
    {
        PopupMenu popupMenu = menuButton.GetPopup();

        Shortcut shortcut = new Shortcut();
        if (shortcutEventName != null)
        {
            Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(shortcutEventName);
            shortcut.Events.AddRange(events);
        }

		popupMenu.AddShortcut(shortcut, -1, false);

        int idx = popupMenu.ItemCount - 1;
        popupMenu.SetItemText(idx, name);

        callables[idx] = Callable.From(pressedAction);
    }

    private void AddMenuSeparator(MenuButton menuButton, String name)
    {
        PopupMenu popupMenu = menuButton.GetPopup();

        popupMenu.AddSeparator(name);
    }

    private void PopupMenuItemSelected(Dictionary<int, Callable> callables, int id)
    {
        Callable callable = callables[id];
        callable.Call();
    }

    private void SetFieldButtonPressed()
    {
        Action pressedAction = () => { ToggleField(); };
        fieldButton.Pressed += pressedAction;
    }

    private void SetupFileDialogs()
    {
        Action<String> saveAction = (String path) => { SaveToFile(path); };
        Action<String> loadAction = (String path) => { LoadFromFile(path); };

        Callable save = Callable.From(saveAction);
        Callable load = Callable.From(loadAction);

        saveDialog.Call("connect_selected", save);
        loadDialog.Call("connect_selected", load);

        loadDialog.Call("set_saving", false);
    }

    public override void _Process(double delta)
    {
        UpdateFPSCounter();
    }

    public void UpdateSelectionBox(Vector2 clickMousePos, Vector2 curMousePos)
    {
        selectionBox.Show();

        selectionBox.Position = GetSelectionBoxTopLeft(clickMousePos, curMousePos);
        selectionBox.Size = GetSelectionBoxSize(clickMousePos, curMousePos);
    }

    public Vector2 GetSelectionBoxTopLeft(Vector2 clickMousePos, Vector2 curMousePos)
    {
        float minX = Math.Min(clickMousePos.X, curMousePos.X), minY = Math.Min(clickMousePos.Y, curMousePos.Y);
        float maxX = Math.Max(clickMousePos.X, curMousePos.X), maxY = Math.Max(clickMousePos.Y, curMousePos.Y);

        Vector2 topLeft = new Vector2(minX, minY);

        return topLeft;
    }

    public Vector2 GetSelectionBoxSize(Vector2 clickMousePos, Vector2 curMousePos)
    {
        float minX = Math.Min(clickMousePos.X, curMousePos.X), minY = Math.Min(clickMousePos.Y, curMousePos.Y);
        float maxX = Math.Max(clickMousePos.X, curMousePos.X), maxY = Math.Max(clickMousePos.Y, curMousePos.Y);

        float width = maxX - minX;
        float height = maxY - minY;
        Vector2 size = new Vector2(width, height);

        return size;
    }

    public void HideSelectionBox()
    {
        selectionBox.Hide();
    }

    private void UpdateFPSCounter()
    {
        fpsCounter.Text = String.Format("FPS: {0}", Engine.GetFramesPerSecond());
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        bool isLeftClick = inputEvent is InputEventMouseButton && ((InputEventMouseButton)inputEvent).ButtonIndex == MouseButton.Left;
        if (isLeftClick)
            DeselectUI();
    }

    private void DeselectUI()
    {
        List<Node> inputs = new List<Node>(GetTree().GetNodesInGroup("INPUT"));
        foreach (Godot.Control input in inputs)
        {
            input.Hide();
            input.Show();
        }
    }

    private void ToggleField()
    {
        field.Visible = !field.Visible;
    }

    public async void CreatePart(PartOption partOption, Dictionary<String, Variant> parameters)
    {
        if (creatingPart)
        {
            inQueue = true;
            queuedPartOption = partOption;
            queuedParameters = parameters;
            return;
        }

        creatingPart = true;
        await control.CreateAndMoveNewPart(partOption, parameters);
        creatingPart = false;

        if (inQueue)
        {
            inQueue = false;
            CreatePart(queuedPartOption, queuedParameters);
        }
    }

    public void StartHotBoxing(Vector2 mousePos)
    {
        hotBox.GlobalPosition = mousePos;
        hotBox.Start();
    }

    public void EndHotBoxing()
    {
        hotBox.End();
    }

    public void ShowSettings()
    {
        settings.Open();
        overlay.Show();
    }

    public void CloseSettings()
    {
        settings.Close();
        overlay.Hide();
    }

    public bool IsSettingsOpen()
    {
        return settings.IsOpen();
    }

    public void ShowSaveDialog()
    {
        saveDialog.Call("show_dialog");
    }

    public void ShowLoadDialog()
    {
        loadDialog.Call("show_dialog");
    }

    public void SaveToFile(String path)
    {
        if (!path.EndsWith(".rbt"))
            path = path + ".rbt";

        SaveManager.Save(path, this, parts);
    }

    public void LoadFromFile(String path)
    {
        if (!path.EndsWith(".rbt"))
        {
            Error("Unknown file extension");
            return;
        }

        SaveManager.Load(path, this, parts);
    }

    public void Error(String text)
    {
        error.Call("set_text", text);
        error.Call("show_error");
    }

    public PartsList GetPartsListNode()
    {
        return partsList;
    }
}
