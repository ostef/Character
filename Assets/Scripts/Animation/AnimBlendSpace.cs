using System.Collections.Generic;
using UnityEngine;

public abstract class AnimBlendSpace<T> : ScriptableObject {
    [System.Serializable]
    public struct Sample {
        public T position;
        public float speedMultiplier;
        public AnimationClip clip;
    }

    public List<Sample> samples;

    [Min(0.0f)]
    public float parameterInterpSpeed = 10.0f;

    public static float Interpolate(float a, float b, float deltaTime, float speed) {
        if (speed <= 0) {
            return b;
        }

        var dist = b - a;
        if (dist * dist < 0.00001f) {
            return b;
        }

        return a + dist * Mathf.Clamp01(deltaTime * speed);
    }

    public abstract T InterpParameter(T a, T b, float deltaTime);
    public abstract void GetWeights(T parameter, float[] weights);
}
