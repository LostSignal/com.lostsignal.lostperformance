//-----------------------------------------------------------------------
// <copyright file="OptimizerSettings.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.Collections.Generic;
    using UnityEngine;

    public class OptimizerSettings : ScriptableObject
    {
#pragma warning disable 0649
        [SerializeField] private List<LODSetting> lodSettings;
        [SerializeField] private bool generateLODGroup;
        [SerializeField] private UnityMeshSimplifierSettings unityMeshSimplifierSettings;
        [SerializeField] private SimplygonSettings simplygonSettings;
#pragma warning restore 0649

        public List<LODSetting> LODSettings => this.lodSettings;

        public bool GenerateLODGroup => this.generateLODGroup;

        public UnityMeshSimplifierSettings UnityMeshSimplifierSettings => this.unityMeshSimplifierSettings;

        public SimplygonSettings SimplygonSettings => this.simplygonSettings;

        protected virtual void OnValidate()
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
    }
}
