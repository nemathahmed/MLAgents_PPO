﻿using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeanutAcademy : Academy
{
    private PeanutArea[] areas;

    /// <summary>
    /// Reset the academy
    /// </summary>
    public override void AcademyReset()
    {
        if (areas == null)
        {
            areas = GameObject.FindObjectsOfType<PeanutArea>();
        }

        foreach (PeanutArea area in areas)
        {

            area.numTruffles = (int)resetParameters["num_truffles"];
            //area.numStumps = (int)resetParameters["num_stumps"];
            area.spawnRange = resetParameters["spawn_range"];

            area.ResetArea();
        }
    }
}