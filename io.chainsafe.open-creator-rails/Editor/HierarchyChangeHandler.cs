using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails.Editor
{
    [InitializeOnLoad]
    public static class HierarchyChangeHandler
    {
        static HierarchyChangeHandler()
        {
            // Fires before any GameObject/asset is deleted
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private static void OnHierarchyChanged()
        {
            OpenCreatorRailsService ocrService = Object.FindAnyObjectByType<OpenCreatorRailsService>();

            if (ocrService == null)
            {
                return;
            }
            
            var field = typeof(OpenCreatorRailsService).GetField("_assets",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var assets = field?.GetValue(ocrService) as List<Asset>;

            assets?.RemoveAll(asset => asset == null);

            if (assets != null)
            {
                var allAssets = Object.FindObjectsByType<Asset>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

                foreach (var asset in allAssets)
                {
                    if (!assets.Contains(asset) && asset.enabled)
                    {
                        assets.Add(asset);
                    }
                }
            }
        }
    }
}