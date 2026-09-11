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
    [Range(0.01f, 1.0f)]
    public float parameterLerpFactor = 0.1f;

    public abstract T LerpParameter(T a, T b);
    public abstract void GetWeights(T parameter, float[] weights);
}
