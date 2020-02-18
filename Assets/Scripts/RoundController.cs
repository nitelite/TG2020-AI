﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundController : MonoBehaviour
{
    public Server server = null;
    GameObject playerHolder = null;
    List<GameObject> cars = null;
    GameObject ui = null;

    public GameObject checkPointHolder = null;
    public Checkpoint[] checkpoints = null;

    float countDownTime = 3;
    float currCountdownValue = 3;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        cars = new List<GameObject>();
    }

    void FreezePlayers(bool freeze)
    {
        foreach( CarController car in server.GetPlayers())
        {
            car.Freeze = freeze; // I might be retarded. This is the same as Is Kinematic
            car.GetComponentInParent<Rigidbody>().isKinematic = freeze;
        }
    }

    internal void notifyHit(CarController carController, Checkpoint checkpoint)
    {
        Debug.Log("Notify hit (" + checkpoint.getId() + ") "+ carController.UserName);
        // TODO: CHECK IS THIS IS THE CORRECT CHECKPOINT

        int index = carController.GetNextCheckpointindex();
        if(checkpoint.getId() == index)
        {
            Debug.Log("Next Checkpoint hit!");
            carController.checkpointsHit[index] = true;
        }

       if( index == carController.checkpointsHit.Length -1)
        {
            notifyDone(carController);
        }
        
    }

    internal void notifyDone(CarController carController)
    {
        carController.DoneWithRace = true;

        bool notDone = false;
        foreach (CarController car in server.GetPlayers())
        {
            if (!car.DoneWithRace)
            {
                notDone = true;
                break;
            }
        }

        server.GameDone();
    }

    void UpdateLeaderBoard()
    {
        /*
         * Used to control the status of the UI
         * The Ranking based on checkpoints and potential contdowns
         * 
         * TODO: Look into using "Get closest point to path" from the track generator
         */
    }

    void UnfreezePlayers()
    {
        FreezePlayers(false);
    }

    public IEnumerator CountDownToStart()
    {
        ui = GameObject.Find("UI");
        Text text = ui.transform.GetChild(0).transform.GetComponent<Text>();

        currCountdownValue = countDownTime;
        while (currCountdownValue > 0)
        {
            text.text = "Countdown: " + currCountdownValue.ToString() + " sec";
            //Debug.Log("Countdown: " + currCountdownValue);
            yield return new WaitForSeconds(1.0f);
            currCountdownValue--;
        }
        UnfreezePlayers();
        ui.SetActive(false);
        server.GameStatus = Server.GameState.Game_Running;
    }

    internal void InitRound()
    {
        // Start figuring out the checkpoints and set the pos of the players

        playerHolder = GameObject.Find("PlayerHolder");
        GameObject cps = GameObject.Find("Checkpoints");

        checkpoints = cps.GetComponentsInChildren<Checkpoint>();

        TrackController tc = GameObject.Find("MapController").GetComponent<TrackController>();

        Vector3 startPos = tc.getStartPos() + new Vector3(0, 5, 0);
        Quaternion startRotation = tc.getStartRotation() * Quaternion.Euler(90, 0, 90);

        string mapStatusStr = server.GenerateCompleteStatus();
        server.mapStatusStr = mapStatusStr;
        // Send players the mapdata
        foreach ( CarController car in server.GetPlayers())
        {   
            cars.Add(car.gameObject);

            car.checkpointsHit = new bool[checkpoints.Length];
            car.transform.parent.transform.position = startPos;
            car.transform.parent.rotation = startRotation;
            car.SendMapStatus();
        }

        for(int i = 0; i < checkpoints.Length; i++)
        {
            checkpoints[i].setId(i);
        }

        StartCoroutine("CountDownToStart");
    }
}
