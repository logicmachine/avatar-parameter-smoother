using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

using dev.logilabo.parameter_smoother.runtime;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEditor.Animations;

// ReSharper disable once CheckNamespace
namespace dev.logilabo.parameter_smoother.editor
{
    public class ParameterSmootherPass : Pass<ParameterSmootherPass>
    {
        private const string OneParameter = "ParameterSmoother/One";
        private const string LocalSmoothnessSuffix = "/LocalSmoothness";
        private const string RemoteSmoothnessSuffix = "/RemoteSmoothness";
        private const float ParameterRange = 1024.0f;

        private static Motion CreateBlendTree(ParameterSmoother smoother, bool isLocal)
        {
            var children = new List<BlendTree>();
            foreach (var config in smoother.configs)
            {
                var source = config.parameterName;
                var smoothed = source + smoother.smoothedSuffix;
                var smoothness = config.parameterName + (isLocal ? LocalSmoothnessSuffix : RemoteSmoothnessSuffix);
                var minCurve = new AnimationCurve(new Keyframe(0.0f, -ParameterRange));
                var maxCurve = new AnimationCurve(new Keyframe(0.0f, ParameterRange));
                var minClip = new AnimationClip();
                var maxClip = new AnimationClip();
                minClip.SetCurve("", typeof(Animator), smoothed, minCurve);
                maxClip.SetCurve("", typeof(Animator), smoothed, maxCurve);
                var oldTree = new BlendTree
                {
                    blendParameter = smoothed,
                    blendType = BlendTreeType.Simple1D,
                    children = new[]
                    {
                        new ChildMotion { motion = minClip, threshold = -ParameterRange },
                        new ChildMotion { motion = maxClip, threshold = ParameterRange }
                    },
                    minThreshold = -ParameterRange,
                    maxThreshold = ParameterRange,
                    name = source + " (old)"
                };
                var newTree = new BlendTree
                {
                    blendParameter = source,
                    blendType = BlendTreeType.Simple1D,
                    children = new[]
                    {
                        new ChildMotion { motion = minClip, threshold = -ParameterRange },
                        new ChildMotion { motion = maxClip, threshold = ParameterRange }
                    },
                    minThreshold = -ParameterRange,
                    maxThreshold = ParameterRange,
                    name = source + " (new)"
                };
                children.Add(new BlendTree
                {
                    blendParameter = smoothness,
                    blendType = BlendTreeType.Simple1D,
                    children = new[]
                    {
                        new ChildMotion { motion = newTree, threshold = 0.0f },
                        new ChildMotion { motion = oldTree, threshold = 1.0f }
                    },
                    minThreshold = 0.0f,
                    maxThreshold = 1.0f,
                    name = source
                });
            }
            return new BlendTree()
            {
                blendParameter = OneParameter,
                blendType = BlendTreeType.Direct,
                children = children
                    .Select(t => new ChildMotion { directBlendParameter = OneParameter, motion = t })
                    .ToArray(),
                name = "Root"
            };
        }

        private static AnimatorController CreateSmoothingAnimator(ParameterSmoother smoother)
        {
            var controller = new AnimatorController();

            // Register parameters
            controller.AddParameter(new AnimatorControllerParameter
            {
                name=OneParameter,
                type=AnimatorControllerParameterType.Float,
                defaultFloat = 1.0f
            });
            controller.AddParameter(new AnimatorControllerParameter
            {
                name="IsLocal",
                type=AnimatorControllerParameterType.Bool,
                defaultBool = false
            });
            foreach (var config in smoother.configs)
            {
                var source = config.parameterName;
                controller.AddParameter(new AnimatorControllerParameter
                {
                    name = source,
                    type = AnimatorControllerParameterType.Float,
                    defaultFloat = 0.0f
                });
                controller.AddParameter(new AnimatorControllerParameter
                {
                    name = source + smoother.smoothedSuffix,
                    type = AnimatorControllerParameterType.Float,
                    defaultFloat = 0.0f
                });
                controller.AddParameter(new AnimatorControllerParameter
                {
                    name = source + LocalSmoothnessSuffix,
                    type = AnimatorControllerParameterType.Float,
                    defaultFloat = config.localSmoothness
                });
                controller.AddParameter(new AnimatorControllerParameter
                {
                    name = source + RemoteSmoothnessSuffix,
                    type = AnimatorControllerParameterType.Float,
                    defaultFloat = config.remoteSmoothness
                });
            }

            // Create layer
            var layer = new AnimatorControllerLayer
            {
                defaultWeight = 1.0f,
                name = "ParameterSmoother",
                stateMachine = new AnimatorStateMachine()
            };
            var remoteState = layer.stateMachine.AddState("Remote (WD On)");
            var localState = layer.stateMachine.AddState("Local (WD On)");
            remoteState.motion = CreateBlendTree(smoother, false);
            localState.motion = CreateBlendTree(smoother, true);
            remoteState.AddTransition(new AnimatorStateTransition
            {
                conditions = new[]
                {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.If,
                        parameter = "IsLocal"
                    }
                },
                destinationState = localState
            });
            controller.AddLayer(layer);
            return controller;
        }

        private static void ExecuteSingle(ParameterSmoother smoother)
        {
            // TODO Use MA merge blend trees
            var obj = smoother.gameObject;
            var mergeAnimator = obj.AddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.animator = CreateSmoothingAnimator(smoother);
            mergeAnimator.layerType = smoother.layerType;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
        }

        protected override void Execute(BuildContext context)
        {
            var components = context.AvatarRootObject.GetComponentsInChildren<ParameterSmoother>();
            foreach (var smoother in components)
            {
                ExecuteSingle(smoother);
                Object.DestroyImmediate(smoother);
            }
        }
    }
}
