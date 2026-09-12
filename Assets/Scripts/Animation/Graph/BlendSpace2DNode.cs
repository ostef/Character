using UnityEngine.Playables;
using Unity.GraphToolkit.Editor;

[System.Serializable]
[Node("Animation", null, "Blend Space 2D")]
public class AnimGraphBlendSpace2DNode : AnimGraphNode {
    protected override void OnDefinePorts(IPortDefinitionContext context) {
        context.AddInputPort<AnimBlendSpace2D>("Blend Space").Build();
        context.AddInputPort<float>("Parameter X").Build();
        context.AddInputPort<float>("Parameter Y").Build();

        AddPoseOutput(context);
    }

    public override Playable CreatePlayable(PlayableGraph graph) {
        var blendSpacePort = GetInputPortByName("Clip");
        blendSpacePort.TryGetValue<AnimBlendSpace2D>(out var blendSpace);

        if (blendSpace == null) {
            throw new System.Exception("Blend space 2D node has no blend space set");
        }

        return AnimBlendSpace2DMixer.Create(graph, blendSpace);
    }
}
