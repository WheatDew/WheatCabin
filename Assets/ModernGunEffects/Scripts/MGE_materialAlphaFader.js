
var fadeSpeed:float=1;

var beginTintAlpha:float=0.5;
var myColor:Color;

function Start () {


}

function Update () {

beginTintAlpha-=Time.deltaTime*fadeSpeed;

renderer.material.SetColor ("_TintColor", Color(myColor.r, myColor.g, myColor.b ,beginTintAlpha));

}