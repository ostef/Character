using System;
using Unity.GraphToolkit.Editor;

public sealed class EditorAnimGraphPose {}

[Serializable]
public abstract class EditorAnimGraphNode : Node {
    public static readonly string OutputName = "Output";

    public EditorAnimGraphNode GetInputAnimNode(string name) {
        var port = GetInputPortByName(name);
        if (port == null) {
            throw new Exception($"Input port '{name}' not found");
        }

        if (!port.IsConnected) {
            return null;
        }

        var inputNode = port.FirstConnectedPort.GetNode();
        if (inputNode is not EditorAnimGraphNode) {
            throw new Exception($"Input port '{name}' is not an animation pose");
        }

        return (EditorAnimGraphNode)inputNode;
    }

    protected void AddPoseOutput(IPortDefinitionContext context) {
        context.AddOutputPort<EditorAnimGraphPose>(OutputName).Build();
    }
}
