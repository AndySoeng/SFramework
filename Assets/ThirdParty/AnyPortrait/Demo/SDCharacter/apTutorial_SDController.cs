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

	public class apTutorial_SDController : MonoBehaviour
	{
		// Target AnyPortrait
		public apPortrait portrait;

		void Start()
		{

		}

		void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (portrait.IsPlaying("Idle"))
				{
					portrait.StopAll(0.3f);
				}
				else
				{
					portrait.CrossFade("Idle", 0.3f);
				}
			}
		}
	}
}