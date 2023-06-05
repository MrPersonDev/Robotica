using Godot;
using System;
using System.Collections.Generic;

public partial class SettingsPanel : Godot.Control
{
    private String name;

    [ExportGroup("Node paths")]
    [Export]
    private NodePath setingEditsPath;

    private VBoxContainer settingEdits;

    public override void _Ready()
    {
        settingEdits = (VBoxContainer)GetNode(setingEditsPath);
    }

    public void UpdateSettingEdits(Dictionary<String, Variant> panelSettings)
    {
        foreach (KeyValuePair<String, Variant> pair in panelSettings)
        {
            foreach (SettingEdit settingEdit in settingEdits.GetChildren())
            {
                if (settingEdit.GetName().Equals(pair.Key))
                {
                    settingEdit.SetValue(pair.Value);
                    break;
                }
            }
        }
    }

    public void SetName(String name)
    {
        this.name = name;
    }

    public String GetName()
    {
        return name;
    }

    public Dictionary<String, Variant> GetCurrentSettings()
    {
        Dictionary<String, Variant> currentSettings = new Dictionary<String, Variant>();

        foreach (SettingEdit settingEdit in settingEdits.GetChildren())
        {
            String settingName = settingEdit.GetName();
            Variant value = settingEdit.GetValue();

            currentSettings[settingName] = value;
        }

        return currentSettings;
    }
}
