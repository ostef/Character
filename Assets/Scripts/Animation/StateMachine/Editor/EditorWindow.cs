using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class AnimStateMachineEditorWindow : EditorWindow {
    private const float GridMinorSpacing = 10.0f;
    private const float GridMajorSpacing = 100.0f;
    private const float MinZoom = 0.5f;
    private const float MaxZoom = 2f;

    private AnimStateMachine stateMachine = null;
    private Vector2 viewOrigin = new Vector2(-100, -100);
    private float zoom = 1.0f;

    private List<string> selectedStateIDs = new();

    private string renamingStateID;
    private string renameBuffer;

    private bool isPanning;
    private Vector2 panLastMouseScreenPosition;

    private bool isDraggingStates;
    private Vector2 dragStateStartMouseWorldPosition;
    private Dictionary<string, Vector2> dragStateStartPositions = new();

    private GUIStyle stateLabelStyle;

    private Rect CanvasRect => new Rect(0, 0, position.width, position.height);

    [MenuItem("Window/Animation State Machine Editor")]
    public static void Open() {
        var window = GetWindow<AnimStateMachineEditorWindow>("State Machine");
        window.minSize = new Vector2(400, 300);
    }

    [UnityEditor.Callbacks.OnOpenAsset(1)]
    public static bool OnOpenAsset(EntityId entityId) {
        var obj = EditorUtility.EntityIdToObject(entityId) as AnimStateMachine;
        if (obj == null) {
            return false;
        }

        var window = GetWindow<AnimStateMachineEditorWindow>("State Machine");
        window.stateMachine = obj;
        // window.FrameAll();
        window.Show();

        return true;
    }

    private void OnEnable() {
        wantsMouseEnterLeaveWindow = true;
        Undo.undoRedoPerformed += Repaint;
    }

    private void OnDisable() {
        Undo.undoRedoPerformed -= Repaint;
    }

    private Vector2 WorldToScreen(Vector2 world) {
        return (world - viewOrigin) * zoom + CanvasRect.position;
    }

    private Vector2 ScreenToWorld(Vector2 screen) {
        return (screen - CanvasRect.position) / zoom + viewOrigin;
    }

    private Rect WorldToScreenRect(Rect worldRect) {
        Vector2 pos = WorldToScreen(worldRect.position);
        Vector2 size = worldRect.size * zoom;
        return new Rect(pos, size);
    }

    private void EnsureStyles() {
        if (stateLabelStyle != null) {
            return;
        }

        stateLabelStyle = new GUIStyle(EditorStyles.boldLabel) {
            alignment=TextAnchor.MiddleCenter,
            wordWrap=true,
            normal={ textColor=Color.white },
        };
    }

    private void OnGUI() {
        EnsureStyles();

        if (stateMachine == null) {
            DrawNoStateMachineGUI();
            return;
        }

        var rect = CanvasRect;
        EditorGUI.DrawRect(rect, new Color(0.17f, 0.17f, 0.17f));

        DrawGrid(rect);

        DrawStates();

        HandleInput(rect);

        if (isPanning || isDraggingStates) {
            Repaint();
        }
    }

    private void DrawNoStateMachineGUI() {
        var rect = CanvasRect;
        GUILayout.BeginArea(rect);
        EditorGUILayout.HelpBox("Select an Anim State Machine asset to begin.", MessageType.Info);
        GUILayout.EndArea();
    }

    private void DrawGrid(Rect rect) {
        Handles.BeginGUI();
        DrawGridLines(rect, GridMinorSpacing, new Color(1, 1, 1, 0.05f));
        DrawGridLines(rect, GridMajorSpacing, new Color(1, 1, 1, 0.12f));
        Handles.EndGUI();
    }

    private void DrawGridLines(Rect rect, float spacing, Color color) {
        var spacingScreen = spacing * zoom;
        if (spacingScreen < 6f) {
            return;
        }

        Handles.color = color;

        var topLeftWorld = ScreenToWorld(rect.min);
        var bottomRightWorld = ScreenToWorld(rect.max);

        var firstX = Mathf.Floor(Mathf.Floor(topLeftWorld.x / spacing) * spacing);
        for (var x = firstX; x < bottomRightWorld.x; x = Mathf.Floor(x + spacing)) {
            var sx = WorldToScreen(new Vector2(x, 0)).x;
            sx = Mathf.Floor(sx);

            Handles.DrawLine(new Vector3(sx, rect.y), new Vector3(sx, rect.yMax));
        }

        var firstY = Mathf.Floor(Mathf.Floor(topLeftWorld.y / spacing) * spacing);
        for (var y = firstY; y < bottomRightWorld.y; y = Mathf.Floor(y + spacing)) {
            var sy = WorldToScreen(new Vector2(0, y)).y;
            sy = Mathf.Floor(sy);

            Handles.DrawLine(new Vector3(rect.x, sy), new Vector3(rect.xMax, sy));
        }
    }

    private void DrawStates() {
        foreach (var state in stateMachine.states) {
            var screenRect = WorldToScreenRect(state.nodeRect);
            if (!CanvasRect.Overlaps(screenRect)) {
                continue;
            }

            EditorGUI.DrawRect(screenRect, state.nodeColor);

            Handles.BeginGUI();
            Handles.color = new Color(0, 0, 0, 0.6f);
            DrawRectOutline(screenRect, 1.5f);

            if (selectedStateIDs.Contains(state.id)) {
                Handles.color = new Color(0.870f, 0.512f, 0.0435f, 1);
                DrawRectOutline(screenRect, 2.5f);
            }

            if (stateMachine.entryStateID == state.id) {
                Handles.color = new Color(0.3f, 0.5f, 0.8f, 1);
                DrawRectOutline(screenRect.ExpandBy(new RectOffset(3, 3, 3, 3)), 2f);
            }

            Handles.EndGUI();

            if (renamingStateID == state.id) {
                GUI.SetNextControlName("RenameField");

                var e = Event.current;
                var commitRename = e?.type == EventType.KeyDown && e.keyCode == KeyCode.Return;
                var cancelRename = e?.type == EventType.KeyDown && e.keyCode == KeyCode.Escape;

                renameBuffer = EditorGUI.TextField(screenRect, renameBuffer);

                if (commitRename) {
                    CommitRenameState();
                } else if (cancelRename) {
                    CancelRenameState();
                }

                EditorGUI.FocusTextInControl("RenameField");
            } else {
                var style = new GUIStyle(stateLabelStyle) { fontSize=Mathf.Clamp(Mathf.RoundToInt(12 * zoom), 8, 20) };
                GUI.Label(screenRect, state.name, style);
            }
        }
    }

    private void DrawRectOutline(Rect rect, float thickness) {
        Handles.DrawAAPolyLine(thickness,
            new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMax, rect.yMin),
            new Vector3(rect.xMax, rect.yMax), new Vector3(rect.xMin, rect.yMax),
            new Vector3(rect.xMin, rect.yMin)
        );
    }

    private AnimState FindStateAtScreenPoint(Vector2 point) {
        for (int i = stateMachine.states.Count - 1; i >= 0; i -= 1) {
            var state = stateMachine.states[i];
            if (WorldToScreenRect(state.nodeRect).Contains(point)) {
                return state;
            }
        }

        return null;
    }

    private void HandleInput(Rect canvas) {
        var e = Event.current;
        if (e == null) {
            return;
        }

        var mouseInCanvas = canvas.Contains(e.mousePosition);

        switch (e.type) {
        case EventType.ScrollWheel:
            if (mouseInCanvas) {
                HandleZoom(e);
            }
            break;

        case EventType.MouseDown:
            HandleMouseDown(e, mouseInCanvas);
            break;
        case EventType.MouseDrag:
            HandleMouseDrag(e);
            break;
        case EventType.MouseUp:
            HandleMouseUp(e);
            break;
        // case EventType.KeyDown:
        //     HandleKeyDown(e);
        //     break;
        }
    }

    private void HandleZoom(Event e) {
        var delta = -e.delta.y * 0.02f;
        var newZoom = Mathf.Clamp(zoom * (1 + delta), MinZoom, MaxZoom);

        // Offset canvas so the zooming happens around the mouse
        var mouseWorldBefore = ScreenToWorld(e.mousePosition);
        zoom = newZoom;

        var canvas = CanvasRect;
        viewOrigin = mouseWorldBefore - (e.mousePosition - canvas.position) / zoom;

        Repaint();

        e.Use();
    }

    private void HandleMouseDown(Event e, bool mouseInCanvas) {
        if (!mouseInCanvas) {
            return;
        }

        if (e.button == (int)MouseButton.Middle || (e.button == (int)MouseButton.Left && e.alt)) {
            isPanning = true;
            panLastMouseScreenPosition = e.mousePosition;
            e.Use();
            return;
        }

        if (e.button == (int)MouseButton.Left) {
            if (renamingStateID != null) {
                CommitRenameState();
                e.Use();
                return;
            }

            var state = FindStateAtScreenPoint(e.mousePosition);
            if (state != null) {
                if (e.clickCount == 2) {
                    BeginRenameState(state);
                    e.Use();
                    return;
                }

                if (e.shift) {
                    if (!selectedStateIDs.Contains(state.id)) {
                        selectedStateIDs.Add(state.id);
                    }
                } else {
                    selectedStateIDs.Clear();
                    selectedStateIDs.Add(state.id);
                }

                isDraggingStates = true;
                dragStateStartMouseWorldPosition = ScreenToWorld(e.mousePosition);
                dragStateStartPositions.Clear();

                foreach (var id in selectedStateIDs) {
                    var selectedState = stateMachine.GetState(id);
                    if (selectedState != null) {
                        dragStateStartPositions[id] = selectedState.nodeRect.position;
                    }
                }

                Undo.RecordObject(stateMachine, "Move States");
                e.Use();

                return;
            }

            if (!e.shift) {
                selectedStateIDs.Clear();
            }
        }
    }

    private void HandleMouseDrag(Event e) {
        if (isPanning) {
            var delta = e.mousePosition - panLastMouseScreenPosition;
            viewOrigin -= delta / zoom;
            panLastMouseScreenPosition = e.mousePosition;
            e.Use();
            Repaint();

            return;
        }

        if (isDraggingStates) {
            var currentWorld = ScreenToWorld(e.mousePosition);
            var delta = currentWorld - dragStateStartMouseWorldPosition;

            foreach (var id in selectedStateIDs) {
                var state = stateMachine.GetState(id);
                if (state != null && dragStateStartPositions.TryGetValue(id, out var startPosition)) {
                    state.nodeRect.position = startPosition + delta;

                    // Snap to grid
                    if (e.control) {
                        var position = state.nodeRect.position;
                        position.x = Mathf.Round(position.x / GridMinorSpacing) * GridMinorSpacing;
                        position.y = Mathf.Round(position.y / GridMinorSpacing) * GridMinorSpacing;

                        state.nodeRect.position = position;
                    }
                }
            }

            EditorUtility.SetDirty(stateMachine);
            e.Use();
            Repaint();

            return;
        }
    }

    private void HandleMouseUp(Event e) {
        if (isPanning && (e.button == (int)MouseButton.Middle || e.button == (int)MouseButton.Left)) {
            isPanning = false;
            e.Use();
        }

        if (isDraggingStates && e.button == (int)MouseButton.Left) {
            isDraggingStates = false;
            e.Use();
        }
    }

    private void BeginRenameState(AnimState state) {
        renamingStateID = state.id;
        renameBuffer = state.name;
    }

    private void CancelRenameState() {
        renamingStateID = null;
        renameBuffer = "";
    }

    private void CommitRenameState() {
        if (renamingStateID == null) {
            return;
        }

        var state = stateMachine.GetState(renamingStateID);
        if (state == null) {
            renamingStateID = null;
            renameBuffer = "";

            Repaint();
            return;
        }

        if (!string.IsNullOrWhiteSpace(renameBuffer)) {
            Undo.RecordObject(stateMachine, "Rename State");
            state.name = renameBuffer.Trim();
            EditorUtility.SetDirty(stateMachine);
        }

        renamingStateID = null;
        renameBuffer = "";

        Repaint();
    }
}