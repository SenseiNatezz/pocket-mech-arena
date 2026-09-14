using UnityEditor;
namespace PocketMech.Editor
{
    public static class RaisedViewBuild
    {
        public static void Build(){PlayerSettings.bundleVersion="0.7.1";AssetDatabase.SaveAssets();ProjectBuilder.BuildWindows();}
    }
}
