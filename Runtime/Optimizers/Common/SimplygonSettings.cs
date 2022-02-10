//-----------------------------------------------------------------------
// <copyright file="SimplygonSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System;
    using UnityEngine;

    [Serializable]
    public class SimplygonSettings
    {
        [Header("Reduction Settings")]
        public float CurvatureImportance = 0.0f;
        public float EdgeSetImportance = 1.0f;
        public float GeometryImportance = 1.0f;
        public float GroupImportance = 1.0f;
        public float MaterialImportance = 1.0f;
        public float ShadingImportance = 1.0f;
        public float SkinningImportance = 1.0f;
        public float TextureImportance = 1.0f;
        public float VertexColorImportance = 1.0f;

        [Header("Repair Settings")]
        public uint ProgressivePasses = 3;
        public bool UseWelding = true;
        public float WeldDist = 0.0f;
        public bool UseTJunctionRemover = true;
        public float TJuncDist = 0.0f;
        public bool WeldOnlyBetweenSceneNodes = false;
        public bool WeldOnlyBorderVertices = false;
        public bool WeldOnlyWithinMaterial = false;
        public bool WeldOnlyWithinSceneNode = false;
    }
}
