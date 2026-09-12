using UnityEngine;

[CreateAssetMenu(menuName="Animation/Blend Space 1D")]
public class AnimBlendSpace1D : AnimBlendSpace<float> {
    public override float InterpParameter(float a, float b, float deltaTime) {
        return Interpolate(a, b, deltaTime, parameterInterpSpeed);
    }

    public override void GetWeights(float parameter, float[] weights) {
        if (weights.Length != samples.Count) {
            Debug.LogError("Weights length mismatches sample count");
            return;
        }

        if (samples.Count == 0) {
            return;
        }

        if (samples.Count == 1) {
            weights[0] = 1;
            return;
        }

        for (int i = 0; i < weights.Length; i += 1) {
            weights[i] = 0;
        }

        if (parameter <= samples[0].position) {
            weights[0] = 1;
            return;
        } else if (parameter >= samples[^1].position) {
            weights[^1] = 1;
            return;
        }

        for (int i = 0; i < samples.Count - 1; i += 1) {
            if (parameter >= samples[i].position && parameter < samples[i + 1].position) {
                var a = samples[i];
                var b = samples[i + 1];
                var t = (parameter - a.position) / (b.position - a.position);

                weights[i] = 1 - t;
                weights[i + 1] = t;

                return;
            }
        }
    }
}
