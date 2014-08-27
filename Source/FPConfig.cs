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

namespace FinePrint
{
    //Real programmers turn away from this dark and desolate place.
    //It is important that the mod has default values to fall back on for config creation and error recovery.
    //There are a lot of configurable values, and most of these values are hand picked, so a loop wouldn't work.
    //So I felt it best to do this manually, as messy as it may be.

    //Class structure mimicks the configuration file, for my own sanity.

    //At the bottom are functions to create a default configuration file if one doesn't exist, and one to load the file on disk.
    //The load function does sort of a feedback loop, it starts at program start with the default value, the value is fed into the load function with a copy of itself.
    //If load fails, it stays at default, if not, it changes to what is on disk.

    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class FPConfig : MonoBehaviour
    {
        public static ConfigNode config;

        private static FPConfig instance;
        private static float timer;

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

            if (System.IO.File.Exists(ConfigFileName()))
            {
                config = ConfigNode.Load(ConfigFileName());
                LoadConfig();
            }

            if (config == null)
                CreateDefaultConfig();

            timer = 0f;
        }

        public void Update()
        {
            timer += UnityEngine.Time.deltaTime;

            if (timer > 10f)
            {
                timer = 0f;
                config = ConfigNode.Load(ConfigFileName());
                LoadConfig();
            }
        }

        public static class Aerial
        {
            public static int MaximumAvailable = 2;
            public static int MaximumActive = 4;
            public static int TrivialWaypoints = 1;
            public static int SignificantWaypoints = 2;
            public static int ExceptionalWaypoints = 3;
            public static double TrivialRange = 100000.0;
            public static double SignificantRange = 200000.0;
            public static double ExceptionalRange = 300000.0;

            public static class Expire
            {
                public static int MinimumExpireDays = 1;
                public static int MaximumExpireDays = 7;
                public static int DeadlineDays = 2130;
            }

            public static class Funds
            {
                public static float BaseAdvance = 4000;
                public static float BaseReward = 17500;
                public static float BaseFailure = 0;
                public static float SignificantMultiplier = 1;
                public static float ExceptionalMultiplier = 1;
                public static float WaypointBaseReward = 3750;
                public static float WaypointSignificantMultiplier = 1;
                public static float WaypointExceptionalMultiplier = 1;
            }

            public static class Science
            {
                public static float BaseReward = 0;
                public static float SignificantMultiplier = 1;
                public static float ExceptionalMultiplier = 1;
                public static float WaypointBaseReward = 7.5f;
                public static float WaypointSignificantMultiplier = 1;
                public static float WaypointExceptionalMultiplier = 1;
            }

            public static class Reputation
            {
                public static float BaseReward = 50;
                public static float BaseFailure = 25;
                public static float SignificantMultiplier = 1;
                public static float ExceptionalMultiplier = 1;
                public static float WaypointBaseReward = 7.5f;
                public static float WaypointSignificantMultiplier = 1;
                public static float WaypointExceptionalMultiplier = 1;
            }
        }

        public static class ARM
        {
            public static int MaximumAvailable = 2;
            public static int MaximumActive = 2;
            public static bool AllowSolarEjections = true;
            public static bool AllowHomeLandings = true;
            public static float SignificantSolarEjectionChance = 10;
            public static float ExceptionalSolarEjectionChance = 30;
            public static float HomeLandingChance = 20;

            public static class Expire
            {
                public static int MinimumExpireDays = 1;
                public static int MaximumExpireDays = 7;
                public static int DeadlineDays = 2982;
            }

            public static class Funds
            {
                public static float BaseAdvance = 50000;
                public static float BaseReward = 90000;
                public static float BaseFailure = 0;
                public static float SignificantMultiplier = 1;
                public static float ExceptionalMultiplier = 1;
                public static float SolarEjectionMultiplier = 1;
            }

            public static class Science
            {
                public static float BaseReward = 225;
                public static float SignificantMultiplier = 1;
                public static float ExceptionalMultiplier = 1;
                public static float SolarEjectionMultiplier = 1;
            }

            public static class Reputation
            {
                public static float BaseReward = 450;
                public static float BaseFailure = 225;
                public static float SignificantMultiplier = 1;
                public static float ExceptionalMultiplier = 1;
                public static float SolarEjectionMultiplier = 1;
            }
        }

        public static class Base
        {
            public static int MaximumExistent = 2;
            public static bool AllowMobile = true;
            public static bool AllowCupola = true;
            public static bool AllowLab = true;
            public static float TrivialMobileChance = 1;
            public static float TrivialCupolaChance = 1;
            public static float TrivialLabChance = 1;
            public static float SignificantMobileChance = 1;
            public static float SignificantCupolaChance = 1;
            public static float SignificantLabChance = 1;
            public static float ExceptionalMobileChance = 1;
            public static float ExceptionalCupolaChance = 1;
            public static float ExceptionalLabChance = 1;

            public static class Expire
            {
                public static int MinimumExpireDays = 1;
                public static int MaximumExpireDays = 7;
                public static int DeadlineDays = 2982;
            }

            public static class Funds
            {
                public static float BaseAdvance = 2;
                public static float BaseReward = 2;
                public static float BaseFailure = 2;
                public static float SignificantMultiplier = 2;
                public static float ExceptionalMultiplier = 2;
                public static float CupolaMultiplier = 2;
                public static float LabMultiplier = 2;
                public static float MobileMultiplier = 2;
            }

            public static class Science
            {
                public static float BaseReward = 2;
                public static float SignificantMultiplier = 2;
                public static float ExceptionalMultiplier = 2;
                public static float CupolaMultiplier = 2;
                public static float LabMultiplier = 2;
                public static float MobileMultiplier = 2;
            }

            public static class Reputation
            {
                public static float BaseReward = 2;
                public static float BaseFailure = 2;
                public static float SignificantMultiplier = 2;
                public static float ExceptionalMultiplier = 2;
                public static float CupolaMultiplier = 2;
                public static float LabMultiplier = 2;
                public static float MobileMultiplier = 2;
            }
        }

        public static class Rover
        {
            public static int MaximumAvailable = 2;
            public static int MaximumActive = 4;
            public static int TrivialWaypoints = 3;
            public static int SignificantWaypoints = 5;
            public static int ExceptionalWaypoints = 7;
            public static double TrivialRange = 2000;
            public static double SignificantRange = 4000;
            public static double ExceptionalRange = 6000;

            public static class Expire
            {
                public static int MinimumExpireDays = 1;
                public static int MaximumExpireDays = 7;
                public static int DeadlineDays = 2130;
            }

            public static class Funds
            {
                public static float BaseAdvance = 4000;
                public static float BaseReward = 17500;
                public static float BaseFailure = 0;
                public static float WaypointBaseReward = 5000;
                public static float WaypointSignificantMultiplier = 1;
                public static float WaypointExceptionalMultiplier = 1;
                public static float SignificantMultiplier = 1;
                public static float ExceptionalMultiplier = 1;
            }

            public static class Science
            {
                public static float BaseReward = 0;
                public static float WaypointBaseReward = 10;
                public static float WaypointSignificantMultiplier = 1;
                public static float WaypointExceptionalMultiplier = 1;
                public static float SignificantMultiplier = 1;
                public static float ExceptionalMultiplier = 1;
            }

            public static class Reputation
            {
                public static float BaseReward = 50;
                public static float BaseFailure = 25;
                public static float SignificantMultiplier = 1;
                public static float ExceptionalMultiplier = 1;
                public static float WaypointBaseReward = 10;
                public static float WaypointSignificantMultiplier = 1;
                public static float WaypointExceptionalMultiplier = 1;
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
            public static float TrivialPartChance = 20;
            public static float SignificantPartChance = 40;
            public static float ExceptionalPartChance = 25;
            public static float TrivialHomeOverrideChance = 20;
            public static float SignificantHomeOverrideChance = 40;
            public static float ExceptionalHomeOverrideChance = 25;
            public static float TrivialSolarChance = 20;
            public static float SignificantSolarChance = 40;
            public static float ExceptionalSolarChance = 25;
            public static bool PreferHome = true;
            public static bool AllowSolar = true;
            public static bool AllowEquatorial = true;
            public static bool AllowPolar = true;
            public static bool AllowSynchronous = true;
            public static bool AllowStationary = true;
            public static bool AllowKolniya = true;
            public static bool AllowTundra = true;

            public static class Expire
            {
                public static int MinimumExpireDays = 1;
                public static int MaximumExpireDays = 7;
                public static int DeadlineDays = 2130;
            }

            public static class Funds
            {
                public static float BaseAdvance = 3000;
                public static float BaseReward = 15000;
                public static float BaseFailure = 2;
                public static float SignificantMultiplier = 2;
                public static float ExceptionalMultiplier = 2;
                public static float PolarMultiplier = 2;
                public static float SynchronousMultiplier = 2;
                public static float StationaryMultiplier = 2;
                public static float KolniyaMultiplier = 2;
                public static float TundraMultiplier = 2;
                public static float PartMultiplier = 2;
                public static float HomeMultiplier = 2;
            }

            public static class Science
            {
                public static float BaseReward = 2;
                public static float SignificantMultiplier = 2;
                public static float ExceptionalMultiplier = 2;
                public static float PolarMultiplier = 2;
                public static float SynchronousMultiplier = 2;
                public static float StationaryMultiplier = 2;
                public static float KolniyaMultiplier = 2;
                public static float TundraMultiplier = 2;
                public static float PartMultiplier = 2;
                public static float HomeMultiplier = 2;
            }

            public static class Reputation
            {
                public static float BaseReward = 2;
                public static float BaseFailure = 2;
                public static float SignificantMultiplier = 2;
                public static float ExceptionalMultiplier = 2;
                public static float PolarMultiplier = 2;
                public static float SynchronousMultiplier = 2;
                public static float StationaryMultiplier = 2;
                public static float KolniyaMultiplier = 2;
                public static float TundraMultiplier = 2;
                public static float PartMultiplier = 2;
                public static float HomeMultiplier = 2;
            }
        }

        public static class Station
        {
            public static int MaximumExistent = 2;
            public static bool AllowAsteroid = true;
            public static bool AllowCupola = true;
            public static bool AllowLab = true;
            public static bool AllowSolar = true;
            public static float TrivialAsteroidChance = 1;
            public static float TrivialCupolaChance = 1;
            public static float TrivialLabChance = 1;
            public static float SignificantAsteroidChance = 1;
            public static float SignificantCupolaChance = 1;
            public static float SignificantLabChance = 1;
            public static float ExceptionalAsteroidChance = 1;
            public static float ExceptionalCupolaChance = 1;
            public static float ExceptionalLabChance = 1;

            public static class Expire
            {
                public static int MinimumExpireDays = 1;
                public static int MaximumExpireDays = 7;
                public static int DeadlineDays = 2982;
            }

            public static class Funds
            {
                public static float BaseAdvance = 2;
                public static float BaseReward = 2;
                public static float BaseFailure = 2;
                public static float SignificantMultiplier = 2;
                public static float ExceptionalMultiplier = 2;
                public static float CupolaMultiplier = 2;
                public static float LabMultiplier = 2;
                public static float AsteroidMultiplier = 2;
            }

            public static class Science
            {
                public static float BaseReward = 2;
                public static float SignificantMultiplier = 2;
                public static float ExceptionalMultiplier = 2;
                public static float CupolaMultiplier = 2;
                public static float LabMultiplier = 2;
                public static float AsteroidMultiplier = 2;
            }

            public static class Reputation
            {
                public static float BaseReward = 2;
                public static float BaseFailure = 2;
                public static float SignificantMultiplier = 2;
                public static float ExceptionalMultiplier = 2;
                public static float CupolaMultiplier = 2;
                public static float LabMultiplier = 2;
                public static float AsteroidMultiplier = 2;
            }
        }

        public static string ConfigFileName()
        {
            return Path.GetFullPath(KSPUtil.ApplicationRootPath) + "GameData/FinePrint/FinePrint.cfg";
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
            aerialNode.AddValue("TrivialRange", FPConfig.Aerial.TrivialRange);
            aerialNode.AddValue("SignificantRange", FPConfig.Aerial.SignificantRange);
            aerialNode.AddValue("ExceptionalRange", FPConfig.Aerial.ExceptionalRange);
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

            armNode.AddValue("MaximumAvailable", FPConfig.ARM.MaximumAvailable);
            armNode.AddValue("MaximumActive", FPConfig.ARM.MaximumActive);
            armNode.AddValue("AllowSolarEjections", FPConfig.ARM.AllowSolarEjections);
            armNode.AddValue("AllowHomeLandings", FPConfig.ARM.AllowHomeLandings);
            armNode.AddValue("SignificantSolarEjectionChance", FPConfig.ARM.SignificantSolarEjectionChance);
            armNode.AddValue("ExceptionalSolarEjectionChance", FPConfig.ARM.ExceptionalSolarEjectionChance);
            armNode.AddValue("HomeLandingChance", FPConfig.ARM.HomeLandingChance);
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
            baseNode.AddValue("AllowMobile", FPConfig.Base.AllowMobile);
            baseNode.AddValue("AllowCupola", FPConfig.Base.AllowCupola);
            baseNode.AddValue("AllowLab", FPConfig.Base.AllowLab);
            baseNode.AddValue("TrivialMobileChance", FPConfig.Base.TrivialMobileChance);
            baseNode.AddValue("SignificantMobileChance", FPConfig.Base.SignificantMobileChance);
            baseNode.AddValue("ExceptionalMobileChance", FPConfig.Base.ExceptionalMobileChance);
            baseNode.AddValue("TrivialCupolaChance", FPConfig.Base.TrivialCupolaChance);
            baseNode.AddValue("SignificantCupolaChance", FPConfig.Base.SignificantCupolaChance);
            baseNode.AddValue("ExceptionalCupolaChance", FPConfig.Base.ExceptionalCupolaChance);
            baseNode.AddValue("TrivialLabChance", FPConfig.Base.TrivialLabChance);
            baseNode.AddValue("SignificantLabChance", FPConfig.Base.SignificantLabChance);
            baseNode.AddValue("ExceptionalLabChance", FPConfig.Base.ExceptionalLabChance);
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
            roverNode.AddValue("TrivialRange", FPConfig.Rover.TrivialRange);
            roverNode.AddValue("SignificantRange", FPConfig.Rover.SignificantRange);
            roverNode.AddValue("ExceptionalRange", FPConfig.Rover.ExceptionalRange);
            roverExpire.AddValue("MinimumExpireDays", FPConfig.Rover.Expire.MinimumExpireDays);
            roverExpire.AddValue("MaximumExpireDays", FPConfig.Rover.Expire.MaximumExpireDays);
            roverExpire.AddValue("DeadlineDays", FPConfig.Rover.Expire.DeadlineDays);
            roverFunds.AddValue("BaseAdvance", FPConfig.Rover.Funds.BaseAdvance);
            roverFunds.AddValue("BaseReward", FPConfig.Rover.Funds.BaseReward);
            roverFunds.AddValue("BaseFailure", FPConfig.Rover.Funds.BaseFailure);
            roverFunds.AddValue("WaypointBaseReward", FPConfig.Rover.Funds.WaypointBaseReward);
            roverFunds.AddValue("WaypointSignificantMultiplier", FPConfig.Rover.Funds.WaypointSignificantMultiplier);
            roverFunds.AddValue("WaypointExceptionalMultiplier", FPConfig.Rover.Funds.WaypointExceptionalMultiplier);
            roverFunds.AddValue("SignificantMultiplier", FPConfig.Rover.Funds.SignificantMultiplier);
            roverFunds.AddValue("ExceptionalMultiplier", FPConfig.Rover.Funds.ExceptionalMultiplier);
            roverScience.AddValue("BaseReward", FPConfig.Rover.Science.BaseReward);
            roverScience.AddValue("WaypointBaseReward", FPConfig.Rover.Science.WaypointBaseReward);
            roverScience.AddValue("WaypointSignificantMultiplier", FPConfig.Rover.Science.WaypointSignificantMultiplier);
            roverScience.AddValue("WaypointExceptionalMultiplier", FPConfig.Rover.Science.WaypointExceptionalMultiplier);
            roverScience.AddValue("SignificantMultiplier", FPConfig.Rover.Science.SignificantMultiplier);
            roverScience.AddValue("ExceptionalMultiplier", FPConfig.Rover.Science.ExceptionalMultiplier);
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
            satelliteNode.AddValue("TrivialPartChance", FPConfig.Satellite.TrivialPartChance);
            satelliteNode.AddValue("SignificantPartChance", FPConfig.Satellite.SignificantPartChance);
            satelliteNode.AddValue("ExceptionalPartChance", FPConfig.Satellite.ExceptionalPartChance);
            satelliteNode.AddValue("TrivialHomeOverrideChance", FPConfig.Satellite.TrivialHomeOverrideChance);
            satelliteNode.AddValue("SignificantHomeOverrideChance", FPConfig.Satellite.SignificantHomeOverrideChance);
            satelliteNode.AddValue("ExceptionalHomeOverrideChance", FPConfig.Satellite.ExceptionalHomeOverrideChance);
            satelliteNode.AddValue("TrivialSolarChance", FPConfig.Satellite.TrivialSolarChance);
            satelliteNode.AddValue("SignificantSolarChance", FPConfig.Satellite.SignificantSolarChance);
            satelliteNode.AddValue("ExceptionalSolarChance", FPConfig.Satellite.ExceptionalSolarChance);
            satelliteNode.AddValue("PreferHome", FPConfig.Satellite.PreferHome);
            satelliteNode.AddValue("AllowSolar", FPConfig.Satellite.AllowSolar);
            satelliteNode.AddValue("AllowEquatorial", FPConfig.Satellite.AllowEquatorial);
            satelliteNode.AddValue("AllowPolar", FPConfig.Satellite.AllowPolar);
            satelliteNode.AddValue("AllowSynchronous", FPConfig.Satellite.AllowSynchronous);
            satelliteNode.AddValue("AllowStationary", FPConfig.Satellite.AllowStationary);
            satelliteNode.AddValue("AllowKolniya", FPConfig.Satellite.AllowKolniya);
            satelliteNode.AddValue("AllowTundra", FPConfig.Satellite.AllowTundra);
            satelliteExpire.AddValue("MinimumExpireDays", FPConfig.Satellite.Expire.MinimumExpireDays);
            satelliteExpire.AddValue("MaximumExpireDays", FPConfig.Satellite.Expire.MaximumExpireDays);
            satelliteExpire.AddValue("DeadlineDays", FPConfig.Satellite.Expire.DeadlineDays);
            satelliteFunds.AddValue("BaseAdvance", FPConfig.Satellite.Funds.BaseAdvance);
            satelliteFunds.AddValue("BaseReward", FPConfig.Satellite.Funds.BaseReward);
            satelliteFunds.AddValue("BaseFailure", FPConfig.Satellite.Funds.BaseFailure);
            satelliteFunds.AddValue("SignificantMultiplier", FPConfig.Satellite.Funds.SignificantMultiplier);
            satelliteFunds.AddValue("ExceptionalMultiplier", FPConfig.Satellite.Funds.ExceptionalMultiplier);
            satelliteFunds.AddValue("PolarMultiplier", FPConfig.Satellite.Funds.PolarMultiplier);
            satelliteFunds.AddValue("SynchronousMultiplier", FPConfig.Satellite.Funds.SynchronousMultiplier);
            satelliteFunds.AddValue("StationaryMultiplier", FPConfig.Satellite.Funds.StationaryMultiplier);
            satelliteFunds.AddValue("KolniyaMultiplier", FPConfig.Satellite.Funds.KolniyaMultiplier);
            satelliteFunds.AddValue("TundraMultiplier", FPConfig.Satellite.Funds.TundraMultiplier);
            satelliteFunds.AddValue("PartMultiplier", FPConfig.Satellite.Funds.PartMultiplier);
            satelliteFunds.AddValue("HomeMultiplier", FPConfig.Satellite.Funds.HomeMultiplier);
            satelliteScience.AddValue("BaseReward", FPConfig.Satellite.Science.BaseReward);
            satelliteScience.AddValue("SignificantMultiplier", FPConfig.Satellite.Science.SignificantMultiplier);
            satelliteScience.AddValue("ExceptionalMultiplier", FPConfig.Satellite.Science.ExceptionalMultiplier);
            satelliteScience.AddValue("PolarMultiplier", FPConfig.Satellite.Science.PolarMultiplier);
            satelliteScience.AddValue("SynchronousMultiplier", FPConfig.Satellite.Science.SynchronousMultiplier);
            satelliteScience.AddValue("StationaryMultiplier", FPConfig.Satellite.Science.StationaryMultiplier);
            satelliteScience.AddValue("KolniyaMultiplier", FPConfig.Satellite.Science.KolniyaMultiplier);
            satelliteScience.AddValue("TundraMultiplier", FPConfig.Satellite.Science.TundraMultiplier);
            satelliteScience.AddValue("PartMultiplier", FPConfig.Satellite.Science.PartMultiplier);
            satelliteScience.AddValue("HomeMultiplier", FPConfig.Satellite.Science.HomeMultiplier);
            satelliteReputation.AddValue("BaseReward", FPConfig.Satellite.Reputation.BaseReward);
            satelliteReputation.AddValue("BaseFailure", FPConfig.Satellite.Reputation.BaseFailure);
            satelliteReputation.AddValue("SignificantMultiplier", FPConfig.Satellite.Reputation.SignificantMultiplier);
            satelliteReputation.AddValue("ExceptionalMultiplier", FPConfig.Satellite.Reputation.ExceptionalMultiplier);
            satelliteReputation.AddValue("PolarMultiplier", FPConfig.Satellite.Reputation.PolarMultiplier);
            satelliteReputation.AddValue("SynchronousMultiplier", FPConfig.Satellite.Reputation.SynchronousMultiplier);
            satelliteReputation.AddValue("StationaryMultiplier", FPConfig.Satellite.Reputation.StationaryMultiplier);
            satelliteReputation.AddValue("KolniyaMultiplier", FPConfig.Satellite.Reputation.KolniyaMultiplier);
            satelliteReputation.AddValue("TundraMultiplier", FPConfig.Satellite.Reputation.TundraMultiplier);
            satelliteReputation.AddValue("PartMultiplier", FPConfig.Satellite.Reputation.PartMultiplier);
            satelliteReputation.AddValue("HomeMultiplier", FPConfig.Satellite.Reputation.HomeMultiplier);

            stationNode.AddValue("MaximumExistent", FPConfig.Station.MaximumExistent);
            stationNode.AddValue("AllowAsteroid", FPConfig.Station.AllowAsteroid);
            stationNode.AddValue("AllowCupola", FPConfig.Station.AllowCupola);
            stationNode.AddValue("AllowLab", FPConfig.Station.AllowLab);
            stationNode.AddValue("AllowSolar", FPConfig.Station.AllowSolar);
            stationNode.AddValue("TrivialAsteroidChance", FPConfig.Station.TrivialAsteroidChance);
            stationNode.AddValue("SignificantAsteroidChance", FPConfig.Station.SignificantAsteroidChance);
            stationNode.AddValue("ExceptionalAsteroidChance", FPConfig.Station.ExceptionalAsteroidChance);
            stationNode.AddValue("TrivialCupolaChance", FPConfig.Station.TrivialCupolaChance);
            stationNode.AddValue("SignificantCupolaChance", FPConfig.Station.SignificantCupolaChance);
            stationNode.AddValue("ExceptionalCupolaChance", FPConfig.Station.ExceptionalCupolaChance);
            stationNode.AddValue("TrivialLabChance", FPConfig.Station.TrivialLabChance);
            stationNode.AddValue("SignificantLabChance", FPConfig.Station.SignificantLabChance);
            stationNode.AddValue("ExceptionalLabChance", FPConfig.Station.ExceptionalLabChance);
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

            config.Save(ConfigFileName());
        }

        private static void LoadConfig()
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

            Util.LoadNode(aerialNode, "FPConfig", "MaximumAvailable", ref FPConfig.Aerial.MaximumAvailable, FPConfig.Aerial.MaximumAvailable);
            Util.LoadNode(aerialNode, "FPConfig", "MaximumActive", ref FPConfig.Aerial.MaximumActive, FPConfig.Aerial.MaximumActive);
            Util.LoadNode(aerialNode, "FPConfig", "TrivialWaypoints", ref FPConfig.Aerial.TrivialWaypoints, FPConfig.Aerial.TrivialWaypoints);
            Util.LoadNode(aerialNode, "FPConfig", "SignificantWaypoints", ref FPConfig.Aerial.SignificantWaypoints, FPConfig.Aerial.SignificantWaypoints);
            Util.LoadNode(aerialNode, "FPConfig", "ExceptionalWaypoints", ref FPConfig.Aerial.ExceptionalWaypoints, FPConfig.Aerial.ExceptionalWaypoints);
            Util.LoadNode(aerialNode, "FPConfig", "TrivialRange", ref FPConfig.Aerial.TrivialRange, FPConfig.Aerial.TrivialRange);
            Util.LoadNode(aerialNode, "FPConfig", "SignificantRange", ref FPConfig.Aerial.SignificantRange, FPConfig.Aerial.SignificantRange);
            Util.LoadNode(aerialNode, "FPConfig", "ExceptionalRange", ref FPConfig.Aerial.ExceptionalRange, FPConfig.Aerial.ExceptionalRange);
            Util.LoadNode(aerialExpire, "FPConfig", "MinimumExpireDays", ref FPConfig.Aerial.Expire.MinimumExpireDays, FPConfig.Aerial.Expire.MinimumExpireDays);
            Util.LoadNode(aerialExpire, "FPConfig", "MaximumExpireDays", ref FPConfig.Aerial.Expire.MaximumExpireDays, FPConfig.Aerial.Expire.MaximumExpireDays);
            Util.LoadNode(aerialExpire, "FPConfig", "DeadlineDays", ref FPConfig.Aerial.Expire.DeadlineDays, FPConfig.Aerial.Expire.DeadlineDays);
            Util.LoadNode(aerialFunds, "FPConfig", "BaseAdvance", ref FPConfig.Aerial.Funds.BaseAdvance, FPConfig.Aerial.Funds.BaseAdvance);
            Util.LoadNode(aerialFunds, "FPConfig", "BaseReward", ref FPConfig.Aerial.Funds.BaseReward, FPConfig.Aerial.Funds.BaseReward);
            Util.LoadNode(aerialFunds, "FPConfig", "BaseFailure", ref FPConfig.Aerial.Funds.BaseFailure, FPConfig.Aerial.Funds.BaseFailure);
            Util.LoadNode(aerialFunds, "FPConfig", "SignificantMultiplier", ref FPConfig.Aerial.Funds.SignificantMultiplier, FPConfig.Aerial.Funds.SignificantMultiplier);
            Util.LoadNode(aerialFunds, "FPConfig", "ExceptionalMultiplier", ref FPConfig.Aerial.Funds.ExceptionalMultiplier, FPConfig.Aerial.Funds.ExceptionalMultiplier);
            Util.LoadNode(aerialFunds, "FPConfig", "WaypointBaseReward", ref FPConfig.Aerial.Funds.WaypointBaseReward, FPConfig.Aerial.Funds.WaypointBaseReward);
            Util.LoadNode(aerialFunds, "FPConfig", "WaypointSignificantMultiplier", ref FPConfig.Aerial.Funds.WaypointSignificantMultiplier, FPConfig.Aerial.Funds.WaypointSignificantMultiplier);
            Util.LoadNode(aerialFunds, "FPConfig", "WaypointExceptionalMultiplier", ref FPConfig.Aerial.Funds.WaypointExceptionalMultiplier, FPConfig.Aerial.Funds.WaypointExceptionalMultiplier);
            Util.LoadNode(aerialScience, "FPConfig", "BaseReward", ref FPConfig.Aerial.Science.BaseReward, FPConfig.Aerial.Science.BaseReward);
            Util.LoadNode(aerialScience, "FPConfig", "SignificantMultiplier", ref FPConfig.Aerial.Science.SignificantMultiplier, FPConfig.Aerial.Science.SignificantMultiplier);
            Util.LoadNode(aerialScience, "FPConfig", "ExceptionalMultiplier", ref FPConfig.Aerial.Science.ExceptionalMultiplier, FPConfig.Aerial.Science.ExceptionalMultiplier);
            Util.LoadNode(aerialScience, "FPConfig", "WaypointBaseReward", ref FPConfig.Aerial.Science.WaypointBaseReward, FPConfig.Aerial.Science.WaypointBaseReward);
            Util.LoadNode(aerialScience, "FPConfig", "WaypointSignificantMultiplier", ref FPConfig.Aerial.Science.WaypointSignificantMultiplier, FPConfig.Aerial.Science.WaypointSignificantMultiplier);
            Util.LoadNode(aerialScience, "FPConfig", "WaypointExceptionalMultiplier", ref FPConfig.Aerial.Science.WaypointExceptionalMultiplier, FPConfig.Aerial.Science.WaypointExceptionalMultiplier);
            Util.LoadNode(aerialReputation, "FPConfig", "BaseReward", ref FPConfig.Aerial.Reputation.BaseReward, FPConfig.Aerial.Reputation.BaseReward);
            Util.LoadNode(aerialReputation, "FPConfig", "BaseFailure", ref FPConfig.Aerial.Reputation.BaseFailure, FPConfig.Aerial.Reputation.BaseFailure);
            Util.LoadNode(aerialReputation, "FPConfig", "SignificantMultiplier", ref FPConfig.Aerial.Reputation.SignificantMultiplier, FPConfig.Aerial.Reputation.SignificantMultiplier);
            Util.LoadNode(aerialReputation, "FPConfig", "ExceptionalMultiplier", ref FPConfig.Aerial.Reputation.ExceptionalMultiplier, FPConfig.Aerial.Reputation.ExceptionalMultiplier);
            Util.LoadNode(aerialReputation, "FPConfig", "WaypointBaseReward", ref FPConfig.Aerial.Reputation.WaypointBaseReward, FPConfig.Aerial.Reputation.WaypointBaseReward);
            Util.LoadNode(aerialReputation, "FPConfig", "WaypointSignificantMultiplier", ref FPConfig.Aerial.Reputation.WaypointSignificantMultiplier, FPConfig.Aerial.Reputation.WaypointSignificantMultiplier);
            Util.LoadNode(aerialReputation, "FPConfig", "WaypointExceptionalMultiplier", ref FPConfig.Aerial.Reputation.WaypointExceptionalMultiplier, FPConfig.Aerial.Reputation.WaypointExceptionalMultiplier);

            Util.LoadNode(armNode, "FPConfig", "MaximumAvailable", ref FPConfig.ARM.MaximumAvailable, FPConfig.ARM.MaximumAvailable);
            Util.LoadNode(armNode, "FPConfig", "MaximumActive", ref FPConfig.ARM.MaximumActive, FPConfig.ARM.MaximumActive);
            Util.LoadNode(armNode, "FPConfig", "AllowSolarEjections", ref FPConfig.ARM.AllowSolarEjections, FPConfig.ARM.AllowSolarEjections);
            Util.LoadNode(armNode, "FPConfig", "AllowHomeLandings", ref FPConfig.ARM.AllowHomeLandings, FPConfig.ARM.AllowHomeLandings);
            Util.LoadNode(armNode, "FPConfig", "SignificantSolarEjectionChance", ref FPConfig.ARM.SignificantSolarEjectionChance, FPConfig.ARM.SignificantSolarEjectionChance);
            Util.LoadNode(armNode, "FPConfig", "ExceptionalSolarEjectionChance", ref FPConfig.ARM.ExceptionalSolarEjectionChance, FPConfig.ARM.ExceptionalSolarEjectionChance);
            Util.LoadNode(armNode, "FPConfig", "HomeLandingChance", ref FPConfig.ARM.HomeLandingChance, FPConfig.ARM.HomeLandingChance);
            Util.LoadNode(armExpire, "FPConfig", "MinimumExpireDays", ref FPConfig.ARM.Expire.MinimumExpireDays, FPConfig.ARM.Expire.MinimumExpireDays);
            Util.LoadNode(armExpire, "FPConfig", "MaximumExpireDays", ref FPConfig.ARM.Expire.MaximumExpireDays, FPConfig.ARM.Expire.MaximumExpireDays);
            Util.LoadNode(armExpire, "FPConfig", "DeadlineDays", ref FPConfig.ARM.Expire.DeadlineDays, FPConfig.ARM.Expire.DeadlineDays);
            Util.LoadNode(armFunds, "FPConfig", "BaseAdvance", ref FPConfig.ARM.Funds.BaseAdvance, FPConfig.ARM.Funds.BaseAdvance);
            Util.LoadNode(armFunds, "FPConfig", "BaseReward", ref FPConfig.ARM.Funds.BaseReward, FPConfig.ARM.Funds.BaseReward);
            Util.LoadNode(armFunds, "FPConfig", "BaseFailure", ref FPConfig.ARM.Funds.BaseFailure, FPConfig.ARM.Funds.BaseFailure);
            Util.LoadNode(armFunds, "FPConfig", "SignificantMultiplier", ref FPConfig.ARM.Funds.SignificantMultiplier, FPConfig.ARM.Funds.SignificantMultiplier);
            Util.LoadNode(armFunds, "FPConfig", "ExceptionalMultiplier", ref FPConfig.ARM.Funds.ExceptionalMultiplier, FPConfig.ARM.Funds.ExceptionalMultiplier);
            Util.LoadNode(armFunds, "FPConfig", "SolarEjectionMultiplier", ref FPConfig.ARM.Funds.SolarEjectionMultiplier, FPConfig.ARM.Funds.SolarEjectionMultiplier);
            Util.LoadNode(armScience, "FPConfig", "BaseReward", ref FPConfig.ARM.Science.BaseReward, FPConfig.ARM.Science.BaseReward);
            Util.LoadNode(armScience, "FPConfig", "SignificantMultiplier", ref FPConfig.ARM.Science.SignificantMultiplier, FPConfig.ARM.Science.SignificantMultiplier);
            Util.LoadNode(armScience, "FPConfig", "ExceptionalMultiplier", ref FPConfig.ARM.Science.ExceptionalMultiplier, FPConfig.ARM.Science.ExceptionalMultiplier);
            Util.LoadNode(armScience, "FPConfig", "SolarEjectionMultiplier", ref FPConfig.ARM.Science.SolarEjectionMultiplier, FPConfig.ARM.Science.SolarEjectionMultiplier);
            Util.LoadNode(armReputation, "FPConfig", "BaseReward", ref FPConfig.ARM.Reputation.BaseReward, FPConfig.ARM.Reputation.BaseReward);
            Util.LoadNode(armReputation, "FPConfig", "BaseFailure", ref FPConfig.ARM.Reputation.BaseFailure, FPConfig.ARM.Reputation.BaseFailure);
            Util.LoadNode(armReputation, "FPConfig", "SignificantMultiplier", ref FPConfig.ARM.Reputation.SignificantMultiplier, FPConfig.ARM.Reputation.SignificantMultiplier);
            Util.LoadNode(armReputation, "FPConfig", "ExceptionalMultiplier", ref FPConfig.ARM.Reputation.ExceptionalMultiplier, FPConfig.ARM.Reputation.ExceptionalMultiplier);
            Util.LoadNode(armReputation, "FPConfig", "SolarEjectionMultiplier", ref FPConfig.ARM.Reputation.SolarEjectionMultiplier, FPConfig.ARM.Reputation.SolarEjectionMultiplier);

            Util.LoadNode(baseNode, "FPConfig", "MaximumExistent", ref FPConfig.Base.MaximumExistent, FPConfig.Base.MaximumExistent);
            Util.LoadNode(baseNode, "FPConfig", "AllowMobile", ref FPConfig.Base.AllowMobile, FPConfig.Base.AllowMobile);
            Util.LoadNode(baseNode, "FPConfig", "AllowCupola", ref FPConfig.Base.AllowCupola, FPConfig.Base.AllowCupola);
            Util.LoadNode(baseNode, "FPConfig", "AllowLab", ref FPConfig.Base.AllowLab, FPConfig.Base.AllowLab);
            Util.LoadNode(baseNode, "FPConfig", "TrivialMobileChance", ref FPConfig.Base.TrivialMobileChance, FPConfig.Base.TrivialMobileChance);
            Util.LoadNode(baseNode, "FPConfig", "TrivialCupolaChance", ref FPConfig.Base.TrivialCupolaChance, FPConfig.Base.TrivialCupolaChance);
            Util.LoadNode(baseNode, "FPConfig", "TrivialLabChance", ref FPConfig.Base.TrivialLabChance, FPConfig.Base.TrivialLabChance);
            Util.LoadNode(baseNode, "FPConfig", "SignificantMobileChance", ref FPConfig.Base.SignificantMobileChance, FPConfig.Base.SignificantMobileChance);
            Util.LoadNode(baseNode, "FPConfig", "SignificantCupolaChance", ref FPConfig.Base.SignificantCupolaChance, FPConfig.Base.SignificantCupolaChance);
            Util.LoadNode(baseNode, "FPConfig", "SignificantLabChance", ref FPConfig.Base.SignificantLabChance, FPConfig.Base.SignificantLabChance);
            Util.LoadNode(baseNode, "FPConfig", "ExceptionalMobileChance", ref FPConfig.Base.ExceptionalMobileChance, FPConfig.Base.ExceptionalMobileChance);
            Util.LoadNode(baseNode, "FPConfig", "ExceptionalCupolaChance", ref FPConfig.Base.ExceptionalCupolaChance, FPConfig.Base.ExceptionalCupolaChance);
            Util.LoadNode(baseNode, "FPConfig", "ExceptionalLabChance", ref FPConfig.Base.ExceptionalLabChance, FPConfig.Base.ExceptionalLabChance);
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

            Util.LoadNode(roverNode, "FPConfig", "MaximumAvailable", ref FPConfig.Rover.MaximumAvailable, FPConfig.Rover.MaximumAvailable);
            Util.LoadNode(roverNode, "FPConfig", "MaximumActive", ref FPConfig.Rover.MaximumActive, FPConfig.Rover.MaximumActive);
            Util.LoadNode(roverNode, "FPConfig", "TrivialWaypoints", ref FPConfig.Rover.TrivialWaypoints, FPConfig.Rover.TrivialWaypoints);
            Util.LoadNode(roverNode, "FPConfig", "SignificantWaypoints", ref FPConfig.Rover.SignificantWaypoints, FPConfig.Rover.SignificantWaypoints);
            Util.LoadNode(roverNode, "FPConfig", "ExceptionalWaypoints", ref FPConfig.Rover.ExceptionalWaypoints, FPConfig.Rover.ExceptionalWaypoints);
            Util.LoadNode(roverNode, "FPConfig", "TrivialRange", ref FPConfig.Rover.TrivialRange, FPConfig.Rover.TrivialRange);
            Util.LoadNode(roverNode, "FPConfig", "SignificantRange", ref FPConfig.Rover.SignificantRange, FPConfig.Rover.SignificantRange);
            Util.LoadNode(roverNode, "FPConfig", "ExceptionalRange", ref FPConfig.Rover.ExceptionalRange, FPConfig.Rover.ExceptionalRange);
            Util.LoadNode(roverExpire, "FPConfig", "MinimumExpireDays", ref FPConfig.Rover.Expire.MinimumExpireDays, FPConfig.Rover.Expire.MinimumExpireDays);
            Util.LoadNode(roverExpire, "FPConfig", "MaximumExpireDays", ref FPConfig.Rover.Expire.MaximumExpireDays, FPConfig.Rover.Expire.MaximumExpireDays);
            Util.LoadNode(roverExpire, "FPConfig", "DeadlineDays", ref FPConfig.Rover.Expire.DeadlineDays, FPConfig.Rover.Expire.DeadlineDays);
            Util.LoadNode(roverFunds, "FPConfig", "BaseAdvance", ref FPConfig.Rover.Funds.BaseAdvance, FPConfig.Rover.Funds.BaseAdvance);
            Util.LoadNode(roverFunds, "FPConfig", "BaseReward", ref FPConfig.Rover.Funds.BaseReward, FPConfig.Rover.Funds.BaseReward);
            Util.LoadNode(roverFunds, "FPConfig", "BaseFailure", ref FPConfig.Rover.Funds.BaseFailure, FPConfig.Rover.Funds.BaseFailure);
            Util.LoadNode(roverFunds, "FPConfig", "SignificantMultiplier", ref FPConfig.Rover.Funds.SignificantMultiplier, FPConfig.Rover.Funds.SignificantMultiplier);
            Util.LoadNode(roverFunds, "FPConfig", "ExceptionalMultiplier", ref FPConfig.Rover.Funds.ExceptionalMultiplier, FPConfig.Rover.Funds.ExceptionalMultiplier);
            Util.LoadNode(roverFunds, "FPConfig", "WaypointBaseReward", ref FPConfig.Rover.Funds.WaypointBaseReward, FPConfig.Rover.Funds.WaypointBaseReward);
            Util.LoadNode(roverFunds, "FPConfig", "WaypointSignificantMultiplier", ref FPConfig.Rover.Funds.WaypointSignificantMultiplier, FPConfig.Rover.Funds.WaypointSignificantMultiplier);
            Util.LoadNode(roverFunds, "FPConfig", "WaypointExceptionalMultiplier", ref FPConfig.Rover.Funds.WaypointExceptionalMultiplier, FPConfig.Rover.Funds.WaypointExceptionalMultiplier);
            Util.LoadNode(roverScience, "FPConfig", "BaseReward", ref FPConfig.Rover.Science.BaseReward, FPConfig.Rover.Science.BaseReward);
            Util.LoadNode(roverScience, "FPConfig", "SignificantMultiplier", ref FPConfig.Rover.Science.SignificantMultiplier, FPConfig.Rover.Science.SignificantMultiplier);
            Util.LoadNode(roverScience, "FPConfig", "ExceptionalMultiplier", ref FPConfig.Rover.Science.ExceptionalMultiplier, FPConfig.Rover.Science.ExceptionalMultiplier);
            Util.LoadNode(roverScience, "FPConfig", "WaypointBaseReward", ref FPConfig.Rover.Science.WaypointBaseReward, FPConfig.Rover.Science.WaypointBaseReward);
            Util.LoadNode(roverScience, "FPConfig", "WaypointSignificantMultiplier", ref FPConfig.Rover.Science.WaypointSignificantMultiplier, FPConfig.Rover.Science.WaypointSignificantMultiplier);
            Util.LoadNode(roverScience, "FPConfig", "WaypointExceptionalMultiplier", ref FPConfig.Rover.Science.WaypointExceptionalMultiplier, FPConfig.Rover.Science.WaypointExceptionalMultiplier);
            Util.LoadNode(roverReputation, "FPConfig", "BaseReward", ref FPConfig.Rover.Reputation.BaseReward, FPConfig.Rover.Reputation.BaseReward);
            Util.LoadNode(roverReputation, "FPConfig", "BaseFailure", ref FPConfig.Rover.Reputation.BaseFailure, FPConfig.Rover.Reputation.BaseFailure);
            Util.LoadNode(roverReputation, "FPConfig", "SignificantMultiplier", ref FPConfig.Rover.Reputation.SignificantMultiplier, FPConfig.Rover.Reputation.SignificantMultiplier);
            Util.LoadNode(roverReputation, "FPConfig", "ExceptionalMultiplier", ref FPConfig.Rover.Reputation.ExceptionalMultiplier, FPConfig.Rover.Reputation.ExceptionalMultiplier);
            Util.LoadNode(roverReputation, "FPConfig", "WaypointBaseReward", ref FPConfig.Rover.Reputation.WaypointBaseReward, FPConfig.Rover.Reputation.WaypointBaseReward);
            Util.LoadNode(roverReputation, "FPConfig", "WaypointSignificantMultiplier", ref FPConfig.Rover.Reputation.WaypointSignificantMultiplier, FPConfig.Rover.Reputation.WaypointSignificantMultiplier);
            Util.LoadNode(roverReputation, "FPConfig", "WaypointExceptionalMultiplier", ref FPConfig.Rover.Reputation.WaypointExceptionalMultiplier, FPConfig.Rover.Reputation.WaypointExceptionalMultiplier);

            Util.LoadNode(satelliteNode, "FPConfig", "MaximumAvailable", ref FPConfig.Satellite.MaximumAvailable, FPConfig.Satellite.MaximumAvailable);
            Util.LoadNode(satelliteNode, "FPConfig", "MaximumActive", ref FPConfig.Satellite.MaximumActive, FPConfig.Satellite.MaximumActive);
            Util.LoadNode(satelliteNode, "FPConfig", "TrivialDeviation", ref FPConfig.Satellite.TrivialDeviation, FPConfig.Satellite.TrivialDeviation);
            Util.LoadNode(satelliteNode, "FPConfig", "SignificantDeviation", ref FPConfig.Satellite.SignificantDeviation, FPConfig.Satellite.SignificantDeviation);
            Util.LoadNode(satelliteNode, "FPConfig", "ExceptionalDeviation", ref FPConfig.Satellite.ExceptionalDeviation, FPConfig.Satellite.ExceptionalDeviation);
            Util.LoadNode(satelliteNode, "FPConfig", "TrivialDifficulty", ref FPConfig.Satellite.TrivialDifficulty, FPConfig.Satellite.TrivialDifficulty);
            Util.LoadNode(satelliteNode, "FPConfig", "SignificantDifficulty", ref FPConfig.Satellite.SignificantDifficulty, FPConfig.Satellite.SignificantDifficulty);
            Util.LoadNode(satelliteNode, "FPConfig", "ExceptionalDifficulty", ref FPConfig.Satellite.ExceptionalDifficulty, FPConfig.Satellite.ExceptionalDifficulty);
            Util.LoadNode(satelliteNode, "FPConfig", "TrivialPartChance", ref FPConfig.Satellite.TrivialPartChance, FPConfig.Satellite.TrivialPartChance);
            Util.LoadNode(satelliteNode, "FPConfig", "SignificantPartChance", ref FPConfig.Satellite.SignificantPartChance, FPConfig.Satellite.SignificantPartChance);
            Util.LoadNode(satelliteNode, "FPConfig", "ExceptionalPartChance", ref FPConfig.Satellite.ExceptionalPartChance, FPConfig.Satellite.ExceptionalPartChance);
            Util.LoadNode(satelliteNode, "FPConfig", "TrivialHomeOverrideChance", ref FPConfig.Satellite.TrivialHomeOverrideChance, FPConfig.Satellite.TrivialHomeOverrideChance);
            Util.LoadNode(satelliteNode, "FPConfig", "SignificantHomeOverrideChance", ref FPConfig.Satellite.SignificantHomeOverrideChance, FPConfig.Satellite.SignificantHomeOverrideChance);
            Util.LoadNode(satelliteNode, "FPConfig", "ExceptionalHomeOverrideChance", ref FPConfig.Satellite.ExceptionalHomeOverrideChance, FPConfig.Satellite.ExceptionalHomeOverrideChance);
            Util.LoadNode(satelliteNode, "FPConfig", "TrivialSolarChance", ref FPConfig.Satellite.TrivialSolarChance, FPConfig.Satellite.TrivialSolarChance);
            Util.LoadNode(satelliteNode, "FPConfig", "SignificantSolarChance", ref FPConfig.Satellite.SignificantSolarChance, FPConfig.Satellite.SignificantSolarChance);
            Util.LoadNode(satelliteNode, "FPConfig", "ExceptionalSolarChance", ref FPConfig.Satellite.ExceptionalSolarChance, FPConfig.Satellite.ExceptionalSolarChance);
            Util.LoadNode(satelliteNode, "FPConfig", "PreferHome", ref FPConfig.Satellite.PreferHome, FPConfig.Satellite.PreferHome);
            Util.LoadNode(satelliteNode, "FPConfig", "AllowSolar", ref FPConfig.Satellite.AllowSolar, FPConfig.Satellite.AllowSolar);
            Util.LoadNode(satelliteNode, "FPConfig", "AllowEquatorial", ref FPConfig.Satellite.AllowEquatorial, FPConfig.Satellite.AllowEquatorial);
            Util.LoadNode(satelliteNode, "FPConfig", "AllowPolar", ref FPConfig.Satellite.AllowPolar, FPConfig.Satellite.AllowPolar);
            Util.LoadNode(satelliteNode, "FPConfig", "AllowSynchronous", ref FPConfig.Satellite.AllowSynchronous, FPConfig.Satellite.AllowSynchronous);
            Util.LoadNode(satelliteNode, "FPConfig", "AllowStationary", ref FPConfig.Satellite.AllowStationary, FPConfig.Satellite.AllowStationary);
            Util.LoadNode(satelliteNode, "FPConfig", "AllowKolniya", ref FPConfig.Satellite.AllowKolniya, FPConfig.Satellite.AllowKolniya);
            Util.LoadNode(satelliteNode, "FPConfig", "AllowTundra", ref FPConfig.Satellite.AllowTundra, FPConfig.Satellite.AllowTundra);
            Util.LoadNode(satelliteExpire, "FPConfig", "MinimumExpireDays", ref FPConfig.Satellite.Expire.MinimumExpireDays, FPConfig.Satellite.Expire.MinimumExpireDays);
            Util.LoadNode(satelliteExpire, "FPConfig", "MaximumExpireDays", ref FPConfig.Satellite.Expire.MaximumExpireDays, FPConfig.Satellite.Expire.MaximumExpireDays);
            Util.LoadNode(satelliteExpire, "FPConfig", "DeadlineDays", ref FPConfig.Satellite.Expire.DeadlineDays, FPConfig.Satellite.Expire.DeadlineDays);
            Util.LoadNode(satelliteFunds, "FPConfig", "BaseAdvance", ref FPConfig.Satellite.Funds.BaseAdvance, FPConfig.Satellite.Funds.BaseAdvance);
            Util.LoadNode(satelliteFunds, "FPConfig", "BaseReward", ref FPConfig.Satellite.Funds.BaseReward, FPConfig.Satellite.Funds.BaseReward);
            Util.LoadNode(satelliteFunds, "FPConfig", "BaseFailure", ref FPConfig.Satellite.Funds.BaseFailure, FPConfig.Satellite.Funds.BaseFailure);
            Util.LoadNode(satelliteFunds, "FPConfig", "SignificantMultiplier", ref FPConfig.Satellite.Funds.SignificantMultiplier, FPConfig.Satellite.Funds.SignificantMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "ExceptionalMultiplier", ref FPConfig.Satellite.Funds.ExceptionalMultiplier, FPConfig.Satellite.Funds.ExceptionalMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "PolarMultiplier", ref FPConfig.Satellite.Funds.PolarMultiplier, FPConfig.Satellite.Funds.PolarMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "SynchronousMultiplier", ref FPConfig.Satellite.Funds.SynchronousMultiplier, FPConfig.Satellite.Funds.SynchronousMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "StationaryMultiplier", ref FPConfig.Satellite.Funds.StationaryMultiplier, FPConfig.Satellite.Funds.StationaryMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "KolniyaMultiplier", ref FPConfig.Satellite.Funds.KolniyaMultiplier, FPConfig.Satellite.Funds.KolniyaMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "TundraMultiplier", ref FPConfig.Satellite.Funds.TundraMultiplier, FPConfig.Satellite.Funds.TundraMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "PartMultiplier", ref FPConfig.Satellite.Funds.PartMultiplier, FPConfig.Satellite.Funds.PartMultiplier);
            Util.LoadNode(satelliteFunds, "FPConfig", "HomeMultiplier", ref FPConfig.Satellite.Funds.HomeMultiplier, FPConfig.Satellite.Funds.HomeMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "BaseReward", ref FPConfig.Satellite.Science.BaseReward, FPConfig.Satellite.Science.BaseReward);
            Util.LoadNode(satelliteScience, "FPConfig", "SignificantMultiplier", ref FPConfig.Satellite.Science.SignificantMultiplier, FPConfig.Satellite.Science.SignificantMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "ExceptionalMultiplier", ref FPConfig.Satellite.Science.ExceptionalMultiplier, FPConfig.Satellite.Science.ExceptionalMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "PolarMultiplier", ref FPConfig.Satellite.Science.PolarMultiplier, FPConfig.Satellite.Science.PolarMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "SynchronousMultiplier", ref FPConfig.Satellite.Science.SynchronousMultiplier, FPConfig.Satellite.Science.SynchronousMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "StationaryMultiplier", ref FPConfig.Satellite.Science.StationaryMultiplier, FPConfig.Satellite.Science.StationaryMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "KolniyaMultiplier", ref FPConfig.Satellite.Science.KolniyaMultiplier, FPConfig.Satellite.Science.KolniyaMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "TundraMultiplier", ref FPConfig.Satellite.Science.TundraMultiplier, FPConfig.Satellite.Science.TundraMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "PartMultiplier", ref FPConfig.Satellite.Science.PartMultiplier, FPConfig.Satellite.Science.PartMultiplier);
            Util.LoadNode(satelliteScience, "FPConfig", "HomeMultiplier", ref FPConfig.Satellite.Science.HomeMultiplier, FPConfig.Satellite.Science.HomeMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "BaseReward", ref FPConfig.Satellite.Reputation.BaseReward, FPConfig.Satellite.Reputation.BaseReward);
            Util.LoadNode(satelliteReputation, "FPConfig", "BaseFailure", ref FPConfig.Satellite.Reputation.BaseFailure, FPConfig.Satellite.Reputation.BaseFailure);
            Util.LoadNode(satelliteReputation, "FPConfig", "SignificantMultiplier", ref FPConfig.Satellite.Reputation.SignificantMultiplier, FPConfig.Satellite.Reputation.SignificantMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "ExceptionalMultiplier", ref FPConfig.Satellite.Reputation.ExceptionalMultiplier, FPConfig.Satellite.Reputation.ExceptionalMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "PolarMultiplier", ref FPConfig.Satellite.Reputation.PolarMultiplier, FPConfig.Satellite.Reputation.PolarMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "SynchronousMultiplier", ref FPConfig.Satellite.Reputation.SynchronousMultiplier, FPConfig.Satellite.Reputation.SynchronousMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "StationaryMultiplier", ref FPConfig.Satellite.Reputation.StationaryMultiplier, FPConfig.Satellite.Reputation.StationaryMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "KolniyaMultiplier", ref FPConfig.Satellite.Reputation.KolniyaMultiplier, FPConfig.Satellite.Reputation.KolniyaMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "TundraMultiplier", ref FPConfig.Satellite.Reputation.TundraMultiplier, FPConfig.Satellite.Reputation.TundraMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "PartMultiplier", ref FPConfig.Satellite.Reputation.PartMultiplier, FPConfig.Satellite.Reputation.PartMultiplier);
            Util.LoadNode(satelliteReputation, "FPConfig", "HomeMultiplier", ref FPConfig.Satellite.Reputation.HomeMultiplier, FPConfig.Satellite.Reputation.HomeMultiplier);

            Util.LoadNode(stationNode, "FPConfig", "MaximumExistent", ref FPConfig.Station.MaximumExistent, FPConfig.Station.MaximumExistent);
            Util.LoadNode(stationNode, "FPConfig", "AllowAsteroid", ref FPConfig.Station.AllowAsteroid, FPConfig.Station.AllowAsteroid);
            Util.LoadNode(stationNode, "FPConfig", "AllowCupola", ref FPConfig.Station.AllowCupola, FPConfig.Station.AllowCupola);
            Util.LoadNode(stationNode, "FPConfig", "AllowLab", ref FPConfig.Station.AllowLab, FPConfig.Station.AllowLab);
            Util.LoadNode(stationNode, "FPConfig", "AllowSolar", ref FPConfig.Station.AllowSolar, FPConfig.Station.AllowSolar);
            Util.LoadNode(stationNode, "FPConfig", "TrivialAsteroidChance", ref FPConfig.Station.TrivialAsteroidChance, FPConfig.Station.TrivialAsteroidChance);
            Util.LoadNode(stationNode, "FPConfig", "TrivialCupolaChance", ref FPConfig.Station.TrivialCupolaChance, FPConfig.Station.TrivialCupolaChance);
            Util.LoadNode(stationNode, "FPConfig", "TrivialLabChance", ref FPConfig.Station.TrivialLabChance, FPConfig.Station.TrivialLabChance);
            Util.LoadNode(stationNode, "FPConfig", "SignificantAsteroidChance", ref FPConfig.Station.SignificantAsteroidChance, FPConfig.Station.SignificantAsteroidChance);
            Util.LoadNode(stationNode, "FPConfig", "SignificantCupolaChance", ref FPConfig.Station.SignificantCupolaChance, FPConfig.Station.SignificantCupolaChance);
            Util.LoadNode(stationNode, "FPConfig", "SignificantLabChance", ref FPConfig.Station.SignificantLabChance, FPConfig.Station.SignificantLabChance);
            Util.LoadNode(stationNode, "FPConfig", "ExceptionalAsteroidChance", ref FPConfig.Station.ExceptionalAsteroidChance, FPConfig.Station.ExceptionalAsteroidChance);
            Util.LoadNode(stationNode, "FPConfig", "ExceptionalCupolaChance", ref FPConfig.Station.ExceptionalCupolaChance, FPConfig.Station.ExceptionalCupolaChance);
            Util.LoadNode(stationNode, "FPConfig", "ExceptionalLabChance", ref FPConfig.Station.ExceptionalLabChance, FPConfig.Station.ExceptionalLabChance);
            Util.LoadNode(stationExpire, "FPConfig", "MinimumExpireDays", ref FPConfig.Station.Expire.MinimumExpireDays, FPConfig.Station.Expire.MinimumExpireDays);
            Util.LoadNode(stationExpire, "FPConfig", "MaximumExpireDays", ref FPConfig.Station.Expire.MaximumExpireDays, FPConfig.Station.Expire.MaximumExpireDays);
            Util.LoadNode(stationExpire, "FPConfig", "DeadlineDays", ref FPConfig.Station.Expire.DeadlineDays, FPConfig.Station.Expire.DeadlineDays);
            Util.LoadNode(stationFunds, "FPConfig", "BaseAdvance", ref FPConfig.Station.Funds.BaseAdvance, FPConfig.Station.Funds.BaseAdvance);
            Util.LoadNode(stationFunds, "FPConfig", "BaseReward", ref FPConfig.Station.Funds.BaseReward, FPConfig.Station.Funds.BaseReward);
            Util.LoadNode(stationFunds, "FPConfig", "BaseFailure", ref FPConfig.Station.Funds.BaseFailure, FPConfig.Station.Funds.BaseFailure);
            Util.LoadNode(stationFunds, "FPConfig", "SignificantMultiplier", ref FPConfig.Station.Funds.SignificantMultiplier, FPConfig.Station.Funds.SignificantMultiplier);
            Util.LoadNode(stationFunds, "FPConfig", "ExceptionalMultiplier", ref FPConfig.Station.Funds.ExceptionalMultiplier, FPConfig.Station.Funds.ExceptionalMultiplier);
            Util.LoadNode(stationFunds, "FPConfig", "CupolaMultiplier", ref FPConfig.Station.Funds.CupolaMultiplier, FPConfig.Station.Funds.CupolaMultiplier);
            Util.LoadNode(stationFunds, "FPConfig", "LabMultiplier", ref FPConfig.Station.Funds.LabMultiplier, FPConfig.Station.Funds.LabMultiplier);
            Util.LoadNode(stationFunds, "FPConfig", "AsteroidMultiplier", ref FPConfig.Station.Funds.AsteroidMultiplier, FPConfig.Station.Funds.AsteroidMultiplier);
            Util.LoadNode(stationScience, "FPConfig", "BaseReward", ref FPConfig.Station.Science.BaseReward, FPConfig.Station.Science.BaseReward);
            Util.LoadNode(stationScience, "FPConfig", "SignificantMultiplier", ref FPConfig.Station.Science.SignificantMultiplier, FPConfig.Station.Science.SignificantMultiplier);
            Util.LoadNode(stationScience, "FPConfig", "ExceptionalMultiplier", ref FPConfig.Station.Science.ExceptionalMultiplier, FPConfig.Station.Science.ExceptionalMultiplier);
            Util.LoadNode(stationScience, "FPConfig", "CupolaMultiplier", ref FPConfig.Station.Science.CupolaMultiplier, FPConfig.Station.Science.CupolaMultiplier);
            Util.LoadNode(stationScience, "FPConfig", "LabMultiplier", ref FPConfig.Station.Science.LabMultiplier, FPConfig.Station.Science.LabMultiplier);
            Util.LoadNode(stationScience, "FPConfig", "AsteroidMultiplier", ref FPConfig.Station.Science.AsteroidMultiplier, FPConfig.Station.Science.AsteroidMultiplier);
            Util.LoadNode(stationReputation, "FPConfig", "BaseReward", ref FPConfig.Station.Reputation.BaseReward, FPConfig.Station.Reputation.BaseReward);
            Util.LoadNode(stationReputation, "FPConfig", "BaseFailure", ref FPConfig.Station.Reputation.BaseFailure, FPConfig.Station.Reputation.BaseFailure);
            Util.LoadNode(stationReputation, "FPConfig", "SignificantMultiplier", ref FPConfig.Station.Reputation.SignificantMultiplier, FPConfig.Station.Reputation.SignificantMultiplier);
            Util.LoadNode(stationReputation, "FPConfig", "ExceptionalMultiplier", ref FPConfig.Station.Reputation.ExceptionalMultiplier, FPConfig.Station.Reputation.ExceptionalMultiplier);
            Util.LoadNode(stationReputation, "FPConfig", "CupolaMultiplier", ref FPConfig.Station.Reputation.CupolaMultiplier, FPConfig.Station.Reputation.CupolaMultiplier);
            Util.LoadNode(stationReputation, "FPConfig", "LabMultiplier", ref FPConfig.Station.Reputation.LabMultiplier, FPConfig.Station.Reputation.LabMultiplier);
            Util.LoadNode(stationReputation, "FPConfig", "AsteroidMultiplier", ref FPConfig.Station.Reputation.AsteroidMultiplier, FPConfig.Station.Reputation.AsteroidMultiplier);
        }
    }
}