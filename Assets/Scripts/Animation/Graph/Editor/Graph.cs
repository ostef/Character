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
