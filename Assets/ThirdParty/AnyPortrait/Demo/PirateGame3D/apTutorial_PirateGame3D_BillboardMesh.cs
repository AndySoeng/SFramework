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

public class apTutorial_PirateGame3D_BillboardMesh : MonoBehaviour
{
	public Transform cameraTransform;
	public bool isReverseZ = false;
	public MeshRenderer meshRenderer = null;
	public SpriteRenderer spriteRenderer = null;
	// Use this for initialization
	void Start ()
	{
		//meshRenderer = GetComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
		if(meshRenderer != null)
		{
			if(!meshRenderer.enabled)
			{
				return;
			}
		}
		if(spriteRenderer != null)
		{
			if(!spriteRenderer.enabled)
			{
				return;
			}
		}

		if (isReverseZ)
		{
			Vector3 angle =Quaternion.LookRotation(-cameraTransform.position + transform.position, Vector3.up).eulerAngles;
			//transform.rotation = Quaternion.LookRotation(-cameraTransform.position + transform.position, Vector3.up);
			angle.z = 0.0f;
			transform.localRotation = Quaternion.Euler(angle);
		}
		else
		{
			Vector3 angle = Quaternion.LookRotation(cameraTransform.position - transform.position, Vector3.up).eulerAngles;
			angle.z = 0.0f;
			transform.localRotation = Quaternion.Euler(angle);
		}
		
		
	}
	
}
