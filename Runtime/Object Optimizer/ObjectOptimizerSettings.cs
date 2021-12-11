//-----------------------------------------------------------------------
// <copyright file="ObjectOptimizerSettings.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Lost/Performance/Object Optimizer Settings")]
    public class ObjectOptimizerSettings : ScriptableObject
    {
        #pragma warning disable 0649
        [SerializeField] private List<LODSetting> lodSettings;
        #pragma warning restore 0649

        public List<LODSetting> LODSettings => this.lodSettings;

        private void OnValidate()
        {
            if (this.lodSettings == null || this.lodSettings.Count == 0)
            {
                this.lodSettings = new List<LODSetting>
                {
                    new LODSetting
                    {
                        Name = "LOD0",
                        Quality = 1.0f,
                        ScreenPercentage = 0.27f,
                    },
                    new LODSetting
                    {
                        Name = "LOD1",
                        Quality = 0.5f,
                        ScreenPercentage = 0.08f,
                    },
                    new LODSetting
                    {
                        Name = "LOD2",
                        Quality = 0.25f,
                        ScreenPercentage = 0.002f,
                    },
                };
            }
        }

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
                
                #if USING_SIMPLYGON
                Simplygon = 1,
                #endif
                
                #if USING_UNITY_MESH_SIMPLIFIER
                UnityMeshSimplifier = 2,
                #endif                
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
}
