using System.Collections.Generic;
using UnityEngine;
namespace PocketMech
{
    // XY remains the combat plane; positive Z is depth below the surface.
    public sealed class RaisedBattlefield : MonoBehaviour
    {
        public bool Frozen { get; private set; }
        public Mesh Surface { get; private set; }
        readonly List<Mesh> meshes = new List<Mesh>();
        Material material;
        public void Rebuild(bool frozen)
        {
            Frozen=frozen;
            foreach(Transform child in transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            foreach(var mesh in meshes)Destroy(mesh);meshes.Clear();
            if(material==null)material=new Material(Shader.Find("Sprites/Default"));
            const int count=32;var upper=new Vector3[count];var lower=new Vector3[count];
            for(int i=0;i<count;i++){
                float angle=i*Mathf.PI*2/count,ripple=1+.022f*Mathf.Sin(i*5.7f)+.012f*Mathf.Cos(i*2.3f);
                float contour=1/Mathf.Pow(Mathf.Abs(Mathf.Cos(angle))+Mathf.Abs(Mathf.Sin(angle)),.65f);
                upper[i]=new Vector3(Mathf.Cos(angle)*9.72f*ripple*contour,Mathf.Sin(angle)*12.42f*ripple*contour,.2f);
                lower[i]=new Vector3(upper[i].x*.96f,upper[i].y*.96f,2.1f+.45f*Mathf.Sin(i*1.9f));
            }
            var floor=frozen?new Color(.30f,.37f,.43f):new Color(.27f,.25f,.24f);
            var rock=frozen?new Color(.19f,.25f,.31f):new Color(.17f,.155f,.15f);
            var vertices=new List<Vector3>();var colors=new List<Color>();var indices=new List<int>();
            for(int i=0;i<count;i++)Triangle(vertices,colors,indices,new Vector3(0,0,.2f),upper[i],upper[(i+1)%count],floor);
            // Top renders after the sides, concealing the far cliff face instead of resembling a bowl.
            Surface=MakeMesh("Open flat combat surface",vertices,colors,indices,-28);
            vertices.Clear();colors.Clear();indices.Clear();
            for(int i=0;i<count;i++){
                int n=(i+1)%count;var tint=rock*(.85f+.25f*Mathf.Sin(i*2.4f));
                Triangle(vertices,colors,indices,upper[i],lower[i],upper[n],tint);
                Triangle(vertices,colors,indices,upper[n],lower[i],lower[n],tint*.93f);
                var inner=Vector3.Lerp(upper[i],new Vector3(0,0,.2f),.045f);var innerNext=Vector3.Lerp(upper[n],new Vector3(0,0,.2f),.045f);
                var rim=frozen?new Color(.55f,.66f,.72f):floor*.88f;
                Triangle(vertices,colors,indices,upper[i],inner,upper[n],rim);
                Triangle(vertices,colors,indices,upper[n],inner,innerNext,rim);
            }
            MakeMesh("Faceted cliff sides and rim",vertices,colors,indices,-29);
        }
        static void Triangle(List<Vector3> v,List<Color> c,List<int> t,Vector3 a,Vector3 b,Vector3 d,Color color)
        {int start=v.Count;v.Add(a);v.Add(b);v.Add(d);color.a=1;c.Add(color);c.Add(color);c.Add(color);t.Add(start);t.Add(start+1);t.Add(start+2);}
        Mesh MakeMesh(string name,List<Vector3> vertices,List<Color> colors,List<int> triangles,int order)
        {
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();meshes.Add(mesh);
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(transform,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=order;return mesh;
        }
        void OnDestroy(){foreach(var mesh in meshes)if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
    }
}
