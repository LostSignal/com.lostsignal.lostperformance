//-----------------------------------------------------------------------
// <copyright file="EditorUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY_EDITOR

namespace Lost
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public static class FolderUtil
    {
        public static void CreateFolder(string path)
        {
            path = path.Replace("\\", "/");
            var directories = path.Split('/');
            Debug.Assert(directories[0] == "Assets");
            string parentDirectory = directories[0];

            for (int i = 1; i < directories.Length; i++)
            {
                string fullDirectory = Path.Combine(parentDirectory, directories[i]).Replace("\\", "/");

                if (AssetDatabase.IsValidFolder(fullDirectory) == false)
                {
                    AssetDatabase.CreateFolder(parentDirectory, directories[i]);
                }

                parentDirectory = fullDirectory;
            }
        }
    }
}

#endif
