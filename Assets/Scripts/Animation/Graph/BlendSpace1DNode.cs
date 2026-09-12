using UnityEngine.Playables;
using Unity.GraphToolkit.Editor;

[System.Serializable]
[Node("Animation", null, "Blend Space 1D")]
public class AnimGraphBlendSpace1DNode : AnimGraphNode {
    protected override void OnDefinePorts(IPortDefinitionContext context) {
        context.AddInputPort<AnimBlendSpace1D>("Blend Space").Build();
        context.AddInputPort<float>("Parameter").Build();

        AddPoseOutput(context);
    }

    public override Playable CreatePlayable(PlayableGraph graph) {
        var blendSpacePort = GetInputPortByName("Clip");
        blendSpacePort.TryGetValue<AnimBlendSpace1D>(out var blendSpace);

        if (blendSpace == null) {
            throw new System.Exception("Blend space 1D node has no blend space set");
        }

        return AnimBlendSpace1DMixer.Create(graph, blendSpace);
    }
}
