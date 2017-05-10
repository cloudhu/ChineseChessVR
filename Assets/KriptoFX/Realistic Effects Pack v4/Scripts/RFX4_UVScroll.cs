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

public class RFX4_UVScroll : MonoBehaviour {

    public Vector2 UvScrollMultiplier = new Vector2(1.0f, 0.0f);
    public RFX4_TextureShaderProperties TextureName = RFX4_TextureShaderProperties._MainTex;

    Vector2 uvOffset = Vector2.zero;

    private Material mat;

    void Start()
    {
        var currentRenderer = GetComponent<Renderer>();
        if (currentRenderer == null)
        {
            var projector = GetComponent<Projector>();
            if (projector != null)
            {
                if (!projector.material.name.EndsWith("(Instance)"))
                    projector.material = new Material(projector.material) { name = projector.material.name + " (Instance)" };
                mat = projector.material;
            }
        }
        else
            mat = currentRenderer.material;
    }

    void Update()
    {
        uvOffset += (UvScrollMultiplier * Time.deltaTime);
        if (mat!=null)
        {
            mat.SetTextureOffset(TextureName.ToString(), uvOffset);
        }
    }
}
