using UnityEngine;

[CreateAssetMenu(menuName="Animation/Blend Space 2D")]
public class AnimBlendSpace2D : AnimBlendSpace<Vector2> {
    public enum Mode {
        Cartesian,
        Polar,
    }

    public Mode mode;

    public override Vector2 LerpParameter(Vector2 a, Vector2 b) {
        return Vector2.Lerp(a, b, parameterLerpFactor);
    }

    public override void GetWeights(Vector2 parameter, float[] weights) {
        switch (mode) {
        case Mode.Cartesian:
            GetWeightsCartesian(parameter, weights);
            break;
        case Mode.Polar:
            GetWeightsPolar(parameter, weights);
            break;
        }
    }

    // https://www.gamedev.net/forums/topic/709390-how-to-calculate-the-weights-of-the-nodes-in-a-2d-blend-space/5436434/
    // http://runevision.com/thesis/rune_skovbo_johansen_thesis.pdf
    // https://www.shadertoy.com/view/XlKXWR
    public void GetWeightsCartesian(Vector2 parameter, float[] weights) {
        if (weights.Length != samples.Count) {
            Debug.LogError("Weights length mismatches sample count");
            return;
        }

        var totalWeight = 0.0f;
        for (int i = 0; i < samples.Count; i += 1) {
            var sample = samples[i];
            var dir = parameter - sample.position;
            var weight = 1.0f;
            for (int j = 0; j < samples.Count; j += 1) {
                if (i == j) {
                    continue;
                }

                var other = samples[j];
                var otherDir = other.position - sample.position;
                var sqrLength = otherDir.sqrMagnitude;
                var newWeight = 1 - Vector2.Dot(dir, otherDir) / sqrLength;
                newWeight = Mathf.Clamp(newWeight, 0.0f, 1.0f);
                weight = Mathf.Min(weight, newWeight);
            }

            weights[i] = weight;
            totalWeight += weight;
        }

        for (int i = 0; i < weights.Length; i += 1) {
            weights[i] /= totalWeight;
        }
    }

    public void GetWeightsPolar(Vector2 parameter, float[] weights) {
        if (weights.Length != samples.Count) {
            Debug.LogError("Weights length mismatches sample count");
            return;
        }

        var totalWeight = 0.0f;
        var sampleMagnitude = parameter.magnitude;
        for (int i = 0; i < samples.Count; i += 1) {
            var first = samples[i];
            var firstMagnitude = first.position.magnitude;
            var weight = 1.0f;

            for (int j = 0; j < samples.Count; j += 1) {
                if (i == j) {
                    continue;
                }

                var second = samples[j];
                var secondMagnitude = second.position.magnitude;
                var ijAvgMagnitude = (firstMagnitude + secondMagnitude) * 0.5f;

                var ipMagnitude = (sampleMagnitude - firstMagnitude) / ijAvgMagnitude;
                var ipAngle = Vector2.SignedAngle(first.position, parameter);

                var ijMagnitude = (secondMagnitude - firstMagnitude) / ijAvgMagnitude;
                var ijAngle = Vector2.SignedAngle(first.position, second.position);

                var ipVector = new Vector2(ipMagnitude, ipAngle * 2);
                var ijVector = new Vector2(ijMagnitude, ijAngle * 2);

                var newWeight = 1.0f - Vector2.Dot(ipVector, ijVector) / ijVector.sqrMagnitude;
                newWeight = Mathf.Clamp(newWeight, 0.0f, 1.0f);
                weight = Mathf.Min(weight, newWeight);
            }

            weights[i] = weight;
            totalWeight += weight;
        }

        for (int i = 0; i < weights.Length; i += 1) {
            weights[i] /= totalWeight;
        }
    }
}
