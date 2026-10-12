using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimStateMachineSelection))]
public class AnimStateMachineSelectionEditor : Editor {
    // The state machine can be edited from the editor window or by undo/redo, keep the Inspector in sync
    public override bool RequiresConstantRepaint() => true;

    public override void OnInspectorGUI() {
        var selection = (AnimStateMachineSelection)target;
        if (selection.stateMachine == null) {
            EditorGUILayout.HelpBox("No state machine.", MessageType.Info);
            return;
        }

        selection.RefreshName();

        var transition = selection.Transition;
        if (transition != null) {
            DrawTransition(selection, transition);
            return;
        }

        var state = selection.State;
        if (state != null) {
            DrawState(selection, state);
            return;
        }

        EditorGUILayout.HelpBox("The selected item no longer exists.", MessageType.Info);
    }

    private void DrawState(AnimStateMachineSelection selection, AnimState state) {
        var stateMachine = selection.stateMachine;

        EditorGUILayout.LabelField("State", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        var newName = EditorGUILayout.DelayedTextField("Name", state.name);
        if (EditorGUI.EndChangeCheck() && !string.IsNullOrWhiteSpace(newName)) {
            Undo.RecordObject(stateMachine, "Rename State");
            state.name = newName.Trim();
            EditorUtility.SetDirty(stateMachine);

            if (selection.window != null) {
                selection.window.Repaint();
            }
        }

        EditorGUILayout.Space();
        DrawTransitionList(selection, "Outgoing Transitions", stateMachine.transitions.Where(t => t.fromStateID == state.id).ToList());

        EditorGUILayout.Space();
        DrawTransitionList(selection, "Incoming Transitions", stateMachine.transitions.Where(t => t.toStateID == state.id).ToList());
    }

    private void DrawTransitionList(AnimStateMachineSelection selection, string header, List<AnimStateTransition> transitions) {
        EditorGUILayout.LabelField(header, EditorStyles.boldLabel);

        if (transitions.Count == 0) {
            EditorGUILayout.LabelField("None", EditorStyles.miniLabel);
            return;
        }

        foreach (var transition in transitions) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(selection.GetTransitionLabel(transition));

            using (new EditorGUI.DisabledScope(selection.window == null)) {
                if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(60))) {
                    selection.window.SelectTransition(transition.id);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawTransition(AnimStateMachineSelection selection, AnimStateTransition transition) {
        EditorGUILayout.LabelField("Transition", EditorStyles.boldLabel);

        DrawStateReference(selection, "From", transition.fromStateID);
        DrawStateReference(selection, "To", transition.toStateID);
    }

    private void DrawStateReference(AnimStateMachineSelection selection, string label, string stateID) {
        var state = selection.stateMachine.GetState(stateID);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, selection.GetStateName(stateID));

        using (new EditorGUI.DisabledScope(selection.window == null || state == null)) {
            if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(60))) {
                selection.window.SelectState(stateID);
            }
        }

        EditorGUILayout.EndHorizontal();
    }
}
