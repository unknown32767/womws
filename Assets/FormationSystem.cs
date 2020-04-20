using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Data.Common;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;

public class FormationSystem : SystemBase
{
    private NavMeshPath path;
    private int currentSegment;

    private float radius;

    private enum SegmentType
    {
        Line,
        Curve,
    };

    private List<(SegmentType type, Vector3 start, Vector3 end)> smoothPath;

    protected override void OnCreate()
    {
        path = new NavMeshPath();

        radius = Spawner.offsets.Select(x => x.magnitude).Max();

        smoothPath = new List<(SegmentType type, Vector3 start, Vector3 end)>();
    }

    private void SmoothPath()
    {
        smoothPath.Clear();

        var corners = path.corners;

        var curvePoints = new List<Vector3>();

        for (int i = 0; i < corners.Length - 2; i++)
        {
            var p0 = corners[i];
            var p1 = corners[i + 1];
            var p2 = corners[i + 2];

            var turnAngle = math.abs(math.radians(Vector3.SignedAngle(p1 - p0, p2 - p1, Vector3.up)));
            var turnLength = radius / math.tan(0.5f * (math.PI - turnAngle));

            var q1 = p1 - (p1 - p0).normalized * turnLength;
            var q2 = p1 + (p2 - p1).normalized * turnLength;

            curvePoints.Add(q1);
            curvePoints.Add(q2);
        }

        if (corners.Length == 2)
        {
            smoothPath.Add((SegmentType.Line, corners[0], corners[1]));
        }
        else
        {
            smoothPath.Add((SegmentType.Line, corners[0], curvePoints[0]));
            for (int i = 0; i < corners.Length - 3; i++)
            {
                smoothPath.Add((SegmentType.Curve, curvePoints[i * 2], curvePoints[i * 2 + 1]));
                smoothPath.Add((SegmentType.Line, curvePoints[i * 2 + 1], curvePoints[i * 2 + 2]));
            }
            smoothPath.Add((SegmentType.Curve, curvePoints[curvePoints.Count-2], curvePoints.Last()));
            smoothPath.Add((SegmentType.Line, curvePoints.Last(), corners.Last()));
        }
    }

    public void UpdatePath(Vector3 end)
    {
        var start = GetSingleton<FormationCenterComponent>().position;

        NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path);

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            SmoothPath();
        }

        currentSegment = 0;
    }

    private float SegmentLength(SegmentType type, Vector3 start, Vector3 end)
    {
        if (type == SegmentType.Line)
        {
            return Vector3.Distance(start, end);
        }
        else
        {
            var chordLength = Vector3.Distance(start, end);
            return math.asin(chordLength * 0.5f / radius) * 2 * radius;
        }
    }

    private float SegmentLength(int index)
    {
        var (type, start, end) = smoothPath[index];

        return SegmentLength(type, start, end);
    }

    private float LeftLength(int index, float3 pos)
    {
        var (type, _, end) = smoothPath[index];

        return SegmentLength(type, pos, end);
    }

    protected override void OnUpdate()
    {
        if (path.status != NavMeshPathStatus.PathComplete)
        {
            return;
        }

        var formationCenter = GetSingleton<FormationCenterComponent>();

        Vector3 nextPos = formationCenter.position;
        var speed = formationCenter.speed;
        var rotation = formationCenter.rotation;
        var travelDistance = speed * Time.DeltaTime;

        while (travelDistance > 0 && currentSegment < smoothPath.Count)
        {
            //在该段路线的起点
            if (currentSegment == 0)
            {
                if (nextPos == path.corners[currentSegment])
                {
                    //转向未完成
                    if (math.dot(math.normalize(math.mul(rotation, Vector3.forward)),
                            math.normalize(path.corners[currentSegment + 1] - path.corners[currentSegment])) < 1)
                    {
                        var turnAngle = math.radians(Vector3.SignedAngle(math.mul(rotation, Vector3.forward),
                            path.corners[currentSegment + 1] - path.corners[currentSegment], Vector3.up));

                        var maxTurnAngle = travelDistance / radius;

                        if (maxTurnAngle >= math.abs(turnAngle))
                        {
                            rotation = math.normalize(quaternion.LookRotation(
                                path.corners[currentSegment + 1] - path.corners[currentSegment], Vector3.up));
                            travelDistance -= math.abs(turnAngle) * radius;
                        }
                        else
                        {
                            rotation = math.mul(rotation, quaternion.Euler(0, maxTurnAngle * math.sign(turnAngle), 0));
                            travelDistance = 0;
                            break;
                        }
                    }
                }
            }

            var (type, start, end) = smoothPath[currentSegment];

            var segmentLength = LeftLength(currentSegment, nextPos);
            var index = (currentSegment + 1) / 2;

            if (travelDistance < segmentLength)
            {

                if (type == SegmentType.Line)
                {
                    nextPos = math.lerp(nextPos, end, travelDistance / segmentLength);
                }
                else
                {
                    var turnAngle = math.radians(Vector3.SignedAngle(math.mul(rotation, Vector3.forward),
                        path.corners[index + 1] - path.corners[index], Vector3.up));
                    var fullAngle = math.radians(Vector3.SignedAngle(path.corners[index] - path.corners[index-1],
                        path.corners[index + 1] - path.corners[index], Vector3.up));
                    var maxTurnAngle = travelDistance / radius * 0.5f;

                    var t = 1-(math.abs(turnAngle) - maxTurnAngle) / math.abs(fullAngle);

                    rotation = math.mul(rotation, quaternion.Euler(0, maxTurnAngle * math.sign(turnAngle), 0));
                    nextPos = (1 - t) * (1 - t) * start + 2 * t * (1 - t) * path.corners[index] + t * t * end;
                }

                travelDistance = 0;
            }
            else
            {
                travelDistance -= segmentLength;
                nextPos = end;

                if (type == SegmentType.Curve)
                {
                    rotation = math.normalize(quaternion.LookRotation(path.corners[index + 1] - path.corners[index],
                        Vector3.up));
                }

                currentSegment += 1;
            }

            //var segmentLength = math.distance(nextPos, path.corners[currentSegment + 1]);



            ////剩余可用路程长度在本段之内
            //if (travelDistance < segmentLength)
            //{
            //    nextPos = math.lerp(nextPos, path.corners[currentSegment + 1], travelDistance / segmentLength);
            //    travelDistance = 0;
            //}
            //else
            //{
            //    travelDistance -= segmentLength;
            //    nextPos = path.corners[currentSegment + 1];
            //    currentSegment += 1;
            //}
        }

        SetSingleton(new FormationCenterComponent
        {
            speed = formationCenter.speed,
            position = nextPos,
            rotation = rotation,
        });

        //for (int i = 0; i < path.corners.Length - 1; i++)
        //{
        //    Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.blue);
        //}

        foreach (var (type, start, end) in smoothPath)
        {
            Debug.DrawLine(start, end, type == SegmentType.Line ? Color.blue : Color.green);
        }

        Debug.DrawRay(nextPos, math.mul(rotation, Vector3.forward), Color.red);
        Debug.DrawRay(nextPos, math.mul(rotation, Vector3.left), Color.red);
        Debug.DrawRay(nextPos, math.mul(rotation, Vector3.right), Color.red);
    }
}