﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneManagement {
	public static partial class SceneLoaderUtility {
		
		/// <summary>
		/// Loads a single scene.
		/// </summary>
		/// <param name="scene">The scene name.</param>
		/// <param name="onFinished">Optional callback action.</param>
		public static IEnumerator CoroutineLoadSceneAsync(string scene, Action onFinished = null) {
			//Block flow if the scene does not exist
			if(!Application.CanStreamedLevelBeLoaded(scene)) {
				if(LogLevel >= LogType.Less)
					Debug.LogWarning(Tag + "The scene \"" + scene + "\" cannot be found or does not exist.");
				yield break;
			}

			AsyncOperation op = SceneManager.LoadSceneAsync(scene);
			
			//Wait until the current scene is loaded
			while(op.progress < 0.9f) {
				yield return null;
			}
			//Debug
			//yield return new WaitForSeconds(1f);

			onFinished?.Invoke();
		}
		
		/// <summary>
		/// Loads in an array of scenes additively.
		/// </summary>
		/// <param name="scenes">The array of scene names.</param>
		/// <param name="onFinished">Optional callback action.</param>
		/// <param name="duplicateScenes">Should duplicate scenes be allowed. False by default.</param>
		public static IEnumerator CoroutineLoadScenesAdditive(string[] scenes, Action onFinished = null, bool duplicateScenes = false) {
			AsyncOperation[] operations = new AsyncOperation[scenes.Length];

			//Step 1: List all of operations
			for(var i = 0; i < scenes.Length; i++) {
				string scene = scenes[i];
				
				//Block flow if the scene does not exist
				if(!Application.CanStreamedLevelBeLoaded(scene)) {
					if(LogLevel >= LogType.Less)
						Debug.LogWarning(Tag + "The scene \"" + scene + "\" cannot be found or does not exist.");
					continue;
				}

				if(!duplicateScenes) {
					//Block flow if the scene has already been loaded
					Scene sceneObj = SceneManager.GetSceneByName(scene);
					if(sceneObj.isLoaded) {
						if(LogLevel >= LogType.All)
							Debug.LogWarning(Tag + "The scene \"" + scene + "\" has already been loaded.");
						continue;
					}
				}

				//Start loading the scene
				AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
				op.allowSceneActivation = false;
				operations[i] = op;

				//Wait until the current scene is loaded but not activated
				while(op.progress < 0.9f) {
					yield return null;
				}
				//Debug
				//yield return new WaitForSeconds(1f);
			}

			//Step 2: Activate all of the operations at once
			foreach(AsyncOperation op in operations) {
				if(op == null) continue;
				
				op.allowSceneActivation = true;

				while(!op.isDone) {
					yield return null;
				}
				//Debug
				//yield return new WaitForSeconds(1f);
			}
			
			onFinished?.Invoke();
		}
		
		public static IEnumerator CoroutineUnloadScene(string[] scenes, Action onFinished = null) {
			foreach(string scene in scenes) {
				//Block flow if the scene does not exist
				if(!Application.CanStreamedLevelBeLoaded(scene)) {
					if(LogLevel >= LogType.Less)
						Debug.LogWarning(Tag + "The scene \"" + scene + "\" cannot be found or does not exist.");
					continue;
				}

				AsyncOperation op = SceneManager.UnloadSceneAsync(scene);

				while(!op.isDone) {
					yield return null;
				}
				//Debug
				//yield return new WaitForSeconds(1f);
			}
			
			onFinished?.Invoke();
		}
	
	}
}