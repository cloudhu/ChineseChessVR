----------------------------------------
----------------------------------------
----------------------------------------
----------------------------------------
Welcome to Hands VR- Ultimate Interaction Toolset

Video explanation and tutorials for HandsVR can be found at:
(Tschirgi Games Youtube Channel)

https://www.youtube.com/channel/UCQfacnuX3qOD79ml7nlSWag

'HandsVR - Setup Tutorial' will walk you through it. Otherwise use the written tutorial below.


Features:
-Picking up
-Dropping
-Class system of object types:
	-Pinch
	-Hilt
	-Gun
	-Sphere
	-Misc
-Each Class has unique animations:
	-hover
	-grasp size
-Each Class uses different adjustable pick up requirements:
	-Grab Distance
	-Hold Point
	-Held Position
	-Held Rotation
---------------------------------------
Included sample objects:
-Left and Right Hand
-Sword
-Phone
-Pen
-Desert Eagle Gun
-Ak47 Gun
-Circlet Crown
-Bomb (with fuze lit animation and explosion animation)
-Chainsaw
-Simple Office Interior
----------------------------------------
----------------------------------------
Summary -- Theory of HandsVR:
HandsVR works using a lock on system where hands will indicate the ability to pick up something by hovering over it and playing the corresponding Preparation animation such as a hand opening up to pick up a sword. Then when the user presses the trigger the object is parented to the hand either at a specific position and rotation or a dynamic one based on the acceptable pick up angle and grab distance. A specific position might be used for something like a pen that you would want to pick up into a drawing orientation. A more dynamic pick up might be used for a sword so you can pick up the sword from any side of the hilt or a spherical object that you would want to pick up from any side like an apple or snowball.

This solves the core problem with hand visuals in Virtual Reality because you want clear feedback to the user that they can pick something up, but having hand animations for every single pickupable object is a nightmare especially when trying to align it from any angle. You can use the grab distance to give the user the ability to pick up objects with bigger hitboxes making things like catching falling objects out the air suddenly much easier, but without the problem of parenting the objects too far from the hand mesh that it would look weird.

The hands are also rotated to match the feel of your hands in the real world so the process of picking things up feels natural.

The Pickup component script can be setup on any object so it's easy to extend any kind of object past the example objects that are included and already setup to show how it works.

----------------------------------------
----------------------------------------

How To -- Getting Started and Working with HandsVR

-------------------------------------------
Step One- Testing the Demo File

Open PhysicsHand.unity scene in Example Scene folder. Play the scene with your HTC Vive turned on and make sure you can see both your hands and are able to pick up and drop all the items on the table and floor with the hands.

-------------------------------------------
Step Two- Understand the Hands setup

This explains the implemented theory of HandsVR, if you don't need to understand why or how it works you can skip to Step 4 to get it setup in your own game.

If you expand the '[CameraRig]' SteamVR plugin game object in the Demo scene and then expand 'Controller (left)' and 'Controller (right)' you should be able to see the 'LeftHand' and 'RightHand' game objects. These contain the animated hand mesh and a bunch of sub objects that help measure the distance the hand grasp from the grabbable objects. Here is a breakdown of them now:

---------
Right Hand
	-TargetOffset
	-Offsetter
		-GrabPoint
		-Hands7HD
			(Hand mesh and animated bone structure)

Right Hand - This gameobject contains the core script and a big spherical collider. The collider detects any "PickUp" tagged objects and optimizes when it should calculate the distances and ability to pick up those objects. The core script 'HandController' does the bulk of the functionality. It gets input from each Vive Controller, manages objects within pickup range and oversees the process of picking up and dropping objects as well as the visual indications of whether the hand is close enough to pick something up (it will move the hand object to the object) and whether the hand is orientated correctly to pick up the object (within a certain range based on object type). It makes use of all the gameobjects below it in the heirarchy to more simply detect distances and snap animations and rotations. Use this object's rotation to control the rotation of the hand visual relative to the controller.

TargetOffset - This gameobject is used to track the true distance of the vive controller from any grabbable objects. Because the hand mesh is moved (from controller) to indicate the ability to pick up an object we need to be able to monitor the distance from the vive controller to the object. If we used something parented to the hand mesh that would change when it got within range and it would never detect being too far away to pick something up. The position of this object should match the 'GrabPoint' object position.

Offsetter - This gameobject is used to move the handmesh between its default position and Prepped-Pickup position. The default position is centered to the vive controller. In other words when the hand mesh feels similarly positioned to your real hand. The Prepped-Pickup position is the handmesh being snapped to a nearby pick up object. AKA the hand looks like it is poised to grab the object such as hovering above the hilt of a sword. This gameobject is positioned to be in exactly the same place as the 'RightHand' parent object. This makes it really easy to move the hand back to it's default position because we just have to Lerp the position back to a local position of 0.

GrabPoint - This gameobject represents the position at which objects will be picked up by. This makes calculating the distance to a pickupable object easier and easy to adjust. When objects are picked up they will be parented to this object because it makes any offset value such as adjusting the place you hold a sword from more intuitive. It is important that this object's position be the exact same as TargetOffset since that calculates the distance from pickupable objects.

Hands7HD - This is the animated mesh that handles all the animations of the mesh from grasping to pinching to squeeze values. It gets passed directions on what animation to play from 'RightHand's HandController script. It is important that the position of this object is used to adjust the hand's visual to match whatever position makes sense for the player's experience of their hand. In other words change the position to make it feel like the hand is in the right place.

-------------------------------------------
Step Three- Understand the Pickup Setup

If you expand the 'AllPickups' gameobject you will see all the gameobjects you can select in the PhysicsHand scene. Some of them are highlighted in blue because they are the root of an imported mesh.

Each of the gameobjects have their tag set to "PickUp". This allows the hands to detect them. Parented to each of the objects is a 'HoldPoint' gameobject.

If you click on the 'DesertEagle' gameobject you will notice it has a Rigidbody, a Collider and and 'Pickup' script attached.

The rigidbody makes it easy to let an object physically fall to the ground when it is dropped and the Collider makes sure it doesn't fall through the floor.

The Pickup script is used to tell the hands what type of an object it is and passes any manual information needed such as an offset position and rotation to position the object perfectly when held by the hand to fit animations as well as possible.

The Holdpoint is an empty gameobject that acts as the pickup center for the hands. This is great for use on a sword or object where the pivot is not where it would be picked up from.

-------------------------------------------
Step Four- Setting up HandsVR for your own project

When importing the package into your game all you absolutely need in the HandsVR package are the folders: 
HandMesh, Prefabs, Scripts

Inside Prefabs is a HandsPrefab, LeftDummy, RightDummy, and TempParent prefab. Go to whichever Unity scene you want the Hands setup in and drag HandsPrefab onto your [CameraRig] SteamVR prefab.

The Transform component of the 'HandsPrefab' should read as 0,0,0 for Position, but if it is not set x, y, and z to be 0. This HandsPrefab root object is only used to quickly setup the Hands.

If you expand the HandsPrefab object you should see RightHand and LeftHand. Drag these gameobjects onto their corresponding Vive Controller Objects:

'RightHand' to 'Controller(right)'
'LeftHand' to 'Controller(left)'

It will ask if you want to break the prefab instance, select continue. At this point I suggest selecting the Model child of both Controllers and turning it off by unchecking the box in the inspector window while selecting the 'Model' object. this just makes the vive controller invisible so all the player sees are the hands.

The final step is to select the RightHand gameobject and drag it's parent 'Controller (right)' into it's first variable in HandController script 'Vive Controller Root'. This allows the script to access the controllers input.

At this point the hands will work and turn to fists when you press the trigger during the game, but we haven't setup objects to pick up yet.

-------------------------------------------
Step Five- Setting up objects to be Picked Up

Any object you want the hands to be able to pick up needs a few things:

Tag = "PickUp" - I'm not sure if importing the package will automatically add the tag "PickUp" but you should add it if it doesnt.

A Rigidbody, a collider, a Pickup script, and an empty gameobject parented to the object that I like to name 'HoldPoint'

Rigidbody - allows the gameobject to fall to the ground when dropped by hands

Collider - used so that hands can detect it and to not fall through ground. If it is a mesh collider, it probably needs to be set to Convex.

Pickup - Passes information about object type (and corresponding animation to use) to Hands
      :
	Size - Type of object it is. This decides what angle it can be picked up from and what animation the hand should play to best 	       visualize picking up the object
	Held Position - Offset of object's pivot to help align object in right hand's grip
	Left Held Position - Offset of object's pivot to help align object in left hand's grip
	Held Rotation - Offset rotation of object's pivot to help align object in right hand's grip
	Left Held Rotation - Offset rotation of object's pivot to help align object in left hand's grip
	Grab Range - Distance hand can grab the object from, usually .2 is good if the scale of your CameraRig prefab (your game) is the 		             Default 1.
	HoverOverGrabRange - if object is actively able to be grabbed by a hand (should be unchecked)
	IsBeingHeld - if object is grabbed by a hand
	Rby - Rigidbody reference, drag it's own object into this field
	HoldPoint - Empty Gameobject that represents pickup pivot for object. It should be parented to this object. Just create one using 		    an empty gameobject child and then position it where the hand should pick it up by (such as the hilt of a sword)
	Squeeze - Value from 0-1 that tells the hand animation how big the object is. Can be adjusted while game is playing for perfect 		  setting. Ex: tiny pencil might be .2 and a big pole might be .8
	

HoldPoint : Needs to be rotated to correct orientation for some object types ('Size') such as Hilt. See example objects in PhysicsHand example scene for correct x,y,z rotation relative to the object. Notice how the y-axis runs parallel to the shaft of the sword. This helps tell the hand what angles the object can be selected from. 

Getting Position/Rotation Offsets:
	In order to get the perfect offset values my suggestion is to bring in the LeftDummy and RightDummy prefabs from the prefabs folder and position them over the object you want to set up the pickup values. Play the game and then if you placed the hand close enough and in the right orientation click as input while cliced into the game window and the hand should pick up the object. Once picked up you can select the object inside the DummyHand heirarchy and adjust the position and rotation manually until you like what you see. You can also adjust the squeeze value to match the size of the object relative to the hand animations. Once you adjust the position to your liking write down the position and/or rotation values and type them in to the object's pickup script Held position/rotation while the game is NOT playing. For most objects you need to do this twice for both right and left hand (use DummyHand prefabs). It's a slightly annoying process, but having objects perfectly aligned to fit generic hand animations is very important so it's a worthy chore to do once for each object that needs re-positioning.


Congratulations you now have working animated hands in VR for the HTC Vive. You should now be able to add any new objects of your own into pickupable objects for your hands to grab. These tools can probably be easily adjusted to work with occulus touch, but it is not currently supported from the get go.

Any questions, comments or reports of errors/feedback can be sent to paultschirgi@gmail.com

You can check out my current game projects such as a Dating Simulation meets Superpowers Combat RPG' at:

http://paultproductions.com/index.html
