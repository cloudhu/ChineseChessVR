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

public class RFX4_ScaleCurves : MonoBehaviour
{
    public AnimationCurve FloatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float GraphTimeMultiplier = 1, GraphIntensityMultiplier = 1;
    public bool IsLoop;

    private bool canUpdate;
    private float startTime;
    private Transform t;
    private int nameId;
    Projector proj;
    private Vector3 startScale;

    private void Awake()
    {
        t = GetComponent<Transform>();
        startScale = t.localScale;
        t.localScale = Vector3.zero;
        proj = GetComponent<Projector>();
    }

    private void OnEnable()
    {
        startTime = Time.time;
        canUpdate = true;
        t.localScale = Vector3.zero;
    }

    private void Update()
    {
        var time = Time.time - startTime;
        if (canUpdate)
        {
            var eval = FloatCurve.Evaluate(time / GraphTimeMultiplier) * GraphIntensityMultiplier;
            t.localScale = eval * startScale;
            if (proj!=null)
                proj.orthographicSize = eval;
        }
        if (time >= GraphTimeMultiplier)
        {
            if (IsLoop) startTime = Time.time;
            else canUpdate = false;
        }
    }
}
