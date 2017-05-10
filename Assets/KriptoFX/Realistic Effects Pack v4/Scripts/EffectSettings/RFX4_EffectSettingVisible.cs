/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using UnityEngine;

public class RFX4_EffectSettingVisible : MonoBehaviour
{
    public bool IsActive = true;
    public float FadeOutTime = 3;

    private bool previousActiveStatus;
    const string rendererAdditionalName = "Loop";

    string[] colorProperties =
    {
        "_TintColor", "_Color", "_EmissionColor", "_BorderColor", "_ReflectColor", "_RimColor",
        "_MainColor", "_CoreColor"
    };

    void Start()
    {

    }

    float alpha;

    void Update()
    {
        if (IsActive) alpha += Time.deltaTime;
        else alpha -= Time.deltaTime;
        alpha = Mathf.Clamp01(alpha);

        if (!IsActive)
        {
            var loopRenderers = GetComponentsInChildren<Renderer>();
            foreach (var loopRenderer in loopRenderers)
            {
                if(loopRenderer.GetComponent<ParticleSystem>()!=null) continue;
                if (!loopRenderer.name.Contains(rendererAdditionalName)) continue;
               
                var mat = loopRenderer.material;
                var shaderColorGradient = loopRenderer.GetComponent<RFX4_ShaderColorGradient>();
                if (shaderColorGradient != null) shaderColorGradient.canUpdate = false;
                
                foreach (var colorProperty in colorProperties)
                {
                    if (mat.HasProperty(colorProperty))
                    {
                        var color = mat.GetColor(colorProperty);
                        color.a = alpha;
                        mat.SetColor(colorProperty, color);
                    }
                }
            }

            var loopProjectors = GetComponentsInChildren<Projector>();
            foreach (var loopProjector in loopProjectors)
            {

                if (!loopProjector.name.Contains(rendererAdditionalName)) continue;
                
                if (!loopProjector.material.name.EndsWith("(Instance)"))
                    loopProjector.material = new Material(loopProjector.material) {name = loopProjector.material.name + " (Instance)"};
                var mat = loopProjector.material;
                
                var shaderColorGradient = loopProjector.GetComponent<RFX4_ShaderColorGradient>();
                if (shaderColorGradient != null) shaderColorGradient.canUpdate = false;

                foreach (var colorProperty in colorProperties)
                {
                    if (mat.HasProperty(colorProperty))
                    {
                        var color = mat.GetColor(colorProperty);
                        color.a = alpha;
                        mat.SetColor(colorProperty, color);
                    }
                }
            }

            var particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in particleSystems)
            {
                if (ps != null) ps.Stop();
            }
            var lights = GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].isActiveAndEnabled)
                {
                    var lightCurves = lights[i].GetComponent<RFX4_LightCurves>();
                    if (lightCurves != null)
                    {
                        lights[i].intensity = alpha*lightCurves.GraphIntensityMultiplier;
                        lightCurves.canUpdate = false;
                    }
                    else
                    {
                        lights[i].intensity = alpha;
                    }
                }
            }
        }

        if (IsActive && !previousActiveStatus)
        {
            var allGO = gameObject.GetComponentsInChildren<Transform>();

            foreach (var go in allGO)
            {
                go.gameObject.SetActive(false);
                go.gameObject.SetActive(true);
            }


        }

        previousActiveStatus = IsActive;
    }

}
