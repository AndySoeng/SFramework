/*
*	Copyright (c) 2017-2023. RainyRizzle Inc. All rights reserved
*	Contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of [AnyPortrait].
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of [Seungjik Lee] of [RainyRizzle team].
*
*	It is illegal to download files from other than the Unity Asset Store and RainyRizzle homepage.
*	In that case, the act could be subject to legal sanctions.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnyPortrait;

namespace AnyPortrait
{

	public class apTutorial_PortraitController : MonoBehaviour
	{
		// Members
		//------------------------------------------------
		public apPortrait portrait;
		public Camera targetCamera;

		public Collider bodyArea;
		public Collider headArea;
		public Transform headCenter;

		public string controlParamName_EyeDirection = "Eye Direction";
		public string controlParamName_HeadDirection = "Head Direction";

		public Vector2 eyeAreaMaxSize = new Vector2(20, 20);
		public Vector2 headAreaMaxSize = new Vector2(10, 10);

		public ParticleSystem touchParticle;
		public Transform handGroup;
		public MeshRenderer handMesh_Released;
		public MeshRenderer handMesh_Pressed;


		private Vector3 touchPos = Vector3.zero;
		private bool isTouched = false;
		private bool isTouchedPrevFrame = false;

		private bool isTouchHead = false;
		private bool isTouchBody = false;

		private bool isTouchDown = false;

		private bool isEyeReturn = false;
		private float eyeReturnTime = 0.0f;
		private float eyeReturnTimeLength = 0.3f;
		private Vector2 lastEyeParam = Vector2.zero;
		private Vector2 lastHeadParam = Vector2.zero;


		// Init / Update
		//------------------------------------------------------------------
		void Start()
		{
			isTouched = false;
			isTouchedPrevFrame = false;
			touchPos = Vector3.zero;

			isTouchHead = false;
			isTouchBody = false;

			isTouchDown = false;
			isEyeReturn = false;

			Cursor.visible = false;
		}

		void Update()
		{
			//Calculate Touch
			isTouched = false;
			Vector2 touchPosScreen = Vector2.zero;


			if (Input.GetMouseButton(0))
			{
				isTouched = true;
				touchPosScreen = Input.mousePosition;
			}
			else if (Input.touchCount == 1)
			{
				TouchPhase touchPhase = Input.GetTouch(0).phase;
				if (touchPhase == TouchPhase.Began ||
					touchPhase == TouchPhase.Moved ||
					touchPhase == TouchPhase.Stationary)
				{
					isTouched = true;
					touchPosScreen = Input.GetTouch(0).position;
				}
			}


			if (isTouched)
			{
				touchPos = targetCamera.ScreenToWorldPoint(new Vector3(touchPosScreen.x, touchPosScreen.y, portrait.transform.position.z));
			}
			else
			{
				isTouchDown = true;
			}


			// Touch Event
			if (isTouched != isTouchedPrevFrame)
			{
				isTouchHead = false;
				isTouchBody = false;

				if (isTouched)
				{
					Ray touchRay = targetCamera.ScreenPointToRay(new Vector3(touchPosScreen.x, touchPosScreen.y, 1000.0f));
					RaycastHit[] hits = Physics.RaycastAll(touchRay);
					if (hits != null && hits.Length > 0)
					{
						for (int i = 0; i < hits.Length; i++)
						{
							if (hits[i].collider == bodyArea)
							{
								isTouchBody = true;
								break;
							}
							if (hits[i].collider == headArea)
							{
								isTouchHead = true;
								break;
							}
						}
					}

					if (isTouchHead)
					{
						if (isTouchDown)
						{
							// Smile!
							if (!portrait.IsPlaying("Smile"))
							{
								portrait.CrossFade("Smile", 0.2f);
								portrait.CrossFadeQueued("Idle");
							}
						}
					}
					else if (isTouchBody)
					{
						if (isTouchDown)
						{
							// Angry
							if (!portrait.IsPlaying("Angry"))
							{
								portrait.Play("Angry");
								portrait.CrossFadeQueued("Idle", 0.2f);
								
							}
						}
					}
				}
				else
				{
					//Reset "Eye" Timer
					isEyeReturn = true;
					eyeReturnTime = 0.0f;
				}
			}


			//Eye/Head Control
			if (isTouched)
			{

				Vector2 head2Touch = new Vector2(touchPos.x - headCenter.position.x, touchPos.y - headCenter.position.y);
				Vector2 eyeParam = new Vector2(Mathf.Clamp(head2Touch.x / eyeAreaMaxSize.x, -1, 1),
												Mathf.Clamp(head2Touch.y / eyeAreaMaxSize.y, -1, 1));

				Vector2 headParam = new Vector2(Mathf.Clamp(head2Touch.x / headAreaMaxSize.x, -1, 1),
													Mathf.Clamp(head2Touch.y / headAreaMaxSize.y, -1, 1));

				portrait.SetControlParamVector2(controlParamName_EyeDirection, eyeParam);
				portrait.SetControlParamVector2(controlParamName_HeadDirection, headParam);

				lastEyeParam = eyeParam;
				lastHeadParam = headParam;

				if (!touchParticle.isPlaying)
				{
					touchParticle.Play();
				}

				handGroup.position = new Vector3(touchPos.x, touchPos.y, handGroup.position.z);
				handMesh_Released.enabled = false;
				handMesh_Pressed.enabled = true;

			}
			else
			{
				if (touchParticle.isPlaying)
				{
					touchParticle.Stop();
				}

				if (Input.mousePresent)
				{

					Vector2 mousePosW = targetCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, portrait.transform.position.z));

					handGroup.position = new Vector3(mousePosW.x, mousePosW.y, handGroup.position.z);
					handMesh_Released.enabled = true;
					handMesh_Pressed.enabled = false;
				}
				else
				{
					handMesh_Released.enabled = false;
					handMesh_Pressed.enabled = false;
				}

				if (isEyeReturn)
				{

					eyeReturnTime += Time.deltaTime;
					if (eyeReturnTime < eyeReturnTimeLength)
					{
						apControlParam controlParam_Eye = portrait.GetControlParam(controlParamName_EyeDirection);
						apControlParam controlParam_Head = portrait.GetControlParam(controlParamName_HeadDirection);

						float itp = 1.0f - (eyeReturnTime / eyeReturnTimeLength);

						portrait.SetControlParamVector2(controlParamName_EyeDirection, lastEyeParam * itp + controlParam_Eye.Vector2ValueWithoutEditing * (1 - itp));
						portrait.SetControlParamVector2(controlParamName_HeadDirection, lastHeadParam * itp + controlParam_Head.Vector2ValueWithoutEditing * (1 - itp));
					}
					else
					{
						isEyeReturn = false;
						eyeReturnTime = 0.0f;
					}
				}
			}

			isTouchedPrevFrame = isTouched;
			if (isTouchDown && isTouched)
			{
				isTouchDown = false;
			}
		}

	}
}