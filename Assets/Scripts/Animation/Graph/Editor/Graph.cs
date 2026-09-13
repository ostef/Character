using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

[System.Serializable]
[Graph("animgraph")]
public class EditorAnimGraph : Graph {
    [MenuItem("Assets/Create/Animation/Graph")]
    static void CreateAssetFile() {
        GraphDatabase.PromptInProjectBrowserToCreateNewAsset<EditorAnimGraph>();
    }
}

[DataTypeStyleMapper(typeof(EditorAnimGraph))]
public class EditorAnimGraphDataStyleMapper : DataTypeStyleMapper {
    public EditorAnimGraphDataStyleMapper() {
        var poseIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/AnimGraphPose.png");
        Register(typeof(EditorAnimGraphPose), poseIcon, Color.white);

        var clipIcon = EditorGUIUtility.IconContent("AnimationClip Icon").image as Texture2D;
        Register(typeof(AnimationClip), clipIcon, Color.white);

        var avatarMaskIcon = EditorGUIUtility.IconContent("AvatarMask Icon").image as Texture2D;
        Register(typeof(AvatarMask), avatarMaskIcon, Color.white);
    }
}
