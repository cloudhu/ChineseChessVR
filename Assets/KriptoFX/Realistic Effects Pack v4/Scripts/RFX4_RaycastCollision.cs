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

public class RFX4_RaycastCollision : MonoBehaviour
{
    public float RaycastDistance = 100;
    public GameObject[] Effects;
    public float Offset = 0;
    public float TimeDelay = 0;
    public float DestroyTime = 3;
    public bool UsePivotPosition;
    public bool UseNormalRotation = true;
    public bool IsWorldSpace = true;
    public bool RealTimeUpdateRaycast;
    public bool DestroyAfterDisabling;
    [HideInInspector]
    public float HUE = -1;
    [HideInInspector]
    public List<GameObject> CollidedInstances = new List<GameObject>();

    private bool isInitialized;
    private bool canUpdate;

    void Start()
    {
        isInitialized = true;
        if (TimeDelay < 0.001f) UpdateRaycast();
        else Invoke("LateEnable", TimeDelay);
    }

	// Use this for initialization
	void OnEnable ()
	{
        CollidedInstances.Clear();
	    if (isInitialized) {
	        if (TimeDelay < 0.001f) {
	            UpdateRaycast();

            }
            else Invoke("LateEnable", TimeDelay);
	    }
	}

    void OnDisable()
    {
        if (DestroyAfterDisabling)
        {
            foreach (var instance in CollidedInstances)
            {
                Destroy(instance);
            }
        }
    }

    void Update()
    {
      
        if (canUpdate) {
            UpdateRaycast();
        }
    }

    void LateEnable()
    {
        UpdateRaycast();
    }

   

    private void UpdateRaycast()
    {
       
        RaycastHit raycastHit;
        if (Physics.Raycast(transform.position, transform.forward, out raycastHit, RaycastDistance)) {
            Vector3 position;
            if (UsePivotPosition)
                position = raycastHit.transform.position;
            else
                position = raycastHit.point + raycastHit.normal * Offset;

            if (CollidedInstances.Count==0)
                foreach (var effect in Effects) {
                    var instance = Instantiate(effect, position, new Quaternion()) as GameObject;
                    CollidedInstances.Add(instance);
                    if (HUE > -0.9f)
                    {
                        RFX4_ColorHelper.ChangeObjectColorByHUE(instance, HUE);
                    }
                    if (!IsWorldSpace)
                        instance.transform.parent = transform;
                    if (UseNormalRotation)
                        instance.transform.LookAt(raycastHit.point + raycastHit.normal);
                    if (DestroyTime > 0.0001f)
                        Destroy(instance, DestroyTime);
                }
            else
                foreach (var instance in CollidedInstances) {
                    if (instance == null) continue;
                    instance.transform.position = position;
                    if (UseNormalRotation)
                        instance.transform.LookAt(raycastHit.point + raycastHit.normal);
                }
        }
        if (RealTimeUpdateRaycast)
            canUpdate = true;
    }


    void OnDrawGizmosSelected()
    {
       Gizmos.color = Color.blue;
       Gizmos.DrawLine(transform.position, transform.position + transform.forward * RaycastDistance);
    }
}
