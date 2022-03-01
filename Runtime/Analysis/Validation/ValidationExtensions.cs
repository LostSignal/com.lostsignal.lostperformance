//-----------------------------------------------------------------------
// <copyright file="ValidationExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class ValidationExtensions
    {
        public static void ValidateNotNull(this MonoBehaviour monoBehaviour, List<ValidationError> errors, UnityEngine.Object obj, string name)
        {
            if (obj == null)
            {
                errors.Add(new ValidationError
                {
                    AffectedObject = monoBehaviour,
                    Name = $"{monoBehaviour.GetType().Name} field {name} is NULL!",
                });
            }
        }

        public static void ValidateHasValues(this MonoBehaviour monoBehaviour, List<ValidationError> errors, IList collection, string name)
        {
            if (collection.Count == 0)
            {
                errors.Add(new ValidationError
                {
                    AffectedObject = monoBehaviour,
                    Name = $"{monoBehaviour.GetType().Name} field {name} has no values!",
                });
            }
            else
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    if (collection[i] == null)
                    {
                        errors.Add(new ValidationError
                        {
                            AffectedObject = monoBehaviour,
                            Name = $"{monoBehaviour.GetType().Name} field {name} has NULL value at index {i}!",
                        });
                    }
                }
            }
        }
    }
}

#endif
