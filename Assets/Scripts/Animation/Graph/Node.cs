using Unity.GraphToolkit.Editor;
using UnityEngine.Playables;

public sealed class AnimGraphPose {}

[System.Serializable]
public abstract class AnimGraphNode : Node {
    public abstract Playable CreatePlayable(PlayableGraph graph);

    protected AnimGraphNode GetInputAnimNode(string name) {
        var port = GetInputPortByName(name);
        if (port == null) {
            throw new System.Exception($"Input port '{name}' not found");
        }

        if (!port.IsConnected) {
            throw new System.Exception($"Input port '{name}' is not connected");
        }

        var inputNode = port.FirstConnectedPort.GetNode();
        if (inputNode is not AnimGraphNode) {
            throw new System.Exception($"Input port '{name}' is not an animation pose");
        }

        return (AnimGraphNode)inputNode;
    }

    protected void AddPoseOutput(IPortDefinitionContext context) {
        context.AddOutputPort<AnimGraphPose>("Output").Build();
    }
}
