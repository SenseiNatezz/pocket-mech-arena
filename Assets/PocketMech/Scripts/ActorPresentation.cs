using UnityEngine;
using UnityEngine.Rendering;
namespace PocketMech
{
    // Only art faces the oblique camera. Independent turret/leg simulation stays in XY.
    public sealed class ActorPresentation : MonoBehaviour
    {
        Transform[] artwork;float[] authoredAngles;SortingGroup group;Transform shadow;
        void Start()
        {
            group=gameObject.AddComponent<SortingGroup>();var list=new System.Collections.Generic.List<Transform>();
            foreach(var sprite in GetComponentsInChildren<SpriteRenderer>())if(sprite.name=="Illustrated shell"||sprite.name=="Torso artwork"||sprite.name=="Leg artwork")list.Add(sprite.transform);
            artwork=list.ToArray();authoredAngles=new float[artwork.Length];for(int i=0;i<artwork.Length;i++)authoredAngles[i]=artwork[i].localEulerAngles.z;
            CreateShadow();
        }
        void CreateShadow()
        {
            shadow=Visuals.Shape("Ground contact shadow",Game.Instance.World,transform.position,new Vector2(1.1f,.65f),new Color(0,0,0,.24f),-5,true).transform;
            var enemy=GetComponent<Enemy>();if(enemy!=null&&enemy.IsBoss)shadow.localScale*=2.5f;
        }
        void LateUpdate()
        {
            if(group==null)return;if(shadow==null)CreateShadow();group.sortingOrder=200-Mathf.RoundToInt(transform.position.y*5);
            for(int i=0;i<artwork.Length;i++){var art=artwork[i];art.rotation=CameraRig.ViewRotation*Quaternion.Euler(0,0,art.parent.eulerAngles.z+authoredAngles[i]);}
            if(shadow!=null)shadow.position=new Vector3(transform.position.x,transform.position.y,.05f);
        }
        void OnDestroy(){if(shadow!=null)Destroy(shadow.gameObject);}
    }
}
