using UnityEngine;
using Unity.GraphToolkit.Editor;

[System.Serializable]
[Node("Animation", null, "Animation Clip")]
public class EditorAnimGraphClipNode : EditorAnimGraphNode {
    public static readonly string ClipName = "Clip";
    public static readonly string SpeedName = "Speed";

    protected override void OnDefinePorts(IPortDefinitionContext context) {
        context.AddInputPort<AnimationClip>(ClipName).Build();
        context.AddInputPort<float>(SpeedName).WithDefaultValue(1.0f).Build();

        AddPoseOutput(context);
    }
}
