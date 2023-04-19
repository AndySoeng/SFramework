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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 에디터에서 기본 오브젝트 (RootUnit, Image, Mesh, MeshGroup, Animation, Control Param)의 출력 순서를 알려주는 클래스
	/// 기본적으로 ID+Order의 조합으로만 저장된다.
	/// 동기화 및 정렬이 자동으로 수행된다.
	/// </summary>
	[Serializable]
	public class apObjectOrders
	{
		//Members
		//----------------------------------------------------
		public enum OBJECT_TYPE
		{
			RootUnit = 0,
			Image = 1,
			Mesh = 2,
			MeshGroup = 3,//<<Root만 저장한다.
			AnimClip = 4,
			ControlParam = 5
		}
		[Serializable]
		public class OrderSet
		{
			[SerializeField, NonBackupField]//<백업은 안된다.
			public OBJECT_TYPE _objectType = OBJECT_TYPE.RootUnit;

			[SerializeField, NonBackupField]
			public int _ID = -1;

			[SerializeField, NonBackupField]
			public int _customOrder = -1;

			//실제로 연결된 데이터
			[NonSerialized]
			public int _regOrder = -1;

			[NonSerialized]
			public apRootUnit _linked_RootUnit = null;

			[NonSerialized]
			public apTextureData _linked_Image = null;

			[NonSerialized]
			public apMesh _linked_Mesh = null;

			[NonSerialized]
			public apMeshGroup _linked_MeshGroup = null;

			[NonSerialized]
			public apAnimClip _linked_AnimClip = null;

			[NonSerialized]
			public apControlParam _linked_ControlParam = null;


			[NonSerialized]
			public bool _isExist = false;

			public OrderSet()
			{

			}


			public void SetRootUnit(apRootUnit rootUnit, int regOrder)
			{
				SetData(OBJECT_TYPE.RootUnit, rootUnit._childMeshGroup._uniqueID, regOrder);
				_linked_RootUnit = rootUnit;
			}

			public void SetImage(apTextureData image, int regOrder)
			{
				SetData(OBJECT_TYPE.Image, image._uniqueID, regOrder);
				_linked_Image = image;
			}

			public void SetMesh(apMesh mesh, int regOrder)
			{
				SetData(OBJECT_TYPE.Mesh, mesh._uniqueID, regOrder);
				_linked_Mesh = mesh;
			}

			public void SetMeshGroup(apMeshGroup meshGroup, int regOrder)
			{
				SetData(OBJECT_TYPE.MeshGroup, meshGroup._uniqueID, regOrder);
				_linked_MeshGroup = meshGroup;
			}

			public void SetAnimClip(apAnimClip animClip, int regOrder)
			{
				SetData(OBJECT_TYPE.AnimClip, animClip._uniqueID, regOrder);
				_linked_AnimClip = animClip;
			}

			public void SetControlParam(apControlParam controlParam, int regOrder)
			{
				SetData(OBJECT_TYPE.ControlParam, controlParam._uniqueID, regOrder);
				_linked_ControlParam = controlParam;
			}


			private void SetData(OBJECT_TYPE objectType, int ID, int regOrder)
			{
				_objectType = objectType;
				_ID = ID;
				_regOrder = regOrder;
			}

			public void SetOrder(int order)
			{
				_customOrder = order;
			}

			public string Name
			{
				get
				{
					switch (_objectType)
					{
						case OBJECT_TYPE.RootUnit:
							return "RootUnit " + _regOrder;

						case OBJECT_TYPE.Image:
							return (_linked_Image != null) ? _linked_Image._name : "";

						case OBJECT_TYPE.Mesh:
							return (_linked_Mesh != null) ? _linked_Mesh._name : "";

						case OBJECT_TYPE.MeshGroup:
							return (_linked_MeshGroup != null) ? _linked_MeshGroup._name : "";

						case OBJECT_TYPE.AnimClip:
							return (_linked_AnimClip != null) ? _linked_AnimClip._name : "";

						case OBJECT_TYPE.ControlParam:
							return (_linked_ControlParam != null) ? _linked_ControlParam._keyName : "";
					}

					return "";
				}
			}
		}

		//Order 정보가 저장되는 직렬화되는 변수들

		[SerializeField, NonBackupField]
		private List<OrderSet> _orderSets_RootUnit = new List<OrderSet>();

		[SerializeField, NonBackupField]
		private List<OrderSet> _orderSets_Image = new List<OrderSet>();

		[SerializeField, NonBackupField]
		private List<OrderSet> _orderSets_Mesh = new List<OrderSet>();

		[SerializeField, NonBackupField]
		private List<OrderSet> _orderSets_MeshGroup = new List<OrderSet>();

		[SerializeField, NonBackupField]
		private List<OrderSet> _orderSets_AnimClip = new List<OrderSet>();

		[SerializeField, NonBackupField]
		private List<OrderSet> _orderSets_ControlParam = new List<OrderSet>();

		[NonSerialized]
		private Dictionary<OBJECT_TYPE, List<OrderSet>> _orderSets = new Dictionary<OBJECT_TYPE, List<OrderSet>>();


		//Sync가 한번이라도 되었는가
		[NonSerialized]
		private bool _isSync = false;


		// Init
		//----------------------------------------------------
		public apObjectOrders()
		{
			if(_orderSets_RootUnit == null)
			{
				_orderSets_RootUnit = new List<OrderSet>();
			}

			if(_orderSets_Image == null)
			{
				_orderSets_Image = new List<OrderSet>();
			}

			if(_orderSets_Mesh == null)
			{
				_orderSets_Mesh = new List<OrderSet>();
			}

			if(_orderSets_MeshGroup == null)
			{
				_orderSets_MeshGroup = new List<OrderSet>();
			}

			if(_orderSets_AnimClip == null)
			{
				_orderSets_AnimClip = new List<OrderSet>();
			}

			if(_orderSets_ControlParam == null)
			{
				_orderSets_ControlParam = new List<OrderSet>();
			}

			if(_orderSets == null)
			{
				_orderSets = new Dictionary<OBJECT_TYPE, List<OrderSet>>();
			}
			_orderSets.Clear();

			_orderSets.Add(OBJECT_TYPE.RootUnit, _orderSets_RootUnit);
			_orderSets.Add(OBJECT_TYPE.Image, _orderSets_Image);
			_orderSets.Add(OBJECT_TYPE.Mesh, _orderSets_Mesh);
			_orderSets.Add(OBJECT_TYPE.MeshGroup, _orderSets_MeshGroup);
			_orderSets.Add(OBJECT_TYPE.AnimClip, _orderSets_AnimClip);
			_orderSets.Add(OBJECT_TYPE.ControlParam, _orderSets_ControlParam);

			_isSync = false;
		}

		public void Clear()
		{
			_orderSets_RootUnit.Clear();
			_orderSets_Image.Clear();
			_orderSets_Mesh.Clear();
			_orderSets_MeshGroup.Clear();
			_orderSets_AnimClip.Clear();
			_orderSets_ControlParam.Clear();

			_orderSets.Clear();

			_orderSets.Add(OBJECT_TYPE.RootUnit, _orderSets_RootUnit);
			_orderSets.Add(OBJECT_TYPE.Image, _orderSets_Image);
			_orderSets.Add(OBJECT_TYPE.Mesh, _orderSets_Mesh);
			_orderSets.Add(OBJECT_TYPE.MeshGroup, _orderSets_MeshGroup);
			_orderSets.Add(OBJECT_TYPE.AnimClip, _orderSets_AnimClip);
			_orderSets.Add(OBJECT_TYPE.ControlParam, _orderSets_ControlParam);

			_isSync = false;
		}

		// Functions
		//----------------------------------------------------
		public void Sync(apPortrait portrait)
		{
			//Debug.Log("Sync : " + portrait.name);
			List<apRootUnit> rootUnits = portrait._rootUnits;
			List<apTextureData> images = portrait._textureData;
			List<apMesh> meshes = portrait._meshes;
			List<apMeshGroup> meshGroups = portrait._meshGroups;
			List<apAnimClip> animClips = portrait._animClips;
			List<apControlParam> controlParams = portrait._controller._controlParams;


			//일단 모두 초기화
			foreach (KeyValuePair<OBJECT_TYPE, List<OrderSet>> subOrderSet in _orderSets)
			{
				List<OrderSet> orderSets = subOrderSet.Value;
				for (int i = 0; i < orderSets.Count; i++)
				{
					orderSets[i]._isExist = false;//<<플래그를 세운다. (응?)
				}
			}

			OrderSet existOrderSet = null;
			List<OrderSet> newOrderSets = new List<OrderSet>();

			//1. Root Unit
			apRootUnit curRootUnit = null;
			
			for (int i = 0; i < rootUnits.Count; i++)
			{
				curRootUnit = rootUnits[i];
				if(curRootUnit == null || curRootUnit._childMeshGroup == null)
				{
					continue;
				}

				existOrderSet = _orderSets_RootUnit.Find(delegate(OrderSet a)
				{
					return a._ID == curRootUnit._childMeshGroup._uniqueID;
				});

				if(existOrderSet != null)
				{
					//이미 등록된 OrderSet이다.
					existOrderSet._isExist = true;
					existOrderSet._regOrder = i;
					existOrderSet._linked_RootUnit = curRootUnit;
				}
				else
				{
					//아직 등록되지 않은 OrderSet이다.
					OrderSet newOrderSet = new OrderSet();
					newOrderSet.SetRootUnit(curRootUnit, i);
					newOrderSets.Add(newOrderSet);
				}
			}

			//2. Images
			apTextureData curImage = null;

			for (int i = 0; i < images.Count; i++)
			{
				curImage = images[i];
				if(curImage == null)
				{
					continue;
				}

				existOrderSet = _orderSets_Image.Find(delegate(OrderSet a)
				{
					return a._ID == curImage._uniqueID;
				});

				if(existOrderSet != null)
				{
					//이미 등록된 OrderSet이다.
					existOrderSet._isExist = true;
					existOrderSet._regOrder = i;
					existOrderSet._linked_Image = curImage;
				}
				else
				{
					//아직 등록되지 않은 OrderSet이다.
					OrderSet newOrderSet = new OrderSet();
					newOrderSet.SetImage(curImage, i);
					newOrderSets.Add(newOrderSet);
				}
			}

			//3. Meshes
			apMesh curMesh = null;

			for (int i = 0; i < meshes.Count; i++)
			{
				curMesh = meshes[i];
				if(curMesh == null)
				{
					continue;
				}

				existOrderSet = _orderSets_Mesh.Find(delegate(OrderSet a)
				{
					return a._ID == curMesh._uniqueID;
				});

				if(existOrderSet != null)
				{
					//이미 등록된 OrderSet이다.
					existOrderSet._isExist = true;
					existOrderSet._regOrder = i;
					existOrderSet._linked_Mesh = curMesh;
				}
				else
				{
					//아직 등록되지 않은 OrderSet이다.
					OrderSet newOrderSet = new OrderSet();
					newOrderSet.SetMesh(curMesh, i);
					newOrderSets.Add(newOrderSet);
				}
			}

			//4. MeshGroup
			apMeshGroup curMeshGroup = null;

			for (int i = 0; i < meshGroups.Count; i++)
			{
				curMeshGroup = meshGroups[i];
				if(curMeshGroup == null)
				{
					continue;
				}
				//자식 MeshGroup인 경우 처리하지 않는다.
				if(curMeshGroup._parentMeshGroup != null)
				{
					continue;
				}

				existOrderSet = _orderSets_MeshGroup.Find(delegate(OrderSet a)
				{
					return a._ID == curMeshGroup._uniqueID;
				});

				if(existOrderSet != null)
				{
					//이미 등록된 OrderSet이다.
					existOrderSet._isExist = true;
					existOrderSet._regOrder = i;
					existOrderSet._linked_MeshGroup = curMeshGroup;
				}
				else
				{
					//아직 등록되지 않은 OrderSet이다.
					OrderSet newOrderSet = new OrderSet();
					newOrderSet.SetMeshGroup(curMeshGroup, i);
					newOrderSets.Add(newOrderSet);
				}
			}

			//5. AnimClip
			apAnimClip curAnimClip = null;

			for (int i = 0; i < animClips.Count; i++)
			{
				curAnimClip = animClips[i];
				if(curAnimClip == null)
				{
					continue;
				}

				existOrderSet = _orderSets_AnimClip.Find(delegate(OrderSet a)
				{
					return a._ID == curAnimClip._uniqueID;
				});

				if(existOrderSet != null)
				{
					//이미 등록된 OrderSet이다.
					existOrderSet._isExist = true;
					existOrderSet._regOrder = i;
					existOrderSet._linked_AnimClip = curAnimClip;
				}
				else
				{
					//아직 등록되지 않은 OrderSet이다.
					OrderSet newOrderSet = new OrderSet();
					newOrderSet.SetAnimClip(curAnimClip, i);
					newOrderSets.Add(newOrderSet);
				}
			}

			//6. Control Param
			apControlParam curControlParam = null;

			for (int i = 0; i < controlParams.Count; i++)
			{
				curControlParam = controlParams[i];
				if(curControlParam == null)
				{
					continue;
				}

				existOrderSet = _orderSets_ControlParam.Find(delegate(OrderSet a)
				{
					return a._ID == curControlParam._uniqueID;
				});

				if(existOrderSet != null)
				{
					//이미 등록된 OrderSet이다.
					existOrderSet._isExist = true;
					existOrderSet._regOrder = i;
					existOrderSet._linked_ControlParam = curControlParam;
				}
				else
				{
					//아직 등록되지 않은 OrderSet이다.
					OrderSet newOrderSet = new OrderSet();
					newOrderSet.SetControlParam(curControlParam, i);
					newOrderSets.Add(newOrderSet);
				}
			}

			bool isAnyChanged = false;

			// 연결이 안된 OrderSet 을 삭제
			foreach (KeyValuePair<OBJECT_TYPE, List<OrderSet>> subOrderSet in _orderSets)
			{
				List<OrderSet> orderSets = subOrderSet.Value;
				int nRemoved = orderSets.RemoveAll(delegate(OrderSet a)
				{
					return !a._isExist;
				});

				if(nRemoved > 0)
				{
					isAnyChanged = true;
				}
			}

			//새로 추가될 OrderSet을 추가한다. 이때, Order를 잘 체크하자
			if(newOrderSets.Count > 0)
			{
				isAnyChanged = true;

				OrderSet newOrderSet = null;
				List<OrderSet> targetList = null;
				for (int i = 0; i < newOrderSets.Count; i++)
				{
					newOrderSet = newOrderSets[i];
					targetList = _orderSets[newOrderSet._objectType];

					newOrderSet.SetOrder(targetList.Count);//리스트의 크기만큼의 Order값을 넣자

					targetList.Add(newOrderSet);//<<리스트에 추가!
				}
			}

			if(isAnyChanged)
			{
				//Sort를 하고 CustomOrder를 작성하자
				foreach (KeyValuePair<OBJECT_TYPE, List<OrderSet>> subOrderSet in _orderSets)
				{
					List<OrderSet> orderSets = subOrderSet.Value;
					orderSets.Sort(delegate(OrderSet a, OrderSet b)
					{
						return a._customOrder - b._customOrder;//오름차순
					});

					for (int i = 0; i < orderSets.Count; i++)
					{
						orderSets[i]._customOrder = i;
					}
				}
			}

			//Debug.Log("Root Units : " + _orderSets[OBJECT_TYPE.RootUnit].Count);
			//Debug.Log("Images : " + _orderSets[OBJECT_TYPE.Image].Count);
			//Debug.Log("Meshes : " + _orderSets[OBJECT_TYPE.Mesh].Count);
			//Debug.Log("Mesh Groups : " + _orderSets[OBJECT_TYPE.MeshGroup].Count);
			//Debug.Log("AnimClips : " + _orderSets[OBJECT_TYPE.AnimClip].Count);
			//Debug.Log("Control Params : " + _orderSets[OBJECT_TYPE.ControlParam].Count);

			_isSync = true;
		}

		// Sort
		//---------------------------------------------------------------------
		public void SortByRegOrder()
		{
			foreach (KeyValuePair<OBJECT_TYPE, List<OrderSet>> subOrderSet in _orderSets)
			{
				List<OrderSet> orderSets = subOrderSet.Value;
				orderSets.Sort(delegate (OrderSet a, OrderSet b)
				{
					return a._regOrder - b._regOrder;
				});
			}

			//Debug.Log("Sort By Reg Order");
		}
		
		public void SortByAlphaNumeric()
		{

			foreach (KeyValuePair<OBJECT_TYPE, List<OrderSet>> subOrderSet in _orderSets)
			{
				List<OrderSet> orderSets = subOrderSet.Value;
				orderSets.Sort(delegate (OrderSet a, OrderSet b)
				{
					return string.Compare(a.Name, b.Name);
				});
			}

			//Debug.Log("Sort By AlphaNumeric");
		}

		public void SortByCustom()
		{
			foreach (KeyValuePair<OBJECT_TYPE, List<OrderSet>> subOrderSet in _orderSets)
			{
				List<OrderSet> orderSets = subOrderSet.Value;
				orderSets.Sort(delegate (OrderSet a, OrderSet b)
				{
					return a._customOrder - b._customOrder;
				});
			}

			//Debug.Log("Sort By Custom");
		}

		// Change Order
		//------------------------------------------------------------
		public bool ChangeOrder(apPortrait portrait, OBJECT_TYPE objectType, int ID, bool isOrderUp)
		{
			//Debug.Log("ChangeOrder : " + objectType + " / " + ID + " / Up : " + isOrderUp);

			//1. 타겟이 있는지 확인
			List<OrderSet> orderSets = _orderSets[objectType];
			OrderSet target = orderSets.Find(delegate(OrderSet a)
			{
				return a._ID == ID;
			});
			if(target == null)
			{
				return false;
			}
			if(objectType == OBJECT_TYPE.MeshGroup)
			{
				//MeshGroup이 없거나 자식 MeshGroup이면 순서를 바꿀 수 없다.
				if(target._linked_MeshGroup == null)
				{
					return false;
				}
				if(target._linked_MeshGroup._parentMeshGroup != null)
				{
					return false;
				}
			}
			
			//Order Up : order 값이 1 줄어든다.
			//Order Down : order 값이 1 증가한다.
			//자리가 바뀔 대상을 찾는다. 
			//단, MeshGroup은 Parent가 없는 것들이어야 한다.

			int prevOrder = target._customOrder;
			int nextOrder = isOrderUp ? (prevOrder - 1) : (prevOrder + 1);

			OrderSet switchTarget = null;
			if(objectType == OBJECT_TYPE.MeshGroup)
			{
				switchTarget = orderSets.Find(delegate(OrderSet a)
				{
					if(a._linked_MeshGroup == null)
					{
						return false;
					}
					if(a._linked_MeshGroup._parentMeshGroup != null)
					{
						return false;
					}
					//MeshGroup이 null이거나 하위 MeshGroup이면 패스

					return a._customOrder == nextOrder && a != target;
				});
			}
			else
			{
				switchTarget = orderSets.Find(delegate(OrderSet a)
				{
					return a._customOrder == nextOrder && a != target;
				});
			}

			if(switchTarget != null)
			{
				//서로의 Order 값을 바꾼다.
				switchTarget._customOrder = prevOrder;
				target._customOrder = nextOrder;

				//Debug.Log("자리 바꾸기 : " + target.Name + " <-> " + switchTarget.Name);
				
				SortByCustom();

				//만약 RootUnit의 경우라면, Portrait에서의 RootUnit 인덱스를 교환할 필요도 있다.
				if(objectType == OBJECT_TYPE.RootUnit)
				{
					portrait._mainMeshGroupIDList.Clear();
					portrait._rootUnits.Clear();

					for (int i = 0; i < orderSets.Count; i++)
					{
						apRootUnit rootUnit = orderSets[i]._linked_RootUnit;
						portrait._mainMeshGroupIDList.Add(rootUnit._childMeshGroup._uniqueID);
						portrait._rootUnits.Add(rootUnit);
					}
					
					
				}
				return true;
			}
			//else
			//{
			//	Debug.LogError("자리 바꾸기 실패 : " + target.Name);
			//}

			return false;
			
		}

		// Get / Set
		//----------------------------------------------------
		public List<OrderSet> RootUnits
		{
			get {  return _orderSets[OBJECT_TYPE.RootUnit]; }
		}

		public List<OrderSet> Images
		{
			get {  return _orderSets[OBJECT_TYPE.Image]; }
		}

		public List<OrderSet> Meshes
		{
			get {  return _orderSets[OBJECT_TYPE.Mesh]; }
		}

		public List<OrderSet> MeshGroups
		{
			get {  return _orderSets[OBJECT_TYPE.MeshGroup]; }
		}

		public List<OrderSet> AnimClips
		{
			get {  return _orderSets[OBJECT_TYPE.AnimClip]; }
		}

		public List<OrderSet> ControlParams
		{
			get {  return _orderSets[OBJECT_TYPE.ControlParam]; }
		}

		public bool IsSync { get { return _isSync; } }
	}
}