using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AnimBlendSpace1DMixer : PlayableBehaviour {
    AnimBlendSpace1D blendSpace;
    AnimationMixerPlayable mixer;
    AnimationClipPlayable[] clipPlayables;

    float targetParameter;
    float parameter;
    float normalizedTime;
    float normalizedTimeSpeed = 1.0f;

    public static ScriptPlayable<AnimBlendSpace1DMixer> Create(PlayableGraph graph, AnimBlendSpace1D blendSpace) {
        var numSamples = blendSpace.samples.Count;
        var mixer = AnimationMixerPlayable.Create(graph, numSamples);
        var clipPlayables = new AnimationClipPlayable[numSamples];

        for (int i = 0; i < numSamples; i += 1) {
            clipPlayables[i] = AnimationClipPlayable.Create(graph, blendSpace.samples[i].clip);
            graph.Connect(clipPlayables[i], 0, mixer, i);
            mixer.SetInputWeight(i, 0.0f);
        }

        var scriptPlayable = ScriptPlayable<AnimBlendSpace1DMixer>.Create(graph, 1);
        graph.Connect(mixer, 0, scriptPlayable, 0);
        scriptPlayable.SetInputWeight(0, 1.0f);

        var behaviour = scriptPlayable.GetBehaviour();
        behaviour.blendSpace = blendSpace;
        behaviour.mixer = mixer;
        behaviour.clipPlayables = clipPlayables;

        return scriptPlayable;
    }

    public void SetParameter(float newParameter) {
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
        parameter = Mathf.Lerp(parameter, targetParameter, blendSpace.parameterLerpFactor);

        if (blendSpace.samples.Count == 0) {
            return;
        }

        if (blendSpace.samples.Count == 1) {
            mixer.SetInputWeight(0, 1.0f);
            return;
        }

        int sampleIndex = -1;

        // Set all input weights to 0 and identify which samples correspond to parameter
        for (int i = 0; i < blendSpace.samples.Count; i += 1) {
            if (i <= blendSpace.samples.Count - 2 && parameter >= blendSpace.samples[i].position && parameter < blendSpace.samples[i + 1].position) {
                sampleIndex = i;
            }

            mixer.SetInputWeight(i, 0.0f);
        }

        // Set weights of the samples associated with parameter
        if (sampleIndex >= 0) {
            var a = blendSpace.samples[sampleIndex];
            var b = blendSpace.samples[sampleIndex + 1];
            var t = (parameter - a.position) / (b.position - a.position);

            mixer.SetInputWeight(sampleIndex, 1 - t);
            mixer.SetInputWeight(sampleIndex + 1, t);

            var clipASpeed = GetAnimationClipNormalizedTimeSpeed(a.clip);
            var clipBSpeed = GetAnimationClipNormalizedTimeSpeed(b.clip);

            normalizedTimeSpeed = Mathf.Lerp(clipASpeed, clipBSpeed, t);
        } else if (parameter <= blendSpace.samples[0].position) {
            mixer.SetInputWeight(0, 1.0f);

            var clip = blendSpace.samples[0].clip;
            normalizedTimeSpeed = GetAnimationClipNormalizedTimeSpeed(clip);
        } else if (parameter >= blendSpace.samples[^1].position) {
            mixer.SetInputWeight(mixer.GetInputCount() - 1, 1.0f);

            var clip = blendSpace.samples[^1].clip;
            normalizedTimeSpeed = GetAnimationClipNormalizedTimeSpeed(clip);
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
