/*using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Contracts;
using KSP;
using KSPAchievements;

namespace FinePrint.Contracts.Parameters
{
    public class EVAParameter : ContractParameter
    {
        private Waypoint shownWaypoint;
        private Waypoint spawnWaypoint;
        private CelestialBody targetBody;
        private int waypointID;
        private double centerLatitude;
        private double centerLongitude;
        private bool submittedWaypoint;
        private bool outerWarning;
        private bool isSecret;
        private double range;
        private PQSCity railScript;
        GameObject railHolder;
        GameObject rail;
        PQSCity.LODRange lodrange;
        WorldObject bob;

        // Game freaks out without a default constructor. I never use it.
        public EVAParameter()
        {
            targetBody = Planetarium.fetch.Home;
            centerLatitude = 0.0;
            centerLongitude = 0.0;
            range = 10000.0;
            shownWaypoint = new Waypoint();
            spawnWaypoint = new Waypoint();
            submittedWaypoint = false;
            outerWarning = false;
            isSecret = false;
        }

        public EVAParameter(int waypointID, CelestialBody targetBody, double centerLatitude, double centerLongitude, double range, bool secret)
        {

            this.targetBody = targetBody;
            this.waypointID = waypointID;
            this.centerLatitude = centerLatitude;
            this.centerLongitude = centerLongitude;
            this.range = range;
            shownWaypoint = new Waypoint();
            spawnWaypoint = new Waypoint();
            submittedWaypoint = false;
            outerWarning = false;
            isSecret = secret;
        }

        protected override string GetHashString()
        {
            return (this.Root.MissionSeed.ToString() + this.Root.DateAccepted.ToString() + this.ID);
        }

        protected override string GetTitle()
        {
            return "Drive by " + Util.integerToGreek(waypointID) + " Site";
        }

        protected override string GetMessageComplete()
        {
            return "You drove by " + Util.integerToGreek(waypointID) + " Site in " + Util.generateSiteName(Root.MissionSeed, (targetBody == Planetarium.fetch.Home)) + ".";
        }

        protected override void OnUnregister()
        {
            if (submittedWaypoint)
                WaypointManager.RemoveWaypoint(shownWaypoint);
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue("waypointID", waypointID);
            node.AddValue("targetBody", bodyID);
            node.AddValue("isSecret", isSecret);
            node.AddValue("centerLatitude", centerLatitude);
            node.AddValue("centerLongitude", centerLongitude);
            node.AddValue("range", range);
        }

        protected override void OnLoad(ConfigNode node)
        {
            Util.LoadNode(node, "RoverWaypointParameter", "targetBody", ref targetBody, Planetarium.fetch.Home);
            Util.LoadNode(node, "RoverWaypointParameter", "isSecret", ref isSecret, false);
            Util.LoadNode(node, "RoverWaypointParameter", "waypointID", ref waypointID, 0);
            Util.LoadNode(node, "RoverWaypointParameter", "centerLatitude", ref centerLatitude, 0.0);
            Util.LoadNode(node, "RoverWaypointParameter", "centerLongitude", ref centerLongitude, 0.0);
            Util.LoadNode(node, "RoverWaypointParameter", "range", ref range, 10000);

            if (HighLogic.LoadedSceneIsFlight && this.Root.ContractState == Contract.State.Active && this.State == ParameterState.Incomplete)
            {
                shownWaypoint.celestialName = targetBody.GetName();
                shownWaypoint.RandomizeNear(centerLatitude, centerLongitude, targetBody.GetName(), range, false);
                shownWaypoint.seed = Root.MissionSeed;
                shownWaypoint.id = waypointID;
                shownWaypoint.siteName = "TEST";
                shownWaypoint.textureName = "rover";
                shownWaypoint.altitude = 0.0;
                shownWaypoint.isClustered = false;
                WaypointManager.AddWaypoint(shownWaypoint);
                submittedWaypoint = true;

                /*railHolder = new GameObject();
                railHolder.name = "railHolder";
                //GameObject rail = .Read(KSPUtil.ApplicationRootPath, "model", ".mu");
                rail = GameDatabase.Instance.GetModel("FinePrint/Models/model");

                rail.transform.parent = railHolder.transform;

                foreach (Transform aTransform in FlightGlobals.ActiveVessel.mainBody.transform)
                {
                    if (aTransform.name == FlightGlobals.ActiveVessel.mainBody.transform.name)
                        railHolder.transform.parent = aTransform;
                }*/
/*

                if (HighLogic.LoadedSceneIsFlight)
                {
                    bob = new WorldObject(-0.03706337763527, 285.3077089361796, -10, 180, 2000);
                    bob.SetScale(5f);
                }
 */
                /*
                railScript = railHolder.AddComponent<PQSCity>();
                railScript.repositionToSphere = true;
                railScript.repositionToSphereSurface = true;
                railScript.reorientToSphere = true;
                railScript.repositionRadial = Util.LLAtoECEF(-0.03706337763527, 285.3077089361796, -10, Planetarium.fetch.Home.Radius);
                railScript.sphere = FlightGlobals.ActiveVessel.mainBody.pqsController;
                railScript.frameDelta = 1;
                Vector3 upDir = Planetarium.fetch.Home.GetSurfaceNVector(-0.03706337763527, 285.3077089361796);
                Quaternion rotation = Quaternion.Euler(0, -90, 0);
                upDir = rotation * upDir;
                railScript.reorientInitialUp = upDir;
                railScript.modEnabled = true;
                railScript.reorientFinalAngle = 180;
                railScript.repositionRadiusOffset = -10;
                lodrange = new PQSCity.LODRange();
                lodrange.objects = new GameObject[0];
                lodrange.visibleRange = 2002;
                railScript.lod = new PQSCity.LODRange[] { lodrange };
                railScript.lod[0].renderers = new GameObject[1];
                railScript.lod[0].renderers[0] = rail;
                rail.transform.localScale = new Vector3(5, 5, 5);
                rail.renderer.sharedMaterial.SetColor("_Color", Color.red);
                rail.SetActive(true);
                lodrange.Setup();
                railScript.OnSetup();
                railScript.Orientate();*/
                /*
                 * 
                 * 		[KSPEvent(guiName = "Plant Flag [1]", guiActive = true)]
		public void PlantFlag()
		{
			this.flagItems--;
			this.fsm.RunEvent(this.KFSMEvent_738);
		}
                 * 
                 */
                //railScript.RebuildSphere();

                /*
                railScript.debugOrientated = false;
                railScript.frameDelta = 1;
                railScript.lod = new PQSCity.LODRange[1];
                railScript.transform.localScale = new Vector3(100f, 100f, 100f);
                railScript.lod[0]
                railScript.modEnabled = true;
                railScript.order = 100;
                railScript.reorientFinalAngle = ;
                railScript.reorientInitialUp = Vector3.up;
                railScript.reorientToSphere = true;

                //railScript.repositionRadiusOffset = 64.3;
                railScript.repositionToSphere = true;
                railScript.repositionToSphereSurface = false;
                railScript.requirements = PQS.ModiferRequirements.Default;
                
                railScript.lod[0].Setup();
                railScript.OnSetup();
                railScript.Orientate();
                railScript.RebuildSphere();*//*
            }

            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                shownWaypoint.celestialName = targetBody.GetName();
                shownWaypoint.RandomizeNear(centerLatitude, centerLongitude, targetBody.GetName(), range, false);
                shownWaypoint.seed = Root.MissionSeed;
                shownWaypoint.id = waypointID;
                shownWaypoint.setName(false);
                shownWaypoint.textureName = "rover";
                shownWaypoint.altitude = 0.0;
                shownWaypoint.isClustered = false;
                WaypointManager.AddWaypoint(shownWaypoint);
                submittedWaypoint = true;
            }
        }

        protected override void OnUpdate()
        {
            if (this.Root.ContractState == Contract.State.Active)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (FlightGlobals.ready)
                    {
                        if (bob != null)
                            bob.Update();

                        //Debug.Log(railHolder.activeSelf + " | " + rail.activeSelf + " | " + railScript.active);
                        //Debug.Log("Me: " + FlightGlobals.ActiveVessel.transform.position);
                        //railHolder.transform.position = FlightGlobals.ActiveVessel.transform.position;
                        //Debug.Log("It: " + (GameObject.Find("KSCRunway").transform.position - Planetarium.fetch.Home.transform.position) + Vector3.up * 50 + Vector3.right * -350);
                        Vessel v = FlightGlobals.ActiveVessel;
                        float distanceToWP = float.PositiveInfinity;

                        if (submittedWaypoint && v.mainBody == targetBody)
                        {
                            if (WaypointManager.Instance() != null)
                            {
                                distanceToWP = WaypointManager.Instance().DistanceToVessel(spawnWaypoint);

                                if (distanceToWP > 10000 && outerWarning)
                                {
                                    outerWarning = false;
                                    ScreenMessages.PostScreenMessage("You are leaving the target area of " + shownWaypoint.siteName + ".", 5.0f, ScreenMessageStyle.UPPER_LEFT);
                                }

                                if (distanceToWP <= 10000 && !outerWarning)
                                {
                                    outerWarning = true;
                                    ScreenMessages.PostScreenMessage("Approaching target area of " + shownWaypoint.siteName + ", checking for anomalous data.", 5.0f, ScreenMessageStyle.UPPER_LEFT);
                                }

                                if (distanceToWP < 5000)
                                {
                                    if (Util.hasWheelsOnGround())
                                    {
                                        if (isSecret)
                                        {
                                            ScreenMessages.PostScreenMessage("This is the source of the anomalous data that we've been looking for in " + shownWaypoint.siteName + "!", 5.0f, ScreenMessageStyle.UPPER_LEFT);

                                            base.SetComplete();

                                            foreach (EVAParameter parameter in Root.AllParameters)
                                            {
                                                if (parameter != null)
                                                {
                                                    if (parameter.State != ParameterState.Complete)
                                                    {
                                                        parameter.shownWaypoint.isExplored = true;
                                                        WaypointManager.deactivateNavPoint(shownWaypoint);
                                                        WaypointManager.RemoveWaypoint(parameter.shownWaypoint);
                                                        parameter.submittedWaypoint = false;
                                                    }
                                                }
                                            }

                                            Root.Complete();
                                        }
                                        else
                                        {
                                            ScreenMessages.PostScreenMessage(Util.generateRoverFailString(Root.MissionSeed, waypointID), 5.0f, ScreenMessageStyle.UPPER_LEFT);
                                            shownWaypoint.isExplored = true;
                                            WaypointManager.deactivateNavPoint(shownWaypoint);
                                            WaypointManager.RemoveWaypoint(shownWaypoint);
                                            submittedWaypoint = false;
                                            base.SetComplete();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
*/