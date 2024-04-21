var createThis:GameObject[];  // list of possible prefabs

private var rndNr:float; // this is for just a random number holder when we need it

var thisManyTimes:int=3;
var overThisTime:float=1.0;

var xWidth:float;  // define the square where prefabs will be generated
var yWidth:float;
var zWidth:float;

var xRotMax:float;  // define maximum rotation of each prefab
var yRotMax:float=180;
var zRotMax:float;

var detachToWorld:boolean=false;


private var xCur:float;  // these are used in the random placement process
private var yCur:float;
private var zCur:float;


private var xRotCur:float;  // these are used in the random rotation process
private var yRotCur:float;
private var zRotCur:float;

private var timeCounter:float;  // counts the time :p
private var effectCounter:int;  // you will guess it

private var trigger:float;  // trigger: at which interwals should we generate a particle



function Start () {
if (thisManyTimes<1) thisManyTimes=1; //hack to avoid division with zero and negative numbers
trigger=overThisTime/thisManyTimes;  //define the intervals of time of the prefab generation.

}


function Update () {

timeCounter+=Time.deltaTime;

	if(timeCounter>trigger&&effectCounter<=thisManyTimes)
		{
		rndNr=Mathf.Floor(Random.value*createThis.length);  //decide which prefab to create


		xCur=(Random.value*xWidth)-(xWidth*0.5);  // decide an actual place
		yCur=(Random.value*yWidth)-(yWidth*0.5);
		zCur=(Random.value*zWidth)-(zWidth*0.5);
	
		xRotCur=transform.localRotation.x+(Random.value*xRotMax*2)-(xRotMax);  // decide rotation
		yRotCur=transform.localRotation.y+(Random.value*yRotMax*2)-(yRotMax);  
		zRotCur=transform.localRotation.z+(Random.value*zRotMax*2)-(zRotMax);  

		var justCreated:GameObject=Instantiate(createThis[rndNr], transform.position, transform.rotation);  //create the prefab
		justCreated.transform.Rotate(xRotCur, yRotCur, zRotCur);
		justCreated.transform.Translate(xCur, yCur, zCur);
if(detachToWorld==true) justCreated.transform.parent=null;
	
	
		
		
		timeCounter-=trigger;  //administration :p
		effectCounter+=1;
		}


}