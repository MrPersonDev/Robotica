using Godot;
using System;

public partial class Pivot : Node3D
{
    private const float ZOOM_SPEED = 0.2f;
    private const float X_ORBIT_SPEED = 200.0f;
    private const float Y_ORBIT_SPEED = 100.0f;
    private const float PAN_SPEED = 100.0f;
    private const float CAM_MAX_ANGLE = 90.0f;
    private const float CAM_MIN_ANGLE = 90.0f;
    private const float ORTHOGONAL_DIST_MULTIPLIER = 2.0f;

    private bool orbiting = false;
    private bool panning = false;
    
    [Export]
    private bool orthogonal = false;

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath camPosPath, mainCamPath, outlineCamPath, orthographicGridPath;

    private Node3D camPos;
    private Camera3D backgroundCam, mainCam, outlineCam;
    private MeshInstance3D orthographicGrid;

    public override void _Ready()
    {
        camPos = (Node3D)GetNode(camPosPath);
        mainCam = (Camera3D)GetNode(mainCamPath);
        outlineCam = (Camera3D)GetNode(outlineCamPath);
        orthographicGrid = (MeshInstance3D)GetNode(orthographicGridPath);
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        bool panningAltInputPressed = inputEvent.IsActionPressed("pan_alt_main") && inputEvent.IsActionPressed("pan_alt_modifier");
        bool panningInputPressed = inputEvent.IsActionPressed("pan") || panningAltInputPressed;

        bool panningAltInputReleased = inputEvent.IsActionReleased("pan_alt_main") || inputEvent.IsActionReleased("pan_alt_modifier");
        bool panningInputReleased = inputEvent.IsActionReleased("pan") || panningAltInputReleased;

        if (!orbiting && panningInputPressed)
            panning = true;
        else if (panningInputReleased)
            panning = false;

        if (!panning && inputEvent.IsActionPressed("orbit"))
            orbiting = true;
        else if (inputEvent.IsActionReleased("orbit"))
            orbiting = false;

        if (Input.IsActionJustPressed("zoom_in"))
            Scale *= 1.0f / (1.0f + ZOOM_SPEED);
        else if (Input.IsActionJustPressed("zoom_out"))
            Scale *= 1.0f + ZOOM_SPEED;

        if (inputEvent is InputEventMouseMotion)
        {
            InputEventMouseMotion mouseEvent = (InputEventMouseMotion)inputEvent;
            if (orbiting)
                OrbitingUpdate(mouseEvent);
            else if (panning)
                PanningUpdate(mouseEvent);
        }
    }

    public override void _Process(double delta)
    {
        UpdateCameraPositions();
        UpdateCameraZoom();
    }

    private void OrbitingUpdate(InputEventMouseMotion mouseEvent)
    {
        Rotate(Vector3.Up, -mouseEvent.Relative.X / Y_ORBIT_SPEED);
        Rotate(Transform.Basis.X.Normalized(), -mouseEvent.Relative.Y / X_ORBIT_SPEED);

        if (RotationDegrees.X < -CAM_MIN_ANGLE || RotationDegrees.X > CAM_MAX_ANGLE)
            ClampCameraAngle();
    }

    private void ClampCameraAngle()
    {
        Vector3 rotation = RotationDegrees;
        rotation.X = (float)Math.Clamp(RotationDegrees.X, -CAM_MIN_ANGLE, CAM_MAX_ANGLE);
        RotationDegrees = rotation;
    }

    private void PanningUpdate(InputEventMouseMotion mouseEvent)
    {
        float curPanSpeed = orthogonal ? PAN_SPEED * ORTHOGONAL_DIST_MULTIPLIER : PAN_SPEED;
        Translate(Vector3.Up * mouseEvent.Relative.Y / curPanSpeed);
        Translate(Vector3.Left * mouseEvent.Relative.X / curPanSpeed);
    }

    private void UpdateCameraPositions()
    {
        mainCam.GlobalTransform = camPos.GlobalTransform;
        outlineCam.GlobalTransform = camPos.GlobalTransform;
    }

    private void UpdateCameraZoom()
    {
        if (orthogonal)
            SetCameraSize(camPos.GlobalPosition.DistanceTo(GlobalPosition) / ORTHOGONAL_DIST_MULTIPLIER);
        else
            SetCameraSize(1.0f);
    }

    private void SetCameraSize(float size)
    {
        mainCam.Size = size;
        outlineCam.Size = size;
    }

    public void SetOrthogonal(bool value)
    {
        if (value != orthogonal)
        {
            if (value)
                Scale *= ORTHOGONAL_DIST_MULTIPLIER;
            else
                Scale /= ORTHOGONAL_DIST_MULTIPLIER;
        }
        
        orthogonal = value;

        orthographicGrid.Visible = value;

        if (value)
            SetCameraProjection(Camera3D.ProjectionType.Orthogonal);
        else
            SetCameraProjection(Camera3D.ProjectionType.Perspective);
    }

    private void SetCameraProjection(Camera3D.ProjectionType projectionType)
    {
        mainCam.Projection = projectionType;
        outlineCam.Projection = projectionType;
    }

    public Vector3 GetWorldPos(Vector2 screenPos, float dist)
    {
        return mainCam.ProjectPosition(screenPos, dist);
    }

    public Vector2 GetScreenPos(Vector3 pos)
    {
        return mainCam.UnprojectPosition(pos);
    }

    public Vector3 GetCamPos()
    {
        return mainCam.GlobalPosition / (orthogonal ? ORTHOGONAL_DIST_MULTIPLIER : 1);
    }

    public float GetCamDist(Vector3 pos)
    {
        if (orthogonal)
            return mainCam.Size;
        
        return GetCamPos().DistanceTo(pos);
    }

    public Vector3 GetCamNormal()
    {
        return mainCam.GlobalTransform.Basis.Z;
    }

    public Transform3D GetCamTransform()
    {
        return mainCam.GlobalTransform;
    }

    public Vector3 GetRayOrigin(Vector2 screenPos)
    {
        return mainCam.ProjectRayOrigin(screenPos);
    }

    public Vector3 GetRayNormal(Vector2 screenPos)
    {
        return mainCam.ProjectRayNormal(screenPos);
    }
}
