using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class CharacterAnimController : MonoBehaviour {
    public AnimBlendSpace2D locomotionBlendSpace;

    PlayableGraph graph;
    ScriptPlayable<AnimBlendSpace2DMixer> locomotionMixer;

    float stride;
    public float Stride {
        get => stride;
        set => stride = Mathf.Clamp01(value);
    }

    float walkRun;
    public float WalkRun {
        get => walkRun;
        set => walkRun = Mathf.Clamp01(value);
    }

    void Start() {
        graph = PlayableGraph.Create("Character");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var animator = GetComponent<Animator>();
        var output = AnimationPlayableOutput.Create(graph, "Animation", animator);

        locomotionMixer = AnimBlendSpace2DMixer.Create(graph, locomotionBlendSpace);

        output.SetSourcePlayable(locomotionMixer);
        graph.Play();
    }

    void OnDestroy() {
        if (graph.IsValid()) {
            graph.Destroy();
        }
    }

    void Update() {
        locomotionMixer.GetBehaviour().SetParameter(WalkRun, Stride);
    }
}
