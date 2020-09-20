using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_RandomRoaming : _State_Base
{
	[SerializeField] [Min(0)] private float RoamingRadius = default;
	[SerializeField] private Vector2 WaitTime = default;

	public override HashSet<System.Type> RequiredComponents => new HashSet<System.Type>() { typeof(NavMeshAgent) };
	public override bool IsValid => true;
	public override _State_Base GetNextState() => throw new System.NotImplementedException();

	private Coroutine RoamingCOR;
	public override void OnStateEnter() => RoamingCOR = StartCoroutine(Roaming());
	public override void OnStateExit() => StopCoroutine(RoamingCOR);

	private IEnumerator Roaming()
	{
		NavMeshAgent NMA = StateMachineManager.GetRequiredComponent<NavMeshAgent>();
		for ( ; ; )
		{
			Vector3 RandomDir = Random.insideUnitSphere;
			RandomDir.y = 0;
			NMA.SetDestination(NMA.transform.TransformPoint(RandomDir * RoamingRadius));
			while (NMA.remainingDistance > NMA.stoppingDistance)
			{
				yield return new WaitForSeconds(0.5f);
			}
			yield return new WaitForSeconds(Random.Range(WaitTime.x, WaitTime.y));
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green * 0.75f;
		Gizmos.DrawWireSphere(transform.parent.parent.position, RoamingRadius);
	}
}
