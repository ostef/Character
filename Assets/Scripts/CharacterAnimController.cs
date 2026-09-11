using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;

[RequireComponent(typeof(Animator))]
public class CharacterAnimController : MonoBehaviour {
    [SerializeField] AnimBlendSpace2D locomotionBlendSpace;
    [SerializeField] AnimBlendSpace2D leanBlendSpace;
    [SerializeField] AnimationClip breathingAdditive;
    [SerializeField] AvatarMask upperBodyMask;
    [SerializeField] float maxAcceleration = 10.0f;

    PlayableGraph graph;
    ScriptPlayable<AnimBlendSpace2DMixer> locomotionMixer;
    ScriptPlayable<AnimBlendSpace2DMixer> leanMixer;

    [Range(-1, 1)]
    public float leanX;

    [Range(-1, 1)]
    public float leanY;

    float stride;
    public float Stride {
        get => stride;
        set => stride = Mathf.Clamp01(value);
    }

    public float heading;

    float walkRun;
    public float WalkRun {
        get => walkRun;
        set => walkRun = Mathf.Clamp01(value);
    }

    Vector3 velocityLastFrame;
    public Vector3 velocity;
    [SerializeField, SerializeReadOnly] Vector3 acceleration;

    void Start() {
        var animator = GetComponent<Animator>();

        graph = PlayableGraph.Create("Character");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        locomotionMixer = AnimBlendSpace2DMixer.Create(graph, locomotionBlendSpace);
        leanMixer = AnimBlendSpace2DMixer.Create(graph, leanBlendSpace);

        var additivePlayable = AnimationClipPlayable.Create(graph, breathingAdditive);

        var additiveMixer = AnimationLayerMixerPlayable.Create(graph, 3);
        additiveMixer.ConnectInput(0, locomotionMixer, 0, 1.0f);
        additiveMixer.ConnectInput(1, leanMixer, 0, 1.0f);
        additiveMixer.ConnectInput(2, additivePlayable, 0, 1.0f);
        additiveMixer.SetLayerAdditive(1, true);
        additiveMixer.SetLayerAdditive(2, true);
        additiveMixer.SetLayerMaskFromAvatarMask(2, upperBodyMask);

        var output = AnimationPlayableOutput.Create(graph, "Animation", animator);
        output.SetSourcePlayable(additiveMixer);

        graph.Play();
    }

    void OnDestroy() {
        if (graph.IsValid()) {
            graph.Destroy();
        }
    }

    void LateUpdate() {
        acceleration = velocity - velocityLastFrame;
        velocityLastFrame = velocity;

        var rotation = Quaternion.Euler(0, heading, 0);
        var forwardVector = rotation * Vector3.forward;
        var rightVector = rotation * Vector3.right;
        var relativeForwardAccel = Vector3.Dot(Vector3.forward * acceleration.z, forwardVector);
        var relativeRightAccel = Vector3.Dot(Vector3.right * acceleration.x, rightVector);
        Debug.Log($"{relativeForwardAccel}, {relativeRightAccel}");

        leanX = Mathf.Clamp(relativeRightAccel * 100, -1, 1);
        leanY = Mathf.Clamp(relativeForwardAccel * 100, -1, 1);
        leanMixer.GetBehaviour().SetParameter(leanX, leanY);
        locomotionMixer.GetBehaviour().SetParameter(WalkRun, Stride);
    }
}
