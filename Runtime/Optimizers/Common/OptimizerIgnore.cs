//-----------------------------------------------------------------------
// <copyright file="ObjectOptimizerIgnore.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using UnityEngine;

    public enum IgnoreLOD
    {
        LOD0AndAbove = 0,
        LOD1AndAbove = 1,
        LOD2AndAbove = 2,
        LOD3AndAbove = 2,
        LOD4AndAbove = 2,
        LOD5AndAbove = 2,
        LOD6AndAbove = 2,
        LOD7AndAbove = 2,
        LOD8AndAbove = 2,
        LOD9AndAbove = 2,
    }

    public class OptimizerIgnore : MonoBehaviour
    {
        [SerializeField] private IgnoreLOD ignoreLevel;

        public IgnoreLOD IgnoreLOD => this.ignoreLevel;
    }
}
