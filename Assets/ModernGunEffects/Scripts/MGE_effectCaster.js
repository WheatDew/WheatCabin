#pragma strict
var moveThis : GameObject; //this is an invisible "cursor" that is always there where the mouse points
private var hit : RaycastHit;
private var cooldown : float;
private var changeCooldown : float;
private var rndNr:float;

var muzzleEffectPlace:GameObject;
var cartridgeEffectPlace:GameObject;
var muzzleEffect:GameObject[];
var guiMuzzleFireText:GUIText;
private var muzzleEffectSelected:int=0;


var cartridgeEffect:GameObject[];
var guiCartridgeEffectText:GUIText;
private var cartridgeEffectSelected:int=0;

var impactEffect:GameObject[];
var guiImpactEffectText:GUIText;
private var impactEffectSelected:int=0;

function Start () {

guiMuzzleFireText.text=muzzleEffect[muzzleEffectSelected].name;
guiImpactEffectText.text=impactEffect[impactEffectSelected].name;
guiCartridgeEffectText.text=cartridgeEffect[cartridgeEffectSelected].name;


}

function Update () {

if(cooldown>0){cooldown-=Time.deltaTime;}
if(changeCooldown>0){changeCooldown-=Time.deltaTime;}

var ray = Camera.main.ScreenPointToRay (Input.mousePosition);

if (Physics.Raycast (ray, hit)) {  //place the CURSOR 


moveThis.transform.position=hit.point;
moveThis.transform.rotation=hit.collider.gameObject.transform.rotation;

transform.LookAt(moveThis.transform);
}


if(Input.GetMouseButton(0)&&cooldown<=0){   //on mouse click

var effect:GameObject=Instantiate(muzzleEffect[muzzleEffectSelected], muzzleEffectPlace.transform.position, muzzleEffectPlace.transform.rotation);  
effect.transform.parent=muzzleEffectPlace.transform;

Instantiate(impactEffect[impactEffectSelected], moveThis.transform.position, moveThis.transform.rotation);

Instantiate(cartridgeEffect[cartridgeEffectSelected], cartridgeEffectPlace.transform.position, cartridgeEffectPlace.transform.rotation);



cooldown+=0.3;

}


//KEY INPUT - Muzzlefire

if (Input.GetKeyDown("w") && changeCooldown<=0)
{
	muzzleEffectSelected+=1;
		if(muzzleEffectSelected>(muzzleEffect.length-1)) {muzzleEffectSelected=0;}
	
	guiMuzzleFireText.text=muzzleEffect[muzzleEffectSelected].name;
	changeCooldown=0.1;
}

if (Input.GetKeyDown("s") && changeCooldown<=0)
{
	muzzleEffectSelected-=1;
		if(muzzleEffectSelected<0) {muzzleEffectSelected=muzzleEffect.length-1;}
	
	guiMuzzleFireText.text=muzzleEffect[muzzleEffectSelected].name;
	changeCooldown=0.1;
}

//KEY INPUT - Impact

if (Input.GetKeyDown("e") && changeCooldown<=0)
{
	impactEffectSelected+=1;
		if(impactEffectSelected>(impactEffect.length-1)) {impactEffectSelected=0;}
	
	guiImpactEffectText.text=impactEffect[impactEffectSelected].name;
	changeCooldown=0.1;
}

if (Input.GetKeyDown("d") && changeCooldown<=0)
{
	impactEffectSelected-=1;
		if(impactEffectSelected<0) {impactEffectSelected=impactEffect.length-1;}
	
	guiImpactEffectText.text=impactEffect[impactEffectSelected].name;
	changeCooldown=0.1;
}

//KEY INPUT - Cartridge


if (Input.GetKeyDown("r") && changeCooldown<=0)
{
	cartridgeEffectSelected+=1;
		if(cartridgeEffectSelected>(cartridgeEffect.length-1)) {cartridgeEffectSelected=0;}
	
	guiCartridgeEffectText.text=cartridgeEffect[cartridgeEffectSelected].name;
	changeCooldown=0.1;
}

if (Input.GetKeyDown("f") && changeCooldown<=0)
{
	cartridgeEffectSelected-=1;
		if(cartridgeEffectSelected<0) {cartridgeEffectSelected=cartridgeEffect.length-1;}
	
	guiCartridgeEffectText.text=cartridgeEffect[cartridgeEffectSelected].name;
	changeCooldown=0.1;
}


}