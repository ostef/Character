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
    [SerializeField] float maxAcceleration = 0.1f;

    PlayableGraph graph;
    ScriptPlayable<AnimBlendSpace2DMixer> locomotionMixer;
    ScriptPlayable<AnimBlendSpace2DMixer> leanMixer;

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

        var relativeAcceleration = Quaternion.Euler(0, -heading, 0) * acceleration;
        var relativeForwardAccel = relativeAcceleration.z;
        var relativeRightAccel = relativeAcceleration.x;

        leanX = Mathf.Clamp(relativeRightAccel / maxAcceleration, -1, 1);
        leanY = Mathf.Clamp(relativeForwardAccel / maxAcceleration, -1, 1);
        leanMixer.GetBehaviour().SetParameter(leanX, leanY);
        locomotionMixer.GetBehaviour().SetParameter(WalkRun, Stride);
    }
}
