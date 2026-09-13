using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AnimGraphInstance : IDisposable {
    public PlayableGraph Graph { get; private set; }

    Dictionary<string, List<Action<float>>> floatBindings = new();

    public AnimGraphInstance(string name, AnimGraph asset, Animator animator) {
        Graph = PlayableGraph.Create(name);
        Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        if (asset != null) {
            var outputNode = asset.GetNode(asset.outputNodeID);
            if (outputNode != null) {
                var outputPlayable = outputNode.Build(this, asset);

                var output = AnimationPlayableOutput.Create(Graph, name, animator);
                output.SetSourcePlayable(outputPlayable);
            } else {
                Debug.LogError("Anim graph asset has no output");
            }
        } else {
            Debug.LogError("Creating anim graph instance with no anim graph asset");
        }
    }

    public void Dispose() {
        if (Graph.IsValid()) {
            Graph.Destroy();
        }
    }

    public void SetFloat(string variableName, float value) {
        if (floatBindings.TryGetValue(variableName, out var callbacks)) {
            foreach (var cb in callbacks) {
                cb(value);
            }
        } else {
            Debug.LogError($"Float variable '{variableName}' does not exist");
        }
    }

    public void BindFloat(string variableName, Action<float> callback) {
        List<Action<float>> list;
        if (!floatBindings.TryGetValue(variableName, out list)) {
            list = new List<Action<float>>();
            floatBindings[variableName] = list;
        }

        list.Add(callback);
    }
}
