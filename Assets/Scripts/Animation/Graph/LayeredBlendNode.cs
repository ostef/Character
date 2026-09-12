using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.GraphToolkit.Editor;

[System.Serializable]
[Node("Animation", null, "Layered Blend")]
public class AnimGraphLayeredBlendNode : AnimGraphNode {
    public enum LayerMode {
        Override,
        Additive
    }

    public static int MinLayers = 1;
    public static int MaxLayers = 8;

    protected override void OnDefineOptions(IOptionDefinitionContext context) {
        context.AddOption<int>("Input Count").WithDefaultValue(MinLayers).Delayed();
    }

    protected override void OnDefinePorts(IPortDefinitionContext context) {
        var inputCountOption = GetNodeOptionByName("Input Count");
        inputCountOption.TryGetValue<int>(out var inputCount);
        inputCount = Mathf.Clamp(inputCount, MinLayers, MaxLayers);

        context.AddInputPort<AnimGraphPose>($"Base Pose").Build();

        for (int i = 0; i < inputCount; i += 1) {
            context.AddInputPort<AnimGraphPose>($"Pose[{i}]").Build();
            context.AddInputPort<float>($"Weight[{i}]").WithDefaultValue(1.0f).Build();
            context.AddInputPort<LayerMode>($"Mode[{i}]").WithDefaultValue(LayerMode.Override).Build();
            context.AddInputPort<AvatarMask>($"Mask[{i}]").Build();
        }

        AddPoseOutput(context);
    }

    public override Playable CreatePlayable(PlayableGraph graph) {
        var inputCountOption = GetNodeOptionByName("Input Count");
        inputCountOption.TryGetValue<int>(out var inputCount);
        inputCount = Mathf.Clamp(inputCount, MinLayers, MaxLayers);

        var basePoseNode = GetInputAnimNode("Base Pose");
        var basePosePlayable = basePoseNode.CreatePlayable(graph);

        var mixer = AnimationLayerMixerPlayable.Create(graph, inputCount + 1);
        mixer.ConnectInput(0, basePosePlayable, 0, 1.0f);

        for (int i = 0; i < inputCount; i += 1) {
            var poseNode = GetInputAnimNode($"Pose[{i}]");
            var playable = poseNode.CreatePlayable(graph);

            // @Todo: weight, avatar mask, additive or override
            mixer.ConnectInput(i + 1, playable, 0, 1.0f);
        }

        return mixer;
    }
}
