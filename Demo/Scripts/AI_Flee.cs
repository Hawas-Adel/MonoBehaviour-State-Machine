using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AI_Flee : _State_Base
{
	[SerializeReference] private _State_Base ExitState = default;
	[SerializeField] [Min(0)] private float EnemyDetectionRange = 5;

	public override HashSet<Type> RequiredComponents => new HashSet<Type> { typeof(NavMeshAgent) };
	public override bool IsValid => ExitState != null;
	public override _State_Base GetNextState() => ExitState;

	private Coroutine FleeCOR;
	public override void OnStateEnter() => FleeCOR = StartCoroutine(Flee());
	public override void OnStateExit()
	{
		StateMachineManager.GetRequiredComponent<NavMeshAgent>().ResetPath();
		StopCoroutine(FleeCOR);
	}

	private Coroutine EnemyMonitorCOR;
	private void OnEnable() => EnemyMonitorCOR = StartCoroutine(EnemyMonitor());
	private void OnDisable() => StopCoroutine(EnemyMonitorCOR);
	private IEnumerator EnemyMonitor()
	{
		for ( ; ; )
		{
			IEnumerable<Collider> _EnemiesInRange = Physics.OverlapSphere(transform.position, EnemyDetectionRange)
				.Where(Coll => Coll.tag == "Enemy");
			if (_EnemiesInRange.Count() > 0)
			{
				EnemiesInRange = _EnemiesInRange.ToList().ConvertAll(coll => (coll.transform.position - StateMachineManager.transform.position).normalized);
				StateMachineManager.InterruptCurrentState(this);
			}
			else
			{
				EnemiesInRange = null;
				if (StateMachineManager.CurrentState == this)
				{
					StateMachineManager.ExitCurrentState();
				}
			}
			yield return new WaitForSeconds(0.5f);
		}
	}
	private IEnumerable<Vector3> EnemiesInRange;
	private IEnumerator Flee()
	{
		NavMeshAgent NMA = StateMachineManager.GetRequiredComponent<NavMeshAgent>();
		for ( ; ; )
		{
			if (EnemiesInRange != null)
			{
				Vector3 GeneralEnemiesDir = EnemiesInRange.Aggregate((V1, V2) => V1 + V2.normalized);
				GeneralEnemiesDir.y = 0;
				NMA.SetDestination(StateMachineManager.transform.position - 10 * GeneralEnemiesDir);
			}
			yield return new WaitForSeconds(0.25f);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red * 0.75f;
		Gizmos.DrawWireSphere(transform.position, EnemyDetectionRange);
	}
}
