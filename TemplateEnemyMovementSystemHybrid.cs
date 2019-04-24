using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;
using System.Collections;
using System.Collections.Generic;
using System;

public class TemplateEnemyMovementSystemHybrid : ComponentSystem
{
    private TemplatePathMove _templatePathMove;
    private TransformAccessArray _templateTransformArray;
    private List<Transform> _templateTransform;

    private JobHandle _jobHandle;
    private TransformAccessArray _transformsAccessArray;

    private EntityQuery _templateQuery;

    protected override void OnCreateManager()
    {
        // Must include the target script to build a query.
        _templateQuery = GetEntityQuery(ComponentType.ReadOnly<TemplateProxyHybrid>());
    }

    protected override void OnStartRunning()
    {
        // base.OnStartRunning();

        _templateTransform = new List<Transform>(); 
    }

    [BurstCompile]
    public struct TemplatePathMove : IJobParallelForTransform
    {
        [ReadOnly] public float3 towardsPos;
        [ReadOnly] public float speed; 
        [ReadOnly] public float deltaTime;
        [ReadOnly] public float rotationSpeed;

        public void Execute(int i, TransformAccess transform)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, towardsPos, deltaTime * speed);

            var lookPos = (Vector3) towardsPos - transform.position;
            if (lookPos != Vector3.zero)
            {
                var rotation = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, deltaTime * rotationSpeed);
            }
        }
    }

    private float _speed;
    private float _rotationSpeed;
    private int _previousLen;
    private bool _repopulate;
    private bool _runOnce = true;

    // OnUpdate runs on the main thread.
    protected override void OnUpdate()
    {

        if (_templateQuery.CalculateLength() != _previousLen)
        {
            _templateTransform.Clear();

            _repopulate = true;
            _previousLen = _templateQuery.CalculateLength();
        }

        if (_repopulate == true)
        {
            Entities.With(_templateQuery).ForEach((Entity e, TemplateProxyHybrid template, Transform transform) =>
            {
                if (_runOnce == true)
                {
                    _speed = template._speed;
                    _rotationSpeed = template._rotationSpeed;
                    _runOnce = false;
                }

                _templateTransform.Add(transform);
            });

            if (_templateTransformArray.isCreated)
                _templateTransformArray.Dispose();

            _templateTransformArray = new TransformAccessArray(_templateTransform.ToArray());

            _repopulate = false;
        }

        _templatePathMove.speed = _speed;
        _templatePathMove.rotationSpeed = _rotationSpeed;
        
        _templatePathMove.towardsPos = new float3(0,0,0); // Change position to where the object should move towards.
        _templatePathMove.deltaTime = Time.deltaTime;

        _jobHandle = _templatePathMove.Schedule(_templateTransformArray);

        JobHandle.ScheduleBatchedJobs();
        _jobHandle.Complete();
    }
}
