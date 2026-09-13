using Unity.GraphToolkit.Editor;

[System.Serializable]
[Node("Animation", null, "Blend Space 1D")]
public class EditorAnimGraphBlendSpace1DNode : EditorAnimGraphNode {
    public static readonly string BlendSpaceName = "Blend Space";
    public static readonly string ParameterName = "Parameter";

    protected override void OnDefinePorts(IPortDefinitionContext context) {
        context.AddInputPort<AnimBlendSpace1D>(BlendSpaceName).Build();
        context.AddInputPort<float>(ParameterName).Build();

        AddPoseOutput(context);
    }
}
