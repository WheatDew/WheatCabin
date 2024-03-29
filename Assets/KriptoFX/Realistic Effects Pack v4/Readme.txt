version 1.3.0

email is "kripto289@gmail.com" 
Discord link https://discord.gg/GUUZ9D96Uq 


!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!   FIRST STEPS  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

1) All effects are configured only for HDR rendering with bloom postprocessing!

2) Settings for STANDARD (non URP or HDRP) rendering:
http://kripto289.com/Shared/Readme/DefaultRenderingSettings.jpg

3) Settings for URP rendering:
http://kripto289.com/Shared/Readme/URPRenderingSettings.jpg
You should import the URP patch from the folder 
(\Assets\KriptoFX\Realistic Effects Pack v4\HDRP and URP patches)

4) Settings for HDRP rendering:
HDRP rendering has correct settings out of the box and you only need to import the patch from the folder 
(\Assets\KriptoFX\Realistic Effects Pack v4\HDRP and URP patches)

---------------------------------------------------------------------------------------------------------------------------------------------------------------------------


-----------------------------------------      EFFECTS USING    ----------------------------------------------------------------------------------------------------------
All effects work automatically, just like the standard particle system. 
Its play automatically at startup, has a position/rotation/scale, or can use "Instantiate(prefab, position, rotation)"

Using with characters and animations:
Here is a video tutorial on how to use effects with characters  
https://youtu.be/AKQCNGEeAaE

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------




----------------------------------------     additional features    ------------------------------------------------------------------------------------------------------
Using projectile collision detection:

	Just add follow script on prefab of effect.

	void Start () {
        var physicsMotion = GetComponentInChildren<RFX4_PhysicsMotion>(true);
        if (physicsMotion != null) physicsMotion.CollisionEnter += CollisionEnter;

	    var raycastCollision = GetComponentInChildren<RFX4_RaycastCollision>(true);
        if(raycastCollision != null) raycastCollision.CollisionEnter += CollisionEnter;
    }

    private void CollisionEnter(object sender, RFX4_PhysicsMotion.RFX4_CollisionInfo e)
    {
        Debug.Log(e.HitPoint); //a collision coordinates in world space
        Debug.Log(e.HitGameObject.name); //a collided gameobject
        Debug.Log(e.HitCollider.name); //a collided collider :)
    }

-------------------------------- Effects modification -------------------------------------------------------------------------------------------------------------------

All prefabs of effect have "EffectSetting" script with follow settings:

ParticlesBudget (range 0 - 1, default 1)
Allow change particles count of effect prefab. For example, particleBudget = 0.5 will reduce the number of particles in half

UseLightShadows (does not work when used mobile build target)
Some effect can use shadows and you can disable this setting for optimisation. Disabled by default for mobiles.

UseFastFlatDecalsForMobiles
If you use non-flat surfaces or  have z-fight problems you can use screen space decals instead of simple quad decals.
Disabled parameter will use screen space decals but it required depth texture and slower!

UseCustomColor
You can override color of effect by HUE. (new color will used only in play mode)
If you want use black/white colors for effect, you need manualy change materials of effects.

IsVisible
Disable this parameter in runtime will smoothly turn off an effect.

FadeoutTime
Smooth turn off time


Follow physics settings visible only if type of effect is projectile

UseCollisionDetection
You can disable collision detection and an effect will fly through the obstacles.

LimitMaxDistance
Limiting the flight of effect (at the end the effect will just disappear)

Follow settings like in the rigidbody physics
Mass
Speed
AirDrag
UseGravity
