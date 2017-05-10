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

public class RFX4_CollisionPropertyDeactiavtion : MonoBehaviour
{

    public float DeactivateTimeDelay = 1;

    private float startTime;
    private WindZone windZone;
    ParticleSystem ps;
    ParticleSystem.CollisionModule collisionModule;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        collisionModule = ps.collision;
    }

    private void OnEnable()
    {
        startTime = Time.time;
        collisionModule.enabled = true;
    }

    private void Update()
    {
        var time = Time.time - startTime;
       
        if (time >= DeactivateTimeDelay)
        {
            collisionModule.enabled = false;
        }
    }
}
