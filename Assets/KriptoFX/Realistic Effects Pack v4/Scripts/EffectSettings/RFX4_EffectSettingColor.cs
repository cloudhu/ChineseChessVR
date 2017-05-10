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

public class RFX4_EffectSettingColor : MonoBehaviour
{
    public Color Color = Color.red;

    private Color previousColor;

    void OnEnable()
    {
        UpdateColor();
    }

    void Update()
    {
        if (previousColor != Color)
        {
            UpdateColor();
        }
    }

    private void UpdateColor()
    {
        var transformMotion = GetComponentInChildren<RFX4_TransformMotion>(true);
        var rayCastCollision = GetComponentInChildren<RFX4_RaycastCollision>(true);
        var hue = RFX4_ColorHelper.ColorToHSV(Color).H;
        RFX4_ColorHelper.ChangeObjectColorByHUE(gameObject, hue);
        if (transformMotion != null) transformMotion.HUE = hue;
        if (rayCastCollision != null) rayCastCollision.HUE = hue;
        previousColor = Color;
    }

}
