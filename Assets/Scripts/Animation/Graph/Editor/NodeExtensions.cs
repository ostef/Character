using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

public static class NodeExtensions {
    public static T GetConnectedNode<T>(this IPort port) where T : Node {
        if (port == null || port.Direction != PortDirection.Input || !port.IsConnected) {
            return null;
        }

        var inputNode = port.FirstConnectedPort.GetNode();
        if (inputNode == null) {
            return null;
        }

        if (inputNode is ISubgraphNode subgraphNode) {
            var subgraph = subgraphNode.GetSubgraph();
            foreach (var node in subgraph.GetNodes()) {
                if (node is IVariableNode variableNode) {
                    var variable = variableNode.Variable;
                    if (variable.VariableKind != VariableKind.Output) {
                        continue;
                    }

                    if (variable.Name == port.FirstConnectedPort.Name) {
                        continue;
                    }

                    var variableInputPort = variableNode.GetInputPort(0);

                    return variableInputPort.GetConnectedNode<T>();
                }
            }

            return null;
        }

        if (inputNode is IVariableNode) {
            return null;
        }

        if (inputNode is not T) {
            throw new Exception($"Input port '{port.Name}' is not an animation pose");
        }

        return (T)inputNode;
    }
}
