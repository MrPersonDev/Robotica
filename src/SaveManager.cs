using Godot;
using System;
using System.Collections.Generic;

public static class SaveManager
{
    private static String currentPath = null;

    public static void Save(String path, Interface ui, Parts parts)
    {
        parts.SetOwner(parts);

        List<Dictionary<String, Variant>> saveInfo = parts.GetSaveInfo();
        using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

        if (file == null)
        {
            Error error = FileAccess.GetOpenError();
            ui.Error(error.ToString());
            return;
        }

        foreach (Dictionary<String, Variant> info in saveInfo)
        {
            Godot.Collections.Dictionary<String, Variant> godotDict = new Godot.Collections.Dictionary<String, Variant>(info);
            String jsonString = Json.Stringify(godotDict);
            file.StoreLine(jsonString);
        }

        List<List<Part>> manualPartGroupings = parts.GetManualPartGroupings();
        Godot.Collections.Array<Godot.Collections.Array<String>> partGroupingArrays = new Godot.Collections.Array<Godot.Collections.Array<String>>();
        foreach (List<Part> partGrouping in manualPartGroupings)
        {
            Godot.Collections.Array<String> partGroupingArray = new Godot.Collections.Array<String>();
            foreach (Part part in partGrouping)
            {
                String partName = part.Name;
                partGroupingArray.Add(partName);
            }

            partGroupingArrays.Add(partGroupingArray);
        }

        String partGroupingJsonString = Json.Stringify(partGroupingArrays);
        file.StoreLine(partGroupingJsonString);

        file.Close();
        SetCurrentPath(path, ui);
    }

    private static bool TryLoad(String path, Interface ui)
    {
        if (!FileAccess.FileExists(path))
        {
            ui.Error("File does not exist");
            return false;
        }

        using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        if (file == null)
        {
            Error error = FileAccess.GetOpenError();
            ui.Error(error.ToString());
            return false;
        }

        file.Close();

        return true;
    }

    public static void Load(String path, Interface ui, Parts parts)
    {
        if (TryLoad(path, ui))
        {
            parts.Clear();
            if (Import(path, ui, parts))
                SetCurrentPath(path, ui);
        }
    }

    public static bool Import(String path, Interface ui, Parts parts)
    {
        if (!TryLoad(path, ui))
            return false;

        using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        World world = (World)parts.GetTree().CurrentScene;
        PartsList partsList = world.GetInterfaceNode().GetPartsListNode();
        
        Dictionary<String, PartGroup> loadedPartGroups = new Dictionary<String, PartGroup>();
        while (file.GetPosition() < file.GetLength())
        {
            String jsonString = file.GetLine();
            Json json = new Json();

            Error parseResult = json.Parse(jsonString);
            if (parseResult != Error.Ok)
            {
                ui.Error(parseResult.ToString());
                return false;
            }

            if (jsonString[0] == '[')
            {
                Godot.Collections.Array<Godot.Collections.Array<String>> partGroupingArrays = (Godot.Collections.Array<Godot.Collections.Array<String>>)json.Data;
                LoadManualPartGroupings(partGroupingArrays, parts);
                break;
            }

            Godot.Collections.Dictionary jsonDict = (Godot.Collections.Dictionary)json.Data;
            Godot.Collections.Dictionary<String, Variant> godotDict = new Godot.Collections.Dictionary<String, Variant>(jsonDict);
            Dictionary<String, Variant> saveInfo = new Dictionary<String, Variant>(godotDict);

            try
            {
                LoadPart(saveInfo, parts, partsList, loadedPartGroups);
            }
            catch (KeyNotFoundException)
            {
                ui.Error("Invalid save file. (Is it from a different version?)");
                return false;
            }
        }

        List<PartGroup> partGroups = new List<PartGroup>(loadedPartGroups.Values);
        parts.LoadPartGroups(partGroups);

        file.Close();

        return true;
    }

    public static void LoadPart(Dictionary<String, Variant> saveInfo, Parts parts, PartsList partsList, Dictionary<String, PartGroup> loadedPartGroups)
    {
        String name = (String)saveInfo["Name"];
        Vector3 basisX = StrToVector3((String)saveInfo["X"]);
        Vector3 basisY = StrToVector3((String)saveInfo["Y"]);
        Vector3 basisZ = StrToVector3((String)saveInfo["Z"]);
        Vector3 origin = StrToVector3((String)saveInfo["O"]);
        String partGroupName = (String)saveInfo["PartGroup"];
        String partOptionName = (String)saveInfo["PartOption"];
        Godot.Collections.Dictionary<String, Variant> godotDict = (Godot.Collections.Dictionary<String, Variant>)saveInfo["Parameters"];
        Dictionary<String, Variant> parameters = new Dictionary<String, Variant>(godotDict);

        PartOption partOption = partsList.GetPartOption(partOptionName);
        PartObject partObject = partOption.GetPartObject(parameters);
        List<Hole> holeList = partOption.GetHoles(partObject);

        Transform3D transform = new Transform3D();
        transform.Basis.X = basisX;
        transform.Basis.Y = basisY;
        transform.Basis.Z = basisZ;
        transform.Origin = origin;

        Part part = parts.CreatePart(partObject, holeList, true);
        part.Name = name;
        part.GlobalTransform = transform;
        part.CallDeferred("UpdateCollisionTransform"); // UpdateCollisionTransform();

        if (partGroupName != "")
        {
            if (!loadedPartGroups.ContainsKey(partGroupName))
            {
                PartGroup partGroup = new PartGroup();
                loadedPartGroups[partGroupName] = partGroup;
            }

            Parts.AdoptChild(loadedPartGroups[partGroupName], part, true);
        }

        partOption.Setup(part, parameters);
        parts.RequireUpdate(part);
    }

    private static void LoadManualPartGroupings(Godot.Collections.Array<Godot.Collections.Array<String>> partGroupingArrays, Parts parts)
    {
        List<Part> partsList = parts.GetAllParts();
        Dictionary<String, Part> partNames = new Dictionary<String, Part>();

        foreach (Part part in partsList)
            partNames[part.Name] = part;


        List<List<Part>> manualPartGroupings = new List<List<Part>>();
        foreach (Godot.Collections.Array<String> partGroupingArray in partGroupingArrays)
        {
            List<Part> groupedParts = new List<Part>();
            foreach (String partName in partGroupingArray)
            {
                String parsedName = partName.Replace("@", "");

                Part part = partNames[parsedName];
                groupedParts.Add(part);
            }

            manualPartGroupings.Add(groupedParts);
        }

        parts.LoadManualPartGroupings(manualPartGroupings);
    }

    public static Vector3 StrToVector3(String str)
    {
        str = str.Substring(1, str.Length - 2);
        String[] values = str.Split(", ");

        Vector3 result = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        return result;
    }

    public static void SetCurrentPath(String path, Interface ui)
    {
        currentPath = path;
        ui.SetCurrentPath(path);
    }

    public static String GetCurrentPath()
    {
        return currentPath;
    }
}
