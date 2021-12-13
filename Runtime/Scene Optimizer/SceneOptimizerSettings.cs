//-----------------------------------------------------------------------
// <copyright file="ObjectOptimizerSettings.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Lost/Performance/Scene Optimizer Settings")]
    public class SceneOptimizerSettings : OptimizerSettings
    {
        #pragma warning disable 0649
        [SerializeField] private int maxTrianglesPerVolume = 150000;
        [SerializeField] private float maxVolumeBoundsSize = 2000;
        [SerializeField] private float minVolumeBoundsSize = 30;
        [SerializeField] private bool generateStreamingLODGroup = true;
        #pragma warning restore 0649

        public int MaxTrianglesPerVolume  => this.maxTrianglesPerVolume;
        public float MaxVolumeBoundsSize => this.maxVolumeBoundsSize;
        public float MinVolumeBoundsSize => this.minVolumeBoundsSize;
        public bool GenerateStreamingLODGroup => this.generateStreamingLODGroup;
    }
}
