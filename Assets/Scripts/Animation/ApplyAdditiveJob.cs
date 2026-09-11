using UnityEngine;
using UnityEngine.Animations;
using Unity.Collections;

public struct ApplyAdditiveJob : IAnimationJob {
    public NativeArray<TransformStreamHandle> bones;

    public void ProcessRootMotion(AnimationStream stream) {
        var pose = stream.GetInputStream(0);
        stream.velocity = pose.velocity;
        stream.angularVelocity = pose.angularVelocity;
    }

    public void ProcessAnimation(AnimationStream stream) {
        var basePose = stream.GetInputStream(0);
        var additivePose = stream.GetInputStream(1);

        for (int i = 0; i < bones.Length; i += 1) {
            var bone = bones[i];

            Quaternion baseRotation = bone.GetLocalRotation(basePose);
            Quaternion additiveRotation = bone.GetLocalRotation(additivePose);
            Quaternion rotation = baseRotation * additiveRotation;

            Vector3 basePosition = bone.GetLocalPosition(basePose);
            Vector3 additivePosition = bone.GetLocalPosition(additivePose);
            Vector3 position = basePosition + additivePosition;

            // @Todo: scale

            bone.SetLocalRotation(stream, rotation);
            bone.SetLocalPosition(stream, position);
        }
    }
}