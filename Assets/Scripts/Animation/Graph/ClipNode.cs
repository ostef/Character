using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.GraphToolkit.Editor;

[System.Serializable]
[Node("Animation", null, "Animation Clip")]
public class AnimGraphClipNode : AnimGraphNode {
    protected override void OnDefinePorts(IPortDefinitionContext context) {
        context.AddInputPort<AnimationClip>("Clip").Build();

        AddPoseOutput(context);
    }

    public override Playable CreatePlayable(PlayableGraph graph) {
        var clipPort = GetInputPortByName("Clip");
        clipPort.TryGetValue<AnimationClip>(out var clip);

        if (clip == null) {
            throw new System.Exception("Clip node has no clip set");
        }

        var playable = AnimationClipPlayable.Create(graph, clip);
        // playable.SetSpeed(speed);

        return playable;
    }
}
