using UnityEngine;
using System;

public class WorkerAnimation : MonoBehaviour
{

    private float moveSpeed = 2f; 

    private Worker _worker;
    private Vector3 _startPositon;
   
    private Vector3 _targetPostion;
    private bool _isMoving;
    private System.Func<int, Vector3> _getPlotWorldPosition;
    public Vector3 StartPositon => _startPositon;

    public void Initialize(Worker worker, System.Func<int, Vector3> getPlotWorldPosition)
    {
        _worker = worker;
        moveSpeed = worker.WorkerSpeed;
        _getPlotWorldPosition = getPlotWorldPosition;
        _targetPostion = transform.position;
        _startPositon = transform.position;
        _isMoving = false;
        if(_worker.LastPositon.Z != -1f)  transform.position = Helper.Vector3Converter.ToUnityVector3(_worker.LastPositon);
       
    }

    public void UpdateWorld(Worker worker)
    {
        if (_worker != worker)
        {
            return;
        }

       
        if (!_worker.IsIdle)
        {
            int plotIndex = worker.PlotTaskIndex;
            if (plotIndex >= 0)
            {
                MoveToPlot(plotIndex);
            }
        }
        else
        {
            _targetPostion = _startPositon;
            _isMoving = true;
        }
        _worker.SetWorkerLastPositon(Helper.Vector3Converter.ToSystemVector3(transform.position));
        //// Cập nhật animation
        //if (_isMoving)
        //{
        //    workerAnimator.SetBool("IsMoving", true);
        //    workerAnimator.SetBool("IsWorking", false);
        //}
        //else if (!_worker.IsIdle)
        //{
        //    workerAnimator.SetBool("IsMoving", false);
        //    workerAnimator.SetBool("IsWorking", true);
        //}
        //else
        //{
        //    workerAnimator.SetBool("IsMoving", false);
        //    workerAnimator.SetBool("IsWorking", false);
        //}
    }

    public void MoveToPlot(int plotIndex)
    {
        if (_getPlotWorldPosition == null) return;
        _targetPostion = _getPlotWorldPosition(plotIndex);
        _isMoving = true;
    }

    private void Update()
    {
        if (!_isMoving) return;

        // Di chuyển transform đến targetPosition
        transform.position = Vector3.MoveTowards(transform.position, _targetPostion, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, _targetPostion) < 0.01f)
        {
            _isMoving = false;
        }
    }

}
