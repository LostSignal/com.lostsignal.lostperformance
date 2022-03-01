//-----------------------------------------------------------------------
// <copyright file="ComponentValidator.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY_EDITOR

namespace Lost
{
    using System.Collections.Generic;
    using UnityEngine.SceneManagement;

    public class ComponentValidator : SceneValidator.Validator
    {
        public override string DisplayName => "Component Validator";

        public override void Run(Scene scene, List<ValidationError> errorResults)
        {
            foreach (var rootObject in scene.GetRootGameObjects())
            {
                foreach (var validate in rootObject.GetComponentsInChildren<IValidate>(true))
                {
                    validate.Validate(errorResults);
                }
            }
        }
    }
}

#endif
