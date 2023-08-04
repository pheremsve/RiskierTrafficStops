﻿using LSPD_First_Response.Mod.API;
using Rage;
using System;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Outcomes
{
    internal class YellInCar
    {

        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;

        internal static void YICEventHandler(LHandle handle)
        {
            try
            {
                Suspect = GetSuspectAndVehicle(handle).Item1;
                suspectVehicle = GetSuspectAndVehicle(handle).Item2;

                if(!Suspect.Exists()) { CleanupEvent(Suspect, suspectVehicle); return; }
                Suspect.PlayAmbientSpeech(Voicelines[rndm.Next(Voicelines.Length)]);
                GameFiber.WaitWhile(() => Suspect.Exists() && Suspect.IsAnySpeechPlaying);
                if (Suspect.Exists())
                {
                    Suspect.PlayAmbientSpeech(Voicelines[rndm.Next(Voicelines.Length)]);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                Error(e, "YellinCar.cs");
            }
        }
    }
}