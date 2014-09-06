using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Contracts;
using Contracts.Parameters;
using KSP;
using KSPAchievements;
using FinePrint.Contracts.Parameters;
using KSP.IO;
using KSPPluginFramework;

namespace FinePrint
{
    //Marker interface for contracts.
    interface IFinePrintContract
    {
    }

    //Real programmers turn away from this dark and desolate place.
    //It is important that the mod has default values to fall back on for config creation and error recovery.
    //There are a lot of configurable values, and most of these values are hand picked, so a loop wouldn't work.
    //So I felt it best to do this manually, as messy as it may be.

    //Class structure mimicks the configuration file, for my own sanity.

    //At the bottom are functions to create a default configuration file if one doesn't exist, and one to load the file on disk.
    //The load function does sort of a feedback loop, it starts at program start with the default value, the value is fed into the load function with a copy of itself.
    //If load fails, it stays at default, if not, it changes to what is on disk.

    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class FPConfig : MonoBehaviourWindow
    {
        public static ConfigNode config;

        private static FPConfig instance;

        ApplicationLauncherButton appButton = null;
        Vector2 scroll;
        public static bool showOfferedTrackingWaypoints = true;
        public static bool showSurfaceWaypoints = true;
        public static bool showOrbitalWaypoints = true;
        bool firstClick = true;

        public static bool PatchReset = true;
        public static string SunStationaryName = "keliostationary";
        public static string HomeStationaryName = "keostationary";
        public static string OtherStationaryName = "stationary";
        public static string SunSynchronousName = "keliosynchronous";
        public static string HomeSynchronousName = "keosynchronous";
        public static string OtherSynchronousName = "synchronous";
        public static string MolniyaName = "Kolniya";

        public static FPConfig Instance
        {
            get
            {
                if (instance == null)
                    instance = new FPConfig();

                return instance;
            }
        }

        public FPConfig()
        {
            instance = this;
            DontDestroyOnLoad(this);
            LoadConfig();
        }

        internal override void Awake()
        {
            System.Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string major = version.Major.ToString();
            string minor = version.Minor.ToString();

            WindowCaption = "Fine Print v" + major + "." + minor;
            WindowRect = new Rect(0, 75, 250, 50);
            Visible = false;
            DragEnabled = true;
            ClampToScreen = true;
            TooltipsEnabled = true;
            TooltipDisplayForSecs = 4;
            TooltipMaxWidth = 200;

            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
            GameEvents.onLevelWasLoaded.Add(EnteringScene);
        }

        internal override void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherDestroyed);
            GameEvents.onLevelWasLoaded.Remove(EnteringScene);

            if (appButton != null)
                ApplicationLauncher.Instance.RemoveModApplication(appButton);
        }

        void OnGUIAppLauncherReady()
        {
            if (ApplicationLauncher.Ready && appButton == null)
            {
                appButton = ApplicationLauncher.Instance.AddModApplication(
                    onAppLaunchToggleOn,
                    onAppLaunchToggleOff,
                    onAppLaunchHoverOn,
                    onAppLaunchHoverOff,
                    onAppLaunchEnable,
                    onAppLaunchDisable,
                    ApplicationLauncher.AppScenes.ALWAYS,
                    Util.LoadTexture("app", 32, 32)
                );
            }
        }

        void OnGUIAppLauncherDestroyed()
        {
            appButton = null;
        }

        void EnteringScene(GameScenes scene)
        {
            if (appButton != null)
                appButton.SetFalse();
        }

        void onAppLaunchToggleOn()
        {
            Visible = true;

            if (firstClick)
            {
                firstClick = false;
                WindowRect = new Rect(Mouse.screenPos.x - 125, 75, 250, 50);
            }
        }
        void onAppLaunchToggleOff() { Visible = false; }
        void onAppLaunchHoverOn() { /*Your code goes in here to show display on*/ }
        void onAppLaunchHoverOff() { /*Your code goes in here to show display off*/ }
        void onAppLaunchEnable() { /*Your code goes in here for if it gets enabled*/  }
        void onAppLaunchDisable() { /*Your code goes in here for if it gets disabled*/  }

        internal override void DrawWindow(int id)
        {
            //GUILayout.BeginVertical();
            //GUILayout.Space(8);
            //scroll = GUILayout.BeginScrollView(scroll);

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Surface Waypoints:", "Display waypoints locked to a planetary surface"), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            showSurfaceWaypoints = GUILayout.Toggle(showSurfaceWaypoints, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Orbital Waypoints:", "Display the icons and lines associated with orbital waypoints"), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            showOrbitalWaypoints = GUILayout.Toggle(showOrbitalWaypoints, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Tracking Station Waypoints:", "Display inactive offered contract waypoints in the Tracking Station"), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            showOfferedTrackingWaypoints = GUILayout.Toggle(showOfferedTrackingWaypoints, "");
            GUILayout.EndHorizontal();

            if (GUILayout.Button(new GUIContent("Reload Configuration", "Manually reload the configuration file")))
                LoadConfig();

            if (GUILayout.Button(new GUIContent("Refresh Contracts", "Manually refresh all offered Fine Print contracts")))
                Util.ManualBoardReset();

            //GUILayout.EndScrollView();
            //GUILayout.Space(18);
            //GUILayout.EndVertical();
        }

        public static class Aerial
        {
            public static int MaximumAvailable = 2;
            public static int MaximumActive = 4;
            public static int TrivialWaypoints = 1;
            public static int SignificantWaypoints = 2;
            public static int ExceptionalWaypoints = 3;
            public static float TrivialHomeNearbyChance = 70;
            public static float SignificantHomeNearbyChance = 35;
            public static float ExceptionalHomeNearbyChance = 0;
            public static float TrivialLowAltitudeChance = 70;
            public static float SignificantLowAltitudeChance = 35;
            public static float ExceptionalLowAltitudeChance = 0;
            public static double TrivialHomeNearbyRange = 100000;
            public static double SignificantHomeNearbyRange = 200000;
            public static double ExceptionalHomeNearbyRange = 300000;
            public static double TrivialLowAltitudeMultiplier = 0.1;
            public static double SignificantLowAltitudeMultiplier = 0.3;
            public static double ExceptionalLowAltitudeMultiplier = 0.5;
            public static double TrivialRange = 100000;
            public static double SignificantRange = 200000;
            public static double ExceptionalRange = 300000;
            public static double TriggerRange = 15000;

            public static class Expire
            {
                public static int MinimumExpireDays = 1;
                public static int MaximumExpireDays = 7;
                public static int DeadlineDays = 2130;
            }

            public static class Funds
            {
                public static float BaseAdvance = 15000;
                public static float BaseReward = 60000;
                public static float BaseFailure = 0;
                public static float SignificantMultiplier = 1.1f;
                public static float ExceptionalMultiplier = 1.2f;
                public static float WaypointBaseReward = 9000;
                public static float WaypointSignificantMultiplier = 1.1f;
                public static float WaypointExceptionalMultiplier = 1.2f;
            }

            public static class Science
            {
                public static float BaseReward = 40;
                public static float SignificantMultiplier = 1.1f;
                public static float ExceptionalMultiplier = 1.2f;
                public static float WaypointBaseReward = 20;
                public static float WaypointSignificantMultiplier = 1.1f;
                public static float WaypointExceptionalMultiplier = 1.2f;
            }

            public static class Reputation
            {
                public static float BaseReward = 80;
                public static float BaseFailure = 40;
                public static float SignificantMultiplier = 1.1f;
                public static float ExceptionalMultiplier = 1.2f;
                public static float WaypointBaseReward = 7.5f;
                public static float WaypointSignificantMultiplier = 1.1f;
                public static float WaypointExceptionalMultiplier = 1.2f;
            }
        }

        public static class ARM
        {
            public static int MaximumExistent = 2;
            public static float SignificantSolarEjectionChance = 10;
            public static float ExceptionalSolarEjectionChance = 20;
            public static float HomeLandingChance = 20;
            public static bool AllowSolarEjections = true;
            public static bool AllowHomeLandings = true;

            public static class Expire
            {
                public static int MinimumExpireDays = 1;
                public static int MaximumExpireDays = 7;
                public static int DeadlineDays = 2982;
            }

            public static class Funds
            {
                public static float BaseAdvance = 100000;
                public static float BaseReward = 125000;
                public static float BaseFailure = 0;
                public static float SignificantMultiplier = 1.5f;
                public static float ExceptionalMultiplier = 2;
                public static float SolarEjectionMultiplier = 1.25f;
            }

            public static class Science
            {
                public static float BaseReward = 175;
                public static float SignificantMultiplier = 1.5f;
                public static float ExceptionalMultiplier = 2.5f;
                public static float SolarEjectionMultiplier = 1;
            }

            public static class Reputation
            {
                public static float BaseReward = 225;
                public static float BaseFailure = 112.5f;
                public static float SignificantMultiplier = 1.5f;
                public static float ExceptionalMultiplier = 2.5f;
                public static float SolarEjectionMultiplier = 1.25f;
            }
        }

        public static class Base
        {
            public static int MaximumExistent = 2;
            public static float TrivialCupolaChance = 20;
            public static float SignificantCupolaChance = 30;
            public static float ExceptionalCupolaChance = 40;
            public static float TrivialLabChance = 20;
            public static float SignificantLabChance = 30;
            public static float ExceptionalLabChance = 40;
            public static float TrivialMobileChance = 0;
            public static float SignificantMobileChance = 10;
            public static float ExceptionalMobileChance = 30;
            public static bool AllowCupola = true;
            public static bool AllowLab = true;
            public static bool AllowMobile = true;

            public static class Expire
            {
                public static int MinimumExpireDays = 1;
                public static int MaximumExpireDays = 7;
                public static int DeadlineDays = 2982;
            }

            public static class Funds
            {
                public static float BaseAdvance = 45000;
                public static float BaseReward = 90000;
                public static float BaseFailure = 0;
                public static float SignificantMultiplier = 1.15f;
                public static float ExceptionalMultiplier = 1.3f;
                public static float CupolaMultiplier = 1.25f;
                public static float LabMultiplier = 1.15f;
                public static float MobileMultiplier = 1.5f;
            }

            public static class Science
            {
                public static float BaseReward = 100;
                public static float SignificantMultiplier = 1.15f;
                public static float ExceptionalMultiplier = 1.3f;
                public static float CupolaMultiplier = 1;
                public static float LabMultiplier = 1.5f;
                public static float MobileMultiplier = 1.25f;
            }

            public static class Reputation
            {
                public static float BaseReward = 90;
                public static float BaseFailure = 60;
                public static float SignificantMultiplier = 1.15f;
                public static float ExceptionalMultiplier = 1.3f;
                public static float CupolaMultiplier = 1.25f;
                public static float LabMultiplier = 1;
                public static float MobileMultiplier = 1.5f;
            }
        }

        public static class Rover
        {
            public static int MaximumAvailable = 2;
            public static int MaximumActive = 4;
            public static int TrivialWaypoints = 3;
            public static int SignificantWaypoints = 5;
            public static int ExceptionalWaypoints = 7;
            public static float TrivialHomeNearbyChance = 70;
            public static float SignificantHomeNearbyChance = 35;
            public static float ExceptionalHomeNearbyChance = 0;
            public static double TrivialHomeNearbyRange = 10000;
            public static double SignificantHomeNearbyRange = 20000;
            public static double ExceptionalHomeNearbyRange = 30000;
            public static double TrivialRange = 2000;
            public static double SignificantRange = 4000;
            public static double ExceptionalRange = 6000;
            public static double TriggerRange = 500;

            public static class Expire
            {
                public static int MinimumExpireDays = 1;
                public static int MaximumExpireDays = 7;
                public static int DeadlineDays = 2130;
            }

            public static class Funds
            {
                public static float BaseAdvance = 15000;
                public static float BaseReward = 60000;
                public static float BaseFailure = 0;
                public static float SignificantMultiplier = 1.1f;
                public static float ExceptionalMultiplier = 1.2f;
                public static float WaypointBaseReward = 6000;
                public static float WaypointSignificantMultiplier = 1.1f;
                public static float WaypointExceptionalMultiplier = 1.2f;
            }

            public static class Science
            {
                public static float BaseReward = 25;
                public static float SignificantMultiplier = 1.1f;
                public static float ExceptionalMultiplier = 1.2f;
                public static float WaypointBaseReward = 8;
                public static float WaypointSignificantMultiplier = 1.1f;
                public static float WaypointExceptionalMultiplier = 1.2f;
            }

            public static class Reputation
            {
                public static float BaseReward = 50;
                public static float BaseFailure = 25;
                public static float SignificantMultiplier = 1.1f;
                public static float ExceptionalMultiplier = 1.2f;
                public static float WaypointBaseReward = 6;
                public static float WaypointSignificantMultiplier = 1.1f;
                public static float WaypointExceptionalMultiplier = 1.2f;
            }
        }

        public static class Satellite
        {
            public static int MaximumAvailable = 2;
            public static int MaximumActive = 4;
            public static double TrivialDeviation = 7;
            public static double SignificantDeviation = 5;
            public static double ExceptionalDeviation = 3;
            public static float TrivialDifficulty = 0.2f;
            public static float SignificantDifficulty = 0.4f;
            public static float ExceptionalDifficulty = 0.8f;
            public static float TrivialHomeOverrideChance = 50;
            public static float SignificantHomeOverrideChance = 30;
            public static float ExceptionalHomeOverrideChance = 10;
            public static float TrivialPartChance = 20;
            public static float SignificantPartChance = 25;
            public static float ExceptionalPartChance = 30;
            public static float TrivialSolarChance = 0;
            public static float SignificantSolarChance = 10;
            public static float ExceptionalSolarChance = 20;
            public static bool PreferHome = true;
            public static bool AllowSolar = true;
            public static bool AllowEquatorial = true;
            public static bool AllowPolar = true;
            public static bool AllowSynchronous = true;
            public static bool AllowStationary = true;
            public static bool AllowTundra = true;
            public static bool AllowKolniya = true;
            public static string PartRequests = "GooExperiment,sensorThermometer,sensorBarometer,sensorGravimeter,sensorAccelerometer";

            public static class Expire
            {
                public static int MinimumExpireDays = 1;
                public static int MaximumExpireDays = 7;
                public static int DeadlineDays = 2130;
            }

            public static class Funds
            {
                public static float BaseAdvance = 9000;
                public static float BaseReward = 45000;
                public static float BaseFailure = 0;
                public static float SignificantMultiplier = 1.1f;
                public static float ExceptionalMultiplier = 1.2f;
                public static float HomeMultiplier = 2;
                public static float PartMultiplier = 1.05f;
                public static float PolarMultiplier = 1;
                public static float SynchronousMultiplier = 1.1f;
                public static float StationaryMultiplier = 1.2f;
                public static float TundraMultiplier = 1.2f;
                public static float KolniyaMultiplier = 1.2f;
            }

            public static class Science
            {
                public static float BaseReward = 25;
                public static float SignificantMultiplier = 1.1f;
                public static float ExceptionalMultiplier = 1.2f;
                public static float HomeMultiplier = 0.5f;
                public static float PartMultiplier = 1.25f;
                public static float PolarMultiplier = 1;
                public static float SynchronousMultiplier = 1.1f;
                public static float StationaryMultiplier = 1.2f;
                public static float TundraMultiplier = 1.2f;
                public static float KolniyaMultiplier = 1.2f;
            }

            public static class Reputation
            {
                public static float BaseReward = 35;
                public static float BaseFailure = 17.5f;
                public static float SignificantMultiplier = 1.1f;
                public static float ExceptionalMultiplier = 1.2f;
                public static float HomeMultiplier = 1.25f;
                public static float PartMultiplier = 1.05f;
                public static float PolarMultiplier = 1;
                public static float SynchronousMultiplier = 1.1f;
                public static float StationaryMultiplier = 1.2f;
                public static float TundraMultiplier = 1.2f;
                public static float KolniyaMultiplier = 1.2f;
            }
        }

        public static class Station
        {
            public static int MaximumExistent = 2;
            public static float TrivialCupolaChance = 20;
            public static float SignificantCupolaChance = 30;
            public static float ExceptionalCupolaChance = 40;
            public static float TrivialLabChance = 20;
            public static float SignificantLabChance = 30;
            public static float ExceptionalLabChance = 40;
            public static float TrivialAsteroidChance = 0;
            public static float SignificantAsteroidChance = 10;
            public static float ExceptionalAsteroidChance = 20;
            public static bool AllowCupola = true;
            public static bool AllowLab = true;
            public static bool AllowAsteroid = true;
            public static bool AllowSolar = true;

            public static class Expire
            {
                public static int MinimumExpireDays = 1;
                public static int MaximumExpireDays = 7;
                public static int DeadlineDays = 2982;
            }

            public static class Funds
            {
                public static float BaseAdvance = 30000;
                public static float BaseReward = 60000;
                public static float BaseFailure = 0;
                public static float SignificantMultiplier = 1;
                public static float ExceptionalMultiplier = 1;
                public static float CupolaMultiplier = 1.1f;
                public static float LabMultiplier = 1.15f;
                public static float AsteroidMultiplier = 1.2f;
            }

            public static class Science
            {
                public static float BaseReward = 80;
                public static float SignificantMultiplier = 1;
                public static float ExceptionalMultiplier = 1;
                public static float CupolaMultiplier = 1;
                public static float LabMultiplier = 1.3f;
                public static float AsteroidMultiplier = 1.3f;
            }

            public static class Reputation
            {
                public static float BaseReward = 60;
                public static float BaseFailure = 30;
                public static float SignificantMultiplier = 1;
                public static float ExceptionalMultiplier = 1;
                public static float CupolaMultiplier = 1.3f;
                public static float LabMultiplier = 1;
                public static float AsteroidMultiplier = 1.3f;
            }
        }

        public static class ISRU
        {
            public static int MaximumExistent = 0;
            public static float TrivialExtractAmount = 500;
            public static float SignificantExtractAmount = 1000;
            public static float ExceptionalExtractAmount = 2500;
            public static float TrivialDeliveryChance = 50;
            public static float SignificantDeliveryChance = 65;
            public static float ExceptionalDeliveryChance = 80;
            public static string AllowableResources = "Karbonite";
            public static string TechnologyUnlocks = "KA_DetectionArray_01,KA_Drill_Radial_01,KA_Drill_125_01,KA_Engine_250_01,KA_Engine_125_01,KA_AtmScoop_125_01,KA_AtmScoop_250_01,KA_ParticleCollector_250_01,KA_Tank_VTS_01,KA_Tank_Radial_01,KA_Tank_250_01,KA_Tank_125_04,KA_Tank_125_03,KA_Tank_125_02,KA_Tank_125_01,KA_Jet_Stack_01,LFKA_Jet_Stack_01,KA_Jet_Radial_01,LFKA_Jet_Radial_01,KA_Jet_PropFan_01,LFKA_Jet_PropFan_01,KA_Distiller_125_01,KA_Distiller_250_01,KA_Converter_250_01,KA_Generator_250_01,kaRadialLeg,KA_LandingFrame_4,KA_LandingFrame";
            public static string ForbiddenCelestials = "Sun";

            public static class Expire
            {
                public static int MinimumExpireDays = 1;
                public static int MaximumExpireDays = 7;
                public static int DeadlineDays = 2982;
            }

            public static class Funds
            {
                public static float BaseAdvance = 16250;
                public static float BaseReward = 32500;
                public static float BaseFailure = 0;
                public static float SignificantMultiplier = 1.1f;
                public static float ExceptionalMultiplier = 1.3f;
                public static float DeliveryMultiplier = 1.8f;
            }

            public static class Science
            {
                public static float BaseReward = 5;
                public static float SignificantMultiplier = 1.1f;
                public static float ExceptionalMultiplier = 1.3f;
                public static float DeliveryMultiplier = 1.8f;
            }

            public static class Reputation
            {
                public static float BaseReward = 60;
                public static float BaseFailure = 30;
                public static float SignificantMultiplier = 1.1f;
                public static float ExceptionalMultiplier = 1.3f;
                public static float DeliveryMultiplier = 1.8f;
            }
        }

        public static class Evacuate
        {
            public static int MaximumAvailable = 1;
            public static int MaximumActive = 4;
            public static bool AllowDestroy = false;
            public static int DestroyChance = 50;

            public static class Expire
            {
                public static int MinimumExpireDays = 1;
                public static int MaximumExpireDays = 1;
                public static int DeadlineDays = 2982;
            }

            public static class Funds
            {
                public static float BaseAdvance = 10000;
                public static float BaseReward = 30000;
                public static float BaseFailure = 30000;
                public static float CrewMultiplier = 1.15f;
                public static float ReliefMultiplier = 2.0f;
                public static float TroubleMultiplier = 2.0f;
                public static float ExplodeMultiplier = 2.0f;
            }

            public static class Science
            {
                public static float BaseReward = 0;
            }

            public static class Reputation
            {
                public static float BaseReward = 50;
                public static float BaseFailure = 50;
                public static float CrewMultiplier = 1.15f;
                public static float ReliefMultiplier = 0.5f;
                public static float ExplodeMultiplier = 1.0f;
                public static float TroubleMultiplier = 1.0f;
            }
        }

        public static String ConfigFileName
        {
            get { return KSPUtil.ApplicationRootPath + "/GameData/FinePrint/FinePrint.cfg"; }
        }

        private static void CreateDefaultConfig()
        {
            config = new ConfigNode();

            //I could probably do this in a loop, but these are really settings I need to modify by hand, so I will do it manually.
            ConfigNode topNode = config.AddNode(new ConfigNode("FinePrint"));

            ConfigNode aerialNode = topNode.AddNode(new ConfigNode("Aerial"));
            ConfigNode aerialExpire = aerialNode.AddNode(new ConfigNode("Expiration"));
            ConfigNode aerialFunds = aerialNode.AddNode(new ConfigNode("Funds"));
            ConfigNode aerialScience = aerialNode.AddNode(new ConfigNode("Science"));
            ConfigNode aerialReputation = aerialNode.AddNode(new ConfigNode("Reputation"));

            ConfigNode armNode = topNode.AddNode(new ConfigNode("ARM"));
            ConfigNode armExpire = armNode.AddNode(new ConfigNode("Expiration"));
            ConfigNode armFunds = armNode.AddNode(new ConfigNode("Funds"));
            ConfigNode armScience = armNode.AddNode(new ConfigNode("Science"));
            ConfigNode armReputation = armNode.AddNode(new ConfigNode("Reputation"));

            ConfigNode baseNode = topNode.AddNode(new ConfigNode("Base"));
            ConfigNode baseExpire = baseNode.AddNode(new ConfigNode("Expiration"));
            ConfigNode baseFunds = baseNode.AddNode(new ConfigNode("Funds"));
            ConfigNode baseScience = baseNode.AddNode(new ConfigNode("Science"));
            ConfigNode baseReputation = baseNode.AddNode(new ConfigNode("Reputation"));

            ConfigNode roverNode = topNode.AddNode(new ConfigNode("Rover"));
            ConfigNode roverExpire = roverNode.AddNode(new ConfigNode("Expiration"));
            ConfigNode roverFunds = roverNode.AddNode(new ConfigNode("Funds"));
            ConfigNode roverScience = roverNode.AddNode(new ConfigNode("Science"));
            ConfigNode roverReputation = roverNode.AddNode(new ConfigNode("Reputation"));

            ConfigNode satelliteNode = topNode.AddNode(new ConfigNode("Satellite"));
            ConfigNode satelliteExpire = satelliteNode.AddNode(new ConfigNode("Expiration"));
            ConfigNode satelliteFunds = satelliteNode.AddNode(new ConfigNode("Funds"));
            ConfigNode satelliteScience = satelliteNode.AddNode(new ConfigNode("Science"));
            ConfigNode satelliteReputation = satelliteNode.AddNode(new ConfigNode("Reputation"));

            ConfigNode stationNode = topNode.AddNode(new ConfigNode("Station"));
            ConfigNode stationExpire = stationNode.AddNode(new ConfigNode("Expiration"));
            ConfigNode stationFunds = stationNode.AddNode(new ConfigNode("Funds"));
            ConfigNode stationScience = stationNode.AddNode(new ConfigNode("Science"));
            ConfigNode stationReputation = stationNode.AddNode(new ConfigNode("Reputation"));

            ConfigNode isruNode = topNode.AddNode(new ConfigNode("ISRU"));
            ConfigNode isruExpire = isruNode.AddNode(new ConfigNode("Expiration"));
            ConfigNode isruFunds = isruNode.AddNode(new ConfigNode("Funds"));
            ConfigNode isruScience = isruNode.AddNode(new ConfigNode("Science"));
            ConfigNode isruReputation = isruNode.AddNode(new ConfigNode("Reputation"));

            ConfigNode evacuateNode = topNode.AddNode(new ConfigNode("Evacuate"));
            ConfigNode evacuateExpire = evacuateNode.AddNode(new ConfigNode("Expiration"));
            ConfigNode evacuateFunds = evacuateNode.AddNode(new ConfigNode("Funds"));
            ConfigNode evacuateScience = evacuateNode.AddNode(new ConfigNode("Science"));
            ConfigNode evacuateReputation = evacuateNode.AddNode(new ConfigNode("Reputation"));

            topNode.AddValue("PatchReset", FPConfig.PatchReset);
            topNode.AddValue("SunStationaryName", FPConfig.SunStationaryName);
            topNode.AddValue("HomeStationaryName", FPConfig.HomeStationaryName);
            topNode.AddValue("OtherStationaryName", FPConfig.OtherStationaryName);
            topNode.AddValue("SunSynchronousName", FPConfig.SunSynchronousName);
            topNode.AddValue("HomeSynchronousName", FPConfig.HomeSynchronousName);
            topNode.AddValue("OtherSynchronousName", FPConfig.OtherSynchronousName);
            topNode.AddValue("MolniyaName", FPConfig.MolniyaName);

            aerialNode.AddValue("MaximumAvailable", FPConfig.Aerial.MaximumAvailable);
            aerialNode.AddValue("MaximumActive", FPConfig.Aerial.MaximumActive);
            aerialNode.AddValue("TrivialWaypoints", FPConfig.Aerial.TrivialWaypoints);
            aerialNode.AddValue("SignificantWaypoints", FPConfig.Aerial.SignificantWaypoints);
            aerialNode.AddValue("ExceptionalWaypoints", FPConfig.Aerial.ExceptionalWaypoints);
            aerialNode.AddValue("TrivialHomeNearbyChance", FPConfig.Aerial.TrivialHomeNearbyChance);
            aerialNode.AddValue("SignificantHomeNearbyChance", FPConfig.Aerial.SignificantHomeNearbyChance);
            aerialNode.AddValue("ExceptionalHomeNearbyChance", FPConfig.Aerial.ExceptionalHomeNearbyChance);
            aerialNode.AddValue("TrivialLowAltitudeChance", FPConfig.Aerial.TrivialLowAltitudeChance);
            aerialNode.AddValue("SignificantLowAltitudeChance", FPConfig.Aerial.SignificantLowAltitudeChance);
            aerialNode.AddValue("ExceptionalLowAltitudeChance", FPConfig.Aerial.ExceptionalLowAltitudeChance);
            aerialNode.AddValue("TrivialHomeNearbyRange", FPConfig.Aerial.TrivialHomeNearbyRange);
            aerialNode.AddValue("SignificantHomeNearbyRange", FPConfig.Aerial.SignificantHomeNearbyRange);
            aerialNode.AddValue("ExceptionalHomeNearbyRange", FPConfig.Aerial.ExceptionalHomeNearbyRange);
            aerialNode.AddValue("TrivialLowAltitudeMultiplier", FPConfig.Aerial.TrivialLowAltitudeMultiplier);
            aerialNode.AddValue("SignificantLowAltitudeMultiplier", FPConfig.Aerial.SignificantLowAltitudeMultiplier);
            aerialNode.AddValue("ExceptionalLowAltitudeMultiplier", FPConfig.Aerial.ExceptionalLowAltitudeMultiplier);
            aerialNode.AddValue("TrivialRange", FPConfig.Aerial.TrivialRange);
            aerialNode.AddValue("SignificantRange", FPConfig.Aerial.SignificantRange);
            aerialNode.AddValue("ExceptionalRange", FPConfig.Aerial.ExceptionalRange);
            aerialNode.AddValue("TriggerRange", FPConfig.Aerial.TriggerRange);
            aerialExpire.AddValue("MinimumExpireDays", FPConfig.Aerial.Expire.MinimumExpireDays);
            aerialExpire.AddValue("MaximumExpireDays", FPConfig.Aerial.Expire.MaximumExpireDays);
            aerialExpire.AddValue("DeadlineDays", FPConfig.Aerial.Expire.DeadlineDays);
            aerialFunds.AddValue("BaseAdvance", FPConfig.Aerial.Funds.BaseAdvance);
            aerialFunds.AddValue("BaseReward", FPConfig.Aerial.Funds.BaseReward);
            aerialFunds.AddValue("BaseFailure", FPConfig.Aerial.Funds.BaseFailure);
            aerialFunds.AddValue("SignificantMultiplier", FPConfig.Aerial.Funds.SignificantMultiplier);
            aerialFunds.AddValue("ExceptionalMultiplier", FPConfig.Aerial.Funds.ExceptionalMultiplier);
            aerialFunds.AddValue("WaypointBaseReward", FPConfig.Aerial.Funds.WaypointBaseReward);
            aerialFunds.AddValue("WaypointSignificantMultiplier", FPConfig.Aerial.Funds.WaypointSignificantMultiplier);
            aerialFunds.AddValue("WaypointExceptionalMultiplier", FPConfig.Aerial.Funds.WaypointExceptionalMultiplier);
            aerialScience.AddValue("BaseReward", FPConfig.Aerial.Science.BaseReward);
            aerialScience.AddValue("SignificantMultiplier", FPConfig.Aerial.Science.SignificantMultiplier);
            aerialScience.AddValue("ExceptionalMultiplier", FPConfig.Aerial.Science.ExceptionalMultiplier);
            aerialScience.AddValue("WaypointBaseReward", FPConfig.Aerial.Science.WaypointBaseReward);
            aerialScience.AddValue("WaypointSignificantMultiplier", FPConfig.Aerial.Science.WaypointSignificantMultiplier);
            aerialScience.AddValue("WaypointExceptionalMultiplier", FPConfig.Aerial.Science.WaypointExceptionalMultiplier);
            aerialReputation.AddValue("BaseReward", FPConfig.Aerial.Reputation.BaseReward);
            aerialReputation.AddValue("BaseFailure", FPConfig.Aerial.Reputation.BaseFailure);
            aerialReputation.AddValue("SignificantMultiplier", FPConfig.Aerial.Reputation.SignificantMultiplier);
            aerialReputation.AddValue("ExceptionalMultiplier", FPConfig.Aerial.Reputation.ExceptionalMultiplier);
            aerialReputation.AddValue("WaypointBaseReward", FPConfig.Aerial.Reputation.WaypointBaseReward);
            aerialReputation.AddValue("WaypointSignificantMultiplier", FPConfig.Aerial.Reputation.WaypointSignificantMultiplier);
            aerialReputation.AddValue("WaypointExceptionalMultiplier", FPConfig.Aerial.Reputation.WaypointExceptionalMultiplier);

            armNode.AddValue("MaximumExistent", FPConfig.ARM.MaximumExistent);
            armNode.AddValue("SignificantSolarEjectionChance", FPConfig.ARM.SignificantSolarEjectionChance);
            armNode.AddValue("ExceptionalSolarEjectionChance", FPConfig.ARM.ExceptionalSolarEjectionChance);
            armNode.AddValue("HomeLandingChance", FPConfig.ARM.HomeLandingChance);
            armNode.AddValue("AllowSolarEjections", FPConfig.ARM.AllowSolarEjections);
            armNode.AddValue("AllowHomeLandings", FPConfig.ARM.AllowHomeLandings);
            armExpire.AddValue("MinimumExpireDays", FPConfig.ARM.Expire.MinimumExpireDays);
            armExpire.AddValue("MaximumExpireDays", FPConfig.ARM.Expire.MaximumExpireDays);
            armExpire.AddValue("DeadlineDays", FPConfig.ARM.Expire.DeadlineDays);
            armFunds.AddValue("BaseAdvance", FPConfig.ARM.Funds.BaseAdvance);
            armFunds.AddValue("BaseReward", FPConfig.ARM.Funds.BaseReward);
            armFunds.AddValue("BaseFailure", FPConfig.ARM.Funds.BaseFailure);
            armFunds.AddValue("SignificantMultiplier", FPConfig.ARM.Funds.SignificantMultiplier);
            armFunds.AddValue("ExceptionalMultiplier", FPConfig.ARM.Funds.ExceptionalMultiplier);
            armFunds.AddValue("SolarEjectionMultiplier", FPConfig.ARM.Funds.SolarEjectionMultiplier);
            armScience.AddValue("BaseReward", FPConfig.ARM.Science.BaseReward);
            armScience.AddValue("SignificantMultiplier", FPConfig.ARM.Science.SignificantMultiplier);
            armScience.AddValue("ExceptionalMultiplier", FPConfig.ARM.Science.ExceptionalMultiplier);
            armScience.AddValue("SolarEjectionMultiplier", FPConfig.ARM.Science.SolarEjectionMultiplier);
            armReputation.AddValue("BaseReward", FPConfig.ARM.Reputation.BaseReward);
            armReputation.AddValue("BaseFailure", FPConfig.ARM.Reputation.BaseFailure);
            armReputation.AddValue("SignificantMultiplier", FPConfig.ARM.Reputation.SignificantMultiplier);
            armReputation.AddValue("ExceptionalMultiplier", FPConfig.ARM.Reputation.ExceptionalMultiplier);
            armReputation.AddValue("SolarEjectionMultiplier", FPConfig.ARM.Reputation.SolarEjectionMultiplier);

            baseNode.AddValue("MaximumExistent", FPConfig.Base.MaximumExistent);
            baseNode.AddValue("TrivialCupolaChance", FPConfig.Base.TrivialCupolaChance);
            baseNode.AddValue("SignificantCupolaChance", FPConfig.Base.SignificantCupolaChance);
            baseNode.AddValue("ExceptionalCupolaChance", FPConfig.Base.ExceptionalCupolaChance);
            baseNode.AddValue("TrivialLabChance", FPConfig.Base.TrivialLabChance);
            baseNode.AddValue("SignificantLabChance", FPConfig.Base.SignificantLabChance);
            baseNode.AddValue("ExceptionalLabChance", FPConfig.Base.ExceptionalLabChance);
            baseNode.AddValue("TrivialMobileChance", FPConfig.Base.TrivialMobileChance);
            baseNode.AddValue("SignificantMobileChance", FPConfig.Base.SignificantMobileChance);
            baseNode.AddValue("ExceptionalMobileChance", FPConfig.Base.ExceptionalMobileChance);
            baseNode.AddValue("AllowCupola", FPConfig.Base.AllowCupola);
            baseNode.AddValue("AllowLab", FPConfig.Base.AllowLab);
            baseNode.AddValue("AllowMobile", FPConfig.Base.AllowMobile);
            baseExpire.AddValue("MinimumExpireDays", FPConfig.Base.Expire.MinimumExpireDays);
            baseExpire.AddValue("MaximumExpireDays", FPConfig.Base.Expire.MaximumExpireDays);
            baseExpire.AddValue("DeadlineDays", FPConfig.Base.Expire.DeadlineDays);
            baseFunds.AddValue("BaseAdvance", FPConfig.Base.Funds.BaseAdvance);
            baseFunds.AddValue("BaseReward", FPConfig.Base.Funds.BaseReward);
            baseFunds.AddValue("BaseFailure", FPConfig.Base.Funds.BaseFailure);
            baseFunds.AddValue("SignificantMultiplier", FPConfig.Base.Funds.SignificantMultiplier);
            baseFunds.AddValue("ExceptionalMultiplier", FPConfig.Base.Funds.ExceptionalMultiplier);
            baseFunds.AddValue("CupolaMultiplier", FPConfig.Base.Funds.CupolaMultiplier);
            baseFunds.AddValue("LabMultiplier", FPConfig.Base.Funds.LabMultiplier);
            baseFunds.AddValue("MobileMultiplier", FPConfig.Base.Funds.MobileMultiplier);
            baseScience.AddValue("BaseReward", FPConfig.Base.Science.BaseReward);
            baseScience.AddValue("SignificantMultiplier", FPConfig.Base.Science.SignificantMultiplier);
            baseScience.AddValue("ExceptionalMultiplier", FPConfig.Base.Science.ExceptionalMultiplier);
            baseScience.AddValue("CupolaMultiplier", FPConfig.Base.Science.CupolaMultiplier);
            baseScience.AddValue("LabMultiplier", FPConfig.Base.Science.LabMultiplier);
            baseScience.AddValue("MobileMultiplier", FPConfig.Base.Science.MobileMultiplier);
            baseReputation.AddValue("BaseReward", FPConfig.Base.Reputation.BaseReward);
            baseReputation.AddValue("BaseFailure", FPConfig.Base.Reputation.BaseFailure);
            baseReputation.AddValue("SignificantMultiplier", FPConfig.Base.Reputation.SignificantMultiplier);
            baseReputation.AddValue("ExceptionalMultiplier", FPConfig.Base.Reputation.ExceptionalMultiplier);
            baseReputation.AddValue("CupolaMultiplier", FPConfig.Base.Reputation.CupolaMultiplier);
            baseReputation.AddValue("LabMultiplier", FPConfig.Base.Reputation.LabMultiplier);
            baseReputation.AddValue("MobileMultiplier", FPConfig.Base.Reputation.MobileMultiplier);

            roverNode.AddValue("MaximumAvailable", FPConfig.Rover.MaximumAvailable);
            roverNode.AddValue("MaximumActive", FPConfig.Rover.MaximumActive);
            roverNode.AddValue("TrivialWaypoints", FPConfig.Rover.TrivialWaypoints);
            roverNode.AddValue("SignificantWaypoints", FPConfig.Rover.SignificantWaypoints);
            roverNode.AddValue("ExceptionalWaypoints", FPConfig.Rover.ExceptionalWaypoints);
            roverNode.AddValue("TrivialHomeNearbyChance", FPConfig.Rover.TrivialHomeNearbyChance);
            roverNode.AddValue("SignificantHomeNearbyChance", FPConfig.Rover.SignificantHomeNearbyChance);
            roverNode.AddValue("ExceptionalHomeNearbyChance", FPConfig.Rover.ExceptionalHomeNearbyChance);
            roverNode.AddValue("TrivialHomeNearbyRange", FPConfig.Rover.TrivialHomeNearbyRange);
            roverNode.AddValue("SignificantHomeNearbyRange", FPConfig.Rover.SignificantHomeNearbyRange);
            roverNode.AddValue("ExceptionalHomeNearbyRange", FPConfig.Rover.ExceptionalHomeNearbyRange);
            roverNode.AddValue("TrivialRange", FPConfig.Rover.TrivialRange);
            roverNode.AddValue("SignificantRange", FPConfig.Rover.SignificantRange);
            roverNode.AddValue("ExceptionalRange", FPConfig.Rover.ExceptionalRange);
            roverNode.AddValue("TriggerRange", FPConfig.Rover.TriggerRange);
            roverExpire.AddValue("MinimumExpireDays", FPConfig.Rover.Expire.MinimumExpireDays);
            roverExpire.AddValue("MaximumExpireDays", FPConfig.Rover.Expire.MaximumExpireDays);
            roverExpire.AddValue("DeadlineDays", FPConfig.Rover.Expire.DeadlineDays);
            roverFunds.AddValue("BaseAdvance", FPConfig.Rover.Funds.BaseAdvance);
            roverFunds.AddValue("BaseReward", FPConfig.Rover.Funds.BaseReward);
            roverFunds.AddValue("BaseFailure", FPConfig.Rover.Funds.BaseFailure);
            roverFunds.AddValue("SignificantMultiplier", FPConfig.Rover.Funds.SignificantMultiplier);
            roverFunds.AddValue("ExceptionalMultiplier", FPConfig.Rover.Funds.ExceptionalMultiplier);
            roverFunds.AddValue("WaypointBaseReward", FPConfig.Rover.Funds.WaypointBaseReward);
            roverFunds.AddValue("WaypointSignificantMultiplier", FPConfig.Rover.Funds.WaypointSignificantMultiplier);
            roverFunds.AddValue("WaypointExceptionalMultiplier", FPConfig.Rover.Funds.WaypointExceptionalMultiplier);
            roverScience.AddValue("BaseReward", FPConfig.Rover.Science.BaseReward);
            roverScience.AddValue("SignificantMultiplier", FPConfig.Rover.Science.SignificantMultiplier);
            roverScience.AddValue("ExceptionalMultiplier", FPConfig.Rover.Science.ExceptionalMultiplier);
            roverScience.AddValue("WaypointBaseReward", FPConfig.Rover.Science.WaypointBaseReward);
            roverScience.AddValue("WaypointSignificantMultiplier", FPConfig.Rover.Science.WaypointSignificantMultiplier);
            roverScience.AddValue("WaypointExceptionalMultiplier", FPConfig.Rover.Science.WaypointExceptionalMultiplier);
            roverReputation.AddValue("BaseReward", FPConfig.Rover.Reputation.BaseReward);
            roverReputation.AddValue("BaseFailure", FPConfig.Rover.Reputation.BaseFailure);
            roverReputation.AddValue("SignificantMultiplier", FPConfig.Rover.Reputation.SignificantMultiplier);
            roverReputation.AddValue("ExceptionalMultiplier", FPConfig.Rover.Reputation.ExceptionalMultiplier);
            roverReputation.AddValue("WaypointBaseReward", FPConfig.Rover.Reputation.WaypointBaseReward);
            roverReputation.AddValue("WaypointSignificantMultiplier", FPConfig.Rover.Reputation.WaypointSignificantMultiplier);
            roverReputation.AddValue("WaypointExceptionalMultiplier", FPConfig.Rover.Reputation.WaypointExceptionalMultiplier);

            satelliteNode.AddValue("MaximumAvailable", FPConfig.Satellite.MaximumAvailable);
            satelliteNode.AddValue("MaximumActive", FPConfig.Satellite.MaximumActive);
            satelliteNode.AddValue("TrivialDeviation", FPConfig.Satellite.TrivialDeviation);
            satelliteNode.AddValue("SignificantDeviation", FPConfig.Satellite.SignificantDeviation);
            satelliteNode.AddValue("ExceptionalDeviation", FPConfig.Satellite.ExceptionalDeviation);
            satelliteNode.AddValue("TrivialDifficulty", FPConfig.Satellite.TrivialDifficulty);
            satelliteNode.AddValue("SignificantDifficulty", FPConfig.Satellite.SignificantDifficulty);
            satelliteNode.AddValue("ExceptionalDifficulty", FPConfig.Satellite.ExceptionalDifficulty);
            satelliteNode.AddValue("TrivialHomeOverrideChance", FPConfig.Satellite.TrivialHomeOverrideChance);
            satelliteNode.AddValue("SignificantHomeOverrideChance", FPConfig.Satellite.SignificantHomeOverrideChance);
            satelliteNode.AddValue("ExceptionalHomeOverrideChance", FPConfig.Satellite.ExceptionalHomeOverrideChance);
            satelliteNode.AddValue("TrivialPartChance", FPConfig.Satellite.TrivialPartChance);
            satelliteNode.AddValue("SignificantPartChance", FPConfig.Satellite.SignificantPartChance);
            satelliteNode.AddValue("ExceptionalPartChance", FPConfig.Satellite.ExceptionalPartChance);
            satelliteNode.AddValue("TrivialSolarChance", FPConfig.Satellite.TrivialSolarChance);
            satelliteNode.AddValue("SignificantSolarChance", FPConfig.Satellite.SignificantSolarChance);
            satelliteNode.AddValue("ExceptionalSolarChance", FPConfig.Satellite.ExceptionalSolarChance);
            satelliteNode.AddValue("PreferHome", FPConfig.Satellite.PreferHome);
            satelliteNode.AddValue("AllowSolar", FPConfig.Satellite.AllowSolar);
            satelliteNode.AddValue("AllowEquatorial", FPConfig.Satellite.AllowEquatorial);
            satelliteNode.AddValue("AllowPolar", FPConfig.Satellite.AllowPolar);
            satelliteNode.AddValue("AllowSynchronous", FPConfig.Satellite.AllowSynchronous);
            satelliteNode.AddValue("AllowStationary", FPConfig.Satellite.AllowStationary);
            satelliteNode.AddValue("AllowTundra", FPConfig.Satellite.AllowTundra);
            satelliteNode.AddValue("AllowKolniya", FPConfig.Satellite.AllowKolniya);
            satelliteNode.AddValue("PartRequests", FPConfig.Satellite.PartRequests);
            satelliteExpire.AddValue("MinimumExpireDays", FPConfig.Satellite.Expire.MinimumExpireDays);
            satelliteExpire.AddValue("MaximumExpireDays", FPConfig.Satellite.Expire.MaximumExpireDays);
            satelliteExpire.AddValue("DeadlineDays", FPConfig.Satellite.Expire.DeadlineDays);
            satelliteFunds.AddValue("BaseAdvance", FPConfig.Satellite.Funds.BaseAdvance);
            satelliteFunds.AddValue("BaseReward", FPConfig.Satellite.Funds.BaseReward);
            satelliteFunds.AddValue("BaseFailure", FPConfig.Satellite.Funds.BaseFailure);
            satelliteFunds.AddValue("SignificantMultiplier", FPConfig.Satellite.Funds.SignificantMultiplier);
            satelliteFunds.AddValue("ExceptionalMultiplier", FPConfig.Satellite.Funds.ExceptionalMultiplier);
            satelliteFunds.AddValue("HomeMultiplier", FPConfig.Satellite.Funds.HomeMultiplier);
            satelliteFunds.AddValue("PartMultiplier", FPConfig.Satellite.Funds.PartMultiplier);
            satelliteFunds.AddValue("PolarMultiplier", FPConfig.Satellite.Funds.PolarMultiplier);
            satelliteFunds.AddValue("SynchronousMultiplier", FPConfig.Satellite.Funds.SynchronousMultiplier);
            satelliteFunds.AddValue("StationaryMultiplier", FPConfig.Satellite.Funds.StationaryMultiplier);
            satelliteFunds.AddValue("TundraMultiplier", FPConfig.Satellite.Funds.TundraMultiplier);
            satelliteFunds.AddValue("KolniyaMultiplier", FPConfig.Satellite.Funds.KolniyaMultiplier);
            satelliteScience.AddValue("BaseReward", FPConfig.Satellite.Science.BaseReward);
            satelliteScience.AddValue("SignificantMultiplier", FPConfig.Satellite.Science.SignificantMultiplier);
            satelliteScience.AddValue("ExceptionalMultiplier", FPConfig.Satellite.Science.ExceptionalMultiplier);
            satelliteScience.AddValue("HomeMultiplier", FPConfig.Satellite.Science.HomeMultiplier);
            satelliteScience.AddValue("PartMultiplier", FPConfig.Satellite.Science.PartMultiplier);
            satelliteScience.AddValue("PolarMultiplier", FPConfig.Satellite.Science.PolarMultiplier);
            satelliteScience.AddValue("SynchronousMultiplier", FPConfig.Satellite.Science.SynchronousMultiplier);
            satelliteScience.AddValue("StationaryMultiplier", FPConfig.Satellite.Science.StationaryMultiplier);
            satelliteScience.AddValue("TundraMultiplier", FPConfig.Satellite.Science.TundraMultiplier);
            satelliteScience.AddValue("KolniyaMultiplier", FPConfig.Satellite.Science.KolniyaMultiplier);
            satelliteReputation.AddValue("BaseReward", FPConfig.Satellite.Reputation.BaseReward);
            satelliteReputation.AddValue("BaseFailure", FPConfig.Satellite.Reputation.BaseFailure);
            satelliteReputation.AddValue("SignificantMultiplier", FPConfig.Satellite.Reputation.SignificantMultiplier);
            satelliteReputation.AddValue("ExceptionalMultiplier", FPConfig.Satellite.Reputation.ExceptionalMultiplier);
            satelliteReputation.AddValue("HomeMultiplier", FPConfig.Satellite.Reputation.HomeMultiplier);
            satelliteReputation.AddValue("PartMultiplier", FPConfig.Satellite.Reputation.PartMultiplier);
            satelliteReputation.AddValue("PolarMultiplier", FPConfig.Satellite.Reputation.PolarMultiplier);
            satelliteReputation.AddValue("SynchronousMultiplier", FPConfig.Satellite.Reputation.SynchronousMultiplier);
            satelliteReputation.AddValue("StationaryMultiplier", FPConfig.Satellite.Reputation.StationaryMultiplier);
            satelliteReputation.AddValue("TundraMultiplier", FPConfig.Satellite.Reputation.TundraMultiplier);
            satelliteReputation.AddValue("KolniyaMultiplier", FPConfig.Satellite.Reputation.KolniyaMultiplier);

            stationNode.AddValue("MaximumExistent", FPConfig.Station.MaximumExistent);
            stationNode.AddValue("TrivialCupolaChance", FPConfig.Station.TrivialCupolaChance);
            stationNode.AddValue("SignificantCupolaChance", FPConfig.Station.SignificantCupolaChance);
            stationNode.AddValue("ExceptionalCupolaChance", FPConfig.Station.ExceptionalCupolaChance);
            stationNode.AddValue("TrivialLabChance", FPConfig.Station.TrivialLabChance);
            stationNode.AddValue("SignificantLabChance", FPConfig.Station.SignificantLabChance);
            stationNode.AddValue("ExceptionalLabChance", FPConfig.Station.ExceptionalLabChance);
            stationNode.AddValue("TrivialAsteroidChance", FPConfig.Station.TrivialAsteroidChance);
            stationNode.AddValue("SignificantAsteroidChance", FPConfig.Station.SignificantAsteroidChance);
            stationNode.AddValue("ExceptionalAsteroidChance", FPConfig.Station.ExceptionalAsteroidChance);
            stationNode.AddValue("AllowCupola", FPConfig.Station.AllowCupola);
            stationNode.AddValue("AllowLab", FPConfig.Station.AllowLab);
            stationNode.AddValue("AllowAsteroid", FPConfig.Station.AllowAsteroid);
            stationNode.AddValue("AllowSolar", FPConfig.Station.AllowSolar);
            stationExpire.AddValue("MinimumExpireDays", FPConfig.Station.Expire.MinimumExpireDays);
            stationExpire.AddValue("MaximumExpireDays", FPConfig.Station.Expire.MaximumExpireDays);
            stationExpire.AddValue("DeadlineDays", FPConfig.Station.Expire.DeadlineDays);
            stationFunds.AddValue("BaseAdvance", FPConfig.Station.Funds.BaseAdvance);
            stationFunds.AddValue("BaseReward", FPConfig.Station.Funds.BaseReward);
            stationFunds.AddValue("BaseFailure", FPConfig.Station.Funds.BaseFailure);
            stationFunds.AddValue("SignificantMultiplier", FPConfig.Station.Funds.SignificantMultiplier);
            stationFunds.AddValue("ExceptionalMultiplier", FPConfig.Station.Funds.ExceptionalMultiplier);
            stationFunds.AddValue("CupolaMultiplier", FPConfig.Station.Funds.CupolaMultiplier);
            stationFunds.AddValue("LabMultiplier", FPConfig.Station.Funds.LabMultiplier);
            stationFunds.AddValue("AsteroidMultiplier", FPConfig.Station.Funds.AsteroidMultiplier);
            stationScience.AddValue("BaseReward", FPConfig.Station.Science.BaseReward);
            stationScience.AddValue("SignificantMultiplier", FPConfig.Station.Science.SignificantMultiplier);
            stationScience.AddValue("ExceptionalMultiplier", FPConfig.Station.Science.ExceptionalMultiplier);
            stationScience.AddValue("CupolaMultiplier", FPConfig.Station.Science.CupolaMultiplier);
            stationScience.AddValue("LabMultiplier", FPConfig.Station.Science.LabMultiplier);
            stationScience.AddValue("AsteroidMultiplier", FPConfig.Station.Science.AsteroidMultiplier);
            stationReputation.AddValue("BaseReward", FPConfig.Station.Reputation.BaseReward);
            stationReputation.AddValue("BaseFailure", FPConfig.Station.Reputation.BaseFailure);
            stationReputation.AddValue("SignificantMultiplier", FPConfig.Station.Reputation.SignificantMultiplier);
            stationReputation.AddValue("ExceptionalMultiplier", FPConfig.Station.Reputation.ExceptionalMultiplier);
            stationReputation.AddValue("CupolaMultiplier", FPConfig.Station.Reputation.CupolaMultiplier);
            stationReputation.AddValue("LabMultiplier", FPConfig.Station.Reputation.LabMultiplier);
            stationReputation.AddValue("AsteroidMultiplier", FPConfig.Station.Reputation.AsteroidMultiplier);

            isruNode.AddValue("MaximumExistent", FPConfig.ISRU.MaximumExistent);
            isruNode.AddValue("TrivialExtractAmount", FPConfig.ISRU.TrivialExtractAmount);
            isruNode.AddValue("SignificantExtractAmount", FPConfig.ISRU.SignificantExtractAmount);
            isruNode.AddValue("ExceptionalExtractAmount", FPConfig.ISRU.ExceptionalExtractAmount);
            isruNode.AddValue("TrivialDeliveryChance", FPConfig.ISRU.TrivialDeliveryChance);
            isruNode.AddValue("SignificantDeliveryChance", FPConfig.ISRU.SignificantDeliveryChance);
            isruNode.AddValue("ExceptionalDeliveryChance", FPConfig.ISRU.ExceptionalDeliveryChance);
            isruNode.AddValue("AllowableResources", FPConfig.ISRU.AllowableResources);
            isruNode.AddValue("TechnologyUnlocks", FPConfig.ISRU.TechnologyUnlocks);
            isruNode.AddValue("ForbiddenCelestials", FPConfig.ISRU.ForbiddenCelestials);
            isruExpire.AddValue("MinimumExpireDays", FPConfig.ISRU.Expire.MinimumExpireDays);
            isruExpire.AddValue("MaximumExpireDays", FPConfig.ISRU.Expire.MaximumExpireDays);
            isruExpire.AddValue("DeadlineDays", FPConfig.ISRU.Expire.DeadlineDays);
            isruFunds.AddValue("BaseAdvance", FPConfig.ISRU.Funds.BaseAdvance);
            isruFunds.AddValue("BaseReward", FPConfig.ISRU.Funds.BaseReward);
            isruFunds.AddValue("BaseFailure", FPConfig.ISRU.Funds.BaseFailure);
            isruFunds.AddValue("SignificantMultiplier", FPConfig.ISRU.Funds.SignificantMultiplier);
            isruFunds.AddValue("ExceptionalMultiplier", FPConfig.ISRU.Funds.ExceptionalMultiplier);
            isruFunds.AddValue("DeliveryMultiplier", FPConfig.ISRU.Funds.DeliveryMultiplier);
            isruScience.AddValue("BaseReward", FPConfig.ISRU.Science.BaseReward);
            isruScience.AddValue("SignificantMultiplier", FPConfig.ISRU.Science.SignificantMultiplier);
            isruScience.AddValue("ExceptionalMultiplier", FPConfig.ISRU.Science.ExceptionalMultiplier);
            isruScience.AddValue("DeliveryMultiplier", FPConfig.ISRU.Science.DeliveryMultiplier);
            isruReputation.AddValue("BaseReward", FPConfig.ISRU.Reputation.BaseReward);
            isruReputation.AddValue("BaseFailure", FPConfig.ISRU.Reputation.BaseFailure);
            isruReputation.AddValue("SignificantMultiplier", FPConfig.ISRU.Reputation.SignificantMultiplier);
            isruReputation.AddValue("ExceptionalMultiplier", FPConfig.ISRU.Reputation.ExceptionalMultiplier);
            isruReputation.AddValue("DeliveryMultiplier", FPConfig.ISRU.Reputation.DeliveryMultiplier);

            evacuateNode.AddValue("MaximumAvailable", FPConfig.Evacuate.MaximumAvailable);
            evacuateNode.AddValue("MaximumActive", FPConfig.Evacuate.MaximumActive);
            evacuateNode.AddValue("AllowDestroy", FPConfig.Evacuate.AllowDestroy);
            evacuateNode.AddValue("DestroyChance", FPConfig.Evacuate.DestroyChance);
            evacuateExpire.AddValue("MinimumExpireDays", FPConfig.Evacuate.Expire.MinimumExpireDays);
            evacuateExpire.AddValue("MaximumExpireDays", FPConfig.Evacuate.Expire.MaximumExpireDays);
            evacuateExpire.AddValue("DeadlineDays", FPConfig.Evacuate.Expire.DeadlineDays);
            evacuateFunds.AddValue("BaseAdvance", FPConfig.Evacuate.Funds.BaseAdvance);
            evacuateFunds.AddValue("BaseReward", FPConfig.Evacuate.Funds.BaseReward);
            evacuateFunds.AddValue("BaseFailure", FPConfig.Evacuate.Funds.BaseFailure);
            evacuateFunds.AddValue("CrewMultiplier", FPConfig.Evacuate.Funds.CrewMultiplier);
            evacuateFunds.AddValue("ReliefMultiplier", FPConfig.Evacuate.Funds.ReliefMultiplier);
            evacuateFunds.AddValue("ExplodeMultiplier", FPConfig.Evacuate.Funds.ExplodeMultiplier);
            evacuateScience.AddValue("BaseReward", FPConfig.Evacuate.Science.BaseReward);
            evacuateReputation.AddValue("BaseReward", FPConfig.Evacuate.Reputation.BaseReward);
            evacuateReputation.AddValue("BaseFailure", FPConfig.Evacuate.Reputation.BaseFailure);
            evacuateReputation.AddValue("CrewMultiplier", FPConfig.Evacuate.Reputation.CrewMultiplier);
            evacuateReputation.AddValue("ReliefMultiplier", FPConfig.Evacuate.Reputation.ReliefMultiplier);
            evacuateReputation.AddValue("ExplodeMultiplier", FPConfig.Evacuate.Reputation.ExplodeMultiplier);

            config.Save(ConfigFileName);
        }

        private static void LoadConfig()
        {
            if (System.IO.File.Exists(ConfigFileName))
            {
                config = ConfigNode.Load(ConfigFileName);
                ProcessConfig();
            }

            if (config == null)
                CreateDefaultConfig();
        }

        private static void ProcessConfig()
        {
            ConfigNode topNode = config.GetNode("FinePrint");

            ConfigNode aerialNode = topNode.GetNode("Aerial");
            ConfigNode aerialExpire = aerialNode.GetNode("Expiration");
            ConfigNode aerialFunds = aerialNode.GetNode("Funds");
            ConfigNode aerialScience = aerialNode.GetNode("Science");
            ConfigNode aerialReputation = aerialNode.GetNode("Reputation");

            ConfigNode armNode = topNode.GetNode("ARM");
            ConfigNode armExpire = armNode.GetNode("Expiration");
            ConfigNode armFunds = armNode.GetNode("Funds");
            ConfigNode armScience = armNode.GetNode("Science");
            ConfigNode armReputation = armNode.GetNode("Reputation");

            ConfigNode baseNode = topNode.GetNode("Base");
            ConfigNode baseExpire = baseNode.GetNode("Expiration");
            ConfigNode baseFunds = baseNode.GetNode("Funds");
            ConfigNode baseScience = baseNode.GetNode("Science");
            ConfigNode baseReputation = baseNode.GetNode("Reputation");

            ConfigNode roverNode = topNode.GetNode("Rover");
            ConfigNode roverExpire = roverNode.GetNode("Expiration");
            ConfigNode roverFunds = roverNode.GetNode("Funds");
            ConfigNode roverScience = roverNode.GetNode("Science");
            ConfigNode roverReputation = roverNode.GetNode("Reputation");

            ConfigNode satelliteNode = topNode.GetNode("Satellite");
            ConfigNode satelliteExpire = satelliteNode.GetNode("Expiration");
            ConfigNode satelliteFunds = satelliteNode.GetNode("Funds");
            ConfigNode satelliteScience = satelliteNode.GetNode("Science");
            ConfigNode satelliteReputation = satelliteNode.GetNode("Reputation");

            ConfigNode stationNode = topNode.GetNode("Station");
            ConfigNode stationExpire = stationNode.GetNode("Expiration");
            ConfigNode stationFunds = stationNode.GetNode("Funds");
            ConfigNode stationScience = stationNode.GetNode("Science");
            ConfigNode stationReputation = stationNode.GetNode("Reputation");

            ConfigNode isruNode = topNode.GetNode("ISRU");
            ConfigNode isruExpire = isruNode.GetNode("Expiration");
            ConfigNode isruFunds = isruNode.GetNode("Funds");
            ConfigNode isruScience = isruNode.GetNode("Science");
            ConfigNode isruReputation = isruNode.GetNode("Reputation");

            ConfigNode evacuateNode = topNode.GetNode("Evacuate");
            ConfigNode evacuateExpire = evacuateNode.GetNode("Expiration");
            ConfigNode evacuateFunds = evacuateNode.GetNode("Funds");
            ConfigNode evacuateScience = evacuateNode.GetNode("Science");
            ConfigNode evacuateReputation = evacuateNode.GetNode("Reputation");

            //It feeds it a reference of itself, which is modified if the function succeeds.
            //It also feeds itself in as a value, which it reverts to if it fails.
            //This ensures that the value stays at default if the load fails.

            Util.LoadNode(topNode, "FPConfig", "PatchReset", ref FPConfig.PatchReset, FPConfig.PatchReset);
            Util.LoadNode(topNode, "FPConfig", "SunStationaryName", ref FPConfig.SunStationaryName, FPConfig.SunStationaryName);
            Util.LoadNode(topNode, "FPConfig", "HomeStationaryName", ref FPConfig.HomeStationaryName, FPConfig.HomeStationaryName);
            Util.LoadNode(topNode, "FPConfig", "OtherStationaryName", ref FPConfig.OtherStationaryName, FPConfig.OtherStationaryName);
            Util.LoadNode(topNode, "FPConfig", "SunSynchronousName", ref FPConfig.SunSynchronousName, FPConfig.SunSynchronousName);
            Util.LoadNode(topNode, "FPConfig", "HomeSynchronousName", ref FPConfig.HomeSynchronousName, FPConfig.HomeSynchronousName);
            Util.LoadNode(topNode, "FPConfig", "OtherSynchronousName", ref FPConfig.OtherSynchronousName, FPConfig.OtherSynchronousName);
            Util.LoadNode(topNode, "FPConfig", "MolniyaName", ref FPConfig.MolniyaName, FPConfig.MolniyaName);

            Util.LoadNode(aerialNode, "FPConfig", "Aerial.MaximumAvailable", ref FPConfig.Aerial.MaximumAvailable, FPConfig.Aerial.MaximumAvailable);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.MaximumActive", ref FPConfig.Aerial.MaximumActive, FPConfig.Aerial.MaximumActive);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.TrivialWaypoints", ref FPConfig.Aerial.TrivialWaypoints, FPConfig.Aerial.TrivialWaypoints);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.SignificantWaypoints", ref FPConfig.Aerial.SignificantWaypoints, FPConfig.Aerial.SignificantWaypoints);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.ExceptionalWaypoints", ref FPConfig.Aerial.ExceptionalWaypoints, FPConfig.Aerial.ExceptionalWaypoints);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.TrivialHomeNearbyChance", ref FPConfig.Aerial.TrivialHomeNearbyChance, FPConfig.Aerial.TrivialHomeNearbyChance);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.SignificantHomeNearbyChance", ref FPConfig.Aerial.SignificantHomeNearbyChance, FPConfig.Aerial.SignificantHomeNearbyChance);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.ExceptionalHomeNearbyChance", ref FPConfig.Aerial.ExceptionalHomeNearbyChance, FPConfig.Aerial.ExceptionalHomeNearbyChance);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.TrivialLowAltitudeChance", ref FPConfig.Aerial.TrivialLowAltitudeChance, FPConfig.Aerial.TrivialLowAltitudeChance);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.SignificantLowAltitudeChance", ref FPConfig.Aerial.SignificantLowAltitudeChance, FPConfig.Aerial.SignificantLowAltitudeChance);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.ExceptionalLowAltitudeChance", ref FPConfig.Aerial.ExceptionalLowAltitudeChance, FPConfig.Aerial.ExceptionalLowAltitudeChance);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.TrivialHomeNearbyRange", ref FPConfig.Aerial.TrivialHomeNearbyRange, FPConfig.Aerial.TrivialHomeNearbyRange);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.SignificantHomeNearbyRange", ref FPConfig.Aerial.SignificantHomeNearbyRange, FPConfig.Aerial.SignificantHomeNearbyRange);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.ExceptionalHomeNearbyRange", ref FPConfig.Aerial.ExceptionalHomeNearbyRange, FPConfig.Aerial.ExceptionalHomeNearbyRange);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.TrivialLowAltitudeMultiplier", ref FPConfig.Aerial.TrivialLowAltitudeMultiplier, FPConfig.Aerial.TrivialLowAltitudeMultiplier);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.SignificantLowAltitudeMultiplier", ref FPConfig.Aerial.SignificantLowAltitudeMultiplier, FPConfig.Aerial.SignificantLowAltitudeMultiplier);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.ExceptionalLowAltitudeMultiplier", ref FPConfig.Aerial.ExceptionalLowAltitudeMultiplier, FPConfig.Aerial.ExceptionalLowAltitudeMultiplier);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.TrivialRange", ref FPConfig.Aerial.TrivialRange, FPConfig.Aerial.TrivialRange);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.SignificantRange", ref FPConfig.Aerial.SignificantRange, FPConfig.Aerial.SignificantRange);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.ExceptionalRange", ref FPConfig.Aerial.ExceptionalRange, FPConfig.Aerial.ExceptionalRange);
            Util.LoadNode(aerialNode, "FPConfig", "Aerial.TriggerRange", ref FPConfig.Aerial.TriggerRange, FPConfig.Aerial.TriggerRange);
            Util.LoadNode(aerialExpire, "FPConfig", "Aerial.Expire.MinimumExpireDays", ref FPConfig.Aerial.Expire.MinimumExpireDays, FPConfig.Aerial.Expire.MinimumExpireDays);
            Util.LoadNode(aerialExpire, "FPConfig", "Aerial.Expire.MaximumExpireDays", ref FPConfig.Aerial.Expire.MaximumExpireDays, FPConfig.Aerial.Expire.MaximumExpireDays);
            Util.LoadNode(aerialExpire, "FPConfig", "Aerial.Expire.DeadlineDays", ref FPConfig.Aerial.Expire.DeadlineDays, FPConfig.Aerial.Expire.DeadlineDays);
            Util.LoadNode(aerialFunds, "FPConfig", "Aerial.Funds.BaseAdvance", ref FPConfig.Aerial.Funds.BaseAdvance, FPConfig.Aerial.Funds.BaseAdvance);
            Util.LoadNode(aerialFunds, "FPConfig", "Aerial.Funds.BaseReward", ref FPConfig.Aerial.Funds.BaseReward, FPConfig.Aerial.Funds.BaseReward);
            Util.LoadNode(aerialFunds, "FPConfig", "Aerial.Funds.BaseFailure", ref FPConfig.Aerial.Funds.BaseFailure, FPConfig.Aerial.Funds.BaseFailure);
            Util.LoadNode(aerialFunds, "FPConfig", "Aerial.Funds.SignificantMultiplier", ref FPConfig.Aerial.Funds.SignificantMultiplier, FPConfig.Aerial.Funds.SignificantMultiplier);
            Util.LoadNode(aerialFunds, "FPConfig", "Aerial.Funds.ExceptionalMultiplier", ref FPConfig.Aerial.Funds.ExceptionalMultiplier, FPConfig.Aerial.Funds.ExceptionalMultiplier);
            Util.LoadNode(aerialFunds, "FPConfig", "Aerial.Funds.WaypointBaseReward", ref FPConfig.Aerial.Funds.WaypointBaseReward, FPConfig.Aerial.Funds.WaypointBaseReward);
            Util.LoadNode(aerialFunds, "FPConfig", "Aerial.Funds.WaypointSignificantMultiplier", ref FPConfig.Aerial.Funds.WaypointSignificantMultiplier, FPConfig.Aerial.Funds.WaypointSignificantMultiplier);
            Util.LoadNode(aerialFunds, "FPConfig", "Aerial.Funds.WaypointExceptionalMultiplier", ref FPConfig.Aerial.Funds.WaypointExceptionalMultiplier, FPConfig.Aerial.Funds.WaypointExceptionalMultiplier);
            Util.LoadNode(aerialScience, "FPConfig", "Aerial.Science.BaseReward", ref FPConfig.Aerial.Science.BaseReward, FPConfig.Aerial.Science.BaseReward);
            Util.LoadNode(aerialScience, "FPConfig", "Aerial.Science.SignificantMultiplier", ref FPConfig.Aerial.Science.SignificantMultiplier, FPConfig.Aerial.Science.SignificantMultiplier);
            Util.LoadNode(aerialScience, "FPConfig", "Aerial.Science.ExceptionalMultiplier", ref FPConfig.Aerial.Science.ExceptionalMultiplier, FPConfig.Aerial.Science.ExceptionalMultiplier);
            Util.LoadNode(aerialScience, "FPConfig", "Aerial.Science.WaypointBaseReward", ref FPConfig.Aerial.Science.WaypointBaseReward, FPConfig.Aerial.Science.WaypointBaseReward);
            Util.LoadNode(aerialScience, "FPConfig", "Aerial.Science.WaypointSignificantMultiplier", ref FPConfig.Aerial.Science.WaypointSignificantMultiplier, FPConfig.Aerial.Science.WaypointSignificantMultiplier);
            Util.LoadNode(aerialScience, "FPConfig", "Aerial.Science.WaypointExceptionalMultiplier", ref FPConfig.Aerial.Science.WaypointExceptionalMultiplier, FPConfig.Aerial.Science.WaypointExceptionalMultiplier);
            Util.LoadNode(aerialReputation, "FPConfig", "Aerial.Reputation.BaseReward", ref FPConfig.Aerial.Reputation.BaseReward, FPConfig.Aerial.Reputation.BaseReward);
            Util.LoadNode(aerialReputation, "FPConfig", "Aerial.Reputation.BaseFailure", ref FPConfig.Aerial.Reputation.BaseFailure, FPConfig.Aerial.Reputation.BaseFailure);
            Util.LoadNode(aerialReputation, "FPConfig", "Aerial.Reputation.SignificantMultiplier", ref FPConfig.Aerial.Reputation.SignificantMultiplier, FPConfig.Aerial.Reputation.SignificantMultiplier);
            Util.LoadNode(aerialReputation, "FPConfig", "Aerial.Reputation.ExceptionalMultiplier", ref FPConfig.Aerial.Reputation.ExceptionalMultiplier, FPConfig.Aerial.Reputation.ExceptionalMultiplier);
            Util.LoadNode(aerialReputation, "FPConfig", "Aerial.Reputation.WaypointBaseReward", ref FPConfig.Aerial.Reputation.WaypointBaseReward, FPConfig.Aerial.Reputation.WaypointBaseReward);
            Util.LoadNode(aerialReputation, "FPConfig", "Aerial.Reputation.WaypointSignificantMultiplier", ref FPConfig.Aerial.Reputation.WaypointSignificantMultiplier, FPConfig.Aerial.Reputation.WaypointSignificantMultiplier);
            Util.LoadNode(aerialReputation, "FPConfig", "Aerial.Reputation.WaypointExceptionalMultiplier", ref FPConfig.Aerial.Reputation.WaypointExceptionalMultiplier, FPConfig.Aerial.Reputation.WaypointExceptionalMultiplier);

            Util.LoadNode(armNode, "FPConfig", "ARM.MaximumExistent", ref FPConfig.ARM.MaximumExistent, FPConfig.ARM.MaximumExistent);
            Util.LoadNode(armNode, "FPConfig", "ARM.SignificantSolarEjectionChance", ref FPConfig.ARM.SignificantSolarEjectionChance, FPConfig.ARM.SignificantSolarEjectionChance);
            Util.LoadNode(armNode, "FPConfig", "ARM.ExceptionalSolarEjectionChance", ref FPConfig.ARM.ExceptionalSolarEjectionChance, FPConfig.ARM.ExceptionalSolarEjectionChance);
            Util.LoadNode(armNode, "FPConfig", "ARM.HomeLandingChance", ref FPConfig.ARM.HomeLandingChance, FPConfig.ARM.HomeLandingChance);
            Util.LoadNode(armNode, "FPConfig", "ARM.AllowSolarEjections", ref FPConfig.ARM.AllowSolarEjections, FPConfig.ARM.AllowSolarEjections);
            Util.LoadNode(armNode, "FPConfig", "ARM.AllowHomeLandings", ref FPConfig.ARM.AllowHomeLandings, FPConfig.ARM.AllowHomeLandings);
            Util.LoadNode(armExpire, "FPConfig", "ARM.Expire.MinimumExpireDays", ref FPConfig.ARM.Expire.MinimumExpireDays, FPConfig.ARM.Expire.MinimumExpireDays);
            Util.LoadNode(armExpire, "FPConfig", "ARM.Expire.MaximumExpireDays", ref FPConfig.ARM.Expire.MaximumExpireDays, FPConfig.ARM.Expire.MaximumExpireDays);
            Util.LoadNode(armExpire, "FPConfig", "ARM.Expire.DeadlineDays", ref FPConfig.ARM.Expire.DeadlineDays, FPConfig.ARM.Expire.DeadlineDays);
            Util.LoadNode(armFunds, "FPConfig", "ARM.Funds.BaseAdvance", ref FPConfig.ARM.Funds.BaseAdvance, FPConfig.ARM.Funds.BaseAdvance);
            Util.LoadNode(armFunds, "FPConfig", "ARM.Funds.BaseReward", ref FPConfig.ARM.Funds.BaseReward, FPConfig.ARM.Funds.BaseReward);
            Util.LoadNode(armFunds, "FPConfig", "ARM.Funds.BaseFailure", ref FPConfig.ARM.Funds.BaseFailure, FPConfig.ARM.Funds.BaseFailure);
            Util.LoadNode(armFunds, "FPConfig", "ARM.Funds.SignificantMultiplier", ref FPConfig.ARM.Funds.SignificantMultiplier, FPConfig.ARM.Funds.SignificantMultiplier);
            Util.LoadNode(armFunds, "FPConfig", "ARM.Funds.ExceptionalMultiplier", ref FPConfig.ARM.Funds.ExceptionalMultiplier, FPConfig.ARM.Funds.ExceptionalMultiplier);
            Util.LoadNode(armFunds, "FPConfig", "ARM.Funds.SolarEjectionMultiplier", ref FPConfig.ARM.Funds.SolarEjectionMultiplier, FPConfig.ARM.Funds.SolarEjectionMultiplier);
            Util.LoadNode(armScience, "FPConfig", "ARM.Science.BaseReward", ref FPConfig.ARM.Science.BaseReward, FPConfig.ARM.Science.BaseReward);
            Util.LoadNode(armScience, "FPConfig", "ARM.Science.SignificantMultiplier", ref FPConfig.ARM.Science.SignificantMultiplier, FPConfig.ARM.Science.SignificantMultiplier);
            Util.LoadNode(armScience, "FPConfig", "ARM.Science.ExceptionalMultiplier", ref FPConfig.ARM.Science.ExceptionalMultiplier, FPConfig.ARM.Science.ExceptionalMultiplier);
            Util.LoadNode(armScience, "FPConfig", "ARM.Science.SolarEjectionMultiplier", ref FPConfig.ARM.Science.SolarEjectionMultiplier, FPConfig.ARM.Science.SolarEjectionMultiplier);
            Util.LoadNode(armReputation, "FPConfig", "ARM.Reputation.BaseReward", ref FPConfig.ARM.Reputation.BaseReward, FPConfig.ARM.Reputation.BaseReward);
            Util.LoadNode(armReputation, "FPConfig", "ARM.Reputation.BaseFailure", ref FPConfig.ARM.Reputation.BaseFailure, FPConfig.ARM.Reputation.BaseFailure);
            Util.LoadNode(armReputation, "FPConfig", "ARM.Reputation.SignificantMultiplier", ref FPConfig.ARM.Reputation.SignificantMultiplier, FPConfig.ARM.Reputation.SignificantMultiplier);
            Util.LoadNode(armReputation, "FPConfig", "ARM.Reputation.ExceptionalMultiplier", ref FPConfig.ARM.Reputation.ExceptionalMultiplier, FPConfig.ARM.Reputation.ExceptionalMultiplier);
            Util.LoadNode(armReputation, "FPConfig", "ARM.Reputation.SolarEjectionMultiplier", ref FPConfig.ARM.Reputation.SolarEjectionMultiplier, FPConfig.ARM.Reputation.SolarEjectionMultiplier);

            Util.LoadNode(baseNode, "FPConfig", "MaximumExistent", ref FPConfig.Base.MaximumExistent, FPConfig.Base.MaximumExistent);
            Util.LoadNode(baseNode, "FPConfig", "TrivialCupolaChance", ref FPConfig.Base.TrivialCupolaChance, FPConfig.Base.TrivialCupolaChance);
            Util.LoadNode(baseNode, "FPConfig", "SignificantCupolaChance", ref FPConfig.Base.SignificantCupolaChance, FPConfig.Base.SignificantCupolaChance);
            Util.LoadNode(baseNode, "FPConfig", "ExceptionalCupolaChance", ref FPConfig.Base.ExceptionalCupolaChance, FPConfig.Base.ExceptionalCupolaChance);
            Util.LoadNode(baseNode, "FPConfig", "TrivialLabChance", ref FPConfig.Base.TrivialLabChance, FPConfig.Base.TrivialLabChance);
            Util.LoadNode(baseNode, "FPConfig", "SignificantLabChance", ref FPConfig.Base.SignificantLabChance, FPConfig.Base.SignificantLabChance);
            Util.LoadNode(baseNode, "FPConfig", "ExceptionalLabChance", ref FPConfig.Base.ExceptionalLabChance, FPConfig.Base.ExceptionalLabChance);
            Util.LoadNode(baseNode, "FPConfig", "TrivialMobileChance", ref FPConfig.Base.TrivialMobileChance, FPConfig.Base.TrivialMobileChance);
            Util.LoadNode(baseNode, "FPConfig", "SignificantMobileChance", ref FPConfig.Base.SignificantMobileChance, FPConfig.Base.SignificantMobileChance);
            Util.LoadNode(baseNode, "FPConfig", "ExceptionalMobileChance", ref FPConfig.Base.ExceptionalMobileChance, FPConfig.Base.ExceptionalMobileChance);
            Util.LoadNode(baseNode, "FPConfig", "AllowCupola", ref FPConfig.Base.AllowCupola, FPConfig.Base.AllowCupola);
            Util.LoadNode(baseNode, "FPConfig", "AllowLab", ref FPConfig.Base.AllowLab, FPConfig.Base.AllowLab);
            Util.LoadNode(baseNode, "FPConfig", "AllowMobile", ref FPConfig.Base.AllowMobile, FPConfig.Base.AllowMobile);
            Util.LoadNode(baseExpire, "FPConfig", "MinimumExpireDays", ref FPConfig.Base.Expire.MinimumExpireDays, FPConfig.Base.Expire.MinimumExpireDays);
            Util.LoadNode(baseExpire, "FPConfig", "MaximumExpireDays", ref FPConfig.Base.Expire.MaximumExpireDays, FPConfig.Base.Expire.MaximumExpireDays);
            Util.LoadNode(baseExpire, "FPConfig", "DeadlineDays", ref FPConfig.Base.Expire.DeadlineDays, FPConfig.Base.Expire.DeadlineDays);
            Util.LoadNode(baseFunds, "FPConfig", "BaseAdvance", ref FPConfig.Base.Funds.BaseAdvance, FPConfig.Base.Funds.BaseAdvance);
            Util.LoadNode(baseFunds, "FPConfig", "BaseReward", ref FPConfig.Base.Funds.BaseReward, FPConfig.Base.Funds.BaseReward);
            Util.LoadNode(baseFunds, "FPConfig", "BaseFailure", ref FPConfig.Base.Funds.BaseFailure, FPConfig.Base.Funds.BaseFailure);
            Util.LoadNode(baseFunds, "FPConfig", "SignificantMultiplier", ref FPConfig.Base.Funds.SignificantMultiplier, FPConfig.Base.Funds.SignificantMultiplier);
            Util.LoadNode(baseFunds, "FPConfig", "ExceptionalMultiplier", ref FPConfig.Base.Funds.ExceptionalMultiplier, FPConfig.Base.Funds.ExceptionalMultiplier);
            Util.LoadNode(baseFunds, "FPConfig", "CupolaMultiplier", ref FPConfig.Base.Funds.CupolaMultiplier, FPConfig.Base.Funds.CupolaMultiplier);
            Util.LoadNode(baseFunds, "FPConfig", "LabMultiplier", ref FPConfig.Base.Funds.LabMultiplier, FPConfig.Base.Funds.LabMultiplier);
            Util.LoadNode(baseFunds, "FPConfig", "MobileMultiplier", ref FPConfig.Base.Funds.MobileMultiplier, FPConfig.Base.Funds.MobileMultiplier);
            Util.LoadNode(baseScience, "FPConfig", "BaseReward", ref FPConfig.Base.Science.BaseReward, FPConfig.Base.Science.BaseReward);
            Util.LoadNode(baseScience, "FPConfig", "SignificantMultiplier", ref FPConfig.Base.Science.SignificantMultiplier, FPConfig.Base.Science.SignificantMultiplier);
            Util.LoadNode(baseScience, "FPConfig", "ExceptionalMultiplier", ref FPConfig.Base.Science.ExceptionalMultiplier, FPConfig.Base.Science.ExceptionalMultiplier);
            Util.LoadNode(baseScience, "FPConfig", "CupolaMultiplier", ref FPConfig.Base.Science.CupolaMultiplier, FPConfig.Base.Science.CupolaMultiplier);
            Util.LoadNode(baseScience, "FPConfig", "LabMultiplier", ref FPConfig.Base.Science.LabMultiplier, FPConfig.Base.Science.LabMultiplier);
            Util.LoadNode(baseScience, "FPConfig", "MobileMultiplier", ref FPConfig.Base.Science.MobileMultiplier, FPConfig.Base.Science.MobileMultiplier);
            Util.LoadNode(baseReputation, "FPConfig", "BaseReward", ref FPConfig.Base.Reputation.BaseReward, FPConfig.Base.Reputation.BaseReward);
            Util.LoadNode(baseReputation, "FPConfig", "BaseFailure", ref FPConfig.Base.Reputation.BaseFailure, FPConfig.Base.Reputation.BaseFailure);
            Util.LoadNode(baseReputation, "FPConfig", "SignificantMultiplier", ref FPConfig.Base.Reputation.SignificantMultiplier, FPConfig.Base.Reputation.SignificantMultiplier);
            Util.LoadNode(baseReputation, "FPConfig", "ExceptionalMultiplier", ref FPConfig.Base.Reputation.ExceptionalMultiplier, FPConfig.Base.Reputation.ExceptionalMultiplier);
            Util.LoadNode(baseReputation, "FPConfig", "CupolaMultiplier", ref FPConfig.Base.Reputation.CupolaMultiplier, FPConfig.Base.Reputation.CupolaMultiplier);
            Util.LoadNode(baseReputation, "FPConfig", "LabMultiplier", ref FPConfig.Base.Reputation.LabMultiplier, FPConfig.Base.Reputation.LabMultiplier);
            Util.LoadNode(baseReputation, "FPConfig", "MobileMultiplier", ref FPConfig.Base.Reputation.MobileMultiplier, FPConfig.Base.Reputation.MobileMultiplier);

            Util.LoadNode(roverNode, "FPConfig", "Rover.MaximumAvailable", ref FPConfig.Rover.MaximumAvailable, FPConfig.Rover.MaximumAvailable);
            Util.LoadNode(roverNode, "FPConfig", "Rover.MaximumActive", ref FPConfig.Rover.MaximumActive, FPConfig.Rover.MaximumActive);
            Util.LoadNode(roverNode, "FPConfig", "Rover.TrivialWaypoints", ref FPConfig.Rover.TrivialWaypoints, FPConfig.Rover.TrivialWaypoints);
            Util.LoadNode(roverNode, "FPConfig", "Rover.SignificantWaypoints", ref FPConfig.Rover.SignificantWaypoints, FPConfig.Rover.SignificantWaypoints);
            Util.LoadNode(roverNode, "FPConfig", "Rover.ExceptionalWaypoints", ref FPConfig.Rover.ExceptionalWaypoints, FPConfig.Rover.ExceptionalWaypoints);
            Util.LoadNode(roverNode, "FPConfig", "Rover.TrivialHomeNearbyChance", ref FPConfig.Rover.TrivialHomeNearbyChance, FPConfig.Rover.TrivialHomeNearbyChance);
            Util.LoadNode(roverNode, "FPConfig", "Rover.SignificantHomeNearbyChance", ref FPConfig.Rover.SignificantHomeNearbyChance, FPConfig.Rover.SignificantHomeNearbyChance);
            Util.LoadNode(roverNode, "FPConfig", "Rover.ExceptionalHomeNearbyChance", ref FPConfig.Rover.ExceptionalHomeNearbyChance, FPConfig.Rover.ExceptionalHomeNearbyChance);
            Util.LoadNode(roverNode, "FPConfig", "Rover.TrivialHomeNearbyRange", ref FPConfig.Rover.TrivialHomeNearbyRange, FPConfig.Rover.TrivialHomeNearbyRange);
            Util.LoadNode(roverNode, "FPConfig", "Rover.SignificantHomeNearbyRange", ref FPConfig.Rover.SignificantHomeNearbyRange, FPConfig.Rover.SignificantHomeNearbyRange);
            Util.LoadNode(roverNode, "FPConfig", "Rover.ExceptionalHomeNearbyRange", ref FPConfig.Rover.ExceptionalHomeNearbyRange, FPConfig.Rover.ExceptionalHomeNearbyRange);
            Util.LoadNode(roverNode, "FPConfig", "Rover.TrivialRange", ref FPConfig.Rover.TrivialRange, FPConfig.Rover.TrivialRange);
            Util.LoadNode(roverNode, "FPConfig", "Rover.SignificantRange", ref FPConfig.Rover.SignificantRange, FPConfig.Rover.SignificantRange);
            Util.LoadNode(roverNode, "FPConfig", "Rover.ExceptionalRange", ref FPConfig.Rover.ExceptionalRange, FPConfig.Rover.ExceptionalRange);
            Util.LoadNode(roverNode, "FPConfig", "Rover.TriggerRange", ref FPConfig.Rover.TriggerRange, FPConfig.Rover.TriggerRange);
            Util.LoadNode(roverExpire, "FPConfig", "Rover.Expire.MinimumExpireDays", ref FPConfig.Rover.Expire.MinimumExpireDays, FPConfig.Rover.Expire.MinimumExpireDays);
            Util.LoadNode(roverExpire, "FPConfig", "Rover.Expire.MaximumExpireDays", ref FPConfig.Rover.Expire.MaximumExpireDays, FPConfig.Rover.Expire.MaximumExpireDays);
            Util.LoadNode(roverExpire, "FPConfig", "Rover.Expire.DeadlineDays", ref FPConfig.Rover.Expire.DeadlineDays, FPConfig.Rover.Expire.DeadlineDays);
            Util.LoadNode(roverFunds, "FPConfig", "Rover.Funds.BaseAdvance", ref FPConfig.Rover.Funds.BaseAdvance, FPConfig.Rover.Funds.BaseAdvance);
            Util.LoadNode(roverFunds, "FPConfig", "Rover.Funds.BaseReward", ref FPConfig.Rover.Funds.BaseReward, FPConfig.Rover.Funds.BaseReward);
            Util.LoadNode(roverFunds, "FPConfig", "Rover.Funds.BaseFailure", ref FPConfig.Rover.Funds.BaseFailure, FPConfig.Rover.Funds.BaseFailure);
            Util.LoadNode(roverFunds, "FPConfig", "Rover.Funds.SignificantMultiplier", ref FPConfig.Rover.Funds.SignificantMultiplier, FPConfig.Rover.Funds.SignificantMultiplier);
            Util.LoadNode(roverFunds, "FPConfig", "Rover.Funds.ExceptionalMultiplier", ref FPConfig.Rover.Funds.ExceptionalMultiplier, FPConfig.Rover.Funds.ExceptionalMultiplier);
            Util.LoadNode(roverFunds, "FPConfig", "Rover.Funds.WaypointBaseReward", ref FPConfig.Rover.Funds.WaypointBaseReward, FPConfig.Rover.Funds.WaypointBaseReward);
            Util.LoadNode(roverFunds, "FPConfig", "Rover.Funds.WaypointSignificantMultiplier", ref FPConfig.Rover.Funds.WaypointSignificantMultiplier, FPConfig.Rover.Funds.WaypointSignificantMultiplier);
            Util.LoadNode(roverFunds, "FPConfig", "Rover.Funds.WaypointExceptionalMultiplier", ref FPConfig.Rover.Funds.WaypointExceptionalMultiplier, FPConfig.Rover.Funds.WaypointExceptionalMultiplier);
            Util.LoadNode(roverScience, "FPConfig", "Rover.Science.BaseReward", ref FPConfig.Rover.Science.BaseReward, FPConfig.Rover.Science.BaseReward);
            Util.LoadNode(roverScience, "FPConfig", "Rover.Science.SignificantMultiplier", ref FPConfig.Rover.Science.SignificantMultiplier, FPConfig.Rover.Science.SignificantMultiplier);
            Util.LoadNode(roverScience, "FPConfig", "Rover.Science.ExceptionalMultiplier", ref FPConfig.Rover.Science.ExceptionalMultiplier, FPConfig.Rover.Science.ExceptionalMultiplier);
            Util.LoadNode(roverScience, "FPConfig", "Rover.Science.WaypointBaseReward", ref FPConfig.Rover.Science.WaypointBaseReward, FPConfig.Rover.Science.WaypointBaseReward);
            Util.LoadNode(roverScience, "FPConfig", "Rover.Science.WaypointSignificantMultiplier", ref FPConfig.Rover.Science.WaypointSignificantMultiplier, FPConfig.Rover.Science.WaypointSignificantMultiplier);
            Util.LoadNode(roverScience, "FPConfig", "Rover.Science.WaypointExceptionalMultiplier", ref FPConfig.Rover.Science.WaypointExceptionalMultiplier, FPConfig.Rover.Science.WaypointExceptionalMultiplier);
            Util.LoadNode(roverReputation, "FPConfig", "Rover.Reputation.BaseReward", ref FPConfig.Rover.Reputation.BaseReward, FPConfig.Rover.Reputation.BaseReward);
            Util.LoadNode(roverReputation, "FPConfig", "Rover.Reputation.BaseFailure", ref FPConfig.Rover.Reputation.BaseFailure, FPConfig.Rover.Reputation.BaseFailure);
            Util.LoadNode(roverReputation, "FPConfig", "Rover.Reputation.SignificantMultiplier", ref FPConfig.Rover.Reputation.SignificantMultiplier, FPConfig.Rover.Reputation.SignificantMultiplier);
            Util.LoadNode(roverReputation, "FPConfig", "Rover.Reputation.ExceptionalMultiplier", ref FPConfig.Rover.Reputation.ExceptionalMultiplier, FPConfig.Rover.Reputation.ExceptionalMultiplier);
            Util.LoadNode(roverReputation, "FPConfig", "Rover.Reputation.WaypointBaseReward", ref FPConfig.Rover.Reputation.WaypointBaseReward, FPConfig.Rover.Reputation.WaypointBaseReward);
            Util.LoadNode(roverReputation, "FPConfig", "Rover.Reputation.WaypointSignificantMultiplier", ref FPConfig.Rover.Reputation.WaypointSignificantMultiplier, FPConfig.Rover.Reputation.WaypointSignificantMultiplier);
            Util.LoadNode(roverReputation, "FPConfig", "Rover.Reputation.WaypointExceptionalMultiplier", ref FPConfig.Rover.Reputation.WaypointExceptionalMultiplier, FPConfig.Rover.Reputation.WaypointExceptionalMultiplier);

            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.MaximumAvailable", ref FPConfig.Satellite.MaximumAvailable, FPConfig.Satellite.MaximumAvailable);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.MaximumActive", ref FPConfig.Satellite.MaximumActive, FPConfig.Satellite.MaximumActive);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.TrivialDeviation", ref FPConfig.Satellite.TrivialDeviation, FPConfig.Satellite.TrivialDeviation);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.SignificantDeviation", ref FPConfig.Satellite.SignificantDeviation, FPConfig.Satellite.SignificantDeviation);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.ExceptionalDeviation", ref FPConfig.Satellite.ExceptionalDeviation, FPConfig.Satellite.ExceptionalDeviation);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.TrivialDifficulty", ref FPConfig.Satellite.TrivialDifficulty, FPConfig.Satellite.TrivialDifficulty);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.SignificantDifficulty", ref FPConfig.Satellite.SignificantDifficulty, FPConfig.Satellite.SignificantDifficulty);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.ExceptionalDifficulty", ref FPConfig.Satellite.ExceptionalDifficulty, FPConfig.Satellite.ExceptionalDifficulty);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.TrivialHomeOverrideChance", ref FPConfig.Satellite.TrivialHomeOverrideChance, FPConfig.Satellite.TrivialHomeOverrideChance);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.SignificantHomeOverrideChance", ref FPConfig.Satellite.SignificantHomeOverrideChance, FPConfig.Satellite.SignificantHomeOverrideChance);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.ExceptionalHomeOverrideChance", ref FPConfig.Satellite.ExceptionalHomeOverrideChance, FPConfig.Satellite.ExceptionalHomeOverrideChance);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.TrivialPartChance", ref FPConfig.Satellite.TrivialPartChance, FPConfig.Satellite.TrivialPartChance);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.SignificantPartChance", ref FPConfig.Satellite.SignificantPartChance, FPConfig.Satellite.SignificantPartChance);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.ExceptionalPartChance", ref FPConfig.Satellite.ExceptionalPartChance, FPConfig.Satellite.ExceptionalPartChance);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.TrivialSolarChance", ref FPConfig.Satellite.TrivialSolarChance, FPConfig.Satellite.TrivialSolarChance);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.SignificantSolarChance", ref FPConfig.Satellite.SignificantSolarChance, FPConfig.Satellite.SignificantSolarChance);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.ExceptionalSolarChance", ref FPConfig.Satellite.ExceptionalSolarChance, FPConfig.Satellite.ExceptionalSolarChance);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.PreferHome", ref FPConfig.Satellite.PreferHome, FPConfig.Satellite.PreferHome);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.AllowSolar", ref FPConfig.Satellite.AllowSolar, FPConfig.Satellite.AllowSolar);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.AllowEquatorial", ref FPConfig.Satellite.AllowEquatorial, FPConfig.Satellite.AllowEquatorial);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.AllowPolar", ref FPConfig.Satellite.AllowPolar, FPConfig.Satellite.AllowPolar);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.AllowSynchronous", ref FPConfig.Satellite.AllowSynchronous, FPConfig.Satellite.AllowSynchronous);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.AllowStationary", ref FPConfig.Satellite.AllowStationary, FPConfig.Satellite.AllowStationary);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.AllowTundra", ref FPConfig.Satellite.AllowTundra, FPConfig.Satellite.AllowTundra);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.AllowKolniya", ref FPConfig.Satellite.AllowKolniya, FPConfig.Satellite.AllowKolniya);
            Util.LoadNode(satelliteNode, "FPConfig", "Satellite.PartRequests", ref FPConfig.Satellite.PartRequests, FPConfig.Satellite.PartRequests);
            Util.LoadNode(satelliteExpire, "FPConfig", "Satellite.Expire.MinimumExpireDays", ref FPConfig.Satellite.Expire.MinimumExpireDays, FPConfig.Satellite.Expire.MinimumExpireDays);
            Util.LoadNode(satelliteExpire, "FPConfig", "Satellite.Expire.MaximumExpireDays", ref FPConfig.Satellite.Expire.MaximumExpireDays, FPConfig.Satellite.Expire.MaximumExpireDays);
            Util.LoadNode(satelliteExpire, "FPConfig", "Satellite.Expire.DeadlineDays", ref FPConfig.Satellite.Expire.DeadlineDays, FPConfig.Satellite.Expire.DeadlineDays);
            Util.LoadNode(satelliteFunds, "FPConfig", "Satellite.Funds.BaseAdvance", ref FPConfig.Satellite.Funds.BaseAdvance, FPConfig.Satellite.Funds.BaseAdvance);
            Util.LoadNode(satelliteFunds, "FPConfig", "Satellite.Funds.BaseReward", ref FPConfig.Satellite.Funds.BaseReward, FPConfig.Satellite.Funds.BaseReward);
            Util.LoadNode(satelliteFunds, "FPConfig", "Satellite.Funds.BaseFailure", ref FPConfig.Satellite.Funds.BaseFailure, FPConfig.Satellite.Funds.BaseFailure);
            Util.LoadNode(satelliteFunds, "FPConfig", "Satellite.Funds.SignificantMultiplier", ref FPConfig.Satellite.Funds.SignificantMultiplier, FPConfig.Satellite.Funds.SignificantMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "Satellite.Funds.ExceptionalMultiplier", ref FPConfig.Satellite.Funds.ExceptionalMultiplier, FPConfig.Satellite.Funds.ExceptionalMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "Satellite.Funds.HomeMultiplier", ref FPConfig.Satellite.Funds.HomeMultiplier, FPConfig.Satellite.Funds.HomeMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "Satellite.Funds.PartMultiplier", ref FPConfig.Satellite.Funds.PartMultiplier, FPConfig.Satellite.Funds.PartMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "Satellite.Funds.PolarMultiplier", ref FPConfig.Satellite.Funds.PolarMultiplier, FPConfig.Satellite.Funds.PolarMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "Satellite.Funds.SynchronousMultiplier", ref FPConfig.Satellite.Funds.SynchronousMultiplier, FPConfig.Satellite.Funds.SynchronousMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "Satellite.Funds.StationaryMultiplier", ref FPConfig.Satellite.Funds.StationaryMultiplier, FPConfig.Satellite.Funds.StationaryMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "Satellite.Funds.TundraMultiplier", ref FPConfig.Satellite.Funds.TundraMultiplier, FPConfig.Satellite.Funds.TundraMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "Satellite.Funds.KolniyaMultiplier", ref FPConfig.Satellite.Funds.KolniyaMultiplier, FPConfig.Satellite.Funds.KolniyaMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "Satellite.Science.BaseReward", ref FPConfig.Satellite.Science.BaseReward, FPConfig.Satellite.Science.BaseReward);
            Util.LoadNode(satelliteScience, "FPConfig", "Satellite.Science.SignificantMultiplier", ref FPConfig.Satellite.Science.SignificantMultiplier, FPConfig.Satellite.Science.SignificantMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "Satellite.Science.ExceptionalMultiplier", ref FPConfig.Satellite.Science.ExceptionalMultiplier, FPConfig.Satellite.Science.ExceptionalMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "Satellite.Science.HomeMultiplier", ref FPConfig.Satellite.Science.HomeMultiplier, FPConfig.Satellite.Science.HomeMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "Satellite.Science.PartMultiplier", ref FPConfig.Satellite.Science.PartMultiplier, FPConfig.Satellite.Science.PartMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "Satellite.Science.PolarMultiplier", ref FPConfig.Satellite.Science.PolarMultiplier, FPConfig.Satellite.Science.PolarMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "Satellite.Science.SynchronousMultiplier", ref FPConfig.Satellite.Science.SynchronousMultiplier, FPConfig.Satellite.Science.SynchronousMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "Satellite.Science.StationaryMultiplier", ref FPConfig.Satellite.Science.StationaryMultiplier, FPConfig.Satellite.Science.StationaryMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "Satellite.Science.TundraMultiplier", ref FPConfig.Satellite.Science.TundraMultiplier, FPConfig.Satellite.Science.TundraMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "Satellite.Science.KolniyaMultiplier", ref FPConfig.Satellite.Science.KolniyaMultiplier, FPConfig.Satellite.Science.KolniyaMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "Satellite.Reputation.BaseReward", ref FPConfig.Satellite.Reputation.BaseReward, FPConfig.Satellite.Reputation.BaseReward);
            Util.LoadNode(satelliteReputation, "FPConfig", "Satellite.Reputation.BaseFailure", ref FPConfig.Satellite.Reputation.BaseFailure, FPConfig.Satellite.Reputation.BaseFailure);
            Util.LoadNode(satelliteReputation, "FPConfig", "Satellite.Reputation.SignificantMultiplier", ref FPConfig.Satellite.Reputation.SignificantMultiplier, FPConfig.Satellite.Reputation.SignificantMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "Satellite.Reputation.ExceptionalMultiplier", ref FPConfig.Satellite.Reputation.ExceptionalMultiplier, FPConfig.Satellite.Reputation.ExceptionalMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "Satellite.Reputation.HomeMultiplier", ref FPConfig.Satellite.Reputation.HomeMultiplier, FPConfig.Satellite.Reputation.HomeMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "Satellite.Reputation.PartMultiplier", ref FPConfig.Satellite.Reputation.PartMultiplier, FPConfig.Satellite.Reputation.PartMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "Satellite.Reputation.PolarMultiplier", ref FPConfig.Satellite.Reputation.PolarMultiplier, FPConfig.Satellite.Reputation.PolarMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "Satellite.Reputation.SynchronousMultiplier", ref FPConfig.Satellite.Reputation.SynchronousMultiplier, FPConfig.Satellite.Reputation.SynchronousMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "Satellite.Reputation.StationaryMultiplier", ref FPConfig.Satellite.Reputation.StationaryMultiplier, FPConfig.Satellite.Reputation.StationaryMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "Satellite.Reputation.TundraMultiplier", ref FPConfig.Satellite.Reputation.TundraMultiplier, FPConfig.Satellite.Reputation.TundraMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "Satellite.Reputation.KolniyaMultiplier", ref FPConfig.Satellite.Reputation.KolniyaMultiplier, FPConfig.Satellite.Reputation.KolniyaMultiplier);

            Util.LoadNode(stationNode, "FPConfig", "Station.MaximumExistent", ref FPConfig.Station.MaximumExistent, FPConfig.Station.MaximumExistent);
            Util.LoadNode(stationNode, "FPConfig", "Station.TrivialCupolaChance", ref FPConfig.Station.TrivialCupolaChance, FPConfig.Station.TrivialCupolaChance);
            Util.LoadNode(stationNode, "FPConfig", "Station.SignificantCupolaChance", ref FPConfig.Station.SignificantCupolaChance, FPConfig.Station.SignificantCupolaChance);
            Util.LoadNode(stationNode, "FPConfig", "Station.ExceptionalCupolaChance", ref FPConfig.Station.ExceptionalCupolaChance, FPConfig.Station.ExceptionalCupolaChance);
            Util.LoadNode(stationNode, "FPConfig", "Station.TrivialLabChance", ref FPConfig.Station.TrivialLabChance, FPConfig.Station.TrivialLabChance);
            Util.LoadNode(stationNode, "FPConfig", "Station.SignificantLabChance", ref FPConfig.Station.SignificantLabChance, FPConfig.Station.SignificantLabChance);
            Util.LoadNode(stationNode, "FPConfig", "Station.ExceptionalLabChance", ref FPConfig.Station.ExceptionalLabChance, FPConfig.Station.ExceptionalLabChance);
            Util.LoadNode(stationNode, "FPConfig", "Station.TrivialAsteroidChance", ref FPConfig.Station.TrivialAsteroidChance, FPConfig.Station.TrivialAsteroidChance);
            Util.LoadNode(stationNode, "FPConfig", "Station.SignificantAsteroidChance", ref FPConfig.Station.SignificantAsteroidChance, FPConfig.Station.SignificantAsteroidChance);
            Util.LoadNode(stationNode, "FPConfig", "Station.ExceptionalAsteroidChance", ref FPConfig.Station.ExceptionalAsteroidChance, FPConfig.Station.ExceptionalAsteroidChance);
            Util.LoadNode(stationNode, "FPConfig", "Station.AllowCupola", ref FPConfig.Station.AllowCupola, FPConfig.Station.AllowCupola);
            Util.LoadNode(stationNode, "FPConfig", "Station.AllowLab", ref FPConfig.Station.AllowLab, FPConfig.Station.AllowLab);
            Util.LoadNode(stationNode, "FPConfig", "Station.AllowAsteroid", ref FPConfig.Station.AllowAsteroid, FPConfig.Station.AllowAsteroid);
            Util.LoadNode(stationNode, "FPConfig", "Station.AllowSolar", ref FPConfig.Station.AllowSolar, FPConfig.Station.AllowSolar);
            Util.LoadNode(stationExpire, "FPConfig", "Station.Expire.MinimumExpireDays", ref FPConfig.Station.Expire.MinimumExpireDays, FPConfig.Station.Expire.MinimumExpireDays);
            Util.LoadNode(stationExpire, "FPConfig", "Station.Expire.MaximumExpireDays", ref FPConfig.Station.Expire.MaximumExpireDays, FPConfig.Station.Expire.MaximumExpireDays);
            Util.LoadNode(stationExpire, "FPConfig", "Station.Expire.DeadlineDays", ref FPConfig.Station.Expire.DeadlineDays, FPConfig.Station.Expire.DeadlineDays);
            Util.LoadNode(stationFunds, "FPConfig", "Station.Funds.BaseAdvance", ref FPConfig.Station.Funds.BaseAdvance, FPConfig.Station.Funds.BaseAdvance);
            Util.LoadNode(stationFunds, "FPConfig", "Station.Funds.BaseReward", ref FPConfig.Station.Funds.BaseReward, FPConfig.Station.Funds.BaseReward);
            Util.LoadNode(stationFunds, "FPConfig", "Station.Funds.BaseFailure", ref FPConfig.Station.Funds.BaseFailure, FPConfig.Station.Funds.BaseFailure);
            Util.LoadNode(stationFunds, "FPConfig", "Station.Funds.SignificantMultiplier", ref FPConfig.Station.Funds.SignificantMultiplier, FPConfig.Station.Funds.SignificantMultiplier);
            Util.LoadNode(stationFunds, "FPConfig", "Station.Funds.ExceptionalMultiplier", ref FPConfig.Station.Funds.ExceptionalMultiplier, FPConfig.Station.Funds.ExceptionalMultiplier);
            Util.LoadNode(stationFunds, "FPConfig", "Station.Funds.CupolaMultiplier", ref FPConfig.Station.Funds.CupolaMultiplier, FPConfig.Station.Funds.CupolaMultiplier);
            Util.LoadNode(stationFunds, "FPConfig", "Station.Funds.LabMultiplier", ref FPConfig.Station.Funds.LabMultiplier, FPConfig.Station.Funds.LabMultiplier);
            Util.LoadNode(stationFunds, "FPConfig", "Station.Funds.AsteroidMultiplier", ref FPConfig.Station.Funds.AsteroidMultiplier, FPConfig.Station.Funds.AsteroidMultiplier);
            Util.LoadNode(stationScience, "FPConfig", "Station.Science.BaseReward", ref FPConfig.Station.Science.BaseReward, FPConfig.Station.Science.BaseReward);
            Util.LoadNode(stationScience, "FPConfig", "Station.Science.SignificantMultiplier", ref FPConfig.Station.Science.SignificantMultiplier, FPConfig.Station.Science.SignificantMultiplier);
            Util.LoadNode(stationScience, "FPConfig", "Station.Science.ExceptionalMultiplier", ref FPConfig.Station.Science.ExceptionalMultiplier, FPConfig.Station.Science.ExceptionalMultiplier);
            Util.LoadNode(stationScience, "FPConfig", "Station.Science.CupolaMultiplier", ref FPConfig.Station.Science.CupolaMultiplier, FPConfig.Station.Science.CupolaMultiplier);
            Util.LoadNode(stationScience, "FPConfig", "Station.Science.LabMultiplier", ref FPConfig.Station.Science.LabMultiplier, FPConfig.Station.Science.LabMultiplier);
            Util.LoadNode(stationScience, "FPConfig", "Station.Science.AsteroidMultiplier", ref FPConfig.Station.Science.AsteroidMultiplier, FPConfig.Station.Science.AsteroidMultiplier);
            Util.LoadNode(stationReputation, "FPConfig", "Station.Reputation.BaseReward", ref FPConfig.Station.Reputation.BaseReward, FPConfig.Station.Reputation.BaseReward);
            Util.LoadNode(stationReputation, "FPConfig", "Station.Reputation.BaseFailure", ref FPConfig.Station.Reputation.BaseFailure, FPConfig.Station.Reputation.BaseFailure);
            Util.LoadNode(stationReputation, "FPConfig", "Station.Reputation.SignificantMultiplier", ref FPConfig.Station.Reputation.SignificantMultiplier, FPConfig.Station.Reputation.SignificantMultiplier);
            Util.LoadNode(stationReputation, "FPConfig", "Station.Reputation.ExceptionalMultiplier", ref FPConfig.Station.Reputation.ExceptionalMultiplier, FPConfig.Station.Reputation.ExceptionalMultiplier);
            Util.LoadNode(stationReputation, "FPConfig", "Station.Reputation.CupolaMultiplier", ref FPConfig.Station.Reputation.CupolaMultiplier, FPConfig.Station.Reputation.CupolaMultiplier);
            Util.LoadNode(stationReputation, "FPConfig", "Station.Reputation.LabMultiplier", ref FPConfig.Station.Reputation.LabMultiplier, FPConfig.Station.Reputation.LabMultiplier);
            Util.LoadNode(stationReputation, "FPConfig", "Station.Reputation.AsteroidMultiplier", ref FPConfig.Station.Reputation.AsteroidMultiplier, FPConfig.Station.Reputation.AsteroidMultiplier);

            Util.LoadNode(isruNode, "FPConfig", "ISRU.MaximumExistent", ref FPConfig.ISRU.MaximumExistent, FPConfig.ISRU.MaximumExistent);
            Util.LoadNode(isruNode, "FPConfig", "ISRU.TrivialExtractAmount", ref FPConfig.ISRU.TrivialExtractAmount, FPConfig.ISRU.TrivialExtractAmount);
            Util.LoadNode(isruNode, "FPConfig", "ISRU.SignificantExtractAmount", ref FPConfig.ISRU.SignificantExtractAmount, FPConfig.ISRU.SignificantExtractAmount);
            Util.LoadNode(isruNode, "FPConfig", "ISRU.ExceptionalExtractAmount", ref FPConfig.ISRU.ExceptionalExtractAmount, FPConfig.ISRU.ExceptionalExtractAmount);
            Util.LoadNode(isruNode, "FPConfig", "ISRU.TrivialDeliveryChance", ref FPConfig.ISRU.TrivialDeliveryChance, FPConfig.ISRU.TrivialDeliveryChance);
            Util.LoadNode(isruNode, "FPConfig", "ISRU.SignificantDeliveryChance", ref FPConfig.ISRU.SignificantDeliveryChance, FPConfig.ISRU.SignificantDeliveryChance);
            Util.LoadNode(isruNode, "FPConfig", "ISRU.ExceptionalDeliveryChance", ref FPConfig.ISRU.ExceptionalDeliveryChance, FPConfig.ISRU.ExceptionalDeliveryChance);
            Util.LoadNode(isruNode, "FPConfig", "ISRU.AllowableResources", ref FPConfig.ISRU.AllowableResources, FPConfig.ISRU.AllowableResources);
            Util.LoadNode(isruNode, "FPConfig", "ISRU.TechnologyUnlocks", ref FPConfig.ISRU.TechnologyUnlocks, FPConfig.ISRU.TechnologyUnlocks);
            Util.LoadNode(isruNode, "FPConfig", "ISRU.ForbiddenCelestials", ref FPConfig.ISRU.ForbiddenCelestials, FPConfig.ISRU.ForbiddenCelestials);
            Util.LoadNode(isruExpire, "FPConfig", "ISRU.Expire.MinimumExpireDays", ref FPConfig.ISRU.Expire.MinimumExpireDays, FPConfig.ISRU.Expire.MinimumExpireDays);
            Util.LoadNode(isruExpire, "FPConfig", "ISRU.Expire.MaximumExpireDays", ref FPConfig.ISRU.Expire.MaximumExpireDays, FPConfig.ISRU.Expire.MaximumExpireDays);
            Util.LoadNode(isruExpire, "FPConfig", "ISRU.Expire.DeadlineDays", ref FPConfig.ISRU.Expire.DeadlineDays, FPConfig.ISRU.Expire.DeadlineDays);
            Util.LoadNode(isruFunds, "FPConfig", "ISRU.Funds.BaseAdvance", ref FPConfig.ISRU.Funds.BaseAdvance, FPConfig.ISRU.Funds.BaseAdvance);
            Util.LoadNode(isruFunds, "FPConfig", "ISRU.Funds.BaseReward", ref FPConfig.ISRU.Funds.BaseReward, FPConfig.ISRU.Funds.BaseReward);
            Util.LoadNode(isruFunds, "FPConfig", "ISRU.Funds.BaseFailure", ref FPConfig.ISRU.Funds.BaseFailure, FPConfig.ISRU.Funds.BaseFailure);
            Util.LoadNode(isruFunds, "FPConfig", "ISRU.Funds.SignificantMultiplier", ref FPConfig.ISRU.Funds.SignificantMultiplier, FPConfig.ISRU.Funds.SignificantMultiplier);
            Util.LoadNode(isruFunds, "FPConfig", "ISRU.Funds.ExceptionalMultiplier", ref FPConfig.ISRU.Funds.ExceptionalMultiplier, FPConfig.ISRU.Funds.ExceptionalMultiplier);
            Util.LoadNode(isruFunds, "FPConfig", "ISRU.Funds.DeliveryMultiplier", ref FPConfig.ISRU.Funds.DeliveryMultiplier, FPConfig.ISRU.Funds.DeliveryMultiplier);
            Util.LoadNode(isruScience, "FPConfig", "ISRU.Science.BaseReward", ref FPConfig.ISRU.Science.BaseReward, FPConfig.ISRU.Science.BaseReward);
            Util.LoadNode(isruScience, "FPConfig", "ISRU.Science.SignificantMultiplier", ref FPConfig.ISRU.Science.SignificantMultiplier, FPConfig.ISRU.Science.SignificantMultiplier);
            Util.LoadNode(isruScience, "FPConfig", "ISRU.Science.ExceptionalMultiplier", ref FPConfig.ISRU.Science.ExceptionalMultiplier, FPConfig.ISRU.Science.ExceptionalMultiplier);
            Util.LoadNode(isruScience, "FPConfig", "ISRU.Science.DeliveryMultiplier", ref FPConfig.ISRU.Science.DeliveryMultiplier, FPConfig.ISRU.Science.DeliveryMultiplier);
            Util.LoadNode(isruReputation, "FPConfig", "ISRU.Reputation.BaseReward", ref FPConfig.ISRU.Reputation.BaseReward, FPConfig.ISRU.Reputation.BaseReward);
            Util.LoadNode(isruReputation, "FPConfig", "ISRU.Reputation.BaseFailure", ref FPConfig.ISRU.Reputation.BaseFailure, FPConfig.ISRU.Reputation.BaseFailure);
            Util.LoadNode(isruReputation, "FPConfig", "ISRU.Reputation.SignificantMultiplier", ref FPConfig.ISRU.Reputation.SignificantMultiplier, FPConfig.ISRU.Reputation.SignificantMultiplier);
            Util.LoadNode(isruReputation, "FPConfig", "ISRU.Reputation.ExceptionalMultiplier", ref FPConfig.ISRU.Reputation.ExceptionalMultiplier, FPConfig.ISRU.Reputation.ExceptionalMultiplier);
            Util.LoadNode(isruReputation, "FPConfig", "ISRU.Reputation.DeliveryMultiplier", ref FPConfig.ISRU.Reputation.DeliveryMultiplier, FPConfig.ISRU.Reputation.DeliveryMultiplier);

            Util.LoadNode(evacuateExpire, "FPConfig", "Evacuate.AllowDestroy", ref FPConfig.Evacuate.AllowDestroy, FPConfig.Evacuate.AllowDestroy);
            Util.LoadNode(evacuateExpire, "FPConfig", "Evacuate.DestroyChance", ref FPConfig.Evacuate.DestroyChance, FPConfig.Evacuate.DestroyChance);
            Util.LoadNode(evacuateNode, "FPConfig", "Evacuate.MaximumAvailable", ref FPConfig.Evacuate.MaximumAvailable, FPConfig.Evacuate.MaximumAvailable);
            Util.LoadNode(evacuateNode, "FPConfig", "Evacuate.MaximumActive", ref FPConfig.Evacuate.MaximumActive, FPConfig.Evacuate.MaximumActive);
            Util.LoadNode(evacuateExpire, "FPConfig", "Evacuate.Expire.MinimumExpireDays", ref FPConfig.Evacuate.Expire.MinimumExpireDays, FPConfig.Evacuate.Expire.MinimumExpireDays);
            Util.LoadNode(evacuateExpire, "FPConfig", "Evacuate.Expire.MaximumExpireDays", ref FPConfig.Evacuate.Expire.MaximumExpireDays, FPConfig.Evacuate.Expire.MaximumExpireDays);
            Util.LoadNode(evacuateExpire, "FPConfig", "Evacuate.Expire.DeadlineDays", ref FPConfig.Evacuate.Expire.DeadlineDays, FPConfig.Evacuate.Expire.DeadlineDays);
            Util.LoadNode(evacuateFunds, "FPConfig", "Evacuate.Funds.BaseAdvance", ref FPConfig.Evacuate.Funds.BaseAdvance, FPConfig.Evacuate.Funds.BaseAdvance);
            Util.LoadNode(evacuateFunds, "FPConfig", "Evacuate.Funds.BaseReward", ref FPConfig.Evacuate.Funds.BaseReward, FPConfig.Evacuate.Funds.BaseReward);
            Util.LoadNode(evacuateFunds, "FPConfig", "Evacuate.Funds.BaseFailure", ref FPConfig.Evacuate.Funds.BaseFailure, FPConfig.Evacuate.Funds.BaseFailure);
            Util.LoadNode(evacuateFunds, "FPConfig", "Evacuate.Funds.CrewMultiplier", ref FPConfig.Evacuate.Funds.CrewMultiplier, FPConfig.Evacuate.Funds.CrewMultiplier);
            Util.LoadNode(evacuateFunds, "FPConfig", "Evacuate.Funds.ExplodeMultiplier", ref FPConfig.Evacuate.Funds.ExplodeMultiplier, FPConfig.Evacuate.Funds.ExplodeMultiplier);
            Util.LoadNode(evacuateFunds, "FPConfig", "Evacuate.Funds.ReliefMultiplier", ref FPConfig.Evacuate.Funds.ReliefMultiplier, FPConfig.Evacuate.Funds.ReliefMultiplier);
            Util.LoadNode(evacuateFunds, "FPConfig", "Evacuate.Funds.TroubleMultiplier", ref FPConfig.Evacuate.Funds.TroubleMultiplier, FPConfig.Evacuate.Funds.TroubleMultiplier);
            Util.LoadNode(evacuateScience, "FPConfig", "Evacuate.Science.BaseReward", ref FPConfig.Evacuate.Science.BaseReward, FPConfig.Evacuate.Science.BaseReward);
            Util.LoadNode(evacuateReputation, "FPConfig", "Evacuate.Reputation.BaseReward", ref FPConfig.Evacuate.Reputation.BaseReward, FPConfig.Evacuate.Reputation.BaseReward);
            Util.LoadNode(evacuateReputation, "FPConfig", "Evacuate.Reputation.BaseFailure", ref FPConfig.Evacuate.Reputation.BaseFailure, FPConfig.Evacuate.Reputation.BaseFailure);
            Util.LoadNode(evacuateReputation, "FPConfig", "Evacuate.Reputation.CrewMultiplier", ref FPConfig.Evacuate.Reputation.CrewMultiplier, FPConfig.Evacuate.Reputation.CrewMultiplier);
            Util.LoadNode(evacuateReputation, "FPConfig", "Evacuate.Reputation.ExplodeMultiplier", ref FPConfig.Evacuate.Reputation.ExplodeMultiplier, FPConfig.Evacuate.Reputation.ExplodeMultiplier);
            Util.LoadNode(evacuateReputation, "FPConfig", "Evacuate.Reputation.ReliefMultiplier", ref FPConfig.Evacuate.Reputation.ReliefMultiplier, FPConfig.Evacuate.Reputation.ReliefMultiplier);
            Util.LoadNode(evacuateReputation, "FPConfig", "Evacuate.Reputation.TroubleMultiplier", ref FPConfig.Evacuate.Reputation.TroubleMultiplier, FPConfig.Evacuate.Reputation.TroubleMultiplier);
        }
    }
}