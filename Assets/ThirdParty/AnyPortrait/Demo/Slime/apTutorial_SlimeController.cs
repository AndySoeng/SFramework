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

using UnityEngine;
using AnyPortrait;

namespace AnyPortrait
{

	public class apTutorial_SlimeController : MonoBehaviour
	{
		// Target AnyPortrait
		public apPortrait portrait;

		// Parameter Values
		private int eyeShape = 0;
		private int mouthShape = 0;
		private int emotion = 0;
		private float verticalPosition = 0.0f;

		private int touchID = -1;

		void Start() { }


		void Update()
		{
			//"Eye Shape" (0, 1, 2, 3 int)
			if (Input.GetKeyDown(KeyCode.E))
			{
				eyeShape++;
				if (eyeShape > 3)
				{ eyeShape = 0; }

				portrait.SetControlParamInt("Eye Shape", eyeShape);
			}

			//"Mouth Shape" (0, 1, 2, int)
			if (Input.GetKeyDown(KeyCode.M))
			{
				mouthShape++;
				if (mouthShape > 2)
				{ mouthShape = 0; }

				portrait.SetControlParamInt("Mouth Shape", mouthShape);
			}

			//"Emotion" (0, 1, 2, 3, int)
			if (Input.GetKeyDown(KeyCode.O))
			{
				emotion++;
				if (emotion > 3)
				{ emotion = 0; }

				portrait.SetControlParamInt("Emotion", emotion);
			}

			//"Vertical Position" (0 ~ 1 float)
			if (Input.GetKey(KeyCode.UpArrow))
			{
				// Move Upward
				verticalPosition += 2 * Time.deltaTime;
				if (verticalPosition > 1)
				{ verticalPosition = 1; }

				portrait.SetControlParamFloat("Vertical Position", verticalPosition);
			}
			else if (Input.GetKey(KeyCode.DownArrow))
			{
				// Move Downward
				verticalPosition -= 2 * Time.deltaTime;
				if (verticalPosition < 0)
				{ verticalPosition = 0; }

				portrait.SetControlParamFloat("Vertical Position", verticalPosition);
			}

			if (Input.GetMouseButton(0))
			{
				Vector2 mousePosW = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
				if (touchID < 0)
				{
					touchID = portrait.AddTouch(mousePosW, 10).TouchID;
				}
				else
				{
					portrait.SetTouchPosition(touchID, mousePosW);
				}
			}
			else
			{
				if (touchID >= 0)
				{
					portrait.ClearTouch();
					touchID = -1;
				}
			}
		}
	}
}