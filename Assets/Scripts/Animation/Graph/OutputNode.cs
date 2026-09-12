using UnityEngine.Playables;
using Unity.GraphToolkit.Editor;

[System.Serializable]
[Node("Animation", null, "Output")]
public class AnimGraphOutputNode : AnimGraphNode {
    protected override void OnDefinePorts(IPortDefinitionContext context) {
        context.AddInputPort<AnimGraphPose>("Input").Build();
    }

    public override Playable CreatePlayable(PlayableGraph graph) {
        var inputNode = GetInputAnimNode("Input");
        return inputNode.CreatePlayable(graph);
    }
}
