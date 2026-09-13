using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;
using System.Linq;
using System;

[ScriptedImporter(1, "animgraph")]
public class AnimGraphAssetImporter : ScriptedImporter {
    public override void OnImportAsset(AssetImportContext ctx) {
        AnimGraph editorGraph = GraphDatabase.LoadGraphForImporter<AnimGraph>(ctx.assetPath);
        AnimGraphAsset runtimeGraph = ScriptableObject.CreateInstance<AnimGraphAsset>();

        var outputNodes = editorGraph.GetNodes().OfType<AnimGraphOutputNode>().ToArray();
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

    AnimGraphAssetNode ProcessNode(AnimGraphAsset graph, AnimGraphNode node) {
        AnimGraphAssetNode result = null;

        switch (node) {
        case AnimGraphClipNode:
            result = ProcessClipNode(graph, (AnimGraphClipNode)node);
            break;

        case AnimGraphBlendSpace1DNode:
            result = ProcessBlendSpace1DNode(graph, (AnimGraphBlendSpace1DNode)node);
            break;

        case AnimGraphBlendSpace2DNode:
            result = ProcessBlendSpace2DNode(graph, (AnimGraphBlendSpace2DNode)node);
            break;

        case AnimGraphBlendNode:
            result = ProcessBlendNode(graph, (AnimGraphBlendNode)node);
            break;

        case AnimGraphLayeredBlendNode:
            result = ProcessLayeredBlendNode(graph, (AnimGraphLayeredBlendNode)node);
            break;

        case AnimGraphOutputNode:
            ProcessOutputNode(graph, (AnimGraphOutputNode)node);
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

    AnimGraphAssetClipNode ProcessClipNode(AnimGraphAsset graph, AnimGraphClipNode node) {
        var clipPort = node.GetInputPortByName(AnimGraphClipNode.ClipName);
        clipPort.TryGetValue<AnimationClip>(out var clip);

        return new AnimGraphAssetClipNode(clip);
    }

    AnimGraphAssetBlendSpace1DNode ProcessBlendSpace1DNode(AnimGraphAsset graph, AnimGraphBlendSpace1DNode node) {
        var blendSpacePort = node.GetInputPortByName(AnimGraphBlendSpace1DNode.BlendSpaceName);
        blendSpacePort.TryGetValue<AnimBlendSpace1D>(out var blendSpace);

        var parameterPort = node.GetInputPortByName(AnimGraphBlendSpace1DNode.ParameterName);
        parameterPort.TryGetValue<float>(out var parameter);

        return new AnimGraphAssetBlendSpace1DNode(blendSpace, parameter, GetPortVariableName(parameterPort));
    }

    AnimGraphAssetBlendSpace2DNode ProcessBlendSpace2DNode(AnimGraphAsset graph, AnimGraphBlendSpace2DNode node) {
        var blendSpacePort = node.GetInputPortByName(AnimGraphBlendSpace2DNode.BlendSpaceName);
        blendSpacePort.TryGetValue<AnimBlendSpace2D>(out var blendSpace);

        var parameterXPort = node.GetInputPortByName(AnimGraphBlendSpace2DNode.ParameterXName);
        parameterXPort.TryGetValue<float>(out var parameterX);

        var parameterYPort = node.GetInputPortByName(AnimGraphBlendSpace2DNode.ParameterYName);
        parameterYPort.TryGetValue<float>(out var parameterY);

        return new AnimGraphAssetBlendSpace2DNode(blendSpace, parameterX, GetPortVariableName(parameterXPort), parameterY, GetPortVariableName(parameterYPort));
    }

    AnimGraphAssetBlendNode ProcessBlendNode(AnimGraphAsset graph, AnimGraphBlendNode node) {
        var inputCountOption = node.GetNodeOptionByName(AnimGraphBlendNode.InputCountName);
        inputCountOption.TryGetValue<int>(out var inputCount);
        inputCount = Mathf.Clamp(inputCount, AnimGraphBlendNode.MinInputs, AnimGraphBlendNode.MaxInputs);

        var inputs = new AnimGraphAssetBlendNode.Input[inputCount];
        for (var i = 0; i < inputCount; i += 1) {
            var inputNode = node.GetInputAnimNode(AnimGraphBlendNode.PoseName(i));
            if (inputNode != null) {
                var inputRuntimeNode = ProcessNode(graph, inputNode);
                inputs[i].nodeID = inputRuntimeNode?.nodeID ?? "";
            } else {
                inputs[i].nodeID = "";
            }

            var weightPort = node.GetInputPortByName(AnimGraphBlendNode.WeightName(i));
            weightPort.TryGetValue<float>(out var weight);

            inputs[i].weight = weight;
            inputs[i].weightVariableName = GetPortVariableName(weightPort);
        }

        return new AnimGraphAssetBlendNode(inputs);
    }

    AnimGraphAssetLayeredBlendNode ProcessLayeredBlendNode(AnimGraphAsset graph, AnimGraphLayeredBlendNode node) {
        var inputCountOption = node.GetNodeOptionByName(AnimGraphLayeredBlendNode.InputCountName);
        inputCountOption.TryGetValue<int>(out var inputCount);
        inputCount = Mathf.Clamp(inputCount, AnimGraphLayeredBlendNode.MinLayers, AnimGraphLayeredBlendNode.MaxLayers);

        AnimGraphAssetNode basePoseRuntimeNode = null;

        var basePoseNode = node.GetInputAnimNode(AnimGraphLayeredBlendNode.BasePoseName);
        if (basePoseNode != null) {
            basePoseRuntimeNode = ProcessNode(graph, basePoseNode);
        }

        var inputs = new AnimGraphAssetLayeredBlendNode.Input[inputCount];
        for (var i = 0; i < inputCount; i += 1) {
            var inputNode = node.GetInputAnimNode(AnimGraphLayeredBlendNode.PoseName(i));
            if (inputNode != null) {
                var inputRuntimeNode = ProcessNode(graph, inputNode);
                inputs[i].nodeID = inputRuntimeNode?.nodeID ?? "";
            } else {
                inputs[i].nodeID = "";
            }

            var weightPort = node.GetInputPortByName(AnimGraphLayeredBlendNode.WeightName(i));
            weightPort.TryGetValue<float>(out var weight);
            inputs[i].weight = weight;
            inputs[i].weightVariableName = GetPortVariableName(weightPort);

            var modePort = node.GetInputPortByName(AnimGraphLayeredBlendNode.ModeName(i));
            modePort.TryGetValue<AnimLayerMode>(out var mode);
            inputs[i].mode = mode;

            var maskPort = node.GetInputPortByName(AnimGraphLayeredBlendNode.MaskName(i));
            maskPort.TryGetValue<AvatarMask>(out var mask);
            inputs[i].mask = mask;
        }

        return new AnimGraphAssetLayeredBlendNode(basePoseRuntimeNode?.nodeID ?? "", inputs);
    }

    void ProcessOutputNode(AnimGraphAsset graph, AnimGraphOutputNode node) {
        var input = node.GetInputAnimNode(AnimGraphOutputNode.InputName);
        if (input != null) {
            var runtimeNode = ProcessNode(graph, input);
            graph.outputNodeID = runtimeNode?.nodeID ?? "";
        }
    }
}
