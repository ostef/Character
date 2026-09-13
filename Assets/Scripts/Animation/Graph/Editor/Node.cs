using System;
using Unity.GraphToolkit.Editor;
using UnityEngine.Playables;

public sealed class AnimGraphPose {}

[Serializable]
public abstract class AnimGraphNode : Node {
    public static readonly string OutputName = "Output";

    public AnimGraphNode GetInputAnimNode(string name) {
        var port = GetInputPortByName(name);
        if (port == null) {
            throw new Exception($"Input port '{name}' not found");
        }

        if (!port.IsConnected) {
            return null;
        }

        var inputNode = port.FirstConnectedPort.GetNode();
        if (inputNode is not AnimGraphNode) {
            throw new Exception($"Input port '{name}' is not an animation pose");
        }

        return (AnimGraphNode)inputNode;
    }

    protected void AddPoseOutput(IPortDefinitionContext context) {
        context.AddOutputPort<AnimGraphPose>(OutputName).Build();
    }
}
