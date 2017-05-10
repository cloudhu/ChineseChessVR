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

public class RFX4_RotateAround : MonoBehaviour
{
    public Vector3 Offset = Vector3.forward;
    public Vector3 RotateVector = Vector3.forward;
    public float LifeTime = 1;

    private Transform t;
    private float currentTime;
    private Quaternion rotation;

    // Use this for initialization
    private void Start()
    {
        t = transform;
        rotation = t.rotation;
    }

    private void OnEnable()
    {
        currentTime = 0;
        if(t!=null) t.rotation = rotation;
    }

    private void Update()
    {
        if (currentTime >= LifeTime && LifeTime > 0.0001f)
            return;
        currentTime += Time.deltaTime;
        t.Rotate(RotateVector * Time.deltaTime);
    }
}
