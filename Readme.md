# Face Detection on HoloLens2 

Connection over server to do openCV section in python on PC.

### Unity 2018.4
### Code is on VS 2017
### Build is on VS 2019

## Scene:
Make sure name of the game objects (Specially the ones in "" are Exactly the same)
* Add MRTK 2.4
	* Add to scene
		* Clone Profile/Input/Speech
* GameObject: "Manager"
	* NetworkCon.cs
		* Connection Port: 9005
	* Context Detection
		* Num_faces: 0
		Is Talking: false
	* Speech Handler
* Main Camera
	* PhotoCapture.cs 
	* it's Children:
		* Application
			* SpriteRenderer
			* AppManager.cs
		* "MessageFace" [is a 3D object -> 3DText] Anchor/alignment: Lower Right/Left
		* "MessageVoice" [is a 3D object -> 3DText] Anchor/alignment: Upper right/Right

## After Build:  
* Make sure you do [this](https://stackoverflow.com/questions/62314810/on-a-hololens-1-when-creating-tcpclient-object-with-default-constructor-argume)

# Python Code
* make sure the other 2 files are in thesame dir
* make sure in the python code:
	* ip is the same as HoloLens 2's IP
	* conf is set as desired
* Do not change the port # [9005 looks to be the only working one!]


Probably urelated:
* Project Settings -> Graphics -> Always-included Shaders: increase the num and add the shader (AR/HolographicImageBlendShader)


