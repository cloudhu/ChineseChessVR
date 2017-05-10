/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof (Camera))]
[AddComponentMenu("KriptoFX/RFX4_BloomAndDistortion")]
#if UNITY_5_4_OR_NEWER
    [ImageEffectAllowedInSceneView]
#endif
public class RFX4_DistortionAndBloom : MonoBehaviour
{
    [Range(0.05f, 1)][Tooltip("Camera render texture resolution")]
    public float RenderTextureResolutoinFactor = 0.25f;

    public bool UseBloom = true;

    [Range(0.1f, 3)][Tooltip("Filters out pixels under this level of brightness.")]
    public float Threshold = 2f;

    [SerializeField, Range(0, 1)] [Tooltip("Makes transition between under/over-threshold gradual.")]
    public float SoftKnee = 0f;

    [Range(1, 7)] [Tooltip("Changes extent of veiling effects in A screen resolution-independent fashion.")]
    public float Radius = 7;
    
    [Tooltip("Blend factor of the result image.")]
    public float Intensity = 1;

    [Tooltip("Controls filter quality and buffer resolution.")]
    public bool HighQuality;


    [Tooltip("Reduces flashing noise with an additional filter.")]
    public bool AntiFlicker;

    const string shaderName = "Hidden/KriptoFX/PostEffects/RFX4_Bloom";
    const string shaderAdditiveName = "Hidden/KriptoFX/PostEffects/RFX4_BloomAdditive";

    private RenderTexture source;
    private RenderTexture destination;
    private int previuosFrameWidth, previuosFrameHeight;
    private float previousScale;
    private Camera _cameraInstance;

    private Material m_Material;

    public Material mat
    {
        get
        {
            if (m_Material == null)
                m_Material = CheckShaderAndCreateMaterial(Shader.Find(shaderName));

            return m_Material;
        }
    }

    private Material m_MaterialAdditive;

    public Material matAdditive
    {
        get
        {
            if (m_MaterialAdditive == null)
            {
                m_MaterialAdditive = CheckShaderAndCreateMaterial(Shader.Find(shaderAdditiveName));
                m_MaterialAdditive.renderQueue = 3900;
            }

            return m_MaterialAdditive;
        }
    }

    public static Material CheckShaderAndCreateMaterial(Shader s)
    {
        if (s == null || !s.isSupported)
            return null;

        var material = new Material(s);
        material.hideFlags = HideFlags.DontSave;
        return material;
    }

    #region Private Members

    private const int kMaxIterations = 16;
    private readonly RenderTexture[] m_blurBuffer1 = new RenderTexture[kMaxIterations];
    private readonly RenderTexture[] m_blurBuffer2 = new RenderTexture[kMaxIterations];

    private void OnDisable()
    {
        if (m_Material != null)
            DestroyImmediate(m_Material);
        m_Material = null;

        if (m_MaterialAdditive != null)
            DestroyImmediate(m_MaterialAdditive);
        m_MaterialAdditive = null;

        if (_cameraInstance != null) _cameraInstance.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_cameraInstance != null) DestroyImmediate(_cameraInstance.gameObject);
    }

    private void OnGUI()
    {
        if (Event.current.type.Equals(EventType.Repaint))
        {
            if (UseBloom) Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), destination, matAdditive);
        }
    }
    
    void Start()
    {
        InitializeRenderTarget();
        //InitializeCameraCopy(); 
    }

    private void LateUpdate()
    {
        if (previuosFrameWidth != Screen.width || previuosFrameHeight != Screen.height || Mathf.Abs(previousScale - RenderTextureResolutoinFactor) > 0.01f)
        {
            InitializeRenderTarget();
            previuosFrameWidth = Screen.width;
            previuosFrameHeight = Screen.height;
            previousScale = RenderTextureResolutoinFactor;
        }
        //InitializeCameraCopy();
        Shader.EnableKeyword("DISTORT_OFF");
        UpdateCameraCopy();
        if (UseBloom) UpdateBloom();
        Shader.SetGlobalTexture("_GrabTextureMobile", source);
        Shader.SetGlobalFloat("_GrabTextureMobileScale", RenderTextureResolutoinFactor);
        Shader.DisableKeyword("DISTORT_OFF");
    }

    private void InitializeRenderTarget()
    {
        var width = (int) (Screen.width*RenderTextureResolutoinFactor);
        var height = (int) (Screen.height*RenderTextureResolutoinFactor);
        source = new RenderTexture(width, height, 16, RenderTextureFormat.DefaultHDR);
        if(UseBloom) destination = new RenderTexture(
            RenderTextureResolutoinFactor > 0.99 ? width : width / 2, 
            RenderTextureResolutoinFactor > 0.99 ? height : height / 2, 0, RenderTextureFormat.ARGB32);
    }

    private void UpdateBloom()
    {
        var useRGBM = Application.isMobilePlatform;

        // source texture size
        var tw = source.width;
        var th = source.height;

        // halve the texture size for the low quality mode
        if (!HighQuality)
        {
            tw /= 2;
            th /= 2;
        }

        // blur buffer format
        var rtFormat = useRGBM ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;

        // determine the iteration count
        var logh = Mathf.Log(th, 2) + Radius - 8;
        var logh_i = (int) logh;
        var iterations = Mathf.Clamp(logh_i, 1, kMaxIterations);

        // update the shader properties
        var threshold = Mathf.GammaToLinearSpace(Threshold);

        mat.SetFloat("_Threshold", threshold);

        var knee = threshold * SoftKnee + 1e-5f;
        var curve = new Vector3(threshold - knee, knee*2, 0.25f/knee);
        mat.SetVector("_Curve", curve);

        var pfo = !HighQuality && AntiFlicker;
        mat.SetFloat("_PrefilterOffs", pfo ? -0.5f : 0.0f);

        mat.SetFloat("_SampleScale", 0.5f + logh - logh_i);
        mat.SetFloat("_Intensity", Mathf.Max(0.0f, Intensity));

        var prefiltered = RenderTexture.GetTemporary(tw, th, 0, rtFormat);

        Graphics.Blit(source, prefiltered, mat, AntiFlicker ? 1 : 0);

        // construct A mip pyramid
        var last = prefiltered;
        for (var level = 0; level < iterations; level++)
        {
            m_blurBuffer1[level] = RenderTexture.GetTemporary(last.width/2, last.height/2, 0, rtFormat);
            Graphics.Blit(last, m_blurBuffer1[level], mat, level == 0 ? (AntiFlicker ? 3 : 2) : 4);
            last = m_blurBuffer1[level];
        }

        // upsample and combine loop
        for (var level = iterations - 2; level >= 0; level--)
        {
            var basetex = m_blurBuffer1[level];
            mat.SetTexture("_BaseTex", basetex);
            m_blurBuffer2[level] = RenderTexture.GetTemporary(basetex.width, basetex.height, 0, rtFormat);
            Graphics.Blit(last, m_blurBuffer2[level], mat, HighQuality ? 6 : 5);
            last = m_blurBuffer2[level];
        }

        destination.DiscardContents();
        Graphics.Blit(last, destination, mat, HighQuality ? 8 : 7);


        for (var i = 0; i < kMaxIterations; i++)
        {
            if (m_blurBuffer1[i] != null) RenderTexture.ReleaseTemporary(m_blurBuffer1[i]);
            if (m_blurBuffer2[i] != null) RenderTexture.ReleaseTemporary(m_blurBuffer2[i]);
            m_blurBuffer1[i] = null;
            m_blurBuffer2[i] = null;
        }

        RenderTexture.ReleaseTemporary(prefiltered);
    }

    void InitializeCameraCopy()
    {
        if(_cameraInstance!=null) _cameraInstance.gameObject.SetActive(true);
        var findedCam = GameObject.Find("RenderTextureCamera");
        if (findedCam == null)
        {
            var renderTextureCamera = new GameObject("RenderTextureCamera");

            renderTextureCamera.transform.parent = Camera.main.transform;
            _cameraInstance = renderTextureCamera.AddComponent<Camera>();
            _cameraInstance.CopyFrom(Camera.main);
            _cameraInstance.clearFlags = Camera.main.clearFlags;
            _cameraInstance.depth--;
#if !UNITY_5_6_OR_NEWER
            _cameraInstance.hdr = true;
#else
            _cameraInstance.allowHDR = true;
#endif
            _cameraInstance.targetTexture = source;
            Shader.SetGlobalTexture("_GrabTextureMobile", source);
            Shader.SetGlobalFloat("_GrabTextureMobileScale", RenderTextureResolutoinFactor);
            _cameraInstance.Render();
            //_cameraInstance.enabled = false;
        }
        else _cameraInstance = findedCam.GetComponent<Camera>();
    }

    void UpdateCameraCopy()
    {
        var cam = Camera.current;

        if (cam != null)
        {
            //_cameraInstance.CopyFrom(cam);
            //_cameraInstance.clearFlags = cam.clearFlags;
            //_cameraInstance.depth--;
            //_cameraInstance.hdr = true;
            //_cameraInstance.targetTexture = source;
            //Shader.SetGlobalTexture("_GrabTextureMobile", source);
            //_cameraInstance.Render();
            //source.DiscardContents();
            if (cam.name == "SceneCamera")
            {
                source.DiscardContents();
                cam.targetTexture = source;
                cam.Render();
                cam.targetTexture = null;
                return;
            }
        }
        cam = Camera.main;
#if !UNITY_5_6_OR_NEWER
        var hdr = cam.hdr;
        source.DiscardContents();
        cam.hdr = true;
        cam.targetTexture = source;
        cam.Render();
        cam.hdr = hdr;
        cam.targetTexture = null;
#else
        var hdr = cam.allowHDR;
        source.DiscardContents();
        cam.allowHDR = true;
        cam.targetTexture = source;
        cam.Render();
        cam.allowHDR = hdr;
        cam.targetTexture = null;
#endif

    }
#endregion
}
