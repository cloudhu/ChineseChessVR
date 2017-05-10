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

public class RFX4_RealtimeReflection : MonoBehaviour
{

 #if UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 
#else
    ReflectionProbe probe;
    private Transform camT;

    void Awake()
    {
        probe = GetComponent<ReflectionProbe>();
        camT = Camera.main.transform;
    }

    void Update()
    {
        var pos = camT.position;
        probe.transform.position = new Vector3(
            pos.x,
            pos.y * -1,
            pos.z
        );
        probe.RenderProbe();
    }
#endif
}
