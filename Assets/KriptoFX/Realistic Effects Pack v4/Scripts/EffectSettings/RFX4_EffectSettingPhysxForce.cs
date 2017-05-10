/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using System;
using UnityEngine;
using System.Collections;

public class RFX4_EffectSettingPhysxForce : MonoBehaviour
{

    public float ForceMultiplier = 1;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //if (Math.Abs(previousForceMultiplier - ForceMultiplier) > 0.001f)
        {
          var transformMotion = GetComponentInChildren<RFX4_TransformMotion>(true);
            if (transformMotion != null)
            {
                var instances = transformMotion.CollidedInstances;
                foreach (var instance in instances)
                {
                    var physxForceCurve = instance.GetComponent<RFX4_PhysicsForceCurves>();
                    if (physxForceCurve != null) physxForceCurve.forceAdditionalMultiplier = ForceMultiplier;
                }
            }
            var physxForceCurves = GetComponentsInChildren<RFX4_PhysicsForceCurves>();
            foreach (var physxForceCurve in physxForceCurves)
            {
                if (physxForceCurve != null) physxForceCurve.forceAdditionalMultiplier = ForceMultiplier;
            }
        }
    }
}
