using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_GoHome : _State_Base
{
	[SerializeField] private Transform Home = default;
	[SerializeField] [Min(0)] private float WaitAtHomeTime = 10;

	[SerializeField] private _State_Base NextState = default;

	public override HashSet<Type> RequiredComponents => new HashSet<Type>() { typeof(NavMeshAgent) };
	public override bool IsValid => NextState != null && Home != null;
	public override _State_Base GetNextState() => NextState;

	private Coroutine HeadHomeCOR;
	public override void OnStateEnter() => HeadHomeCOR = StartCoroutine(HeadHome());
	public override void OnStateExit() => StopCoroutine(HeadHomeCOR);

	private IEnumerator HeadHome()
	{
		NavMeshAgent NMA = StateMachineManager.GetRequiredComponent<NavMeshAgent>();
		NMA.SetDestination(Home.position);
		yield return null;
		while (NMA.remainingDistance >= NMA.stoppingDistance)
		{ yield return new WaitForSeconds(0.5f); }
		yield return new WaitForSeconds(WaitAtHomeTime);
		StateMachineManager.ExitCurrentState();
	}
}
