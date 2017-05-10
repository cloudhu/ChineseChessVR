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

public class RFX4_EffectSettingProjectile : MonoBehaviour
{
    public float FlyDistanceForProjectiles = 30;
    public float SpeedMultiplier = 1;
    public LayerMask CollidesWith = ~0;

    float startSpeed;
    const string particlesAdditionalName = "Distance";

    void Awake()
    {
        var transformMotion = GetComponentInChildren<RFX4_TransformMotion>(true);
        if (transformMotion != null)
        {
            startSpeed = transformMotion.Speed;
        }
    }

    void OnEnable()
    {
        var transformMotion = GetComponentInChildren<RFX4_TransformMotion>(true);
        if (transformMotion != null)
        {
            transformMotion.Distance = FlyDistanceForProjectiles;
            transformMotion.CollidesWith = CollidesWith;
            transformMotion.Speed = startSpeed * SpeedMultiplier;
        }
        var rayCastCollision = GetComponentInChildren<RFX4_RaycastCollision>(true);
        if (rayCastCollision != null) rayCastCollision.RaycastDistance = FlyDistanceForProjectiles;
        var particlesystems = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particlesystems)
        {

            if (ps.name.Contains(particlesAdditionalName))
#if !UNITY_5_5_OR_NEWER
                ps.GetComponent<ParticleSystemRenderer>().lengthScale = FlyDistanceForProjectiles / ps.startSize;
#else
                ps.GetComponent<ParticleSystemRenderer>().lengthScale = FlyDistanceForProjectiles / ps.main.startSize.constantMax;
#endif
        }
    }
}
