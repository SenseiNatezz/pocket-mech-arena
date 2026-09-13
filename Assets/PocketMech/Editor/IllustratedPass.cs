using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PocketMech.Editor
{
    public static class IllustratedPass
    {
        const string Art = "Assets/PocketMech/Resources/Illustrated/";
        public static void Build()
        {
            // Normalize the generated atlas into independent alpha sprites. The flood fill
            // removes only border-connected neutral backdrop; ink-enclosed white armor stays.
            var atlas = new Texture2D(2, 2); atlas.LoadImage(File.ReadAllBytes("../Art-References/sprite-atlas.png"));
            Slice(atlas, "Torso", 50, 45, 380, 340);
            Slice(atlas, "Legs", 468, 80, 330, 320);
            Slice(atlas, "Scout", 927, 126, 228, 226);
            Slice(atlas, "Shooter", 90, 471, 260, 248);
            Slice(atlas, "Bomber", 475, 468, 292, 257);
            Slice(atlas, "Shield", 860, 448, 350, 290);
            Slice(atlas, "Rush", 67, 847, 305, 287);
            Slice(atlas, "Boss", 412, 739, 435, 475);
            Slice(atlas, "Impact", 864, 775, 372, 430);
            Object.DestroyImmediate(atlas);
            AssetDatabase.Refresh();
            foreach (var f in Directory.GetFiles(Art, "*.png")) Import(f.Replace('\\', '/'));
            var scene = EditorSceneManager.OpenScene("Assets/PocketMech/Scenes/ArenaA1.unity");
            var old = GameObject.Find("Arena A-1 / Placeholder Environment"); if (old != null) Object.DestroyImmediate(old);
            var root = new GameObject("Arena A-1 / Illustrated Courtyard");
            var arena = new GameObject("Painted environment", typeof(SpriteRenderer)); arena.transform.SetParent(root.transform);
            var render = arena.GetComponent<SpriteRenderer>(); render.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "Arena.png"); render.sortingOrder = -20;
            arena.transform.localScale = new Vector3(13f / render.sprite.bounds.size.x, 23.12f / render.sprite.bounds.size.y, 1);
            var b = AssetDatabase.LoadAssetAtPath<Balance>("Assets/PocketMech/Resources/RunBalance.asset");
            b.arenaHalfSize = new Vector2(5.15f, 9.65f); b.maxEnemies = 45; EditorUtility.SetDirty(b);
            // These authored prefab images replace every old geometric actor.
            var player = PrefabUtility.LoadPrefabContents("Assets/PocketMech/Resources/Prefabs/PlayerMech.prefab");
            var oldChildren = new List<GameObject>(); foreach (Transform t in player.transform) oldChildren.Add(t.gameObject); foreach (var child in oldChildren) Object.DestroyImmediate(child);
            var legs = new GameObject("Legs").transform; legs.SetParent(player.transform, false);
            AddSprite("Leg artwork", legs, "Legs", new Vector2(0, -.54f), 1.24f, 3);
            var turret = new GameObject("Turret").transform; turret.SetParent(player.transform, false);
            AddSprite("Torso artwork", turret, "Torso", new Vector2(0, .16f), 1.8f, 4);
            Save(player, "PlayerMech");
            foreach (EnemyKind kind in System.Enum.GetValues(typeof(EnemyKind)))
            {
                var e = PrefabUtility.LoadPrefabContents("Assets/PocketMech/Resources/Prefabs/" + kind + ".prefab");
                var children = new List<GameObject>(); foreach (Transform t in e.transform) children.Add(t.gameObject); foreach (var t in children) Object.DestroyImmediate(t);
                e.transform.localScale = Vector3.one;
                AddSprite("Illustrated shell", e.transform, kind.ToString(), Vector2.zero, kind == EnemyKind.Boss ? 3.3f : kind == EnemyKind.Shield ? 1.6f : 1.42f, 4);
                Save(e, kind.ToString());
            }
            PlayerSettings.bundleVersion = "0.2.0";
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            ProjectBuilder.BuildWindows();
        }
        static void Save(GameObject g, string name) { PrefabUtility.SaveAsPrefabAsset(g, "Assets/PocketMech/Resources/Prefabs/" + name + ".prefab"); PrefabUtility.UnloadPrefabContents(g); }
        static void AddSprite(string name, Transform parent, string asset, Vector2 position, float width, int order)
        {
            var g = new GameObject(name, typeof(SpriteRenderer)); g.transform.SetParent(parent, false); g.transform.localPosition = position;
            var r = g.GetComponent<SpriteRenderer>(); r.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + asset + ".png"); r.sortingOrder = order;
            g.transform.localScale = Vector3.one * width / r.sprite.bounds.size.x;
        }
        static void Slice(Texture2D atlas, string name, int left, int top, int width, int height)
        {
            float scale = atlas.width / 1254f; int x = Mathf.RoundToInt(left * scale), y = Mathf.RoundToInt(top * scale), w = Mathf.RoundToInt(width * scale), h = Mathf.RoundToInt(height * scale);
            Color[] pixels = atlas.GetPixels(x, atlas.height - y - h, w, h); var visited = new bool[pixels.Length]; var queue = new Queue<int>();
            for (int i = 0; i < w; i++) { queue.Enqueue(i); queue.Enqueue((h - 1) * w + i); }
            for (int i = 0; i < h; i++) { queue.Enqueue(i * w); queue.Enqueue(i * w + w - 1); }
            while (queue.Count > 0)
            {
                int index = queue.Dequeue(); if (visited[index]) continue; visited[index] = true; Color c = pixels[index];
                float low = Mathf.Min(c.r, Mathf.Min(c.g, c.b)), high = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                if (c.a > .01f && (low < .58f || high - low > .085f)) continue;
                pixels[index] = Color.clear; int px = index % w, py = index / w;
                if (px > 0) queue.Enqueue(index - 1); if (px < w - 1) queue.Enqueue(index + 1); if (py > 0) queue.Enqueue(index - w); if (py < h - 1) queue.Enqueue(index + w);
            }
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false); t.SetPixels(pixels); t.Apply(); File.WriteAllBytes(Art + name + ".png", t.EncodeToPNG()); Object.DestroyImmediate(t);
        }
        static void Import(string path)
        {
            var t = (TextureImporter)AssetImporter.GetAtPath(path); t.textureType = TextureImporterType.Sprite; t.spritePixelsPerUnit = 100; t.alphaIsTransparency = true; t.mipmapEnabled = !path.EndsWith("Hero.png") && !path.EndsWith("Arena.png"); t.filterMode = FilterMode.Trilinear; t.textureCompression = TextureImporterCompression.Uncompressed; t.maxTextureSize = 2048; t.SaveAndReimport();
        }
    }
}
