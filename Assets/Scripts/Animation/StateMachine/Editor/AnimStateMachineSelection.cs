using System;
using UnityEngine;

// Proxy object put in Selection.activeObject so the Inspector can show the state or transition selected in the
// state machine editor window.
public class AnimStateMachineSelection : ScriptableObject {
    public AnimStateMachine stateMachine;
    public string stateID;
    public string transitionID;

    [NonSerialized]
    public AnimStateMachineEditorWindow window;

    public AnimState State => stateMachine != null && stateID != null ? stateMachine.GetState(stateID) : null;
    public AnimStateTransition Transition => stateMachine != null && transitionID != null ? stateMachine.GetTransition(transitionID) : null;

    public void RefreshName() {
        var transition = Transition;
        if (transition != null) {
            name = GetTransitionLabel(transition);
            return;
        }

        var state = State;
        name = state != null ? state.name : "";
    }

    public string GetStateName(string id) {
        var state = stateMachine != null ? stateMachine.GetState(id) : null;
        return state != null ? state.name : "(Missing)";
    }

    public string GetTransitionLabel(AnimStateTransition transition) {
        var label = $"{GetStateName(transition.fromStateID)} -> {GetStateName(transition.toStateID)}";
        if (!string.IsNullOrEmpty(transition.name)) {
            label = $"{transition.name} ({label})";
        }

        return label;
    }
}
