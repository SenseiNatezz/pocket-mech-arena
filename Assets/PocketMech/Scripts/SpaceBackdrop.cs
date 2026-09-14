using System.Collections.Generic;
using UnityEngine;

namespace PocketMech
{
    // Decorative camera-space scenery sits behind the arena and has no colliders.
    public sealed class SpaceBackdrop : MonoBehaviour
    {
        readonly List<Mesh> meshes = new List<Mesh>();
        readonly List<Transform> rocks = new List<Transform>();
        readonly List<Vector3> origins = new List<Vector3>();
        Transform layer;
        Material rockMaterial;
        Texture2D nebulaTexture, planetTexture;
        Sprite nebulaSprite, planetSprite;
        float elapsed;
        void Start()
        {
            Visuals.Initialize();
            layer = new GameObject("Orbital scenery").transform; layer.SetParent(transform, false); layer.localPosition = new Vector3(0, 0, 65);
            var random = new System.Random(816);
            // Subtle, seamless-looking blue-violet clouds provide depth without competing with bullets.
            nebulaTexture = new Texture2D(128, 256, TextureFormat.RGBA32, false);
            var pixels = new Color[128 * 256];
            for (int y = 0; y < 256; y++) for (int x = 0; x < 128; x++)
            {
                float n = Mathf.PerlinNoise(x * .018f + 13, y * .016f + 7);
                float wisps = Mathf.PerlinNoise(x * .051f + 2, y * .047f + 3);
                var c = Color.Lerp(new Color(.018f, .026f, .065f), new Color(.19f, .11f, .32f), n * n);
                c += new Color(.025f, .06f, .085f, 0) * wisps * n; c.a = 1; pixels[y * 128 + x] = c;
            }
            nebulaTexture.SetPixels(pixels); nebulaTexture.Apply();
            nebulaSprite = Sprite.Create(nebulaTexture, new Rect(0, 0, 128, 256), new Vector2(.5f, .5f), 128);
            var sky = Visuals.Shape("Distant nebula", layer, Vector2.zero, new Vector2(60, 35), Color.white, -120);
            sky.GetComponent<SpriteRenderer>().sprite = nebulaSprite;
            for (int i = 0; i < 340; i++)
            {
                var p = new Vector2((float)random.NextDouble() * 45 - 22.5f, (float)random.NextDouble() * 42 - 21);
                float size = i % 11 == 0 ? .075f : .024f + (float)random.NextDouble() * .025f;
                var color = Color.Lerp(new Color(.3f, .45f, .7f), new Color(.8f, .9f, 1), (float)random.NextDouble());
                Visuals.Shape("Distant star", layer, p, Vector2.one * size, color, -110, true);
                if (i % 11 == 0)
                {
                    Visuals.Shape("Star ray", layer, p, new Vector2(size * 3, size * .35f), color * .65f, -109);
                    Visuals.Shape("Star ray", layer, p, new Vector2(size * .35f, size * 3), color * .65f, -109);
                }
            }
            var planet = Visuals.Shape("Distant blue planet", layer, new Vector2(-5.5f, 7.8f), Vector2.one * 4.6f, new Color(.1f, .27f, .35f), -105, true);
            planetTexture = new Texture2D(192, 192, TextureFormat.RGBA32, false);
            var planetPixels = new Color[192 * 192];
            for (int y = 0; y < 192; y++) for (int x = 0; x < 192; x++)
            {
                float nx = (x - 95.5f) / 95.5f, ny = (y - 95.5f) / 95.5f;
                float radial = nx * nx + ny * ny;
                if (radial > 1) { planetPixels[y * 192 + x] = Color.clear; continue; }
                float z = Mathf.Sqrt(1 - radial);
                float light = Mathf.Clamp01(Vector3.Dot(new Vector3(nx, ny, z), new Vector3(-.65f, .5f, .55f).normalized));
                float land = Mathf.PerlinNoise(nx * 3.5f + 7, ny * 3.5f + 2);
                var c = land > .54f ? new Color(.25f, .44f, .41f) : new Color(.045f, .19f, .3f);
                float cloud = Mathf.PerlinNoise(nx * 6 + ny * 2 + 19, ny * 13 + 31);
                if (cloud > .64f) c = Color.Lerp(c, new Color(.6f, .72f, .72f), (cloud - .64f) * 4);
                c *= .12f + light * .88f; c.a = Mathf.Clamp01((1 - radial) * 100); planetPixels[y * 192 + x] = c;
            }
            planetTexture.SetPixels(planetPixels); planetTexture.Apply();
            planetSprite = Sprite.Create(planetTexture, new Rect(0, 0, 192, 192), new Vector2(.5f, .5f), 192);
            planet.GetComponent<SpriteRenderer>().sprite = planetSprite; planet.GetComponent<SpriteRenderer>().color = Color.white;
            var halo = Visuals.Shape("Planet atmosphere", layer, new Vector2(-5.5f, 7.8f), Vector2.one * 5.2f, new Color(.12f, .45f, .6f, .13f), -106, true);
            halo.GetComponent<SpriteRenderer>().sprite = Visuals.Glow;
            rockMaterial = new Material(Shader.Find("Sprites/Default"));
            for (int i = 0; i < 16; i++)
            {
                float x = (i % 2 == 0 ? -1 : 1) * (5.9f + (i % 4) * .85f);
                float y = -12 + i * 1.65f;
                var position = new Vector3(x, y, -1);
                var rock = Rock(i, .4f + (i % 4) * .18f); rock.localPosition = position;
                rocks.Add(rock); origins.Add(position);
            }
        }
        Transform Rock(int seed, float radius)
        {
            var vertices = new List<Vector3>(); var colors = new List<Color>(); var triangles = new List<int>();
            for (int i = 0; i < 9; i++)
            {
                float a = i * Mathf.PI * 2 / 9, b = (i + 1) * Mathf.PI * 2 / 9;
                float r1 = radius * (1 + .18f * Mathf.Sin(i * 7.1f + seed));
                float r2 = radius * (1 + .18f * Mathf.Sin((i + 1) % 9 * 7.1f + seed));
                var color = Color.Lerp(new Color(.12f, .17f, .25f), new Color(.32f, .4f, .47f), .5f + .45f * Mathf.Sin(a + .6f));
                int n = vertices.Count; vertices.Add(new Vector3(-radius * .17f, radius * .14f, 0));
                vertices.Add(new Vector3(Mathf.Cos(a) * r1, Mathf.Sin(a) * r1, 0));
                vertices.Add(new Vector3(Mathf.Cos(b) * r2, Mathf.Sin(b) * r2, 0));
                colors.Add(color); colors.Add(color); colors.Add(color); triangles.Add(n); triangles.Add(n + 1); triangles.Add(n + 2);
            }
            var mesh = new Mesh { name = "Faceted asteroid" }; mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds(); meshes.Add(mesh);
            var go = new GameObject("Floating asteroid", typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(layer, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh; var renderer = go.GetComponent<MeshRenderer>(); renderer.sharedMaterial = rockMaterial; renderer.sortingOrder = -95;
            Visuals.Shape("Asteroid crater", go.transform, new Vector2(-radius * .1f, radius * .05f), Vector2.one * radius * .45f, new Color(.1f, .14f, .2f), -94, true);
            return go.transform;
        }
        void LateUpdate()
        {
            if (layer == null) return;
            if (Game.Instance != null && Game.Instance.State == RunState.Playing) elapsed += Time.deltaTime;
            layer.localPosition = new Vector3(-transform.position.x * .09f, -(transform.position.y + 32) * .06f, 65);
            for (int i = 0; i < rocks.Count; i++)
            {
                rocks[i].localPosition = origins[i] + new Vector3(Mathf.Sin(elapsed * .13f + i) * .5f, Mathf.Sin(elapsed * .18f + i * 2) * .65f, 0);
                rocks[i].localRotation = Quaternion.Euler(0, 0, i * 31 + elapsed * (i % 2 == 0 ? 3 : -2));
            }
        }
        void OnDestroy()
        {
            foreach (var mesh in meshes) if (mesh != null) Destroy(mesh);
            if (rockMaterial != null) Destroy(rockMaterial);
            if (nebulaSprite != null) Destroy(nebulaSprite);
            if (nebulaTexture != null) Destroy(nebulaTexture);
            if (planetSprite != null) Destroy(planetSprite);
            if (planetTexture != null) Destroy(planetTexture);
        }
    }
}
