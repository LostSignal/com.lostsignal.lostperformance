//-----------------------------------------------------------------------
// <copyright file="MeshColliderNegativeScaleValidator.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY_EDITOR

namespace Lost
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class MeshColliderNegativeScaleValidator : SceneValidator.Validator
    {
        public override string DisplayName => "MeshCollider Negative Scale Validator";

        public override void Run(Scene scene, List<ValidationError> errorResults)
        {
            foreach (var rootObject in scene.GetRootGameObjects())
            { 
                foreach (var meshCollider in rootObject.GetComponentsInChildren<MeshCollider>(true))
                {
                    if (this.DoesTransformHaveNegativeScale(meshCollider.transform))
                    {
                        string fullPath = meshCollider.transform.GetFullPathWithSceneName();

                        errorResults.Add(new ValidationError
                        {
                            AffectedObject = meshCollider,
                            Name = $"Negative MeshCollider Scaling: {fullPath}",
                            Description = $"MeshCollider {fullPath} has negative scaling which means it's MeshCollider will be recalcuated at runtime and not precalculated during build time.",
                        });
                    }
                }
            }
        }

        private bool DoesTransformHaveNegativeScale(Transform transform)
        {
            if (transform == null)
            {
                return false;
            }
            else if (transform.localScale.x < 0 || transform.localScale.y < 0 || transform.localScale.z < 0)
            {
                return true;
            }
            else
            {
                return this.DoesTransformHaveNegativeScale(transform.parent);
            }
        }
    }
}

#endif
