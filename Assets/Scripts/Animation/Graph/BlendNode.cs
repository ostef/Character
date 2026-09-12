using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.GraphToolkit.Editor;

[System.Serializable]
[Node("Animation", null, "Blend")]
public class AnimGraphBlendNode : AnimGraphNode {
    public static int MinInputs = 2;
    public static int MaxInputs = 8;

    protected override void OnDefineOptions(IOptionDefinitionContext context) {
        context.AddOption<int>("Input Count").WithDefaultValue(MinInputs).Delayed();
    }

    protected override void OnDefinePorts(IPortDefinitionContext context) {
        var poseCountOption = GetNodeOptionByName("Input Count");
        poseCountOption.TryGetValue<int>(out var poseCount);
        poseCount = Mathf.Clamp(poseCount, MinInputs, MaxInputs);

        for (int i = 0; i < poseCount; i += 1) {
            context.AddInputPort<AnimGraphPose>($"Pose[{i}]").Build();
            context.AddInputPort<float>($"Weight[{i}]").WithDefaultValue(i == 0 ? 1.0f : 0.0f).Build();
        }

        AddPoseOutput(context);
    }

    public override Playable CreatePlayable(PlayableGraph graph) {
        var inputCountOption = GetNodeOptionByName("Input Count");
        inputCountOption.TryGetValue<int>(out var inputCount);
        inputCount = Mathf.Clamp(inputCount, MinInputs, MaxInputs);

        var mixer = AnimationMixerPlayable.Create(graph, inputCount + 1);
        for (int i = 0; i < inputCount; i += 1) {
            var poseNode = GetInputAnimNode($"Pose[{i}]");
            var playable = poseNode.CreatePlayable(graph);
            var weightNode = GetInputPortByName($"Weight[{i}]");

            // @Todo: weight
            mixer.ConnectInput(i, playable, 0);
        }

        return mixer;
    }
}
