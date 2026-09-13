using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.GraphToolkit.Editor;

[System.Serializable]
[Node("Animation", null, "Layered Blend")]
public class AnimGraphLayeredBlendNode : AnimGraphNode {
    public static readonly int MinLayers = 1;
    public static readonly int MaxLayers = 8;

    public static readonly string InputCountName = "Input Count";
    public static readonly string BasePoseName = "Base Pose";

    public static string PoseName(int index) => $"Pose[{index}]";
    public static string WeightName(int index) => $"Weight[{index}]";
    public static string ModeName(int index) => $"Mode[{index}]";
    public static string MaskName(int index) => $"Mask[{index}]";

    protected override void OnDefineOptions(IOptionDefinitionContext context) {
        context.AddOption<int>(InputCountName).WithDefaultValue(MinLayers).Delayed();
    }

    protected override void OnDefinePorts(IPortDefinitionContext context) {
        var inputCountOption = GetNodeOptionByName(InputCountName);
        inputCountOption.TryGetValue<int>(out var inputCount);
        inputCount = Mathf.Clamp(inputCount, MinLayers, MaxLayers);

        context.AddInputPort<AnimGraphPose>(BasePoseName).Build();

        for (int i = 0; i < inputCount; i += 1) {
            context.AddInputPort<AnimGraphPose>(PoseName(i)).Build();
            context.AddInputPort<float>(WeightName(i)).WithDefaultValue(1.0f).Build();
            context.AddInputPort<AnimLayerMode>(ModeName(i)).WithDefaultValue(AnimLayerMode.Override).Build();
            context.AddInputPort<AvatarMask>(MaskName(i)).Build();
        }

        AddPoseOutput(context);
    }
}
