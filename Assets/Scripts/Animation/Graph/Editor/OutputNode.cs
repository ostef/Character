using Unity.GraphToolkit.Editor;

[System.Serializable]
[Node("Animation", null, "Output")]
public class EditorAnimGraphOutputNode : EditorAnimGraphNode {
    public static readonly string InputName = "Input";

    protected override void OnDefinePorts(IPortDefinitionContext context) {
        context.AddInputPort<EditorAnimGraphPose>(InputName).Build();
    }
}
