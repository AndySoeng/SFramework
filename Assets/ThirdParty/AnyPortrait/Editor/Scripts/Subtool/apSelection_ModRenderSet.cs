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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AnyPortrait
{

	//Selection의 ModRenderVert 등의 서브 클래스를 정의한 코드
	public partial class apSelection
	{
		//------------------------------------------------
		// Mod Vert + Render Vert = ModRenderVert
		//------------------------------------------------

		/// <summary>
		/// Mod Vert와 Render Vert는 동시에 선택이 된다.
		/// </summary>
		public class ModRenderVert
		{
			public apModifiedVertex _modVert = null;
			public apRenderVertex _renderVert = null;
			//추가
			//ModVert가 아니라 ModVertRig가 매칭되는 경우도 있다.
			//Gizmo에서 주로 사용하는데 에러 안나게 주의할 것
			public apModifiedVertexRig _modVertRig = null;

			public apModifiedVertexWeight _modVertWeight = null;


			/// <summary>
			/// SoftSelection, Blur, Volume등의 "편집 과정에서의 Weight"를 임시로 결정하는 경우의 값
			/// </summary>
			public float _vertWeightByTool = 1.0f;

			public ModRenderVert()
			{
				_modVert = null;
				_modVertRig = null;
				_modVertWeight = null;

				_renderVert = null;
				_vertWeightByTool = 1.0f;
			}

			public ModRenderVert(apModifiedVertex modVert, apRenderVertex renderVert)
			{
				_modVert = modVert;
				_modVertRig = null;
				_modVertWeight = null;

				_renderVert = renderVert;
				_vertWeightByTool = 1.0f;

			}

			public ModRenderVert(apModifiedVertexRig modVertRig, apRenderVertex renderVert)
			{
				_modVert = null;
				_modVertRig = modVertRig;
				_modVertWeight = null;

				_renderVert = renderVert;
				_vertWeightByTool = 1.0f;
			}

			public ModRenderVert(apModifiedVertexWeight modVertWeight, apRenderVertex renderVert)
			{
				_modVert = null;
				_modVertRig = null;
				_modVertWeight = modVertWeight;

				_renderVert = renderVert;
				_vertWeightByTool = _modVertWeight._weight;//<<이건 갱신해야할 것
			}

			//다음 World 좌표값을 받아서 ModifiedVertex의 값을 수정하자
			public void SetWorldPosToModifier_VertLocal(Vector2 nextWorldPos)
			{
				//NextWorld Pos에서 -> [VertWorld] -> [MeshTransform] -> Vert Local 적용 후의 좌표 -> Vert Local 적용 전의 좌표 
				//적용 전-후의 좌표 비교 = 그 차이값을 ModVert에 넣자
				//이전
				//apMatrix3x3 matToAfterVertLocal = (_renderVert._matrix_Cal_VertWorld * _renderVert._matrix_MeshTransform).inverse;

				//변경 21.4.5 : 리깅 적용 (World 중 "Rigging > MeshTF > VertWorld")
				//이전
				//apMatrix3x3 matToAfterVertLocal = (_renderVert._matrix_Cal_VertWorld 
				//									* _renderVert._matrix_MeshTransform 
				//									* _renderVert._matrix_Rigging).inverse;

				//변경 22.5.9 (v1.4.0)
				apMatrix3x3 matToAfterVertLocal = _renderVert._matrix_Rigging_MeshTF_VertWorld.inverse;
				
				Vector2 nextLocalMorphedPos = matToAfterVertLocal.MultiplyPoint(nextWorldPos);
				
				//이전
				//Vector2 beforeLocalMorphedPos = (_renderVert._matrix_Cal_VertLocal * _renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(_renderVert._vertex._pos);

				//변경 22.5.9 (v1.4.0)
				Vector2 beforeLocalMorphedPos = _renderVert._deltaPos_Vert2Mesh_VertLocal + _renderVert._vertex._pos;



				_modVert._deltaPos.x += (nextLocalMorphedPos.x - beforeLocalMorphedPos.x);
				_modVert._deltaPos.y += (nextLocalMorphedPos.y - beforeLocalMorphedPos.y);
			}
		}

		//------------------------------------------------------------------------
		// Mod Pin + Render Pin = ModRenderPin
		//------------------------------------------------------------------------
		public class ModRenderPin
		{
			public apModifiedPin _modPin = null;
			public apRenderPin _renderPin = null;

			/// <summary>
			/// Soft Selection, Blur 등의 편집 과정의 Weight를 임시로 저장하는 변수
			/// </summary>
			public float _pinWeightByTool = 1.0f;

			public ModRenderPin()
			{
				_modPin = null;
				_renderPin = null;
				_pinWeightByTool = 1.0f;
			}

			/// <summary>
			/// World 좌표값을 받아서 Modified Pin의 값을 수정하자
			/// </summary>
			/// <param name="nextWorldPos"></param>
			public void SetWorldToModifier_PinLocal(Vector2 nextWorldPos)
			{
				//이동 이후의 "Default + Morph" 상태의 값을 계산하자
				//Vector2 localMorphedPos_Next = _renderPin._matrix_MeshTransform.inverse.MultiplyPoint(nextWorldPos);				
				Vector2 localMorphedPos_Next = _renderPin._parentRenderUnit._matrix_TF_Inv.MultiplyPoint(nextWorldPos);

				//Vector2 localMorphedPos_Prev = (_renderPin._matrix_Cal_PinLocal * _renderPin._matrix_Static_Vert2Mesh).MultiplyPoint(_renderPin._srcPin._defaultPos);
				Vector2 localMorphedPos_Prev = (_renderPin._deltaPos_Pin2Mesh_PinLocal + _renderPin._srcPin._defaultPos);

				Vector2 deltaPos = localMorphedPos_Next - localMorphedPos_Prev;
				_modPin._deltaPos.x += deltaPos.x;
				_modPin._deltaPos.y += deltaPos.y;
			}

		}
	}
}