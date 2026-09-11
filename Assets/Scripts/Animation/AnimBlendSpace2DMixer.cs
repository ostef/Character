using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class AnimBlendSpace2DMixer : AnimBlendSpaceMixer<AnimBlendSpace2D, Vector2> {
    public void SetParameter(float x, float y) {
        targetParameter.x = x;
        targetParameter.y = y;
    }

    public static ScriptPlayable<AnimBlendSpace2DMixer> Create(PlayableGraph graph, AnimBlendSpace2D blendSpace) {
        var numSamples = blendSpace.samples.Count;
        var mixer = AnimationMixerPlayable.Create(graph, numSamples);
        var clipPlayables = new AnimationClipPlayable[numSamples];

        for (int i = 0; i < numSamples; i += 1) {
            clipPlayables[i] = AnimationClipPlayable.Create(graph, blendSpace.samples[i].clip);
            mixer.ConnectInput(i, clipPlayables[i], 0);
            mixer.SetInputWeight(i, 0.0f);
        }

        var scriptPlayable = ScriptPlayable<AnimBlendSpace2DMixer>.Create(graph, 1);
        scriptPlayable.ConnectInput(0, mixer, 0);
        scriptPlayable.SetInputWeight(0, 1.0f);

        var behaviour = scriptPlayable.GetBehaviour();
        behaviour.blendSpace = blendSpace;
        behaviour.mixer = mixer;
        behaviour.clipPlayables = clipPlayables;
        behaviour.weights = new float[numSamples];

        return scriptPlayable;
    }
}
