using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AI_Chase : _State_Base
{
	[SerializeReference] private _State_Base NextState = default;
	[SerializeField] [Min(0)] private float TargetAcqusitionRange = 5;
	[SerializeField] [Min(0.01f)] private float RangeCheckFequency = 1;

	private Transform Target = null;

	public override HashSet<Type> RequiredComponents => new HashSet<System.Type>() { typeof(NavMeshAgent) };
	public override bool IsValid => NextState != null;
	public override _State_Base GetNextState() => NextState;

	public override void OnStateEnter() { }
	public override void OnStateExit() => StateMachineManager.GetRequiredComponent<NavMeshAgent>().ResetPath();


	private void Start() => StartCoroutine(TargetMonitor());
	private IEnumerator TargetMonitor()
	{
		NavMeshAgent NMA = StateMachineManager.GetRequiredComponent<NavMeshAgent>();
		for ( ; ; )
		{
			if (StateMachineManager.CurrentState != this && Target == null)
			{
				IEnumerable<Collider> NPCsInRange = Physics.OverlapSphere(transform.position, TargetAcqusitionRange)
					.Where(C => C.tag == "NPC");
				if (NPCsInRange.Count() > 0)
				{
					Target = NPCsInRange.First().transform;
					StateMachineManager.InterruptCurrentState(this);
				}
			}
			else if (StateMachineManager.CurrentState == this && Target != null)
			{
				if (Vector3.Distance(transform.position, Target.position) > TargetAcqusitionRange)
				{
					Target = null;
					StateMachineManager.ExitCurrentState();
				}
				else
				{
					NMA.SetDestination(Target.position);
				}
			}
			yield return new WaitForSeconds(1 / RangeCheckFequency);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red * 0.75f;
		if (Target == null)
		{
			Gizmos.DrawWireSphere(transform.position, TargetAcqusitionRange);
		}
		else if (Target != null)
		{
			Gizmos.DrawLine(transform.position, Target.position);
		}
	}
}
