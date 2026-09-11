using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

public static class AnimatorExtensions {
    public static NativeArray<TransformStreamHandle> CreateBonesTransformStreamHandleArray(this Animator animator) {
        Transform[] bones;

        if (animator.isHuman) {
            List<Transform> boneList = new List<Transform>();

            for (int i = 0; i < (int)HumanBodyBones.LastBone; i += 1) {
                var bone = (HumanBodyBones)i;
                var t = animator.GetBoneTransform(bone);
                if (t != null) {
                    boneList.Add(t);
                }
            }

            bones = boneList.ToArray();
        } else {
            bones = animator.transform.GetComponentsInChildren<Transform>();
        }

        var boneArray = new NativeArray<TransformStreamHandle>(bones.Length, Allocator.Persistent);
        for (int i = 0; i < bones.Length; i++) {
            boneArray[i] = animator.BindStreamTransform(bones[i]);
        }

        return boneArray;
    }
}