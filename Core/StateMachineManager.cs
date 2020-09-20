using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StateMachineManager : MonoBehaviour
{
	[SerializeField] private _State_Base _CurrentState;
	/// <summary>
	/// Refrence to the currently active state
	/// </summary>
	public _State_Base CurrentState
	{
		get => _CurrentState;
		private set
		{
			_CurrentState = value;
			OnStateChange?.Invoke();
		}
	}
	public UnityEvent OnStateChange;

	private Dictionary<System.Type, Component> ParentComponents = new Dictionary<System.Type, Component>();

	private void Awake()
	{
		//Validation Of <Starting State>, <Child States>, <Required Components from Parent>
		if (!ValidateStartingState() || !ValidateStates() || !ValidateRequiredComponentsOnParent())
		{ gameObject.SetActive(false); return; }
	}
	private void Start() => EnterState(CurrentState);

	private bool ValidateStartingState()
	{
		if (CurrentState == null)
		{
			Debug.LogError($"{name} has no Default Starting State");
			return false;
		}
		return true;
	}
	private bool ValidateStates()
	{
		IEnumerable<_State_Base> InvalidStates = GetComponentsInChildren<_State_Base>().
			Where(S => S.IsValid == false);
		if (InvalidStates.Count() > 0)
		{
			Debug.LogError($"States {InvalidStates.ToList().ConvertAll(S => S.name).Aggregate((s1, s2) => $"{s1}, {s2}")} on {transform.parent.name} are Invalid");
			return false;
		}
		return true;
	}
	private bool ValidateRequiredComponentsOnParent()
	{
		IEnumerable<System.Type> RequiredComponents = GetComponentsInChildren<_State_Base>().ToList()
			.ConvertAll(S => S.RequiredComponents).Aggregate((RC1, RC2) => { RC1.UnionWith(RC2); return RC1; });
		List<System.Type> MissingRequiredComponents = new List<System.Type>();

		foreach (System.Type item in RequiredComponents)
		{
			if (item.IsSubclassOf(typeof(Component)))
			{
				if (transform.parent.TryGetComponent(item, out Component C))
				{ ParentComponents.Add(item, C); }
				else
				{ MissingRequiredComponents.Add(item); } 
			}
		}

		if (MissingRequiredComponents.Count > 0)
		{
			Debug.LogError($"Components {MissingRequiredComponents.ConvertAll(S => S.Name).Aggregate((s1, s2) => $"{s1}, {s2}")} couldn't be found on {transform.parent.name} but They are required by it's AI States");
			return false;
		}
		return true;
	}

	/// <summary>
	/// Get a component that was marked as required by attached states,
	/// the StateMachineManager builds a cache of these components for quick retreval
	/// </summary>
	/// <typeparam name="T">Type of required component</typeparam>
	/// <returns>refrence to component</returns>
	public T GetRequiredComponent<T>() where T : Component
	{
		if (ParentComponents.Keys.Contains(typeof(T))) { return (T)ParentComponents[typeof(T)]; }
		else { Debug.LogWarning($"Type {typeof(T).Name} was not listed as a required component"); return null; }
	}

	private void EnterState(_State_Base S)
	{
		CurrentState = S;
		CurrentState.OnStateEnter();
	}

	/// <summary>
	/// Leave current state and move to the next state it points to
	/// </summary>
	public void ExitCurrentState()
	{
		CurrentState.OnStateExit();
		EnterState(CurrentState.GetNextState());
	}
	/// <summary>
	/// Break the normal flow of the state machine leaving current state an jumping to the provided state
	/// </summary>
	/// <param name="InterruptingState">state the becomes the active state</param>
	public void InterruptCurrentState(_State_Base InterruptingState)
	{
		CurrentState.OnStateExit();
		EnterState(InterruptingState);
	}



#if UNITY_EDITOR
	[MenuItem("GameObject/[State Machine Manager]", false, 10)]
	private static void CreateCustomGameObject(MenuCommand menuCommand)
	{
		// Create a custom game object
		GameObject go = new GameObject("[State Machine Manager]");
		// Ensure it gets reparented if this was a context click (otherwise does nothing)
		GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
		go.AddComponent<StateMachineManager>();
		// Register the creation in the undo system
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}
#endif
}
