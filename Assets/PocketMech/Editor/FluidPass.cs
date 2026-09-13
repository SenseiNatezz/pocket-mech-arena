using UnityEditor;
using UnityEngine;

namespace PocketMech.Editor
{
    public static class FluidPass
    {
        public static void Build()
        {
            var balance = AssetDatabase.LoadAssetAtPath<Balance>("Assets/PocketMech/Resources/RunBalance.asset");
            balance.damage = 60; balance.shotInterval = .3f; balance.dashDuration = .24f;
            EditorUtility.SetDirty(balance); PlayerSettings.bundleVersion = "0.3.0"; AssetDatabase.SaveAssets(); ProjectBuilder.BuildWindows();
        }
    }
}
