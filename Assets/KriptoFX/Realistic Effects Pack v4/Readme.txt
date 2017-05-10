Pack includes prefabs of main effects + additional effects (character effects, collision effects, etc). 

NOTE:

For correct work as in demo scene you need enable "HDR" on main camera and add realistic bloom(or use RFX4_BloomAndDistortion). 
https://www.assetstore.unity3d.com/en/#!/content/51515 link on free unity physically correct bloom.
Use follow settings:
Threshold 2
Radius 7
Intencity 1
High quality true
Anti flicker true

In forward mode, HDR does not work with antialiasing. So you need disable antialiasing (edit->project settings->quality)
or use deffered rendering mode.





Support platforms:

PC/Mobile/Consoles/VR
For mobile platform use myself script for optimized distortions and optimized physically correct bloom.
Just add script "RFX4_DistortionAndBloom.cs" to main camera.
VR distortions and bloom supported. All effects tested on Oculus Rift CV1 with single and dual mode rendering and work perfect. 





Using effects:
Just drag and drop prefab of effect on scene and use that :)
If you want use effects in runtime, use follow code:

"Instantiate(prefabEffect, position, rotation);"

Using projectile collision event:
void Start ()
{
	var tm = GetComponentInChildren<RFX4_TransformMotion>(true);
	if (tm!=null) tm.CollisionEnter += Tm_CollisionEnter;
}

private void Tm_CollisionEnter(object sender, RFX4_TransformMotion.RFX4_CollisionInfo e)
{
        Debug.Log(e.Hit.transform.name); //will print collided object name to the console.
}





Effect modification:
All effects includes helpers scripts (collision behaviour, light/shader animation etc) for work out of box. 
Also you can add additional scripts for easy change of base effects settings.
 
RFX4_EffectSettingColor - for change color of effect (uses HUE color). Can be added on any effect.
RFX4_EffectSettingPhysxForce - for change physx force of effects with rocks levitation (and for effect 6 with black hole force)
RFX4_EffectSettingProjectile - for change projectile fly distance, speed and collided layers. 
RFX4_EffectSettingVisible - for change visible status of effect using smooth fading by time. 


