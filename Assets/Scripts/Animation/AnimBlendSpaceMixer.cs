using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AnimBlendSpaceMixer<TBlendSpace, TParameter> : PlayableBehaviour
where TBlendSpace : AnimBlendSpace<TParameter> {
    protected TBlendSpace blendSpace;
    protected AnimationMixerPlayable mixer;
    protected AnimationClipPlayable[] clipPlayables;

    protected TParameter targetParameter;
    protected TParameter parameter;
    protected float normalizedTime;
    protected float normalizedTimeSpeed = 1.0f;
    protected float[] weights;

    public void SetParameter(TParameter newParameter) {
        targetParameter = newParameter;
    }

    float GetAnimationClipNormalizedTimeSpeed(AnimationClip clip) {
        var frameLength = 1.0f / clip.frameRate;
        if (clip.length <= frameLength) {
            return 0.0f;
        }

        return 1 / clip.length;
    }

    public override void PrepareFrame(Playable playable, FrameData info) {
        parameter = blendSpace.LerpParameter(parameter, targetParameter);

        blendSpace.GetWeights(parameter, weights);

        normalizedTimeSpeed = 0.0f;
        for (int i = 0; i < weights.Length; i += 1) {
            mixer.SetInputWeight(i, weights[i]);

            if (weights[i] > 0) {
                var sample = blendSpace.samples[i];
                var clipSpeed = GetAnimationClipNormalizedTimeSpeed(sample.clip) * sample.speedMultiplier;
                normalizedTimeSpeed += clipSpeed * weights[i];
            }
        }

        normalizedTime += normalizedTimeSpeed * info.deltaTime;
        normalizedTime %= 1.0f;

        // Update playable times
        for (int i = 0; i < clipPlayables.Length; i += 1) {
            var clipLength = clipPlayables[i].GetAnimationClip().length;
            clipPlayables[i].SetTime(normalizedTime * clipLength);
        }
    }
}
