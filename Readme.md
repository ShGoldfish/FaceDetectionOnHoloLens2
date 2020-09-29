# Face Detection on HoloLens2 

### Unity 2018.4
### Code is on VS 2017
### Build is on VS 2019

## Scene:
Make sure name of the game objects (Specially the ones in "" are Exactly the same)
* Add MRTK 2.4 (Foundation)
	* Add to scene
		* Clone Profile/Input/Speech
* GameObject: "Manager"
	* Manager (Script)
	* Speech Handler (Script)
* MixedRealityPlayspace
	* Main Camera
		* Photo Capture (Script) 
		* it's Children:
			* GameObject: HeadFixedApplications
				* GameObject: App1
					* SpriteRenderer
					* Box Collider
					* App Manager (Script) 
						* attribute: other GO :(WorldFixedApplications/App1)
						* attribute: MsgBlocking -> its child
					* Input Action Handler:
						* attribute: isFocusRequired: true
						* attribute: InputAction: Select
						* attribute: OnInputActionStarted
							* attribute: -> App 1
							* attribute: AppManager.ChangeFixation
					* Children:
						* GameObject: "MsgBlocking1" [is a 3D object -> 3DText] Anchor/alignment: Lower Right/Left
						* GameObject: "FxationIcon"
							* Box Collider
							* Sprite Renderer
				* GameObject: App 2 : same as App1
				* GameObject: App 3 : same as App1
				* GameObject: Notes
					* GameObject: "MessageFace" [is a 3D object -> 3DText] Anchor/alignment: Lower Right/Left
					* GameObject: "MessageVoice" [is a 3D object -> 3DText] Anchor/alignment: Upper right/Right
* GameObject: WorldFixedApplications
	* GameObject: App 1 [disable]
		* Copy HeadFixedApplications/App1
		* App Manager (Script) 
			* Attr: other GO :(HeadFixedApplications/App1)
	* GameObject: App 2 [disable] : same as App1
	* GameObject: App 3 [disable] : same as App1


## Requirements:
* Run faceDetection.py on the server.
* Make sure the in ipEndPoint in Manager.cs matches your server's ip


Probably urelated:
* Project Settings -> Graphics -> Always-included Shaders: increase the num and add the shader (AR/HolographicImageBlendShader)
