//-----------------------------------------------------------------------
// <copyright file="ObjectOptimizerSettings.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Lost/Performance/Object Optimizer Settings")]
    public class ObjectOptimizerSettings : OptimizerSettings
    {
        #pragma warning disable 0649
        [SerializeField] private bool generateLODGroup = true;
        #pragma warning restore 0649

        public bool GenerateLODGroup => this.generateLODGroup;
    }
}
