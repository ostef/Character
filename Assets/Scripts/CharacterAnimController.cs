using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;

[RequireComponent(typeof(Animator))]
public class CharacterAnimController : MonoBehaviour {
    [SerializeField] AnimGraph animGraph;
    AnimGraphInstance animGraphInstance;

    [SerializeField] float maxAcceleration = 0.1f;


    [Header("Internal")]
    [SerializeField, SerializeReadOnly] float leanX;
    [SerializeField, SerializeReadOnly] float leanY;

    [SerializeField, SerializeReadOnly] float stride;
    public float Stride {
        get => stride;
        set => stride = Mathf.Clamp01(value);
    }

    [SerializeReadOnly] public float heading;

    [SerializeField, SerializeReadOnly] float walkRun;
    public float WalkRun {
        get => walkRun;
        set => walkRun = Mathf.Clamp01(value);
    }

    Vector3 velocityLastFrame;
    [SerializeField, SerializeReadOnly] public Vector3 velocity;
    [SerializeField, SerializeReadOnly] Vector3 acceleration;

    void Start() {
        var animator = GetComponent<Animator>();

        animGraphInstance = new AnimGraphInstance("Character", animGraph, animator);
        animGraphInstance.Graph.Play();
    }

    void OnDestroy() {
        if (animGraphInstance != null) {
            animGraphInstance.Dispose();
        }
    }

    void LateUpdate() {
        acceleration = velocity - velocityLastFrame;
        velocityLastFrame = velocity;

        var relativeAcceleration = Quaternion.Euler(0, -heading, 0) * acceleration;
        var relativeForwardAccel = relativeAcceleration.z;
        var relativeRightAccel = relativeAcceleration.x;

        leanX = Mathf.Clamp(relativeRightAccel / maxAcceleration, -1, 1);
        leanY = Mathf.Clamp(relativeForwardAccel / maxAcceleration, -1, 1);

        animGraphInstance.SetFloat("LeanRL", leanX);
        animGraphInstance.SetFloat("LeanFB", leanY);
        animGraphInstance.SetFloat("WalkRun", WalkRun);
        animGraphInstance.SetFloat("Stride", Stride);
    }
}
