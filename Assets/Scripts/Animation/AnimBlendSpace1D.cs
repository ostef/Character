using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Animation/Blend Space 1D")]
public class AnimBlendSpace1D : ScriptableObject {
    [System.Serializable]
    public struct Sample {
        public float position;
        public AnimationClip clip;
    }

    public List<Sample> samples;
    [Range(0.01f, 1.0f)]
    public float parameterLerpFactor = 0.1f;
}
