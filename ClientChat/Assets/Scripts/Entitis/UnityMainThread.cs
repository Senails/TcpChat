using System;
using System.Collections.Generic;
using UnityEngine;

internal class UnityMainThread : MonoBehaviour
{
    public static UnityMainThread wkr;
    private Queue<Action> _jobs = new Queue<Action>();

    private void Awake() {
        wkr = this;
    }

    private void Update() {
        while (_jobs.Count > 0) 
            _jobs.Dequeue().Invoke();
    }

    public void AddJob(Action newJob) {
        _jobs.Enqueue(newJob);
    }
}
