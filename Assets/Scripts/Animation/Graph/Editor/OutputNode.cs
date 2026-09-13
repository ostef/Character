using UnityEngine.Playables;
using Unity.GraphToolkit.Editor;

[System.Serializable]
[Node("Animation", null, "Output")]
public class AnimGraphOutputNode : AnimGraphNode {
    public static readonly string InputName = "Input";

    protected override void OnDefinePorts(IPortDefinitionContext context) {
        context.AddInputPort<AnimGraphPose>(InputName).Build();
    }
}
