//-----------------------------------------------------------------------
// <copyright file="UnityMeshSimplifierSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System;
    using UnityEngine;

    [Serializable]
    public class UnityMeshSimplifierSettings
    {
        [Tooltip("If the border edges should be preserved.")]
        public bool PreserveBorderEdges = false;

        [Tooltip("If the UV seam edges should be preserved.")]
        public bool PreserveUVSeamEdges = false;

        [Tooltip("If the UV foldover edges should be preserved.")]
        public bool PreserveUVFoldoverEdges = false;

        [Tooltip("If the discrete curvature of the mesh surface be taken into account during simplification. Taking surface curvature into account can result in very good quality mesh simplification, but it can slow the simplification process significantly.")]
        public bool PreserveSurfaceCurvature = false;

        [Tooltip("If a feature for smarter vertex linking should be enabled, reducing artifacts at the cost of slower simplification.")]
        public bool EnableSmartLink = true;

        [Tooltip("The maximum distance between two vertices in order to link them.")]
        public double VertexLinkDistance = double.Epsilon;

        [Tooltip("The maximum iteration count. Higher number is more expensive but can bring you closer to your target quality.")]
        public int MaxIterationCount = 100;

        [Tooltip("The agressiveness of the mesh simplification. Higher number equals higher quality, but more expensive to run.")]
        public double Agressiveness = 7.0;

        [Tooltip("If a manual UV component count should be used (set by UV Component Count below), instead of the automatic detection.")]
        public bool ManualUVComponentCount = false;

        [Range(0, 4), Tooltip("The UV component count. The same UV component count will be used on all UV channels.")]
        public int UVComponentCount = 2;
    }
}
