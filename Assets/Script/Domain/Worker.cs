using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;


public class Worker
{
    public bool IsIdle { get; private set; } = true;
    public float WorkerSpeed {  get; private set; }
    public Vector3 LastPositon { get; private set; } = new(0, 0, -1);
    public Vector3 UpdatedLastPositon { get; private set; }
    private readonly FarmManager _farm;
    private double _taskTimeRemaining;
    private int _plotTaskIndex = -1;
    private float TaskDuration = 120; 
    private Queue<Action> _taskQueue = new Queue<Action>();
    public int PlotTaskIndex => _plotTaskIndex;
    public double TaskTimeRemaining => _taskTimeRemaining;
    public Worker(FarmManager farm)
    {
        _farm = farm;
        WorkerSpeed = _farm.Config.WorkerSpeed;
        TaskDuration = _farm.Config.WorkerTaskDuration;
    }
    public void LoadSaveData(WorkerSaveData saveData)
    {
        _taskTimeRemaining = 0f;
        IsIdle = saveData.IsIdle;
        LastPositon = saveData.LastPostion;
    }
    public void Update(double deltaTime)
    {
        if (!IsIdle)
        {
            _taskTimeRemaining -= deltaTime;
            if (_taskTimeRemaining <= 0)
            {
                ProcessNextTask();
            }
        }
        else
        {
            AssignTask();
        }
    }
    public void SetWorkerLastPositon(Vector3 updatedLastPostion) => UpdatedLastPositon = updatedLastPostion;
    
    private void AssignTask()
    {
        // Prioritize: Harvest, Plant, Milk
        for (int i = 0; i < _farm.Plots.Count; i++)
        {
            var plot = _farm.Plots[i];
            
            if (plot.CanHarvest && !plot.IsAsignedWorker)
            {
                int index = i;
                _taskQueue.Enqueue(() => _farm.Harvest(index));
                StartTask();
                _plotTaskIndex = index;
                plot.AssignWorker(true);
                return;
            }
            else if (plot.IsEmpty && _farm.InventoryOnHand.Any(s => s.Value > 0) && !plot.IsAsignedWorker)
            {
                var seed = _farm.InventoryOnHand.FirstOrDefault(s => s.Value > 0).Key;
                if (seed != null)
                {
                    int index = i;
                    _taskQueue.Enqueue(() => _farm.PlantCrop(index, seed, DateTime.Now));
                    _plotTaskIndex = index;
                    plot.AssignWorker(true);
                    StartTask();
                    return;
                }
            }
        }
    }
    
    

    private void StartTask()
    {
        if (_taskQueue.Count > 0)
        {
            IsIdle = false;
            _taskTimeRemaining = TaskDuration;
        }
    }

    private void ProcessNextTask()
    {
        if (_taskQueue.Count > 0)
        {
            var task = _taskQueue.Dequeue();
            task.Invoke();
            StartTask();
            _farm.Plots[_plotTaskIndex].AssignWorker(false);
            _plotTaskIndex = -1;
        }
        else
        {
            IsIdle = true;
            _plotTaskIndex = -1;
        }
    }
}