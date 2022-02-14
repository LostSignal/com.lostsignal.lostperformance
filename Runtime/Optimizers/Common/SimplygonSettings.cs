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
    public class SimplygonSettings : IEquatable<SimplygonSettings>
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

        public override bool Equals(object obj)
        {
            return Equals(obj as SimplygonSettings);
        }

        public bool Equals(SimplygonSettings other)
        {
            return other != null &&

                // Reduction
                this.CurvatureImportance == other.CurvatureImportance &&
                this.EdgeSetImportance == other.EdgeSetImportance &&
                this.GeometryImportance == other.GeometryImportance &&
                this.GroupImportance == other.GroupImportance &&
                this.MaterialImportance == other.MaterialImportance &&
                this.ShadingImportance == other.ShadingImportance &&
                this.SkinningImportance == other.SkinningImportance &&
                this.TextureImportance == other.TextureImportance &&
                this.VertexColorImportance == other.VertexColorImportance &&

                // Repair
                this.ProgressivePasses == other.ProgressivePasses &&
                this.UseWelding == other.UseWelding &&
                this.WeldDist == other.WeldDist &&
                this.UseTJunctionRemover == other.UseTJunctionRemover &&
                this.WeldOnlyBetweenSceneNodes == other.WeldOnlyBetweenSceneNodes &&
                this.WeldOnlyBorderVertices == other.WeldOnlyBorderVertices &&
                this.WeldOnlyWithinMaterial == other.WeldOnlyWithinMaterial &&
                this.WeldOnlyWithinSceneNode == other.WeldOnlyWithinSceneNode;
        }

        public override int GetHashCode()
        {
            // Reduction
            int hashCode = this.CurvatureImportance.GetHashCode();
            hashCode = HashCode.Combine(hashCode, this.EdgeSetImportance);
            hashCode = HashCode.Combine(hashCode, this.GeometryImportance);
            hashCode = HashCode.Combine(hashCode, this.GroupImportance);
            hashCode = HashCode.Combine(hashCode, this.MaterialImportance);
            hashCode = HashCode.Combine(hashCode, this.ShadingImportance);
            hashCode = HashCode.Combine(hashCode, this.SkinningImportance);
            hashCode = HashCode.Combine(hashCode, this.TextureImportance);
            hashCode = HashCode.Combine(hashCode, this.VertexColorImportance);

            // Repair
            hashCode = HashCode.Combine(hashCode, this.ProgressivePasses);
            hashCode = HashCode.Combine(hashCode, this.UseWelding);
            hashCode = HashCode.Combine(hashCode, this.WeldDist);
            hashCode = HashCode.Combine(hashCode, this.UseTJunctionRemover);
            hashCode = HashCode.Combine(hashCode, this.WeldOnlyBetweenSceneNodes);
            hashCode = HashCode.Combine(hashCode, this.WeldOnlyBorderVertices);
            hashCode = HashCode.Combine(hashCode, this.WeldOnlyWithinMaterial);
            hashCode = HashCode.Combine(hashCode, this.WeldOnlyWithinSceneNode);

            return hashCode;
        }
    }
}
