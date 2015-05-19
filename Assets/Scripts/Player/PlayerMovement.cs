using UnityEngine;

public class PlayerMovement : MonoBehaviour{
	public float speed = 6f;

	Vector3 movement;
	Animator anim;
	Rigidbody playerRigidbody;
	int floorMask;
	float camRayLength = 100f;

	// Awake() gets called regardless of whether the script is enabled or not
	// it's good for setting up references and things like that 
	void Awake(){
		// get mask from the floor 
		floorMask = LayerMask.GetMask ("Floor");

		// get the references to the animator and the rigidbody
		anim = GetComponent <Animator> ();
		playerRigidbody = GetComponent <Rigidbody> ();
	}

	// Called automatically by Unity 
	// The fixed update run with physics. It fires every physics update
	// Since we are moving a physics character, he’s got a rigidbody attached we’re going to use fixed update to move him
	void FixedUpdate(){

		// Get input from the horizontal and vertical axis, but we're not going to get the stardard input, we're going to get the raw input
		// So whereas a normal axis would have values varying between -1 and 1, the raw axis will only have a value of -1, 0 or 1.
		// we are going to immedietly snap to full speed, which will give us a much more responisive feel (rather than slowly accelerating towards it's full speed) 
		float h = Input.GetAxisRaw ("Horizontal");
		float v = Input.GetAxisRaw ("Vertical");

		Move (h,v);
		Turning ();
		Animating(h,v);
	}

	void Move (float h, float v){

		// we don't need a vertical component, so we set Y to 0
		// X and Z are flat along the ground
		// the horizontal adn vertical movement that we give it will translate to lateral movement in the game
		movement.Set (h, 0f, v);

		//but if you move just in the Z axis or just in the X axis, then you've got a value of 1, like a size of 1 for the vector.
		//however if you use both then the length of the vector is different, it's 1.4, so we need to change that so that you don't get an advantage by moving diagonally
		//what we want to do is normalise that. So what that means is it's going to take a direction that we have, but it's going to make sure that the size is alway 1.
		//Effectively make sure that player moves at the same speed regardless of which key combination you use.
		//We don't want it to move at a speed of 1, we want it to move at our speed, so we're going to times that by our speed variables that we stored.
		//Also, this is called fixed update. So we don't want it to move at 6 units per fixed update, it would move 6 units every 50th of a second and we wouldn't see our player again
		//movement = movement.normalized * speed

		//So instead we're going to change it so that it's per seconds and the way we do that is by multiplying by Time.DeltaTime.
		//DeltaTime is the time between each update call
		//So if you're moving it by that much per 50th of a second over the course of 50 50ths of secound it is going to move 6 units
		movement = movement.normalized * speed * Time.deltaTime;

		//apply that movement to the player
		//move player to the position in the world space, so we need to move it relative to the position that the character currently is.
		//we need to add our movement to the player's position
		playerRigidbody.MovePosition (transform.position + movement);
	}

	// We don't require any parameters for this.
	//Because the direction the character is facing is based on the mouse input rather that the input that we're already stored.
	void Turning(){

		//create a ray that we cast from the camera in to the scence.
		//if you think of the camera as something from where you are looking at the game on the screen forward on to the game level
		//you cast a ray, a single invisible line from that point to the floor quad to get a particular position back
		//and we want to use that because we want the character to turn and face the point of wherever the camera's looking.
		//So when you move the mouse around in the game, he's going to turn around and face that position so that you can trun around and also shoot in a paticular direction

		//a ray that coming from the camera
		//take a point on the screen and cast a ray from that point forwards in to the scene
		//so the point that we're going to give it is the mouse position
		//so it's always going to find the point underneath the mouse if you imagine
		//so if you're looking at the game, there's a mouse on your screen, the point underneath that mouse is the point it's going to find if that hits the floor quad.
		Ray camRay = Camera.main.ScreenPointToRay (Input.mousePosition);

		//we need to get information back when we do this raycast and in order to get information back from this raycast
		//we need a RaycastHit variable so that's what we're creating here.
		RaycastHit floorHit;

		//actually cast the ray that we've created.
		//so we created this imaginary invisible line and now we need to actually perform the action of casting the ray so that it can hit something.
		//a raycast function will return true if it has hit something and it will return false if it hasn't
		//distance = postion and direction of the cast that we're going to have
		//Out means that we're going to get information out of this function and we're going to store it in that floorHit variable.
		//Next we need to give it a length, so how far we are going to do this raycast for?
		//And that's the variable camRayLength that we store earlier.
		//And finally we want to make sure that this raycast is only trying to hit things on the floor layer.
		//That's that floor mask that we created earlier.
		if (Physics.Raycast (camRay, out floorHit, camRayLength, floorMask)) {
			//create the vector3 from the player to where the mouse has hit.
			//And thatis the floorHit.Point, so that's the point that it's hit the floor minus transform.position, that's the postion of the player.
			Vector3 playerToMouse = floorHit.point - transform.position;

			//apply this to the charecter to make him turn but we don't want him to sort of start learning back so we need to make sure that the Y component of this vector is definitely 0.
			playerToMouse.y = 0f;

			//we cant set a player's rotation based on a vector so we need to change that from a vector in to the horrible word, quaternion.
			//Quaternion is a way of storing a rotation.
			//we have a vector3 but we cant use that to store a rotation so we use a quaternion and we are going to create one called newRotation
			//and quaternions are also a class that has a number of functions of which we're going to use one called LookRotation
			//so what lookRotation dose, the default for charecters and camera and things like that in Unity and in most 3D modelling
			//is that the Z axis is thier forward axis.
			//So we want to made the playerToMouse vector, the forward vector point. 
			Quaternion newRotation = Quaternion.LookRotation(playerToMouse);

			//When we actually have to apply it so we're going to address the player rigidbody.
			//we are going to use the moveRotation function and since we dont want to give it an offset, we are trying to give it a completely new rotation.
			//we dont need to do transfrom.rotation + newRotation
			playerRigidbody.MoveRotation(newRotation);
		}
	}	

	//we need h and v since whether or not player move or idel is dependent on the input
	void Animating(float h, float v){

		//if there is an input (h or v) - a value that's non-0 then it's true and the player is walking
		//first of all H is that not equal to 0? That will return either true of false depending on whether it's 0 or not 0.
		//OR is V not eaual to 0? what this is basically saying is, did we press the horizontal axis or did we press vertical axis
		//If we pressed either of those we're walking. If we didnot, we are not.
		//We want to parse this to our animator component 
		bool walking = h != 0f || v != 0f;
		anim.SetBool ("IsWalking", walking);

	}
}
