﻿using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using System;
using static RiskierTrafficStops.Systems.Helper;
using static RiskierTrafficStops.Systems.Logger;
using RAGENativeUI;

namespace RiskierTrafficStops.Outcomes
{
    internal class Yelling
    {
        internal enum YellingScenarioOutcomes
        {
            GetBackInVehicle,
            ContinueYelling,
            PullOutKnife
        }

        internal static Ped Suspect;
        internal static Vehicle suspectVehicle;
        internal static RelationshipGroup suspectRelationshipGroup = new("Suspect");
        internal static YellingScenarioOutcomes chosenOutcome;
        internal static bool isSuspectInVehicle = false;


        internal static void YellingOutcome(LHandle handle)
        {
            try
            {
                if (!GetSuspectAndVehicle(handle, out Suspect, out suspectVehicle))
                {
                    CleanupEvent(Suspect, suspectVehicle);
                    return;
                }

                Debug("Making Suspect Leave Vehicle");
                Suspect.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                Debug("Making Suspect Face Player");
                NativeFunction.Natives.x5AD23D40115353AC(Suspect, MainPlayer, -1);

                Debug("Making suspect Yell at Player");
                int timesToSpeak = 0;

                for (int i = 0; i < timesToSpeak; i++)
                {
                    Debug($"Making Suspect Yell, time: {i}");
                    Suspect.PlayAmbientSpeech(Voicelines[rndm.Next(Voicelines.Length)]);
                    GameFiber.WaitWhile(() => Suspect.Exists() && Suspect.IsAnySpeechPlaying);
                }

                Debug("Choosing outome from possible Yelling outcomes");
                YellingScenarioOutcomes[] ScenarioList = (YellingScenarioOutcomes[])Enum.GetValues(typeof(YellingScenarioOutcomes));
                chosenOutcome = ScenarioList[rndm.Next(ScenarioList.Length)];
                Debug($"Chosen Outcome: {chosenOutcome}");

                switch (chosenOutcome)
                {
                    case YellingScenarioOutcomes.GetBackInVehicle:
                        if (Suspect.Exists() && !Functions.IsPedArrested(Suspect)) //Double checking if suspect exists
                        {
                            Suspect.Tasks.EnterVehicle(suspectVehicle, -1);
                        }
                        break;
                    case YellingScenarioOutcomes.PullOutKnife:
                        OutcomePullKnife();
                        break;
                    case YellingScenarioOutcomes.ContinueYelling:
                        GameFiber.StartNew(KeyPressed);
                        while (!isSuspectInVehicle && Suspect.Exists() && !Functions.IsPedArrested(Suspect))
                        {
                            GameFiber.Yield();
                            Suspect.PlayAmbientSpeech(Voicelines[rndm.Next(Voicelines.Length)]);
                            GameFiber.WaitWhile(() => Suspect.Exists() && Suspect.IsAnySpeechPlaying);
                        }
                        break;
                }
            }
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                Error(e, "Yell.cs");
            }
        }
        internal static void KeyPressed()
        {
            Game.DisplayHelp($"~BLIP_INFO_ICON~ Press {Settings.GetBackInKey.GetInstructionalId()} to have the suspect get back in their vehicle", 10000);
            while (Suspect.Exists() && !isSuspectInVehicle)
            {
                GameFiber.Yield();
                if (Game.IsKeyDown(Settings.GetBackInKey))
                {
                    isSuspectInVehicle = true;
                    Suspect.Tasks.EnterVehicle(suspectVehicle, -1);
                    break;
                }
            }
        }

        internal static void OutcomePullKnife()
        {
            if (Suspect.Exists() && !Functions.IsPedArrested(Suspect) && !Functions.IsPedGettingArrested(Suspect))
            {
                Suspect.Inventory.GiveNewWeapon(meleeWeapons[rndm.Next(meleeWeapons.Length)], -1, true);

                Debug("Setting Suspect relationship group");
                Suspect.RelationshipGroup = suspectRelationshipGroup;
                suspectRelationshipGroup.SetRelationshipWith(MainPlayer.RelationshipGroup, Relationship.Hate);
                suspectRelationshipGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);

                MainPlayer.RelationshipGroup.SetRelationshipWith(suspectRelationshipGroup, Relationship.Hate);
                RelationshipGroup.Cop.SetRelationshipWith(suspectRelationshipGroup, Relationship.Hate); //Relationship groups work both ways

                Debug("Giving Suspect FightAgainstClosestHatedTarget Task");
                Suspect.BlockPermanentEvents = true;
                Suspect.Tasks.FightAgainstClosestHatedTarget(40f, -1);
            }
        }
    }
}