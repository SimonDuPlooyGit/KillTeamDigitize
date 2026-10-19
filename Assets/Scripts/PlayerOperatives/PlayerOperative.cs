using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using TMPro;

//-----PrototypeUnit Class (Specific functionality for the player units, inherits BaseOperativeClass)
public class PlayerOperative : BaseOperativeClass
{
    [Header("Navmesh and moving info")]
    public GameObject moveMarker;
    public float remainingMovement;
    public GameObject movementInfo;

    private NavMeshAgent _operativeAgent;
    private NavMeshAgent _moveMarkerAgent;
    public NavMeshPath Path;
    private bool _pathDrawn;
    List<Vector3> _limitedPoints = new List<Vector3>();
    private float _currentPathDistance;

    public bool hasMovedThisActivation = false;
    public bool hasShotThisActivation = false;

    protected override void Awake()
    {
        base.Awake();
        acted = false;
        
        remainingMovement = meterMovement;
        _operativeAgent = GetComponent<NavMeshAgent>();
        _moveMarkerAgent = moveMarker.GetComponent<NavMeshAgent>();
        Path = new NavMeshPath();
        moveMarker.SetActive(false);

        //Find UI for this operative
        healthFill = transform.Find("UnitUI/HealthBar/HealthFill").gameObject;
        movementInfo = transform.Find("MoveMarker/MarkerCanvas/DistanceMarker/DistanceText").gameObject;
        aplCount = transform.Find("UnitUI/APL/APLNumber").gameObject;

        SetHealth();
    }

    protected override void Update()
    {
        if (currentApl == 0)
        {
            acted = true;
        }
        
        base.Update();

        if (selected)
        {
            if (moveMarker.activeSelf && !_pathDrawn && !_moveMarkerAgent.pathPending &&
                _moveMarkerAgent.velocity.sqrMagnitude < 0.01f)
            {
                DrawPath(_limitedPoints.ToArray());
            }
        }
    }

    public void UpdatePathDrawing()
    {
        if (selected)
        {
            if (moveMarker.activeSelf &&
                !_pathDrawn &&
                !_moveMarkerAgent.pathPending &&
                _moveMarkerAgent.velocity.sqrMagnitude < 0.01f &&
                _moveMarkerAgent.remainingDistance <= _moveMarkerAgent.stoppingDistance &&
                selected)
            {
                DrawPath(_limitedPoints.ToArray());
            }
        }
    }

    //Click the board and then calculate a path to that spot limited by movement distance
    public void ClickToPathfind()
    {
        // Do not pathfind if clicking on UI elements/buttons
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return; 
        }
        
        if (selected)
        {
            Debug.Log("ClickToPathfind is entered and unit is selected");
            _limitedPoints.Clear();
            _currentPathDistance = 0f;
            lineRenderer.enabled = false;
            _pathDrawn = false;
            moveMarker.SetActive(true);

            movementInfo.GetComponent<TextMeshProUGUI>().text =
                $"{Math.Round(remainingMovement * 39.37f / 10)}/{movementStat}\"";

            if (remainingMovement <= 0.05f)
            {
                Debug.Log($"No remaining movement: {remainingMovement}");
                return;
            }

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100) &&
                selected && hit.transform.CompareTag("Board"))
            {
                Vector3 finalDestination = LimitPathDistance(hit.point, remainingMovement, out _limitedPoints, out _currentPathDistance);
                _moveMarkerAgent.destination = finalDestination;
            }

            float projectedSpentMeters = (meterMovement - remainingMovement) + _currentPathDistance;
            movementInfo.GetComponent<TextMeshProUGUI>().text =
                $"{Math.Round(projectedSpentMeters * 39.37f / 10)}/{movementStat}\"";
        }
    }

    //Draw a path to the destination
    private void DrawPath(Vector3[] points)
    {
        if (selected)
        {
            if (!_pathDrawn)
            {
                lineRenderer.positionCount = points.Length;
                lineRenderer.SetPositions(points);
                lineRenderer.enabled = true;
                _pathDrawn = true;
            }
        }
    }

    //Move the player operative the move marker
    public void MoveOperativeToMarker()
    {
        hasMovedThisActivation = true;
        _operativeAgent.destination = moveMarker.transform.position;
        remainingMovement -= _currentPathDistance;
        _currentPathDistance = 0f;

        if (transform.position == moveMarker.transform.position)
        {
            float spentSoFar = meterMovement - remainingMovement;
            movementInfo.GetComponent<TextMeshProUGUI>().text =
                $"{Math.Round(spentSoFar * 39.37f / 10)}/{movementStat}\"";
            ResetMoveMarker();
        }
    }

    public override void ResetForNewTurningPoint()
    {
        base.ResetForNewTurningPoint();
        remainingMovement = meterMovement;
        hasMovedThisActivation = false;
        hasShotThisActivation = false;
        ResetMoveMarker();
    }
    
    public override void TurnFinished()
    {
        base.TurnFinished();
        if (!hasMovedThisActivation)
        {
            remainingMovement = meterMovement;
        }
        ResetMoveMarker();
    }
    
    //Bring the move marker back to the player and SetActive false
    public void ResetMoveMarker()
    {
        if (moveMarker != null)
        {
            moveMarker.transform.position = transform.position;
            moveMarker.SetActive(false);
        }

        _limitedPoints.Clear();
        _currentPathDistance = 0f;
        _pathDrawn = false;
    }
    
    public void Reset()
    {
        TurnFinished();
    }
}