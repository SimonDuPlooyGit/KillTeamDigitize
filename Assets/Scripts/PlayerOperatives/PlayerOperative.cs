using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
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

    public bool hasRepositionedThisActivation = false;
    public bool hasDashedThisActivation = false;
    public bool hasChargedThisActivation = false;
    public bool hasShotThisActivation = false; //This needs to be tweaked because they can all shoot twice but a bolt weapon needed to be used
    public bool validMoveMarkerPosition = true;

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

    //Click the board and then calculate a path to that spot limited by movement distance, move the move marker there
    public void ClickToPathfindReposition()
    {
        // Do not pathfind if clicking on UI elements/buttons
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return; 
        }
        
        if (selected)
        {
            Debug.Log("ClickToPathfindReposition is entered and unit is selected");
            _limitedPoints.Clear();
            _currentPathDistance = 0f;
            lineRenderer.enabled = false;
            _pathDrawn = false;
            moveMarker.SetActive(true);

            movementInfo.GetComponent<TextMeshProUGUI>().text =
                $"{Math.Round(remainingMovement * 39.37f / 10)}/{movementStat}\""; //The display on top of the move marker

            if (remainingMovement <= 0.05f)
            {
                Debug.Log($"No remaining movement: {remainingMovement}");
                return;
            }

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100) &&
                selected && hit.transform.CompareTag("Board"))
            {
                Vector3 rawDestination = LimitPathDistance(hit.point, remainingMovement, out _limitedPoints, out _currentPathDistance);
                Vector3 finalDestination = GetControlRangeSafeDestination(rawDestination);
                _moveMarkerAgent.destination = finalDestination;
            }

            float projectedSpentMeters = (meterMovement - remainingMovement) + _currentPathDistance;
            movementInfo.GetComponent<TextMeshProUGUI>().text =
                $"{Math.Round(projectedSpentMeters * 39.37f / 10)}/{movementStat}\"";
        }
    }
    
    //Wait for the move marker to reach its destination to check if an enemy is in control range
    private Coroutine _moveMarker;
    public void ClickToPathfindCharge()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return; 

        if (selected)
        {
            //Debug.Log("ClickToPathfindCharge is entered and unit is selected");
            _limitedPoints.Clear();
            _currentPathDistance = 0f;
            lineRenderer.enabled = false;
            _pathDrawn = false;
            moveMarker.SetActive(true);

            movementInfo.GetComponent<TextMeshProUGUI>().text =
                $"{Math.Round(_currentPathDistance * 39.37f / 10)}/{movementStat + 2}\"";

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100) &&
                selected && hit.transform.CompareTag("Board"))
            {
                Vector3 finalDestination = LimitPathDistance(hit.point, chargeDistanceScaled, out _limitedPoints, out _currentPathDistance);
                _moveMarkerAgent.destination = finalDestination;

                StartMarkerCheckCoroutine(PlayerMovementType.Charge);
            }
            movementInfo.GetComponent<TextMeshProUGUI>().text =
                $"{Math.Round(_currentPathDistance * 39.37f / 10)}/{movementStat + 2}\"";
        }
    }

    //Same as reposition pathfind method but restricted to 3 inches
    public void ClickToPathfindDash()
    {
        // Do not pathfind if clicking on UI elements/buttons
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return; 
        }
        
        if (selected)
        {
            Debug.Log("ClickToPathfindReposition is entered and unit is selected");
            _limitedPoints.Clear();
            _currentPathDistance = 0f;
            lineRenderer.enabled = false;
            _pathDrawn = false;
            moveMarker.SetActive(true);

            movementInfo.GetComponent<TextMeshProUGUI>().text =
                $"{Math.Round(_currentPathDistance * 39.37f / 10)}/{dashDistanceInches}\"";

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit, 100) &&
                selected && hit.transform.CompareTag("Board"))
            {
                Vector3 rawDestination = LimitPathDistance(hit.point, dashDistanceScaled, out _limitedPoints, out _currentPathDistance);
                Vector3 finalDestination = GetControlRangeSafeDestination(rawDestination);
                _moveMarkerAgent.destination = finalDestination;
            }
            
            movementInfo.GetComponent<TextMeshProUGUI>().text =
                $"{Math.Round(_currentPathDistance * 39.37f / 10)}/{dashDistanceInches}\"";
        }
    }

    //Start the coroutine to wait for the marker to reach its final destination
    private void StartMarkerCheckCoroutine(PlayerMovementType movementType)
    {
        if (_moveMarker != null)
        {
            StopCoroutine(_moveMarker);
        }
        _moveMarker = StartCoroutine(WaitForMarker(movementType));
    }

    //Coroutine to wait for the marker
    private IEnumerator WaitForMarker(PlayerMovementType movementType)
    {
        yield return new WaitUntil(() => !_moveMarkerAgent.pathPending);

        while (_moveMarkerAgent.remainingDistance > _moveMarkerAgent.stoppingDistance ||
               _moveMarkerAgent.velocity.sqrMagnitude > 0.01f)
        {
            yield return null;
        }
        
        MoveMarkerSphereCheck sphereCheck = moveMarker.GetComponent<MoveMarkerSphereCheck>();
        if (sphereCheck != null)
        {
            bool enemyInControlRange = sphereCheck.CheckEnemyInControlRange(gameObject.tag);

            if (movementType == PlayerMovementType.Charge)
            {
                validMoveMarkerPosition = enemyInControlRange;
                if (validMoveMarkerPosition)
                {
                    Debug.Log("Charge here ends in control range");
                }
                else
                {
                    Debug.Log("Charge here does not end in control range");
                }
            }
            else
            {
                validMoveMarkerPosition = !enemyInControlRange;
                if (!validMoveMarkerPosition)
                {
                    Debug.Log("Dash Or Reposition can't end within control range of NPO");
                }
            }
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
    public bool MoveOperativeToMarker()
    {
        if (!validMoveMarkerPosition)
        {
            Debug.Log("MoveMarker is invalid");
            return false;
        }
        
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

        return true;
    }

    public override void ResetForNewTurningPoint()
    {
        base.ResetForNewTurningPoint();
        remainingMovement = meterMovement;
        hasRepositionedThisActivation = false;
        hasShotThisActivation = false;
        ResetMoveMarker();
    }
    
    public override void TurnFinished()
    {
        base.TurnFinished();
        if (!hasRepositionedThisActivation)
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
    
    //Only allow safe pathfinding outside control ranges
    private Vector3 GetControlRangeSafeDestination(Vector3 destination)
    {
        float controlRange = 0.414f;
        float minSafeDistance = 0.6f;
        
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(transform.position, destination, _operativeAgent.areaMask, path) &&
            path.corners.Length > 0)
        {
            Vector3 finalPoint = path.corners[path.corners.Length - 1];

            foreach (var npo in FindFirstObjectByType<GameManager>().SharedContext.AllNpoUnits)
            {
                if (npo == null || npo.dead || !npo.gameObject.activeInHierarchy) continue;
                Vector3 npoPos = npo.transform.position;
                float distance = Vector3.Distance(finalPoint, npoPos);

                if (distance < controlRange)
                {
                    Debug.Log($"{name}'s movement destination ends inside control range of {npo.name}. Adjusting to stay safe");

                    Vector3 awayFromNpo = (finalPoint - npoPos);
                    awayFromNpo.y = 0;

                    if (awayFromNpo.magnitude < 0.001f)
                    {
                        awayFromNpo = (transform.position - npoPos);
                        awayFromNpo.y = 0;
                    }
                    
                    awayFromNpo = awayFromNpo.normalized;
                    Vector3 safeCandidate = npoPos + (awayFromNpo * minSafeDistance);
                    
                    if (NavMesh.SamplePosition(safeCandidate, out NavMeshHit hit, 1.0f, _operativeAgent.areaMask))
                    {
                        return hit.position;
                    }
                }
            }
            return finalPoint;
        }
        return destination;
    }
    
    public void Reset()
    {
        TurnFinished();
    }
}