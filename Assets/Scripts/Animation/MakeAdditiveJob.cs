using UnityEngine;
using UnityEngine.Animations;
using Unity.Collections;

public struct MakeAdditiveJob : IAnimationJob {
    public NativeArray<TransformStreamHandle> bones;

    public void ProcessRootMotion(AnimationStream stream) {
        var pose = stream.GetInputStream(1);
        stream.velocity = pose.velocity;
        stream.angularVelocity = pose.angularVelocity;
    }

    public void ProcessAnimation(AnimationStream stream) {
        var basePose = stream.GetInputStream(0);
        var referencePose = stream.GetInputStream(1);

        for (int i = 0; i < bones.Length; i += 1) {
            var bone = bones[i];

            Quaternion baseRotation = bone.GetLocalRotation(basePose);
            Quaternion referenceRotation = bone.GetLocalRotation(referencePose);
            Quaternion deltaRotation = baseRotation * Quaternion.Inverse(referenceRotation);

            Vector3 basePosition = bone.GetLocalPosition(basePose);
            Vector3 referencePosition = bone.GetLocalPosition(referencePose);
            Vector3 deltaPosition = basePosition - referencePosition;

            // @Todo: scale

            bone.SetLocalRotation(stream, deltaRotation);
            bone.SetLocalPosition(stream, deltaPosition);
        }
    }
}