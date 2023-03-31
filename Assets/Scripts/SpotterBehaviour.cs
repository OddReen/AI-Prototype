using Panda;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class SpotterBehaviour : MonoBehaviour
{
    public delegate void OnSpotterAlert(Vector3 targetPos);
    public static event OnSpotterAlert alert;

    [SerializeField] private NavMeshAgent _navMeshAgent;

    [Header("Chase")]
    [SerializeField] private Transform target;

    [Header("Find")]
    [SerializeField] private Vector3 targetLastKnownLocation;

    [Header("Search")]
    [SerializeField] private float timeSearching;
    [SerializeField] private float timeSearchingSpot;
    [SerializeField] private float randomPosDistance;
    [SerializeField] private Vector3 searchingSpot;
    [SerializeField] private float timerSearching;
    [SerializeField] private float timerSearchingSpot;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolLocations;
    [SerializeField] private bool patrolArrived = false;
    [SerializeField] private int patrolLocation = 0;

    [Header("Snipe")]
    [SerializeField] private Transform sniperLocation;

    [Header("Booleans")]
    [SerializeField] private bool inView = false;
    [SerializeField] private bool isAware = false;
    [SerializeField] private bool asArrived = false;

    private void Awake()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player").transform;
        if (_navMeshAgent == null) _navMeshAgent = GetComponent<NavMeshAgent>();
        alert += HearAlert;
    }

    [Task]
    bool InView()
    {
        RaycastHit ray;
        if (Physics.Raycast(transform.position, target.position - transform.position, out ray) && ray.collider.tag == "Player")
        {
            isAware = true;
            inView = true;
            targetLastKnownLocation = target.position;
        }
        else
        {
            inView = false;
        }
        return inView;
    }
    [Task]
    bool IsAware()
    {
        return isAware;
    }
    [Task]
    bool AsArrived()
    {
        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            asArrived = true;
        }
        else
        {
            asArrived = false;
        }
        return asArrived;
    }
    [Task]
    void Alert()
    {
        if (alert != null)
        {
            alert(targetLastKnownLocation);
        }
    }
    void HearAlert(Vector3 targetVector)
    {
        isAware = true;
        targetLastKnownLocation = targetVector;
    }
    [Task]
    void Snipe()
    {
        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            _navMeshAgent.SetDestination(sniperLocation.position);
        }
    }
    [Task]
    void Chase()
    {
        if (target != null)
        {
            _navMeshAgent.SetDestination(targetLastKnownLocation);
        }
    }
    [Task]
    void Find()
    {
        _navMeshAgent.SetDestination(targetLastKnownLocation);
    }
    [Task]
    void Search()
    {
        timerSearching += Time.deltaTime;
        timerSearchingSpot += Time.deltaTime;
        if (timerSearching >= timeSearching) { timerSearching = 0.0f; timerSearchingSpot = 0.0f; isAware = false; ThisTask.Fail(); }
        if (timerSearchingSpot >= timeSearchingSpot)
        {
            timerSearchingSpot = 0.0f;
            searchingSpot = (new Vector3(transform.position.x + Random.insideUnitSphere.x * randomPosDistance, transform.position.y, transform.position.z + Random.insideUnitSphere.z * randomPosDistance));
            _navMeshAgent.SetDestination(searchingSpot);
        }
    }
    [Task]
    void Patrol()
    {
        if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance && !_navMeshAgent.pathPending)
        {
            patrolLocation = (patrolLocation + 1) % patrolLocations.Length;
            _navMeshAgent.SetDestination(patrolLocations[patrolLocation].position);
        }
    }


    private void OnDrawGizmos()
    {
        if (!inView)
        {
            if (_navMeshAgent.destination != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, _navMeshAgent.destination);
            }
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawLine(transform.position, target.position);
    }
}
