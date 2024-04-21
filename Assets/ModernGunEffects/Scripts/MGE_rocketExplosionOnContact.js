#pragma strict

var createThis:GameObject;

function Start () {

}

function Update () {

}

function OnCollisionEnter(collision : Collision) {
var explosion:GameObject=Instantiate(createThis, transform.position, transform.rotation);
explosion.transform.parent=null;
Destroy(gameObject.rigidbody);
Destroy(gameObject, 1);


}