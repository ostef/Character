using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class CharacterAnimController : MonoBehaviour {
    public AnimBlendSpace1D locomotionBlendSpace;

    PlayableGraph graph;
    ScriptPlayable<AnimBlendSpace1DMixer> locomotionMixer;

    public float movementSpeed;

    void Start() {
        graph = PlayableGraph.Create("Character");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var animator = GetComponent<Animator>();
        var output = AnimationPlayableOutput.Create(graph, "Animation", animator);

        locomotionMixer = AnimBlendSpace1DMixer.Create(graph, locomotionBlendSpace);

        output.SetSourcePlayable(locomotionMixer);
        graph.Play();
    }

    void Update() {
        locomotionMixer.GetBehaviour().SetParameter(movementSpeed);
    }
}
