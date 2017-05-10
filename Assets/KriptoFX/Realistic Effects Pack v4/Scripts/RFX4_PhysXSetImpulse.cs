/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using UnityEngine;
using System.Collections;

public class RFX4_PhysXSetImpulse : MonoBehaviour
{

    public float Force = 1;
    public ForceMode ForceMode = ForceMode.Force;
  
    private Rigidbody rig;
    private Transform t;
	// Use this for initialization
	void Start () {
        rig = GetComponent<Rigidbody>();
	    t = transform;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if(rig!=null) rig.AddForce(t.forward * Force, ForceMode);
	}

    void OnDisable()
    {
        if (rig!=null)
            rig.velocity = Vector3.zero;
    }
}
