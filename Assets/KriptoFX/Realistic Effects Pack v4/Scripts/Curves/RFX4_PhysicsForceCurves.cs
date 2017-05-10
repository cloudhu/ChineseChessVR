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
using System.Collections.Generic;

public class RFX4_PhysicsForceCurves : MonoBehaviour
{

    public float ForceRadius = 5;
    public float ForceMultiplier = 1;
    public AnimationCurve ForceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public ForceMode ForceMode;
    public float GraphTimeMultiplier = 1, GraphIntensityMultiplier = 1;
    public bool IsLoop;
    public float DestoryDistance = -1;
    public bool UseDistanceScale = false;
    public AnimationCurve DistanceScaleCurve = AnimationCurve.EaseInOut(1, 1, 1, 1);
    public bool UseUPVector = false;
    public AnimationCurve DragCurve = AnimationCurve.EaseInOut(0, 0, 0, 1);
    public float DragGraphTimeMultiplier = -1, DragGraphIntensityMultiplier = -1;
    public string AffectedName;

    [HideInInspector] public float forceAdditionalMultiplier = 1;
    private bool canUpdate;
    private float startTime;
    private Transform t;

    private void Awake()
    {
        t = transform;
    }

    private void OnEnable()
    {
        startTime = Time.time;
        canUpdate = true;
        forceAdditionalMultiplier = 1;
    }

    private void FixedUpdate()
    {
        var time = Time.time - startTime;
        if (canUpdate)
        {
            float eval = ForceCurve.Evaluate(time / GraphTimeMultiplier) * GraphIntensityMultiplier;
            var hitColliders = Physics.OverlapSphere(t.position, ForceRadius);
            foreach (var hitCollider in hitColliders)
            {
                var rig = hitCollider.GetComponent<Rigidbody>();
                if (rig == null) continue;
                if (AffectedName.Length > 0 && !hitCollider.name.Contains(AffectedName)) {
                    
                    continue;
                }
                
                Vector3 distVector;
                float dist;
                if (UseUPVector)
                {
                    distVector = Vector3.up;
                    var pos = hitCollider.transform.position;
                    dist = 1 - Mathf.Clamp01(pos.y - t.position.y);
                    dist *= 1 - ((hitCollider.transform.position - t.position)).magnitude / ForceRadius;
                }
                else {
                    distVector = (hitCollider.transform.position - t.position);
                    dist = 1 - distVector.magnitude / ForceRadius;
                }
                if(UseDistanceScale) hitCollider.transform.localScale = DistanceScaleCurve.Evaluate(dist) * hitCollider.transform.localScale;
              
                if (DestoryDistance > 0 && distVector.magnitude < DestoryDistance)
                {
                    Destroy(hitCollider.gameObject);
                }
                rig.AddForce(distVector.normalized * dist * ForceMultiplier * eval * forceAdditionalMultiplier, ForceMode);
                if (DragGraphTimeMultiplier > 0) {
                    rig.drag = DragCurve.Evaluate(time / DragGraphTimeMultiplier) * DragGraphIntensityMultiplier;
                    rig.angularDrag = rig.drag / 10;
                }
                
            }
        }
        if (time >= GraphTimeMultiplier)
        {
            if (IsLoop) startTime = Time.time;
            else canUpdate = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ForceRadius);
    }
}
