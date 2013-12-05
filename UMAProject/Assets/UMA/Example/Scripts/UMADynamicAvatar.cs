using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;
using LitJson;
using System.Collections;
using System.Collections.Generic;
using UMA;

public class UMADynamicAvatar : MonoBehaviour {

	public UMAContext context;
	public UMAData umaData;
	public UMARecipeBase umaRecipe;
	public bool loadOnStart;
	public RuntimeAnimatorController animationController;
	[NonSerialized]
	public GameObject umaChild;

	public void Start()
	{
		Initialize();
		if (loadOnStart)
		{
			Load(umaRecipe);
		}
	}
	public void Initialize()
	{
		if (context == null)
		{
			context = UMAContext.FindInstance();
		}
		
		#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
			{
				context.UpdateDictionaries();
			}
		#endif

		if (umaData == null)
		{
			umaData = GetComponent<UMAData>();
			if (umaData == null)
			{
				umaData = gameObject.AddComponent<UMAData>();

				#if UNITY_EDITOR
					if( !UnityEditor.EditorApplication.isPlaying )
					{
						umaData.umaRecipe = new UMAData.UMARecipe();
						umaData.atlasList = new UMAData.AtlasList();
					}
				#endif
			}
		}
	}

	public void Load(UMARecipeBase umaRecipe)
	{
		var oldRace = umaData.umaRecipe.raceData;
		this.umaRecipe = umaRecipe;
		umaRecipe.Load(umaData, context);
		if (oldRace != umaData.umaRecipe.raceData)
		{
			UpdateNewRace();
		}
		else
		{
			UpdateSameRace();
		}
	}

	public void UpdateSameRace()
	{
#if UNITY_EDITOR
		if (UnityEditor.EditorApplication.isPlaying)
		{
			umaData.Dirty(true, true, true);
		}
#else
			umaData.Dirty(true, true, true);
#endif
	}

	public void UpdateNewRace()
	{
		var position = transform.position;
		var rotation = transform.rotation;
		if (umaChild != null)
		{
			umaData.cleanMesh(false);
			umaData.firstBake = true;
			position = umaChild.transform.position;
			rotation = umaChild.transform.rotation;
			Destroy(umaChild);
		}
		umaChild = Instantiate(umaData.umaRecipe.raceData.racePrefab) as GameObject;
		umaChild.transform.parent = transform;
		umaChild.transform.position = position;
		umaChild.transform.rotation = rotation;
		UMAData newUMA = umaChild.GetComponentInChildren<UMAData>();
#if UNITY_EDITOR
		if (!UnityEditor.EditorApplication.isPlaying)
		{
			newUMA.SetupOnAwake();
		}
#endif
		umaData.animator = newUMA.animator;
		umaData.myRenderer = newUMA.myRenderer;
		umaData.atlasResolutionScale = 1f;
		umaData.updateBoneList = newUMA.updateBoneList;
		umaData.boneList = newUMA.boneList;
		umaData.umaRoot = newUMA.gameObject;
		umaData.animationController = animationController ?? newUMA.animationController;

		newUMA.animator = null;
		DestroyImmediate(newUMA);

#if UNITY_EDITOR
		if (UnityEditor.EditorApplication.isPlaying)
		{
			umaData.Dirty(true, true, true);
		}
#else
			umaData.Dirty(true, true, true);
#endif
	}
}