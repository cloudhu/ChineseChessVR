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

public class RFX4_CameraShake : MonoBehaviour
{
    public AnimationCurve ShakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float Duration = 2;
    public float Speed = 22;
    public float Magnitude = 1;
    public float DistanceForce = 100;
    public float RotationDamper = 2;
    public bool IsEnabled = true;

    bool isPlaying;
    [HideInInspector]
    public bool canUpdate;

    void PlayShake()
    {
        StopAllCoroutines();
        StartCoroutine(Shake());
    }

    void Update()
    {
        if (isPlaying && IsEnabled) {
            isPlaying = false;
            PlayShake();
        }
    }

    void OnEnable()
    {
        isPlaying = true;
        var shakes = FindObjectsOfType(typeof(RFX4_CameraShake)) as RFX4_CameraShake[];
        if(shakes!=null)
        foreach (var shake in shakes)
        {
            shake.canUpdate = false;
        }
        canUpdate = true;
    }

    IEnumerator Shake()
    {
        var elapsed = 0.0f;
        var camT = Camera.main.transform;
        var originalCamRotation = camT.rotation.eulerAngles;
        var direction = (transform.position - camT.position).normalized;
        var time = 0f;
        var randomStart = Random.Range(-1000.0f, 1000.0f);
        var distanceDamper = 1 - Mathf.Clamp01((camT.position - transform.position).magnitude / DistanceForce);
        Vector3 oldRotation = Vector3.zero;
        while (elapsed < Duration && canUpdate) {
            elapsed += Time.deltaTime;
            var percentComplete = elapsed / Duration;
            var damper = ShakeCurve.Evaluate(percentComplete) * distanceDamper;
            time += Time.deltaTime * damper;
            camT.position -= direction * Time.deltaTime * Mathf.Sin(time * Speed) * damper * Magnitude/2;

            var alpha = randomStart + Speed * percentComplete / 10;
            var x = Mathf.PerlinNoise(alpha, 0.0f) * 2.0f - 1.0f;
            var y = Mathf.PerlinNoise(1000 + alpha, alpha + 1000) * 2.0f - 1.0f;
            var z = Mathf.PerlinNoise(0.0f, alpha) * 2.0f - 1.0f;

            if (Quaternion.Euler(originalCamRotation + oldRotation)!=camT.rotation)
                originalCamRotation = camT.rotation.eulerAngles;
            oldRotation = Mathf.Sin(time * Speed) * damper * Magnitude * new Vector3(0.5f + y, 0.3f + x, 0.3f + z) * RotationDamper;
            camT.rotation = Quaternion.Euler(originalCamRotation + oldRotation);

            yield return null;
        }
    }
}
