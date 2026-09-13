using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PocketMech.Editor
{
    public static class FrostlinePass
    {
        const string Art = "Assets/PocketMech/Resources/Illustrated/";
        public static void Build()
        {
            AssetDatabase.Refresh();
            var arena = (TextureImporter)AssetImporter.GetAtPath(Art + "FrostArena.png");
            Setup(arena); arena.spriteImportMode = SpriteImportMode.Single; arena.SaveAndReimport();
            var atlas = (TextureImporter)AssetImporter.GetAtPath(Art + "FrostMachines.png");
            Setup(atlas); atlas.spriteImportMode = SpriteImportMode.Multiple;
            atlas.GetSourceTextureWidthAndHeight(out int width, out int height);
            float sx = width / 1254f, sy = height / 1254f;
            string[] names = { "IceSkimmer", "RailSentinel", "CryoMortar", "GlacierColossus" };
            // Authored atlas regions; importing sub-sprites preserves the original generated alpha.
            Rect[] regions = { new Rect(120, 75, 390, 375), new Rect(735, 70, 390, 440), new Rect(45, 640, 510, 510), new Rect(612, 540, 642, 640) };
            var metadata = new SpriteMetaData[4];
            for (int i = 0; i < 4; i++) { var r = regions[i]; metadata[i] = new SpriteMetaData { name = names[i], alignment = 0, pivot = new Vector2(.5f,.5f), rect = new Rect(r.x * sx, (1254 - r.y - r.height) * sy, r.width * sx, r.height * sy) }; }
#pragma warning disable 618
            atlas.spritesheet = metadata;
#pragma warning restore 618
            atlas.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(Art + "FrostMachines.png").OfType<Sprite>().ToArray();
            for (int i = 0; i < names.Length; i++) {
                var root = new GameObject(names[i], typeof(Enemy));
                var shell = new GameObject("Illustrated shell", typeof(SpriteRenderer)); shell.transform.SetParent(root.transform, false);
                var renderer = shell.GetComponent<SpriteRenderer>(); renderer.sprite = sprites.Single(s => s.name == names[i]); renderer.sortingOrder = 4;
                shell.transform.localScale = Vector3.one * (i == 3 ? 3.3f : i == 0 ? 1.25f : 1.65f) / renderer.sprite.bounds.size.x;
                PrefabUtility.SaveAsPrefabAsset(root, "Assets/PocketMech/Resources/Prefabs/" + names[i] + ".prefab"); Object.DestroyImmediate(root);
            }
            var scene = EditorSceneManager.OpenScene("Assets/PocketMech/Scenes/ArenaA1.unity");
            var environments = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Where(r => r.name == "Painted environment").ToArray();
            for (int i = 1; i < environments.Length; i++) Object.DestroyImmediate(environments[i].gameObject);
            EditorSceneManager.SaveScene(scene);
            PlayerSettings.bundleVersion = "0.5.0"; AssetDatabase.SaveAssets(); ProjectBuilder.BuildWindows();
        }
        static void Setup(TextureImporter t) { t.textureType = TextureImporterType.Sprite; t.npotScale = TextureImporterNPOTScale.None; t.spritePixelsPerUnit = 100; t.alphaIsTransparency = true; t.mipmapEnabled = false; t.filterMode = FilterMode.Bilinear; t.textureCompression = TextureImporterCompression.Uncompressed; t.maxTextureSize = 2048; var settings = new TextureImporterSettings(); t.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect; t.SetTextureSettings(settings); }
    }
}
