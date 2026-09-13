using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

public sealed class EditorAnimGraphPose {}

[Serializable]
public abstract class EditorAnimGraphNode : Node {
    public static readonly string OutputName = "Output";

    public EditorAnimGraphNode GetInputAnimNode(string name) {
        var port = GetInputPortByName(name);
        if (port == null) {
            throw new Exception($"Input port '{name}' not found");
        }

        return port.GetConnectedNode<EditorAnimGraphNode>();
    }

    protected void AddPoseOutput(IPortDefinitionContext context) {
        context.AddOutputPort<EditorAnimGraphPose>(OutputName).Build();
    }
}
