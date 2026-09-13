using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;
using System.Linq;
using System;

[ScriptedImporter(1, "animgraph")]
public class AnimGraphAssetImporter : ScriptedImporter {
    public override void OnImportAsset(AssetImportContext ctx) {
        EditorAnimGraph editorGraph = GraphDatabase.LoadGraphForImporter<EditorAnimGraph>(ctx.assetPath);
        AnimGraph runtimeGraph = ScriptableObject.CreateInstance<AnimGraph>();

        var outputNodes = editorGraph.GetNodes().OfType<EditorAnimGraphOutputNode>().ToArray();
        if (outputNodes.Length > 1) {
            throw new Exception("Anim graph contains more than one output node");
        }

        if (outputNodes.Length > 0) {
            var outputNode = outputNodes[0];
            ProcessOutputNode(runtimeGraph, outputNode);
        }

        ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
    }

    AnimGraphNode ProcessNode(AnimGraph graph, EditorAnimGraphNode node) {
        AnimGraphNode result = null;

        switch (node) {
        case EditorAnimGraphClipNode:
            result = ProcessClipNode(graph, (EditorAnimGraphClipNode)node);
            break;

        case EditorAnimGraphBlendSpace1DNode:
            result = ProcessBlendSpace1DNode(graph, (EditorAnimGraphBlendSpace1DNode)node);
            break;

        case EditorAnimGraphBlendSpace2DNode:
            result = ProcessBlendSpace2DNode(graph, (EditorAnimGraphBlendSpace2DNode)node);
            break;

        case EditorAnimGraphBlendNode:
            result = ProcessBlendNode(graph, (EditorAnimGraphBlendNode)node);
            break;

        case EditorAnimGraphLayeredBlendNode:
            result = ProcessLayeredBlendNode(graph, (EditorAnimGraphLayeredBlendNode)node);
            break;

        case EditorAnimGraphOutputNode:
            ProcessOutputNode(graph, (EditorAnimGraphOutputNode)node);
            break;

        default:
            throw new Exception("Node is not an AnimGraphNode");
        }

        if (result != null) {
            graph.nodes.Add(result);
        }

        return result;
    }

    string GetPortVariableName(IPort port) {
        if (port.IsConnected && port.FirstConnectedPort.GetNode() is IVariableNode variableNode) {
            return variableNode.Variable.Name;
        }

        return "";
    }

    AnimGraphClipNode ProcessClipNode(AnimGraph graph, EditorAnimGraphClipNode node) {
        var clipPort = node.GetInputPortByName(EditorAnimGraphClipNode.ClipName);
        clipPort.TryGetValue<AnimationClip>(out var clip);

        return new AnimGraphClipNode(clip);
    }

    AnimGraphBlendSpace1DNode ProcessBlendSpace1DNode(AnimGraph graph, EditorAnimGraphBlendSpace1DNode node) {
        var blendSpacePort = node.GetInputPortByName(EditorAnimGraphBlendSpace1DNode.BlendSpaceName);
        blendSpacePort.TryGetValue<AnimBlendSpace1D>(out var blendSpace);

        var parameterPort = node.GetInputPortByName(EditorAnimGraphBlendSpace1DNode.ParameterName);
        parameterPort.TryGetValue<float>(out var parameter);

        return new AnimGraphBlendSpace1DNode(blendSpace, parameter, GetPortVariableName(parameterPort));
    }

    AnimGraphBlendSpace2DNode ProcessBlendSpace2DNode(AnimGraph graph, EditorAnimGraphBlendSpace2DNode node) {
        var blendSpacePort = node.GetInputPortByName(EditorAnimGraphBlendSpace2DNode.BlendSpaceName);
        blendSpacePort.TryGetValue<AnimBlendSpace2D>(out var blendSpace);

        var parameterXPort = node.GetInputPortByName(EditorAnimGraphBlendSpace2DNode.ParameterXName);
        parameterXPort.TryGetValue<float>(out var parameterX);

        var parameterYPort = node.GetInputPortByName(EditorAnimGraphBlendSpace2DNode.ParameterYName);
        parameterYPort.TryGetValue<float>(out var parameterY);

        return new AnimGraphBlendSpace2DNode(blendSpace, parameterX, GetPortVariableName(parameterXPort), parameterY, GetPortVariableName(parameterYPort));
    }

    AnimGraphBlendNode ProcessBlendNode(AnimGraph graph, EditorAnimGraphBlendNode node) {
        var inputCountOption = node.GetNodeOptionByName(EditorAnimGraphBlendNode.InputCountName);
        inputCountOption.TryGetValue<int>(out var inputCount);
        inputCount = Mathf.Clamp(inputCount, EditorAnimGraphBlendNode.MinInputs, EditorAnimGraphBlendNode.MaxInputs);

        var inputs = new AnimGraphBlendNode.Input[inputCount];
        for (var i = 0; i < inputCount; i += 1) {
            var inputNode = node.GetInputAnimNode(EditorAnimGraphBlendNode.PoseName(i));
            if (inputNode != null) {
                var inputRuntimeNode = ProcessNode(graph, inputNode);
                inputs[i].nodeID = inputRuntimeNode?.nodeID ?? "";
            } else {
                inputs[i].nodeID = "";
            }

            var weightPort = node.GetInputPortByName(EditorAnimGraphBlendNode.WeightName(i));
            weightPort.TryGetValue<float>(out var weight);

            inputs[i].weight = weight;
            inputs[i].weightVariableName = GetPortVariableName(weightPort);
        }

        return new AnimGraphBlendNode(inputs);
    }

    AnimGraphLayeredBlendNode ProcessLayeredBlendNode(AnimGraph graph, EditorAnimGraphLayeredBlendNode node) {
        var inputCountOption = node.GetNodeOptionByName(EditorAnimGraphLayeredBlendNode.InputCountName);
        inputCountOption.TryGetValue<int>(out var inputCount);
        inputCount = Mathf.Clamp(inputCount, EditorAnimGraphLayeredBlendNode.MinLayers, EditorAnimGraphLayeredBlendNode.MaxLayers);

        AnimGraphNode basePoseRuntimeNode = null;

        var basePoseNode = node.GetInputAnimNode(EditorAnimGraphLayeredBlendNode.BasePoseName);
        if (basePoseNode != null) {
            basePoseRuntimeNode = ProcessNode(graph, basePoseNode);
        }

        var inputs = new AnimGraphLayeredBlendNode.Input[inputCount];
        for (var i = 0; i < inputCount; i += 1) {
            var inputNode = node.GetInputAnimNode(EditorAnimGraphLayeredBlendNode.PoseName(i));
            if (inputNode != null) {
                var inputRuntimeNode = ProcessNode(graph, inputNode);
                inputs[i].nodeID = inputRuntimeNode?.nodeID ?? "";
            } else {
                inputs[i].nodeID = "";
            }

            var weightPort = node.GetInputPortByName(EditorAnimGraphLayeredBlendNode.WeightName(i));
            weightPort.TryGetValue<float>(out var weight);
            inputs[i].weight = weight;
            inputs[i].weightVariableName = GetPortVariableName(weightPort);

            var modePort = node.GetInputPortByName(EditorAnimGraphLayeredBlendNode.ModeName(i));
            modePort.TryGetValue<AnimLayerMode>(out var mode);
            inputs[i].mode = mode;

            var maskPort = node.GetInputPortByName(EditorAnimGraphLayeredBlendNode.MaskName(i));
            maskPort.TryGetValue<AvatarMask>(out var mask);
            inputs[i].mask = mask;
        }

        return new AnimGraphLayeredBlendNode(basePoseRuntimeNode?.nodeID ?? "", inputs);
    }

    void ProcessOutputNode(AnimGraph graph, EditorAnimGraphOutputNode node) {
        var input = node.GetInputAnimNode(EditorAnimGraphOutputNode.InputName);
        if (input != null) {
            var runtimeNode = ProcessNode(graph, input);
            graph.outputNodeID = runtimeNode?.nodeID ?? "";
        }
    }
}
