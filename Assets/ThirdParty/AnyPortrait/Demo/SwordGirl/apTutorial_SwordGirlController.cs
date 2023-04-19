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

	public class apTutorial_SwordGirlController : MonoBehaviour
	{
		public apPortrait portrait;

		public Vector2 moveVelocity = new Vector2(10, 10);
		public Vector2 moveAcc = new Vector2(30, 30);

		private Vector3 curVelocity = Vector3.zero;

		public Transform limitPos_L;
		public Transform limitPos_R;

		public Camera targetCamera;

		public float farSize = 20.0f;
		public float nearSize = 5.0f;
		public float maxDistToCharacter = 5.0f;

		public ParticleSystem footFx_Left = null;
		public ParticleSystem footFx_Right = null;

		public TrailRenderer swordFx = null;



		private enum Motion
		{
			Idle,
			Run,
			Attack
		}
		private Motion motion = Motion.Idle;


		void Start()
		{
			curVelocity = Vector3.zero;
			motion = Motion.Idle;
		}

		void Update()
		{
			if (motion == Motion.Attack)
			{
				//Attack
				//...Wait
			}
			else
			{
				//Idle or Run

				int inputX = 0;
				if (Input.GetKey(KeyCode.LeftArrow))
				{
					inputX += -1;
				}
				if (Input.GetKey(KeyCode.RightArrow))
				{
					inputX += 1;
				}
				if (Input.GetKeyDown(KeyCode.Space))
				{
					motion = Motion.Attack;
					portrait.CrossFade("Attack", 0.1f);

				}

				if (motion != Motion.Attack)
				{
					if (inputX == 0)
					{
						// Stop
						curVelocity = Vector2.zero;

						if (motion == Motion.Run)
						{
							//Run -> Idle
							portrait.CrossFade("Idle");
							motion = Motion.Idle;
						}

					}
					else
					{
						// Move
						curVelocity.x += inputX * moveAcc.x * Time.deltaTime; //Move Left / Right
						curVelocity.x = Mathf.Clamp(curVelocity.x, -moveVelocity.x, moveVelocity.x);

						if (motion == Motion.Idle)
						{
							//Idle -> Run
							portrait.CrossFade("Run");
							motion = Motion.Run;
						}
					}

					bool isXFlip = false;
					if (curVelocity.x < -0.3f && portrait.transform.localScale.x < 0.0f)
					{
						//Turn To Left
						isXFlip = true;
					}
					else if (curVelocity.x > 0.3f && portrait.transform.localScale.x > 0.0f)
					{
						//Turn To Right
						isXFlip = true;
					}

					if (isXFlip)
					{
						Vector3 curScale = portrait.transform.localScale;
						curScale.x *= -1;

						portrait.transform.localScale = curScale;
					}

					Vector3 nextPos = portrait.transform.position + curVelocity * Time.deltaTime;
					if (nextPos.x < limitPos_L.position.x && curVelocity.x < 0.0f)
					{
						nextPos.x = limitPos_L.position.x;
					}
					else if (nextPos.x > limitPos_R.position.x && curVelocity.x > 0.0f)
					{
						nextPos.x = limitPos_R.position.x;
					}
					portrait.transform.position = nextPos;
				}
			}


			//카메라를 움직이자
			Vector3 cameraPos = targetCamera.transform.position;
			float deltaXCamera2Character = (portrait.transform.position.x - cameraPos.x);
			if (Mathf.Abs(deltaXCamera2Character) > maxDistToCharacter)
			{
				//카메라가 한계점에 도달했다.
				if (deltaXCamera2Character < 0.0f)
				{
					cameraPos.x = portrait.transform.position.x + maxDistToCharacter;
				}
				else
				{
					cameraPos.x = portrait.transform.position.x - maxDistToCharacter;
				}
			}
			else
			{
				cameraPos.x = cameraPos.x * 0.98f + portrait.transform.position.x * 0.02f;
			}

			float itpSize = Mathf.Clamp01((Mathf.Abs(deltaXCamera2Character) / maxDistToCharacter));

			targetCamera.transform.position = cameraPos;
			float nextOrthoSize = nearSize * (1.0f - itpSize) + farSize * itpSize;
			targetCamera.orthographicSize = targetCamera.orthographicSize * 0.9f + nextOrthoSize * 0.1f;

		}

		void FootFx(bool isLeftFoot)
		{
			if (isLeftFoot)
			{
				footFx_Left.Play();
			}
			else
			{
				footFx_Right.Play();
			}
		}

		void SwordFx(bool isFxEnabled)
		{
			if (isFxEnabled)
			{
				swordFx.enabled = true;
			}
			else
			{
				swordFx.enabled = false;
			}
		}

		void AttackEnd()
		{
			motion = Motion.Idle;
			portrait.CrossFade("Idle");
		}
	}
}