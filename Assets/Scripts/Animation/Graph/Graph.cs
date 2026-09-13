using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class AnimGraph : ScriptableObject {
    public string outputNodeID;
    [SerializeReference]
    public List<AnimGraphNode> nodes = new();

    public AnimGraphNode GetNode(string id) {
        return nodes.Find(node => node.nodeID == id);
    }
}

[Serializable]
public abstract class AnimGraphNode {
    public enum Kind {
        Clip,
        BlendSpace1D,
        BlendSpace2D,
        Blend,
        LayeredBlend,
    }

    public string nodeID;
    public Kind kind;

    public abstract Playable Build(AnimGraphInstance graph, AnimGraph asset);
}

[Serializable]
public class AnimGraphClipNode : AnimGraphNode {
    public AnimationClip clip;
    public float speed;
    public string speedVariableName;

    public AnimGraphClipNode(AnimationClip clip) {
        nodeID = Guid.NewGuid().ToString();
        kind = Kind.Clip;
        this.clip = clip;
    }

    public override Playable Build(AnimGraphInstance graph, AnimGraph asset) {
        var playable = AnimationClipPlayable.Create(graph.Graph, clip);
        playable.SetSpeed(speed);

        if (speedVariableName != "") {
            graph.BindFloat(speedVariableName, speed => playable.SetSpeed(speed));
        }

        return playable;
    }
}

[Serializable]
public class AnimGraphBlendSpace1DNode : AnimGraphNode {
    public AnimBlendSpace1D blendSpace;
    public float parameter;
    public string parameterVariableName;

    public AnimGraphBlendSpace1DNode(AnimBlendSpace1D blendSpace, float parameter, string parameterVariableName) {
        nodeID = Guid.NewGuid().ToString();
        kind = Kind.BlendSpace1D;
        this.blendSpace = blendSpace;
        this.parameter = parameter;
        this.parameterVariableName = parameterVariableName;
    }

    public override Playable Build(AnimGraphInstance graph, AnimGraph asset) {
        var mixer = AnimBlendSpace1DMixer.Create(graph.Graph, blendSpace);
        mixer.GetBehaviour().SetParameter(parameter);

        if (parameterVariableName != "") {
            graph.BindFloat(parameterVariableName, param => mixer.GetBehaviour().SetParameter(param));
        }

        return mixer;
    }
}

[Serializable]
public class AnimGraphBlendSpace2DNode : AnimGraphNode {
    public AnimBlendSpace2D blendSpace;
    public float parameterX;
    public string parameterXVariableName;
    public float parameterY;
    public string parameterYVariableName;

    public AnimGraphBlendSpace2DNode(AnimBlendSpace2D blendSpace, float parameterX, string parameterXVariableName, float parameterY, string parameterYVariableName) {
        nodeID = Guid.NewGuid().ToString();
        kind = Kind.BlendSpace2D;
        this.blendSpace = blendSpace;
        this.parameterX = parameterX;
        this.parameterXVariableName = parameterXVariableName;
        this.parameterY = parameterY;
        this.parameterYVariableName = parameterYVariableName;
    }

    public override Playable Build(AnimGraphInstance graph, AnimGraph asset) {
        var mixer = AnimBlendSpace2DMixer.Create(graph.Graph, blendSpace);
        mixer.GetBehaviour().SetParameter(parameterX, parameterY);

        if (parameterXVariableName != "") {
            graph.BindFloat(parameterXVariableName, param => mixer.GetBehaviour().SetParameterX(param));
        }

        if (parameterYVariableName != "") {
            graph.BindFloat(parameterYVariableName, param => mixer.GetBehaviour().SetParameterY(param));
        }

        return mixer;
    }
}

[Serializable]
public class AnimGraphBlendNode : AnimGraphNode {
    [Serializable]
    public struct Input {
        public string nodeID;
        public float weight;
        public string weightVariableName;
    }

    public Input[] inputs;

    public AnimGraphBlendNode(Input[] inputs) {
        nodeID = Guid.NewGuid().ToString();
        kind = Kind.Blend;
        this.inputs = inputs;
    }

    public override Playable Build(AnimGraphInstance graph, AnimGraph asset) {
        var mixer = AnimationMixerPlayable.Create(graph.Graph, inputs.Length);
        for (var i = 0; i < inputs.Length; i += 1) {
            var input = inputs[i];
            var inputNode = asset.GetNode(input.nodeID);
            var playable = inputNode.Build(graph, asset);
            mixer.ConnectInput(i, playable, 0, input.weight);

            if (input.weightVariableName != "") {
                graph.BindFloat(input.weightVariableName, weight => mixer.SetInputWeight(i, weight));
            }
        }

        return mixer;
    }
}

public enum AnimLayerMode {
    Override,
    Additive
}

[Serializable]
public class AnimGraphLayeredBlendNode : AnimGraphNode {
    [Serializable]
    public struct Input {
        public string nodeID;
        public float weight;
        public string weightVariableName;
        public AnimLayerMode mode;
        public AvatarMask mask;
    }

    public string baseInputID;
    public Input[] inputs;

    public AnimGraphLayeredBlendNode(string baseInputID, Input[] inputs) {
        nodeID = Guid.NewGuid().ToString();
        kind = Kind.LayeredBlend;
        this.baseInputID = baseInputID;
        this.inputs = inputs;
    }

    public override Playable Build(AnimGraphInstance graph, AnimGraph asset) {
        var mixer = AnimationLayerMixerPlayable.Create(graph.Graph, inputs.Length + 1);
        var baseNode = asset.GetNode(baseInputID);
        var basePlayable = baseNode.Build(graph, asset);
        mixer.ConnectInput(0, basePlayable, 0, 1.0f);

        for (var i = 0; i < inputs.Length; i += 1) {
            var input = inputs[i];
            var inputNode = asset.GetNode(input.nodeID);
            var playable = inputNode.Build(graph, asset);
            mixer.ConnectInput(i + 1, playable, 0, input.weight);

            if (input.weightVariableName != "") {
                graph.BindFloat(input.weightVariableName, weight => mixer.SetInputWeight(i + 1, weight));
            }

            if (input.mode == AnimLayerMode.Additive) {
                mixer.SetLayerAdditive((uint)i + 1, true);
            }
            if (input.mask != null) {
                mixer.SetLayerMaskFromAvatarMask((uint)i + 1, input.mask);
            }
        }

        return mixer;
    }
}
