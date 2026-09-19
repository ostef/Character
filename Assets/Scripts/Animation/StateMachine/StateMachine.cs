using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="New Animation State Machine", menuName="Animation/State Machine", order=1)]
public class AnimStateMachine : ScriptableObject {
    public List<AnimState> states = new();
    public List<AnimStateTransition> transitions = new();
    public string entryStateID;

    public AnimState GetState(string id) {
        return states.Find(state => state.id == id);
    }

    public AnimStateTransition GetTransition(string id) {
        return transitions.Find(transition => transition.id == id);
    }
}

[Serializable]
public class AnimState {
    public string id = Guid.NewGuid().ToString();
    public string name = "New State";
    public AnimGraph graph;
    public Rect nodeRect = new Rect(0, 0, 160, 60);
    public Color nodeColor = new Color(0.22f, 0.22f, 0.22f, 1);
}

[Serializable]
public class AnimStateTransition {
    public string id = Guid.NewGuid().ToString();
    public string name = "";
    public string fromStateID;
    public string toStateID;

    [SerializeReference]
    public ConditionNode conditionExpression = new ConditionBoolValue(true);
}

[Serializable]
public abstract class ConditionNode {
    public enum Kind {
        Identifier,
        FloatValue,
        IntValue,
        BoolValue,
        Add,
        Sub,
        Mul,
        Div,
        Neg,
        Equals,
        NotEquals,
        LessThan,
        LessOrEqual,
        GreaterThan,
        GreaterOrEqual,
    }

    public Kind kind;
}

[Serializable]
public abstract class ConditionUnary : ConditionNode {
    public ConditionNode expr;
}

[Serializable]
public abstract class ConditionBinary : ConditionNode {
    public ConditionNode left;
    public ConditionNode right;
}

[Serializable]
public class ConditionIdentifier : ConditionNode {
    public string identifier;

    public ConditionIdentifier(string identifier) {
        kind = Kind.Identifier;
        this.identifier = identifier;
    }
}

[Serializable]
public class ConditionFloatValue : ConditionNode {
    public float value;

    public ConditionFloatValue(float value) {
        kind = Kind.FloatValue;
        this.value = value;
    }
}

[Serializable]
public class ConditionIntValue : ConditionNode {
    public int value;

    public ConditionIntValue(int value) {
        kind = Kind.IntValue;
        this.value = value;
    }
}

[Serializable]
public class ConditionBoolValue : ConditionNode {
    public bool value;

    public ConditionBoolValue(bool value) {
        kind = Kind.BoolValue;
        this.value = value;
    }
}

[Serializable]
public class ConditionAdd : ConditionBinary {
    public ConditionAdd(ConditionNode left, ConditionNode right) {
        kind = Kind.Add;
        this.left = left;
        this.right = right;
    }
}

[Serializable]
public class ConditionSub : ConditionBinary {
    public ConditionSub(ConditionNode left, ConditionNode right) {
        kind = Kind.Sub;
        this.left = left;
        this.right = right;
    }
}

[Serializable]
public class ConditionMul : ConditionBinary {
    public ConditionMul(ConditionNode left, ConditionNode right) {
        kind = Kind.Mul;
        this.left = left;
        this.right = right;
    }
}

[Serializable]
public class ConditionDiv : ConditionBinary {
    public ConditionDiv(ConditionNode left, ConditionNode right) {
        kind = Kind.Div;
        this.left = left;
        this.right = right;
    }
}

[Serializable]
public class ConditionNeg : ConditionUnary {
    public ConditionNeg(ConditionNode expr) {
        kind = Kind.Neg;
        this.expr = expr;
    }
}

[Serializable]
public class ConditionEquals : ConditionBinary {
    public ConditionEquals(ConditionNode left, ConditionNode right) {
        kind = Kind.Equals;
        this.left = left;
        this.right = right;
    }
}

[Serializable]
public class ConditionNotEquals : ConditionBinary {
    public ConditionNotEquals(ConditionNode left, ConditionNode right) {
        kind = Kind.NotEquals;
        this.left = left;
        this.right = right;
    }
}

[Serializable]
public class ConditionLessThan : ConditionBinary {
    public ConditionLessThan(ConditionNode left, ConditionNode right) {
        kind = Kind.LessThan;
        this.left = left;
        this.right = right;
    }
}

[Serializable]
public class ConditionLessOrEqual : ConditionBinary {
    public ConditionLessOrEqual(ConditionNode left, ConditionNode right) {
        kind = Kind.LessOrEqual;
        this.left = left;
        this.right = right;
    }
}

[Serializable]
public class ConditionGreaterThan : ConditionBinary {
    public ConditionGreaterThan(ConditionNode left, ConditionNode right) {
        kind = Kind.GreaterThan;
        this.left = left;
        this.right = right;
    }
}

[Serializable]
public class ConditionGreaterOrEqual : ConditionBinary {
    public ConditionGreaterOrEqual(ConditionNode left, ConditionNode right) {
        kind = Kind.GreaterOrEqual;
        this.left = left;
        this.right = right;
    }
}
