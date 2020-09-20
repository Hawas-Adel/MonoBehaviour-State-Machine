using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class _State_Base : MonoBehaviour
{
	protected StateMachineManager StateMachineManager;
	public abstract HashSet<System.Type> RequiredComponents { get; }
	public abstract bool IsValid { get; }

	public abstract void OnStateEnter();
	public abstract void OnStateExit();
	public abstract _State_Base GetNextState();


	protected void Awake()
	{
		StateMachineManager = GetComponentInParent<StateMachineManager>();
		if (StateMachineManager == null)
		{
			Debug.LogWarning($"State [{name}] is without a [StatMachineManager] parent, Destroying state");
			Destroy(gameObject);
		}
	}
}
