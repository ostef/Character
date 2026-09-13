using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.GraphToolkit.Editor;

[System.Serializable]
[Node("Animation", null, "Blend")]
public class AnimGraphBlendNode : AnimGraphNode {
    public static int MinInputs = 2;
    public static int MaxInputs = 8;

    public static readonly string InputCountName = "Input Count";

    public static string PoseName(int index) => $"Pose[{index}]";
    public static string WeightName(int index) => $"Weight[{index}]";

    protected override void OnDefineOptions(IOptionDefinitionContext context) {
        context.AddOption<int>(InputCountName).WithDefaultValue(MinInputs).Delayed();
    }

    protected override void OnDefinePorts(IPortDefinitionContext context) {
        var poseCountOption = GetNodeOptionByName(InputCountName);
        poseCountOption.TryGetValue<int>(out var poseCount);
        poseCount = Mathf.Clamp(poseCount, MinInputs, MaxInputs);

        for (int i = 0; i < poseCount; i += 1) {
            context.AddInputPort<AnimGraphPose>(PoseName(i)).Build();
            context.AddInputPort<float>(WeightName(i)).WithDefaultValue(i == 0 ? 1.0f : 0.0f).Build();
        }

        AddPoseOutput(context);
    }
}
