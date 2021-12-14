//-----------------------------------------------------------------------
// <copyright file="LODSetting.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System;
    using UnityEngine;

    [Serializable]
    public class LODSetting
    {
        #pragma warning disable 0649
        [SerializeField] private string name;
        [SerializeField] private float quality;
        [SerializeField] private float screenPercentage;
        [SerializeField] private MeshSimplifier simplifier;
            
        #if USING_UNITY_MESH_SIMPLIFIER
        [ShowIf(nameof(simplifier), MeshSimplifier.UnityMeshSimplifier)]
        [SerializeField] private SimplificationOptions unityMeshSimplifierOptions = SimplificationOptions.Default;
        #endif
        #pragma warning restore 0649

        public enum MeshSimplifier
        {
            None = 0,
            Simplygon = 1,
            UnityMeshSimplifier = 2,
        }
            
        #if USING_UNITY_MESH_SIMPLIFIER
        public SimplificationOptions Options
        {
            get => this.unityMeshSimplifierOptions;
            set => this.unityMeshSimplifierOptions = value;
        }
        #endif

        public string Name
        {
            get => this.name;
            set => this.name = value;
        }

        public float Quality
        {
            get => this.quality;
            set => this.quality = value;
        }

        public float ScreenPercentage
        {
            get => this.screenPercentage;
            set => this.screenPercentage = value;
        }

        public MeshSimplifier Simplifier
        {
            get => this.simplifier;
            set => this.simplifier = value;
        }
    }
}
