using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Work : _State_Base
{
	[SerializeReference] private _State_Base NextState = default;

	[SerializeReference] private List<Transform> WorkRefrences = default;
	[SerializeField] [Min(0)] private float TotalWorkTime = 15;
	[SerializeField] [Min(0)] private float WorkTimePerItem = 2;


	public override HashSet<System.Type> RequiredComponents => new HashSet<System.Type> { typeof(NavMeshAgent) };
	public override bool IsValid => WorkRefrences.Count > 0;
	public override _State_Base GetNextState() => NextState;

	private Coroutine WorkCOR;
	public override void OnStateEnter() => WorkCOR = StartCoroutine(Work());
	public override void OnStateExit() => StopCoroutine(WorkCOR);

	private IEnumerator Work()
	{
		NavMeshAgent NMA = StateMachineManager.GetRequiredComponent<NavMeshAgent>();
		float EndTime = Time.time + TotalWorkTime;
		for ( ; ; )
		{
			Transform WorkREF = WorkRefrences[Random.Range(0, WorkRefrences.Count)];
			NMA.SetDestination(WorkREF.position);

			while (NMA.remainingDistance > NMA.stoppingDistance)
				yield return new WaitForSeconds(0.5f);

			yield return new WaitForSeconds(WorkTimePerItem);

			if (Time.time >= EndTime)
				StateMachineManager.ExitCurrentState();
		}
	}
}
