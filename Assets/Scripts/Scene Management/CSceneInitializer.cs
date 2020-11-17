using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CSceneInitializer : MonoBehaviour 
{
	// MonoBehaviour-Methods
	protected abstract void Awake();


	protected virtual void Start()
	{
		// Whenever a scene is loaded with a scene initializer, the current scene will be finalized and the upcoming scene will get initialized.
		CSceneManager.GetInstance().GetCurrentSceneInitializer().Final();
		CSceneManager.GetInstance().SetCurrentSceneInitializer(this);
		CSceneManager.GetInstance().GetCurrentSceneInitializer().Init();
	}


	// Methods
	public abstract void Init();


	public abstract void Final();
}
