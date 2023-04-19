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


	/// <summary>
	/// The Class for the character the player controls
	/// </summary>
	public class apTutorial_PirateGame_Actor : MonoBehaviour
	{
		// Members
		//--------------------------------------------------
		public apPortrait girl;
		public SpriteRenderer aimUI;
		public Animator girlAnimator;

		public Transform girlFootPosY_Default;
		public Transform girlFootPosY_Min;
		public Transform girlFootPosY_Max;

		public Transform hitBox_LT;
		public Transform hitBox_RB;

		public apPortrait robot;
		public Transform robotTransform;
		public Animator robotAnimator;

		public Transform robotFootPosY_Default;
		public Transform robotFootPosY_Min;
		public Transform robotFootPosY_Max;
		public Transform robotFootRaycast_L;
		public Transform robotFootRaycast_R;

		public float forwardVelocity = 30.0f;
		public float backwardVelocity = 20.0f;
		public float robotVelocity = 8.0f;
		public float robotDistX = 15.0f;




		public apTutorial_PirateGame_Manager gameManager;

		private bool _isLive = false;
		private bool _isRobotShootable = false;
		private bool _isGirlShootable = false;



		private const int MOVE_FORWARD = 1;
		private const int MOVE_BACKWARD = -1;
		private const int MOVE_STAND = 0;
		private const int MOVE_ROBOT_LEFT = -1;
		private const int MOVE_ROBOT_RIGHT = 1;

		private Vector3 _aimPosition;
		private bool _isIKCalculate = true;
		public bool IsIKCalculate { get { return _isIKCalculate; } }


		// Initialize
		//--------------------------------------------------
		void Start()
		{
			Reset();
			_isIKCalculate = true;
		}


		//Initialize the Mecanim state.
		public void Reset()
		{
			girlAnimator.SetBool("IsLive", true);
			girlAnimator.SetInteger("Move", MOVE_STAND);
			girlAnimator.ResetTrigger("Shoot");
			girlAnimator.SetTrigger("ShootEnd");

			robotAnimator.SetInteger("Move", MOVE_STAND);
			robotAnimator.ResetTrigger("Shoot");
			robotAnimator.SetTrigger("ShootEnd");

			_isLive = true;
			_isRobotShootable = true;
			_isGirlShootable = true;

			//Modify Position of Girl
			Vector3 girlPosition = transform.position;
			if (girlPosition.x < gameManager.leftBound.position.x)
			{
				girlPosition.x = gameManager.leftBound.position.x;
			}
			if (girlPosition.x > gameManager.rightBound.position.x)
			{
				girlPosition.x = gameManager.rightBound.position.x;
			}
			RaycastHit2D raycastHit_GirlPosition = Physics2D.Raycast(girlPosition + new Vector3(0.0f, 20.0f, 0.0f), new Vector2(0.0f, -1.0f), 100);
			if (raycastHit_GirlPosition && raycastHit_GirlPosition.collider != null)
			{
				float nextGirlPosY = raycastHit_GirlPosition.point.y - (girlFootPosY_Default.localPosition.y);
				if (Mathf.Abs(girlPosition.y - nextGirlPosY) < 0.2f)
				{
					girlPosition.y = girlPosition.y * 0.7f + nextGirlPosY * 0.3f;
				}
				else
				{
					girlPosition.y = nextGirlPosY;
				}
			}
			transform.position = girlPosition;



			Vector3 robotPosition = robotTransform.position;

			robotAnimator.SetInteger("Move", MOVE_STAND);

			//Modify Position of Robot
			if (robotPosition.x < gameManager.leftBound.position.x)
			{
				robotPosition.x = gameManager.leftBound.position.x;
			}
			if (robotPosition.x > gameManager.rightBound.position.x)
			{
				robotPosition.x = gameManager.rightBound.position.x;
			}
			RaycastHit2D raycastHit_RobotPosition = Physics2D.Raycast(robotPosition + new Vector3(0.0f, 20.0f, 0.0f), new Vector2(0.0f, -1.0f), 100);
			if (raycastHit_RobotPosition && raycastHit_RobotPosition.collider != null)
			{
				float nextRobotPosY = raycastHit_RobotPosition.point.y - (robotFootPosY_Default.localPosition.y);
				if (Mathf.Abs(robotPosition.y - nextRobotPosY) < 0.2f)
				{
					robotPosition.y = robotPosition.y * 0.7f + nextRobotPosY * 0.3f;
				}
				else
				{
					robotPosition.y = nextRobotPosY;
				}
			}
			robotTransform.position = robotPosition;

			//Set Aim Helper Bone Position
			robot.SetBonePosition("Helper Aim", _aimPosition, Space.World);

			RaycastHit2D raycastHit_RobotFootL = Physics2D.Raycast(robotFootRaycast_L.position, new Vector2(0.0f, -1.0f), 30);
			RaycastHit2D raycastHit_RobotFootR = Physics2D.Raycast(robotFootRaycast_R.position, new Vector2(0.0f, -1.0f), 30);


			if (raycastHit_RobotFootL && raycastHit_RobotFootL.collider != null)
			{
				robot.SetBonePositionConstraintBySurface("Helper Foot Front L",
															robotFootPosY_Default.position.y,
															raycastHit_RobotFootL.point.y,
															robotFootPosY_Min.position.y,
															robotFootPosY_Max.position.y, ConstraintSurface.Ysurface, Space.World);

				robot.SetBonePositionConstraintBySurface("Helper Foot Back L",
														robotFootPosY_Default.position.y,
														raycastHit_RobotFootL.point.y,
														robotFootPosY_Min.position.y,
														robotFootPosY_Max.position.y, ConstraintSurface.Ysurface, Space.World);
			}

			if (raycastHit_RobotFootR && raycastHit_RobotFootR.collider != null)
			{
				robot.SetBonePositionConstraintBySurface("Helper Foot Front R",
														robotFootPosY_Default.position.y,
														raycastHit_RobotFootR.point.y,
														robotFootPosY_Min.position.y,
														robotFootPosY_Max.position.y, ConstraintSurface.Ysurface, Space.World);

				robot.SetBonePositionConstraintBySurface("Helper Foot Back R",
														robotFootPosY_Default.position.y,
														raycastHit_RobotFootR.point.y,
														robotFootPosY_Min.position.y,
														robotFootPosY_Max.position.y, ConstraintSurface.Ysurface, Space.World);
			}

			_isIKCalculate = true;
		}

		public void SetIKCalculateToggle()
		{
			_isIKCalculate = !_isIKCalculate;
		}

		// Update
		//--------------------------------------------------
		void Update()
		{
			if (!gameManager.IsLive())
			{
				aimUI.enabled = false;
				return;
			}
			if (!_isLive)
			{
				aimUI.enabled = false;
				return;
			}

			// Set Aim by Mouse Position
			Vector2 mousePos_Screen = Input.mousePosition;
			_aimPosition = Camera.main.ScreenToWorldPoint(mousePos_Screen);
			_aimPosition.z = 0;
			aimUI.transform.position = _aimPosition;
			aimUI.enabled = true;

			bool isLeftIsForward = (_aimPosition.x < transform.position.x);

			// Set Scale X of Girl
			if (!isLeftIsForward)
			{
				if (transform.localScale.x > 0.0f)
				{
					transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
				}
			}
			else
			{
				if (transform.localScale.x < 0.0f)
				{
					transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
				}
			}



			// Move Girl
			Vector3 girlPosition = transform.position;
			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
			{
				if (isLeftIsForward)
				{
					girlAnimator.SetInteger("Move", MOVE_FORWARD);
					girlPosition.x -= forwardVelocity * Time.deltaTime;
				}
				else
				{
					girlAnimator.SetInteger("Move", MOVE_BACKWARD);
					girlPosition.x -= backwardVelocity * Time.deltaTime;
				}
			}
			else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
			{
				if (isLeftIsForward)
				{
					girlAnimator.SetInteger("Move", MOVE_BACKWARD);
					girlPosition.x += backwardVelocity * Time.deltaTime;
				}
				else
				{
					girlAnimator.SetInteger("Move", MOVE_FORWARD);
					girlPosition.x += forwardVelocity * Time.deltaTime;
				}
			}
			else
			{
				girlAnimator.SetInteger("Move", MOVE_STAND);
			}

			//Modify Position of Girl
			if (girlPosition.x < gameManager.leftBound.position.x)
			{
				girlPosition.x = gameManager.leftBound.position.x;
			}
			if (girlPosition.x > gameManager.rightBound.position.x)
			{
				girlPosition.x = gameManager.rightBound.position.x;
			}
			RaycastHit2D raycastHit_GirlPosition = Physics2D.Raycast(girlPosition + new Vector3(0.0f, 20.0f, 0.0f), new Vector2(0.0f, -1.0f), 100);
			if (raycastHit_GirlPosition && raycastHit_GirlPosition.collider != null)
			{
				float nextGirlPosY = raycastHit_GirlPosition.point.y - (girlFootPosY_Default.localPosition.y);
				if (Mathf.Abs(girlPosition.y - nextGirlPosY) < 0.2f)
				{
					girlPosition.y = girlPosition.y * 0.7f + nextGirlPosY * 0.3f;
				}
				else
				{
					girlPosition.y = nextGirlPosY;
				}
			}
			transform.position = girlPosition;

			if (_isIKCalculate)
			{
				//Set Aim Helper Bone Position
				girl.SetBonePosition("Helper Aim", _aimPosition, Space.World);


				//Set Foot Height
				Vector3 footPos_L = girl.GetBoneSocket("Helper Foot L").transform.position;
				Vector3 footPos_R = girl.GetBoneSocket("Helper Foot R").transform.position;

				RaycastHit2D raycastHit_FootL = Physics2D.Raycast(footPos_L + new Vector3(0.0f, 10.0f, 0.0f), new Vector2(0.0f, -1.0f), 30);
				RaycastHit2D raycastHit_FootR = Physics2D.Raycast(footPos_R + new Vector3(0.0f, 10.0f, 0.0f), new Vector2(0.0f, -1.0f), 30);

				if (raycastHit_FootL && raycastHit_FootL.collider != null && raycastHit_FootL.point.y < transform.position.y)
				{
					girl.SetBonePositionConstraintBySurface("Helper Foot L",
								girlFootPosY_Default.position.y,
								raycastHit_FootL.point.y,
								girlFootPosY_Min.position.y,
								girlFootPosY_Max.position.y, ConstraintSurface.Ysurface, Space.World);
				}

				if (raycastHit_FootR && raycastHit_FootR.collider != null && raycastHit_FootR.point.y < transform.position.y)
				{
					girl.SetBonePositionConstraintBySurface("Helper Foot R",
								girlFootPosY_Default.position.y,
								raycastHit_FootR.point.y,
								girlFootPosY_Min.position.y,
								girlFootPosY_Max.position.y, ConstraintSurface.Ysurface, Space.World);
				}
			}

			//Robot Movement
			Vector3 robotTargetPos = Vector3.zero;


			if (isLeftIsForward)
			{
				robotTargetPos = transform.position + new Vector3(robotDistX, 0.0f, 0.0f);
			}
			else
			{
				robotTargetPos = transform.position + new Vector3(-robotDistX, 0.0f, 0.0f);
			}

			if (_aimPosition.x > robotTransform.position.x)
			{
				if (robotTransform.localScale.x > 0.0f)
				{
					robotTransform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
				}
			}
			else
			{
				if (robotTransform.localScale.x < 0.0f)
				{
					robotTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
				}
			}


			Vector3 robotPosition = robotTransform.position;
			float deltaX = robotTargetPos.x - robotTransform.position.x;
			if (Mathf.Abs(robotPosition.x - transform.position.x) > robotDistX)
			{
				if (deltaX < 0)
				{
					robotPosition.x -= Time.deltaTime * robotVelocity;
				}
				else
				{
					robotPosition.x += Time.deltaTime * robotVelocity;
				}

				//When moving
				if (robotTransform.localScale.x > 0)
				{
					//Look Left
					if (deltaX < 0)
					{
						robotAnimator.SetInteger("Move", MOVE_ROBOT_LEFT);
					}
					else
					{
						robotAnimator.SetInteger("Move", MOVE_ROBOT_RIGHT);
					}
				}
				else
				{
					//Look Right
					if (deltaX < 0)
					{
						robotAnimator.SetInteger("Move", MOVE_ROBOT_RIGHT);
					}
					else
					{
						robotAnimator.SetInteger("Move", MOVE_ROBOT_LEFT);
					}
				}
			}
			else
			{
				robotAnimator.SetInteger("Move", MOVE_STAND);
			}

			//Modify Position of Robot
			if (robotPosition.x < gameManager.leftBound.position.x)
			{
				robotPosition.x = gameManager.leftBound.position.x;
			}
			if (robotPosition.x > gameManager.rightBound.position.x)
			{
				robotPosition.x = gameManager.rightBound.position.x;
			}
			RaycastHit2D raycastHit_RobotPosition = Physics2D.Raycast(robotPosition + new Vector3(0.0f, 20.0f, 0.0f), new Vector2(0.0f, -1.0f), 100);
			if (raycastHit_RobotPosition && raycastHit_RobotPosition.collider != null)
			{
				float nextRobotPosY = raycastHit_RobotPosition.point.y - (robotFootPosY_Default.localPosition.y);
				if (Mathf.Abs(robotPosition.y - nextRobotPosY) < 0.2f)
				{
					robotPosition.y = robotPosition.y * 0.7f + nextRobotPosY * 0.3f;
				}
				else
				{
					robotPosition.y = nextRobotPosY;
				}
			}
			robotTransform.position = robotPosition;

			if (_isIKCalculate)
			{
				//Set Aim Helper Bone Position
				robot.SetBonePosition("Helper Aim", _aimPosition, Space.World);

				RaycastHit2D raycastHit_RobotFootL = Physics2D.Raycast(robotFootRaycast_L.position, new Vector2(0.0f, -1.0f), 30);
				RaycastHit2D raycastHit_RobotFootR = Physics2D.Raycast(robotFootRaycast_R.position, new Vector2(0.0f, -1.0f), 30);


				if (raycastHit_RobotFootL && raycastHit_RobotFootL.collider != null)
				{
					robot.SetBonePositionConstraintBySurface("Helper Foot Front L",
																robotFootPosY_Default.position.y,
																raycastHit_RobotFootL.point.y,
																robotFootPosY_Min.position.y,
																robotFootPosY_Max.position.y, ConstraintSurface.Ysurface, Space.World);

					robot.SetBonePositionConstraintBySurface("Helper Foot Back L",
															robotFootPosY_Default.position.y,
															raycastHit_RobotFootL.point.y,
															robotFootPosY_Min.position.y,
															robotFootPosY_Max.position.y, ConstraintSurface.Ysurface, Space.World);
				}

				if (raycastHit_RobotFootR && raycastHit_RobotFootR.collider != null)
				{
					robot.SetBonePositionConstraintBySurface("Helper Foot Front R",
															robotFootPosY_Default.position.y,
															raycastHit_RobotFootR.point.y,
															robotFootPosY_Min.position.y,
															robotFootPosY_Max.position.y, ConstraintSurface.Ysurface, Space.World);

					robot.SetBonePositionConstraintBySurface("Helper Foot Back R",
															robotFootPosY_Default.position.y,
															raycastHit_RobotFootR.point.y,
															robotFootPosY_Min.position.y,
															robotFootPosY_Max.position.y, ConstraintSurface.Ysurface, Space.World);
				}
			}

			// Shoot
			if (Input.GetMouseButton(0))
			{
				if (_isGirlShootable)
				{
					girlAnimator.SetTrigger("Shoot");
					_isGirlShootable = false;
				}				

			}

			// Fire Bombs
			if (Input.GetMouseButton(1))
			{
				if (_isRobotShootable)
				{
					robotAnimator.SetTrigger("Shoot");
					_isRobotShootable = false;
				}
			}
		}

		void OnGirlShoot()
		{
			gameManager.Shoot(girl.GetBoneSocket("Bone Gun").position, _aimPosition);
		}

		void OnGirlShootEnded()
		{
			girlAnimator.ResetTrigger("Shoot");
			girlAnimator.SetTrigger("ShootEnd");
			_isGirlShootable = true;
		}
		void OnRobotBombShoot()
		{
			gameManager.FireBomb(robot.GetBoneSocket("Bone WeaponSocket").position, _aimPosition);
		}

		void OnRobotShootable()
		{
			robotAnimator.ResetTrigger("Shoot");
			_isRobotShootable = true;
		}

		//Check if the character is hit by enemies.
		public bool HitTestByPoint(Vector3 position)
		{
			if (!_isLive)
			{
				return false;
			}

			if (position.x > Mathf.Min(hitBox_LT.position.x, hitBox_RB.position.x)
				&& position.x < Mathf.Max(hitBox_LT.position.x, hitBox_RB.position.x)
				&& position.y > Mathf.Min(hitBox_RB.position.y, hitBox_LT.position.y)
				&& position.y < Mathf.Max(hitBox_RB.position.y, hitBox_LT.position.y))
			{
				_isLive = false;
				girlAnimator.SetBool("IsLive", false);
				return true;
			}
			return false;
		}

		public bool IsLive()
		{
			return _isLive;
		}

	}

}