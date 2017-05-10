/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using UnityEngine;

public class RFX4_TornadoParticles : MonoBehaviour
{

    public Material TornadoMaterial;

    private ParticleSystem.Particle[] particleArray;
    private ParticleSystem particleSys;
    private Light myLight;

    private Vector4 _twistScale;
    private int materialID = -1;
    
	// Use this for initialization
	void Start () {
        particleSys = GetComponent<ParticleSystem>();
	    myLight = GetComponent<Light>();
#if !UNITY_5_5_OR_NEWER
        if (particleSys!=null)
            particleArray = new ParticleSystem.Particle[particleSys.maxParticles];
#else
        if (particleSys != null)
            particleArray = new ParticleSystem.Particle[particleSys.main.maxParticles];
#endif

        if (TornadoMaterial.HasProperty("_TwistScale"))
	        materialID = Shader.PropertyToID("_TwistScale");
	    else {
            Debug.Log(TornadoMaterial.name + " not have property twist");
	    }
        if (materialID != -1)
            _twistScale = TornadoMaterial.GetVector(materialID);
	}
	
	// Update is called once per frame
    private void Update()
    {
        if (particleSys!=null) {
            var numParticlesAlive = particleSys.GetParticles(particleArray);

            for (int i = 0; i < numParticlesAlive; i++) {
                var pos = particleArray[i].position;

                var height = (pos.y - transform.position.y) * _twistScale.y;
                pos.x = Mathf.Sin(Time.time * _twistScale.z + pos.y * _twistScale.x) * height;
                pos.z = Mathf.Sin(Time.time * _twistScale.z + pos.y * _twistScale.x + 3.1415f / 2) * height;
                particleArray[i].position = pos;

                particleSys.SetParticles(particleArray, numParticlesAlive);
            }
        }
        if (myLight!=null) {
            var pos = transform.localPosition;
            var height = pos.y * _twistScale.y;
            pos.x = Mathf.Sin(Time.time * _twistScale.z + pos.y * _twistScale.x) * height;
            pos.z = Mathf.Sin(Time.time * _twistScale.z + pos.y * _twistScale.x + 3.1415f / 2) * height;
            transform.localPosition = pos;
        }
    }

}
