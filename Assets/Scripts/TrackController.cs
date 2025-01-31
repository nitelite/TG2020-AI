﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;



[RequireComponent(typeof(PathCreator))]
public class TrackController : MonoBehaviour
{

    SettingsHolder sh;

    public GameObject trackArea;
    public BezierPath trackPath;
    public int Points_min;
    public int Points_max;
    public int numCheckpoints; // Checkpoints
    private Bounds bounds;

    public GameObject checkpointHolder = null;

    // Start is called before the first frame update
    void Start()
    {
        sh = GameObject.Find("SettingsHolder").GetComponent<SettingsHolder>();
        Points_min = sh.settings.mapPointsMin;
        Points_max = sh.settings.mapPointsMax;
    }

    private void generatePath()
    {
        bounds = trackArea.GetComponent<MeshRenderer>().bounds;
        List<Vector3> points = new List<Vector3>();
        int num_points = Random.Range(Points_min, Points_max);


        RoadMeshCreator pst = (RoadMeshCreator)GetComponent<PathSceneTool>();
        PathCreator pc = GetComponent<PathCreator>();

        for (int i = 0; i < num_points; i++)
        {
            float size_x = bounds.size.x;
            float size_z = bounds.size.z;
            float size_y = bounds.size.y;

            //print(string.Format("Size X: {0}, Y: {1} Z: {2}", size_x, size_y, size_z));

            float x = Random.Range(bounds.center.x - size_x / 2, bounds.center.x + size_x / 2);
            float z = Random.Range(bounds.center.z - size_z / 2, bounds.center.z + size_z / 2);
            float y = 1;

            // I want to use some dumb raycasting so that the Y level always is on top of the terrain
            RaycastHit hit;
            if(Physics.Raycast(new Vector3(x, 10000, z), Vector3.down, out hit))
            {
                y = hit.point.y + pst.thickness;
            }   

            //print(string.Format("Point X: {0}, Y: {1} Z: {2}", x, y, z));

            Vector3 point = new Vector3(x,y,z);
            points.Add(point);
        }

        trackPath = new BezierPath(points, true, PathSpace.xyz);
        trackPath.AutoControlLength = Random.Range(0.1f, 1.0f);
        trackPath.IsClosed = true;

        /*
        for(int i = 0; i < trackPath.NumPoints; i++)
        {
            Vector3 point = trackPath.GetPoint(i);
            float y = Random.Range(0.0f, 10.0f);
            point.y = y;
        }
        */
        trackPath.NotifyPathModified();

        pc.bezierPath = trackPath;

        pst.TriggerUpdate();

        GameObject road = GameObject.Find("Road Mesh Holder");
        MeshCollider roadCollider = road.GetComponent<MeshCollider>();
        Destroy(roadCollider);
        roadCollider = road.AddComponent<MeshCollider>();
        MeshFilter roadMesh = road.GetComponent<MeshFilter>();
        roadCollider.sharedMesh = roadMesh.mesh;
    }

    private void generateCheckpoints()
    {
        GameObject startpoint = Resources.Load<GameObject>("Startpoint");
        GameObject checkpoint = Resources.Load<GameObject>("Checkpoint");
        PathCreator pc = GetComponent<PathCreator>();

        if (checkpointHolder == null){
            GameObject obj = new GameObject();
            obj.name = "Checkpoints";
            checkpointHolder = obj;
        }

        VertexPath path = pc.path;
        numCheckpoints = Mathf.Clamp((int)path.length / 10, 4, 10);
        float spacing = path.length  / numCheckpoints;
        float dst = 0;


        dst += spacing;

        while (dst < path.length)
        {
            Vector3 point = path.GetPointAtDistance(dst);
            Quaternion rot = path.GetRotationAtDistance(dst) * Quaternion.Euler(90, 0,90);
            Instantiate(checkpoint, point, rot, checkpointHolder.transform);
            dst += spacing;
        }

        // Startpoint:
        // Add the startpoint last since its the last checkpoint technically
        Vector3 point_start = path.GetPointAtDistance(0);
        Quaternion rot_start = path.GetRotationAtDistance(0) * Quaternion.Euler(90, 0, 90);
        Instantiate(startpoint, point_start, rot_start, checkpointHolder.transform);
    }

    public Vector3 getStartPos()
    {
        return GetComponent<PathCreator>().path.GetPointAtDistance(0.0f);
    }

    public Quaternion getStartRotation()
    {
        return GetComponent<PathCreator>().path.GetRotationAtDistance(0.0f);
    }

    private void generateFoilage()
    {

    }
    private void Awake()
    {
        DontDestroyOnLoad(this);
        generatePath();
        generateCheckpoints();
    }



    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {
        // generatePath();
    }
}
