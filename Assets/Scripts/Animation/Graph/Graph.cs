using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[System.Serializable]
[Graph("animgraph")]
public class AnimGraph : Graph {
    [MenuItem("Assets/Create/Animation/Graph")]
    static void CreateAssetFile() {
        GraphDatabase.PromptInProjectBrowserToCreateNewAsset<AnimGraph>();
    }

    public PlayableGraph CreatePlayableGraph(string name, Animator animator) {
        var outputNodes = GetNodes().OfType<AnimGraphOutputNode>().ToArray();
        if (outputNodes.Length == 0) {
            throw new System.Exception("Anim graph contains no output node"); // @Todo: use a better exception type
        }
        if (outputNodes.Length > 1) {
            throw new System.Exception("Anim graph contains more than one output node"); // @Todo: use a better exception type
        }

        var outputNode = outputNodes[0];

        var graph = PlayableGraph.Create(name);
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var outputPlayable = outputNode.CreatePlayable(graph);

        var output = AnimationPlayableOutput.Create(graph, name, animator);
        output.SetSourcePlayable(outputPlayable);

        return graph;
    }
}

[DataTypeStyleMapper(typeof(AnimGraph))]
public class AnimGraphDataStyleMapper : DataTypeStyleMapper {
    public AnimGraphDataStyleMapper() {
        var poseIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/AnimGraphPose.png");
        Register(typeof(AnimGraphPose), poseIcon, Color.white);

        var clipIcon = EditorGUIUtility.IconContent("AnimationClip Icon").image as Texture2D;
        Register(typeof(AnimationClip), clipIcon, Color.white);

        var avatarMaskIcon = EditorGUIUtility.IconContent("AvatarMask Icon").image as Texture2D;
        Register(typeof(AvatarMask), avatarMaskIcon, Color.white);
    }
}
