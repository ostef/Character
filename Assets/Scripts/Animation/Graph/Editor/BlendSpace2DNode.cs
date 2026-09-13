using Unity.GraphToolkit.Editor;

[System.Serializable]
[Node("Animation", null, "Blend Space 2D")]
public class EditorAnimGraphBlendSpace2DNode : EditorAnimGraphNode {
    public static readonly string BlendSpaceName = "Blend Space";
    public static readonly string ParameterXName = "Parameter X";
    public static readonly string ParameterYName = "Parameter Y";

    protected override void OnDefinePorts(IPortDefinitionContext context) {
        context.AddInputPort<AnimBlendSpace2D>(BlendSpaceName).Build();
        context.AddInputPort<float>(ParameterXName).Build();
        context.AddInputPort<float>(ParameterYName).Build();

        AddPoseOutput(context);
    }
}
