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
(\Assets\KriptoFX\Realistic Effects Pack v1\HDRP and URP patches)

4) Settings for HDRP rendering:
HDRP rendering has correct settings out of the box and you only need to import the patch from the folder 
(\Assets\KriptoFX\Realistic Effects Pack v1\HDRP and URP patches)

---------------------------------------------------------------------------------------------------------------------------------------------------------------------------


-----------------------------------------      EFFECTS USING    ----------------------------------------------------------------------------------------------------------
All effects work automatically, just like the standard particle system. 
Its play automatically at startup, has a position/rotation/scale, or can use "Instantiate(prefab, position, rotation)"

Using with characters and animations:
Here is a video tutorial on how to use effects with characters  
https://youtu.be/AKQCNGEeAaE

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------




----------------------------------------     additional features    ------------------------------------------------------------------------------------------------------
1) Using projectile collision detection:

	Just add follow script on prefab of effect.

	void Start () {
        var tm = GetComponentInChildren<RFX1_TransformMotion>(true);
	    if (tm!=null) tm.CollisionEnter += Tm_CollisionEnter;
    }

	private void Tm_CollisionEnter(object sender, RFX1_TransformMotion.RFX1_CollisionInfo e)
	{
			Debug.Log(e.Hit.transform.name); //will print collided object name to the console.
	}


2) Using shield interaction:
You need to add script "RFX1_Shield Interaction" on projectiles, which should react on shields.

------------------------------------------------------------------------------------------------------------------------------------------------------------------------



-------------------------------- Effects modification -------------------------------------------------------------------------------------------------------------------

For scaling just change "transform" scale of effect.
All effects includes helpers scripts (collision behaviour, light/shader animation etc) for work out of box.
Also you can add additional scripts for easy change of base effects settings. Just add follow scripts to prefab of effect.

RFX1_EffectSettingColor - for change color of effect (uses HUE color). Can be added on any effect.
RFX1_EffectSettingProjectile - for change projectile fly distance, speed and collided layers.
RFX1_EffectSettingVisible - for change visible status of effect using smooth fading by time.
RFX1_Target - for homing move to target.

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------