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

public class RFX4_WindCurves : MonoBehaviour
{
    public AnimationCurve WindCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float GraphTimeMultiplier = 1, GraphIntensityMultiplier = 1;
    public bool IsLoop;

    private bool canUpdate;
    private float startTime;
    private WindZone windZone;

    private void Awake()
    {
        windZone = GetComponent<WindZone>();
        windZone.windMain = WindCurve.Evaluate(0);
    }

    private void OnEnable()
    {
        startTime = Time.time;
        canUpdate = true;
    }

    private void Update()
    {
        var time = Time.time - startTime;
        if (canUpdate) {
            var eval = WindCurve.Evaluate(time / GraphTimeMultiplier) * GraphIntensityMultiplier;
            windZone.windMain = eval;
        }
        if (time >= GraphTimeMultiplier) {
            if (IsLoop) startTime = Time.time;
            else canUpdate = false;
        }
    }
}
