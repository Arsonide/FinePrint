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
using FinePrint.Contracts;

namespace FinePrint
{
    public enum LoadResult
    {
        NULL,
        NOVALUE,
        INVALID,
        SUCCESS
    }

	public class Util
    {
        public const int frameSuccessDelay = 5;

        #region Vessel Loops

        public static bool shipHasPartName(string partName)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (FlightGlobals.ready)
                {
                    Vessel v = FlightGlobals.ActiveVessel;

                    if (v != null)
                    {
                        foreach (Part part in v.Parts)
                        {
                            if (part.name == partName)
                                return true;

                            // Any that fall through either don't match or are bugged. Check for bug.
                            AvailablePart info = part.partInfo;

                            if (info != null)
                            {
                                if (info.name == partName)
                                    return true;
                            }
                        }
                    }
                    else
                        Debug.LogWarning("Fine Print: Attempted to check for ship modules on a nonexistent ship.");
                }
                else
                    Debug.LogWarning("Fine Print: Attempted to check for ship modules before flight scene was ready.");
            }
            else
                Debug.LogWarning("Fine Print: Attempted to check for ship modules outside of flight scene.");

            return false;
        }

        public static bool shipHasPartClass(string partClass)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (FlightGlobals.ready)
                {
                    Vessel v = FlightGlobals.ActiveVessel;

                    if (v != null)
                    {
                        foreach (Part part in v.Parts)
                        {
                            if (part.ClassName == partClass)
                                return true;
                        }
                    }
                    else
                        Debug.LogWarning("Fine Print: Attempted to check for ship modules on a nonexistent ship.");
                }
                else
                    Debug.LogWarning("Fine Print: Attempted to check for ship modules before flight scene was ready.");
            }
            else
                Debug.LogWarning("Fine Print: Attempted to check for ship modules outside of flight scene.");

            return false;
        }

        public static bool hasWheelsOnGround()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (FlightGlobals.ready)
                {
                    Vessel v = FlightGlobals.ActiveVessel;

                    if (v != null)
                    {
                        foreach (Part part in v.Parts)
                        {
                            if (part.GroundContact)
                            {
                                foreach (PartModule module in part.Modules)
                                {
                                    List<string> moduleList = new List<string>() { "ModuleWheel", "ModuleLandingGear", "FSWheel" };

                                    foreach (string checkModule in moduleList)
                                    {
                                        if (module.moduleName == checkModule || module.ClassName == checkModule)
                                            return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static bool shipHasModuleList(List<string> moduleList)
        {
            //This checks a list of modules, prevents us from having to loop over the vessel multiple times.
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (FlightGlobals.ready)
                {
                    Vessel v = FlightGlobals.ActiveVessel;

                    if (v != null)
                    {
                        foreach (Part part in v.Parts)
                        {
                            foreach (PartModule module in part.Modules)
                            {
                                //We must go deeper.
                                foreach (string checkModule in moduleList)
                                {
                                    if (module.moduleName == checkModule || module.ClassName == checkModule )
                                        return true;
                                }
                            }
                        }
                    }
                    else
                        Debug.LogWarning("Fine Print: Attempted to check for ship modules on a nonexistent ship.");
                }
                else
                    Debug.LogWarning("Fine Print: Attempted to check for ship modules before flight scene was ready.");
            }
            else
                Debug.LogWarning("Fine Print: Attempted to check for ship modules outside of flight scene.");

            return false;
        }

        public static bool shipHasModuleClass(string moduleClass)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (FlightGlobals.ready)
                {
                    Vessel v = FlightGlobals.ActiveVessel;

                    if (v != null)
                    {
                        foreach (Part part in v.Parts)
                        {
                            foreach (PartModule module in part.Modules)
                            {
                                if (module.ClassName == moduleClass)
                                    return true;
                            }
                        }
                    }
                    else
                        Debug.LogWarning("Fine Print: Attempted to check for ship modules on a nonexistent ship.");
                }
                else
                    Debug.LogWarning("Fine Print: Attempted to check for ship modules before flight scene was ready.");
            }
            else
                Debug.LogWarning("Fine Print: Attempted to check for ship modules outside of flight scene.");

            return false;
        }

        public static bool shipHasModule<T>() where T : class
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (FlightGlobals.ready)
                {
                    Vessel v = FlightGlobals.ActiveVessel;

                    if (v != null)
                    {
                        return (v.FindPartModulesImplementing<T>().Count() > 0);
                    }
                    else
                        Debug.LogWarning("Fine Print: Attempted to check for ship modules on a nonexistent ship.");
                }
                else
                    Debug.LogWarning("Fine Print: Attempted to check for ship modules before flight scene was ready.");
            }
            else
                Debug.LogWarning("Fine Print: Attempted to check for ship modules outside of flight scene.");

            return false;
        }

        #endregion

        public static bool haveTechnology(string tech)
		{
			tech = tech.Replace('_', '.');
			AvailablePart ap = PartLoader.getPartInfoByName(tech);

			if (ap != null)
			{
				if (ResearchAndDevelopment.PartTechAvailable(ap))
					return true;
			}
			else
				Debug.LogWarning("Fine Print: Attempted to check for nonexistent technology: \"" + tech + "\".");

			return false;
		}

		public static string randomKerbalName(int seed)
		{
			bool goodName = false;
			string name = "";
			// Trying to get this as close to stock as humanly possible.
			List<string> prefix = new List<string> { "Ad", "Al", "Ald", "An", "Bar", "Bart", "Bil", "Billy-Bob", "Bob", "Bur", "Cal", "Cam", "Chad", "Cor", "Dan", "Der", "Des", "Dil", "Do", "Don", "Dood", "Dud", "Dun", "Ed", "El", "En", "Er", "Fer", "Fred", "Gene", "Geof", "Ger", "Gil", "Greg", "Gus", "Had", "Hal", "Han", "Har", "Hen", "Her", "Hud", "Jed", "Jen", "Jer", "Joe", "John", "Jon", "Jor", "Kel", "Ken", "Ker", "Kir", "Lan", "Lem", "Len", "Lo", "Lod", "Lu", "Lud", "Mac", "Mal", "Mat", "Mel", "Mer", "Mil", "Mit", "Mun", "Ned", "Neil", "Nel", "New", "Ob", "Or", "Pat", "Phil", "Ray", "Rib", "Rich", "Ro", "Rod", "Ron", "Sam", "Sean", "See", "Shel", "Shep", "Sher", "Sid", "Sig", "Son", "Thom", "Thomp", "Tom", "Wehr", "Wil" };
			List<string> suffix = new List<string> { "ald", "bal", "bald", "bart", "bas", "berry", "bert", "bin", "ble", "bles", "bo", "bree", "brett", "bro", "bur", "burry", "bus", "by", "cal", "can", "cas", "cott", "dan", "das", "den", "din", "do", "don", "dorf", "dos", "dous", "dred", "drin", "dun", "ely", "emone", "emy", "eny", "fal", "fel", "fen", "field", "ford", "fred", "frey", "frey", "frid", "frod", "fry", "furt", "gan", "gard", "gas", "gee", "gel", "ger", "gun", "hat", "ing", "ke", "kin", "lan", "las", "ler", "ley", "lie", "lin", "lin", "lo", "lock", "long", "lorf", "ly", "mal", "man", "min", "ming", "mon", "more", "mund", "my", "nand", "nard", "ner", "ney", "nie", "ny", "oly", "ory", "rey", "rick", "rie", "righ", "rim", "rod", "ry", "sby", "sel", "sen", "sey", "ski", "son", "sted", "ster", "sy", "ton", "top", "trey", "van", "vey", "vin", "vis", "well", "wig", "win", "wise", "zer", "zon", "zor" };
			List<string> proper = new List<string> { "Adam", "Al", "Alan", "Archibald", "Buzz", "Carson", "Chad", "Charlie", "Chris", "Chuck", "Dean", "Ed", "Edan", "Edlu", "Frank", "Franklin", "Gus", "Hans", "Jack", "James", "Jim", "Kirk", "Kurt", "Lars", "Luke", "Mac", "Matt", "Phil", "Randall", "Scott", "Sean", "Steve", "Tom", "Will" };
			System.Random generator = new System.Random(seed);

			while (!goodName)
			{
				name = "";

				if (generator.Next(0, 21) == 15)
					name = proper[generator.Next(0, proper.Count)];
				else
				{
					name += prefix[generator.Next(0, prefix.Count)];
					name += suffix[generator.Next(0, suffix.Count)];
				}

				// Apparently these get filtered. Sorry Dildo Kerman. You will not be going to space today.
				if (name.Contains("Dildo") || name.Contains("Kerman") || name.Contains("Kerbal") || name.Contains("eee") || name.Contains("rrr"))
					goodName = false;
				else
					goodName = true;
			}

			return name;
		}

		public static string generateSiteName(int seed, bool isAtHome)
		{
			List<string> prefix = new List<string> { "Jebediah's", "Bill's", "Bob's", "Wernher's", "Gene's", "Dinkelstein's", "Dawton's", "Eumon's", "Bobak's", "Kirrim's", "Kerman's", "Kerbin's", "Scientist's", "Engineer's", "Pilot's", "Kerbonaut's", "Kraken's", "Scott's", "Nerd's", "Manley's" };
			List<string> suffix = new List<string> { "Folly", "Hope", "Legacy", "Doom", "Rock", "Gambit", "Bane", "End", "Drift", "Frontier", "Pride", "Retreat", "Escape", "Legend", "Sector", "Abyss", "Void", "Vision", "Wisdom", "Refuge", "Doubt", "Redemption", "Anomaly", "Trek", "Monolith", "Emitter", "Wonder", "Lament", "Hindsight", "Mistake", "Isolation", "Hole", "Jest", "Stretch", "Scar", "Surprise", "Whim", "Whimsy", "Target", "Insanity", "Goal", "Dirge", "Adventure", "Fate", "Point", "Descent", "Ascent", "Dawn", "Dusk" };
			List<string> kerbinSuffix = new List<string> { "Backyard", "Bar and Grill", "Junkyard", "Lab", "Testing Range", "Quarantine Zone", "Snack Pile", "Headquarters", "Discount Warehouse", };
			List<string> alphaNumeric = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", };
			List<string> developerNames = new List<string> { "Ayarza's", "Goya's", "Falanghe's", "Mora's", "Geelan's", "Salcedo's", "Jenkins'", "Rosas'", "Safi's", "Benjaminsen's", "Pina's", "Montano's", "Holtzman's", "Everett's", "Guzzardo's", "Reyes'", "Dominguez'", "Gutierrez'", "Demeneghi's", "Vazquez'", "Rosas'", "Maqueo's", "Silisko's", "Keeton's", "Kupperian's", "Chiarello's", "Zuev's", "Nelson's" };
			System.Random generator = new System.Random(seed);
			string siteName = "";

            int namedChance = isAtHome ? 50 : 25;

			if (generator.Next(0, 101) < namedChance)
			{
				if (isAtHome)
					suffix.AddRange(kerbinSuffix);

				// Developer names should pop up rarely, only put like two in the list each time.
				prefix.Add(developerNames[generator.Next(0, developerNames.Count)]);
				prefix.Add(developerNames[generator.Next(0, developerNames.Count)]);

				// Throw in more variety.
				for (int x = 0; x < 5; x++)
				{
					string randomName = randomKerbalName(seed + x);

					if (randomName.EndsWith("s") || randomName.EndsWith("ch") || randomName.EndsWith("z"))
						randomName += "'";
					else
						randomName += "'s";

					prefix.Add(randomName);
				}

				siteName += prefix[generator.Next(0, prefix.Count)];
				siteName += " ";
				siteName += suffix[generator.Next(0, suffix.Count)];
			}
			else
			{
				bool usedHyphen = false;
				int repeat = generator.Next(4, 9);
				siteName = "Site ";

				for (int x = 1; x <= repeat; x++)
				{
					if (generator.Next(0, 101) > 70 && x != 1 && x != repeat && usedHyphen == false)
					{
						siteName += '-';
						usedHyphen = true;
					}
					else
						siteName += alphaNumeric[generator.Next(0, alphaNumeric.Count)];
				}
			}

			return siteName;
		}

		public static string generateRoverFailString(int missionSeed, int waypointID)
		{
			System.Random generator = new System.Random(missionSeed);

			List<string> roverFail = new List<string>
			{
				"You didn't find anything on the anomaly, but you did find " + randomKerbalName(missionSeed + waypointID) + "'s rover keys!",
				"There is nothing anomalous about the data in this area.",
				"Nothing to see here, moving along...",
				"This all checks out, the anomaly must be elsewhere.",
				"If all the data is this boring, you're going to need more snacks.",
				"Nothing. You consider just forging a report to appease the agency.",
				"Maybe we should have put more boosters on this thing. This might take a while.",
			};

			return roverFail[generator.Next(0, roverFail.Count)];
		}

		public static string integerToWord(int x)
		{
			string[] integerMap = new string[21] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen", "twenty" };
			x = Math.Max(x, 0);
			x = Math.Min(x, 20);
			return integerMap[x];
		}

		public static string integerToGreek(int x)
		{
			//To make rover site briefings distinguishible.
			string[] greekMap = new string[24] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta", "Iota", "Kappa", "Lambda", "Mu", "Nu", "Xi", "Omicron", "Pi", "Rho", "Sigma", "Tau", "Upsilon", "Phi", "Chi", "Psi", "Omega" };
			x = Math.Max(x, 0);
			x = Math.Min(x, 23);
			return greekMap[x];
		}

        private static string ShortName(string verbose)
        {
            //Aerial.Funds.BaseComplete => BaseComplete
            return verbose.Substring(verbose.LastIndexOf(".") + 1);
        }

        public static bool TryConvert<T>(string input, out T value, ref string error)
        {
            if (typeof(T).IsEnum)
            {
                try
                {
                    value = (T)Enum.Parse(typeof(T), input);
                    return true;
                }
                catch (Exception e)
                {
                    error = e.ToString();
                    value = default(T);
                    return false;
                }
            }
            else
            {
                try
                {
                    value = (T)System.Convert.ChangeType(input, typeof(T));
                    return true;
                }
                catch (Exception e)
                {
                    error = e.ToString();
                    value = default(T);
                    return false;
                }
            }
        }

        public static void LoadNode<T>(ConfigNode node, string className, string valueName, ref T value, T defaultValue)
        {
            LoadResult result = LoadResult.NULL;
            string error = "";

            if (node == null)
                result = LoadResult.NULL;
            else
            {
                string shortName = ShortName(valueName);

                if (node.HasValue(shortName))
                {
                    if (typeof(T) == typeof(CelestialBody))
                    {
                        int bodyIndex = 0;
                        if (!TryConvert<int>(node.GetValue(shortName), out bodyIndex, ref error))
                            result = LoadResult.INVALID;
                        else
                        {
                            CelestialBody body = null;

                            foreach (var cb in FlightGlobals.Bodies)
                            {
                                if (cb.flightGlobalsIndex == bodyIndex)
                                    body = cb;
                            }

                            if (body == null)
                            {
                                error = "CelestialException: Celestial body is out of range.";
                                result = LoadResult.INVALID;
                            }
                            else
                            {
                                value = (T)(object)body;
                                result = LoadResult.SUCCESS;
                            }
                        }
                    }
                    else
                    {
                        if (!TryConvert<T>(node.GetValue(shortName), out value, ref error))
                            result = LoadResult.INVALID;
                        else
                            result = LoadResult.SUCCESS;
                    }
                }
                else
                    result = LoadResult.NOVALUE;
            }

            if (result != LoadResult.SUCCESS)
            {
                switch (result)
                {
                    case LoadResult.NULL:
                        Debug.LogWarning("Fine Print: " + className + " cannot load " + valueName + " from a null node. Initializing with default of " + defaultValue + "!");
                        break;
                    case LoadResult.NOVALUE:
                        Debug.LogWarning("Fine Print: " + className + " cannot load " + valueName + ", it is not in the node. Initializing with default of " + defaultValue + "!");
                        break;
                    case LoadResult.INVALID:
                        Debug.LogWarning("Fine Print: " + className + " parsed an invalid value from " + valueName + ". (" + error + "). Initializing with default of " + defaultValue + "!");
                        break;
                }

                value = defaultValue;
            }
        }

        public static void CheckForPatchReset()
        {
            if (FPConfig.PatchReset == false)
                return;

            FPConfig.PatchReset = false;

            FPConfig.config.GetNode("FinePrint").SetValue("PatchReset", FPConfig.PatchReset.ToString());
            FPConfig.config.Save(FPConfig.ConfigFileName);

            Debug.LogError("Fine Print has been patched and will now reset all Fine Print contracts.");

            foreach (AerialContract c in ContractSystem.Instance.GetCurrentContracts<AerialContract>())
            {
                c.Unregister();
                ContractSystem.Instance.Contracts.Remove(c);
            }

            foreach (ARMContract c in ContractSystem.Instance.GetCurrentContracts<ARMContract>())
            {
                c.Unregister();
                ContractSystem.Instance.Contracts.Remove(c);
            }

            foreach (BaseContract c in ContractSystem.Instance.GetCurrentContracts<BaseContract>())
            {
                c.Unregister();
                ContractSystem.Instance.Contracts.Remove(c);
            }

            foreach (RoverContract c in ContractSystem.Instance.GetCurrentContracts<RoverContract>())
            {
                c.Unregister();
                ContractSystem.Instance.Contracts.Remove(c);
            }

            foreach (SatelliteContract c in ContractSystem.Instance.GetCurrentContracts<SatelliteContract>())
            {
                c.Unregister();
                ContractSystem.Instance.Contracts.Remove(c);
            }

            foreach (StationContract c in ContractSystem.Instance.GetCurrentContracts<StationContract>())
            {
                c.Unregister();
                ContractSystem.Instance.Contracts.Remove(c);
            }
        }

        public static Vector3 LLAtoECEF(double lat, double lon, double alt, double radius)
        {
            const double degreesToRadians = Math.PI / 180.0;
            lat = (lat - 90) * degreesToRadians;
            lon *= degreesToRadians;
            double x, y, z;
            double n = radius; // for now, it's still a sphere, so just the radius
            x = (n + alt) * -1.0 * Math.Sin(lat) * Math.Cos(lon);
            y = (n + alt) * Math.Cos(lat); // for now, it's still a sphere, so no eccentricity
            z = (n + alt) * -1.0 * Math.Sin(lat) * Math.Sin(lon);
            return new Vector3((float)x, (float)y, (float)z);
        }

        public static double getMinimumOrbitalAltitude(CelestialBody body)
        {
            double atmosphere = 0.0;
            double timeWarp = 0.0;

            if (!body.atmosphere)
                atmosphere = 0.0;
            else
                atmosphere = -body.atmosphereScaleHeight * Math.Log(1E-6) * 1000;

            //We need to calculate PeR and ApR, radius needs to be a part of this altitude.
            atmosphere += body.Radius;

            timeWarp = body.timeWarpAltitudeLimits[1] + body.Radius;

            return Math.Max(atmosphere, timeWarp);
        }

        public static double synchronousSMA(CelestialBody body)
        {
            if ((object)body == null)
                return 0.0;

            return Math.Pow(body.gravParameter * Math.Pow(body.rotationPeriod / (2.0 * Math.PI), 2.0), (1.0 / 3.0));
        }

        public static double kolniyaSMA(CelestialBody body)
        {
            if ((object)body == null)
                return 0.0;

            // Kolniya orbits have periods of half a day.
            double period = body.rotationPeriod / 2;

            return Math.Pow(body.gravParameter * Math.Pow(period / (2.0 * Math.PI), 2.0), (1.0 / 3.0));
        }

        public static bool canBodyBeKolniya(CelestialBody body)
        {
			// highly inclined orbits around the sun are extremely difficult, and unlikely tasks in reality
            if (((object)body == null) || (body == Planetarium.fetch.Sun))
                return false;

            double semiMajorAxis = kolniyaSMA(body);
            double periapsis = getMinimumOrbitalAltitude(body) * 1.05;
            double apoapsis = (semiMajorAxis * 2) - periapsis;

            if (apoapsis > body.sphereOfInfluence)
                return false;
            else
                return true;
        }

        public static bool canBodyBeTundra(CelestialBody body)
        {
            if (((object)body == null) || (body == Planetarium.fetch.Sun))
                return false;

            double semiMajorAxis = synchronousSMA(body);
            double periapsis = getMinimumOrbitalAltitude(body) * 1.05;
            double apoapsis = (semiMajorAxis * 2) - periapsis;

            if (apoapsis > body.sphereOfInfluence)
                return false;
            else
                return true;
        }

        public static bool canBodyBeSynchronous(CelestialBody body, double eccentricity)
        {
            if (((object)body == null) || (body == Planetarium.fetch.Sun))
                return false;

            double semiMajorAxis = synchronousSMA(body);
            double apoapsis = (1.0 + eccentricity) * semiMajorAxis;

            if (apoapsis > body.sphereOfInfluence)
                return false;
            else
                return true;
        }

        public static double angleOfAscendingNode(Orbit currentOrbit, Orbit targetOrbit)
        {
            //Credit to the folks with MechJeb for pioneering this stuff.
            //Get the raw normals of both orbits.
            Vector3d currentNormal = -currentOrbit.GetOrbitNormal().xzy.normalized;
            Vector3d targetNormal = -targetOrbit.GetOrbitNormal().xzy.normalized;

            //Math inc...
            Vector3d vectorToAN = Vector3d.Cross(currentNormal, targetNormal);

            Vector3d projected = Vector3d.Exclude(currentNormal, vectorToAN);

            Vector3d localVectorToAN = Quaternion.AngleAxis(-(float)currentOrbit.LAN, Planetarium.up) * Planetarium.right;
            Vector3d localvectorToPe = Quaternion.AngleAxis((float)currentOrbit.argumentOfPeriapsis, currentNormal) * localVectorToAN;

            Vector3d vectorToPe = currentOrbit.PeR * localvectorToPe;
            double angleFromPe = Vector3d.Angle(vectorToPe, projected);

            //If the vector points to the infalling part of the orbit then we need to do 360 minus the angle from Pe to get the true anomaly.
            double trueAnomalyOfAscendingNode = 0.0;
            if (Math.Abs(Vector3d.Angle(projected, Vector3d.Cross(currentNormal, vectorToPe))) < 90)
                trueAnomalyOfAscendingNode = angleFromPe;
            else
                trueAnomalyOfAscendingNode = 360 - angleFromPe;

            return trueAnomalyOfAscendingNode;
        }

        public static double angleOfDescendingNode(Orbit currentOrbit, Orbit targetOrbit)
        {
            double trueAnomalyOfDescendingNode = angleOfAscendingNode(currentOrbit, targetOrbit);
            trueAnomalyOfDescendingNode = (trueAnomalyOfDescendingNode + 180) % 360;

            return trueAnomalyOfDescendingNode;
        }

        public static Vector3d positionOfPeriapsis(Orbit o)
        {
            return o.getPositionFromTrueAnomaly(0.0);
        }

        public static Vector3d positionOfApoapsis(Orbit o)
        {
            return o.getPositionFromTrueAnomaly(Math.PI);
        }

        public static double getRelativeInclination(Orbit o, Orbit other)
        {
            //Credit to Blizzy and PreciseNode
            Vector3d normal = o.GetOrbitNormal().xzy.normalized;
            Vector3d otherNormal = other.GetOrbitNormal().xzy.normalized;
            double angle = Vector3d.Angle(normal, otherNormal);
            bool south = Vector3d.Dot(Vector3d.Cross(normal, otherNormal), normal.xzy) > 0;
            return south ? -angle : angle;
        }

        public static Texture2D LoadTexture(string textureName, int width, int height)
        {
            textureName = "FinePrint/Textures/" + textureName;

            Texture2D texture = GameDatabase.Instance.GetTexture(textureName, false);

            if (texture == null)
            {
                texture = new Texture2D(width, height);
                texture.SetPixels32(Enumerable.Repeat((Color32)Color.magenta, width * height).ToArray());
                texture.Apply();
            }

            return texture;
        }

        public static Orbit GenerateOrbit(OrbitType orbitType, int seed, CelestialBody targetBody, double difficultyFactor, double eccentricity = 0.0)
        {
            if ((object)targetBody == null)
                return null;

            Orbit o = new Orbit();
            System.Random generator = new System.Random(seed);

            //Initialize all the things.
            //Default inclination needs to be greater than one, just...just trust me on this.
            double inc = Math.Max(1, (generator.NextDouble() * 90.0) * difficultyFactor);
            double desiredPeriapsis = 0.0;
            double desiredApoapsis = 0.0;
            double pointA = 0.0;
            double pointB = 0.0;
            double easeFactor = 1.0 - difficultyFactor;
            o.referenceBody = targetBody;
            o.LAN = generator.NextDouble() * 360.0;

			/*
            float maximumAltitude = 0f;
            double minimumAltitude = Util.getMinimumOrbitalAltitude(targetBody);
            //If it chooses the sun, the infinite SOI can cause NAN, so choose Eeloo's altitude instead.
            //Use 90% of the SOI to give a little leeway for error correction.
            if (targetBody == Planetarium.fetch.Sun)
                maximumAltitude = 113549713200f;
            else
                maximumAltitude = Math.Max((float)minimumAltitude, (float)targetBody.sphereOfInfluence * (float)difficultyFactor);
			//*/
            //*

			// match altitudes more closely to the delta-v required to reach them

            double minimumAltitude      = 0;
            double maximumAltitude      = 0;
            double lowestSafeAltitude   = Util.getMinimumOrbitalAltitude(targetBody);
            double scienceSpaceBorder   = targetBody.Radius + (double) targetBody.scienceValues.spaceAltitudeThreshold;
            double sphereOfInfluence    = (double) targetBody.sphereOfInfluence;
            double maxLowOrbit          = lowestSafeAltitude + (scienceSpaceBorder - lowestSafeAltitude) / 2;
            double maxHighOrbit         = scienceSpaceBorder + (sphereOfInfluence - scienceSpaceBorder) / 2;
            double orbitMoho            = 5263138000;
            double orbitEve             = 9832684000;
            double orbitKerbin          = 13599840000;
            double orbitDuna            = 20726155000;
            double orbitEloo            = 113549713200;

            
            if (targetBody == Planetarium.fetch.Sun) {
				inc *= 0.1;
                if (difficultyFactor <= 0.2) {
                    // near Kerbin
                    maximumAltitude = orbitDuna;
                    minimumAltitude = orbitEve;
                } else if (difficultyFactor <= 0.4) {
                    // far from sun
                    maximumAltitude = orbitEloo;
                    minimumAltitude = orbitDuna;
                } else {
                    // close to sun
                    maximumAltitude = orbitEve;
                    minimumAltitude = lowestSafeAltitude;
                }
            } else if (targetBody == Planetarium.fetch.Home) {
                if (difficultyFactor <= 0.2) {
                    minimumAltitude = lowestSafeAltitude;
                    maximumAltitude = maxLowOrbit;
                } else if (difficultyFactor <= 0.4) {
                    minimumAltitude = maxLowOrbit;
                    maximumAltitude = maxHighOrbit;
                } else {
                    minimumAltitude = maxHighOrbit;
                    maximumAltitude = sphereOfInfluence * 0.9;
                }
            } else {
                if (difficultyFactor <= 0.2) {
                    maximumAltitude = sphereOfInfluence * 0.9;
                    minimumAltitude = maxHighOrbit;
                } else if (difficultyFactor <= 0.4) {
                    maximumAltitude = maxHighOrbit;
                    minimumAltitude = maxLowOrbit;
                } else {
                    maximumAltitude = maxLowOrbit;
                    minimumAltitude = lowestSafeAltitude;
                }
            }
            //*/

            if (orbitType == OrbitType.RANDOM || orbitType == OrbitType.POLAR || orbitType == OrbitType.EQUATORIAL)
            {
                pointA = minimumAltitude + ((maximumAltitude - minimumAltitude) * generator.NextDouble());
                pointB = minimumAltitude + ((maximumAltitude - minimumAltitude) * generator.NextDouble());
                pointA = UnityEngine.Mathf.Lerp((float)pointA, (float)pointB, (float)easeFactor);
                desiredApoapsis = Math.Max(pointA, pointB);
                desiredPeriapsis = Math.Min(pointA, pointB);
                o.semiMajorAxis = (desiredApoapsis + desiredPeriapsis) / 2.0;
                o.eccentricity = (desiredApoapsis - desiredPeriapsis) / (desiredApoapsis + desiredPeriapsis);
                o.argumentOfPeriapsis = generator.NextDouble() * 360.0;
            }
            else if (orbitType == OrbitType.KOLNIYA)
            {
                o.semiMajorAxis = Util.kolniyaSMA(targetBody);
                desiredPeriapsis = minimumAltitude * 1.05;
                desiredApoapsis = (o.semiMajorAxis * 2) - desiredPeriapsis;
                o.eccentricity = (desiredApoapsis - desiredPeriapsis) / (desiredApoapsis + desiredPeriapsis);

                if (generator.Next(0, 100) > 50)
                    o.argumentOfPeriapsis = 270;
                else
                    o.argumentOfPeriapsis = 90;
            }
            else if (orbitType == OrbitType.TUNDRA)
            {
                o.semiMajorAxis = Util.synchronousSMA(targetBody);
                desiredPeriapsis = minimumAltitude * 1.05;
                desiredApoapsis = (o.semiMajorAxis * 2) - desiredPeriapsis;
                o.eccentricity = (desiredApoapsis - desiredPeriapsis) / (desiredApoapsis + desiredPeriapsis);

                if (generator.Next(0, 100) > 50)
                    o.argumentOfPeriapsis = 270;
                else
                    o.argumentOfPeriapsis = 90;
            }
            else if (orbitType == OrbitType.SYNCHRONOUS || orbitType == OrbitType.STATIONARY)
            {
                o.semiMajorAxis = Util.synchronousSMA(targetBody);

                if (orbitType == OrbitType.SYNCHRONOUS)
                    o.eccentricity = eccentricity;
                else
                    o.eccentricity = 0.0;

                o.argumentOfPeriapsis = generator.NextDouble() * 360.0;
            }

            if (orbitType == OrbitType.POLAR)
                inc = 90;
            else if (orbitType == OrbitType.EQUATORIAL || orbitType == OrbitType.STATIONARY)
            {
                inc = 0;
                o.an = Vector3.zero;
                o.LAN = 0.0;
            }

            //Retrograde orbits are harder on Kerbin and the Sun, but otherwise, 50% chance.
            //Kolniya and Tundra have invalid inclinations until this point.
            if (targetBody == Planetarium.fetch.Home || targetBody == Planetarium.fetch.Sun)
            {
                if (orbitType == OrbitType.RANDOM || orbitType == OrbitType.POLAR || orbitType == OrbitType.EQUATORIAL || orbitType == OrbitType.SYNCHRONOUS)
                {
                    if (generator.Next(0, 100) < difficultyFactor * 50)
                        inc = 180.0 - inc;
                }
                else if (orbitType == OrbitType.KOLNIYA || orbitType == OrbitType.TUNDRA)
                {
                    if (generator.Next(0, 100) < difficultyFactor * 50)
                        inc = 116.6;
                    else
                        inc = 63.4;
                }
            }
            else
            {
                if (orbitType == OrbitType.RANDOM || orbitType == OrbitType.POLAR || orbitType == OrbitType.EQUATORIAL || orbitType == OrbitType.SYNCHRONOUS)
                {
                    if (generator.Next(0, 100) > 50)
                        inc = 180.0 - inc;
                }
                else if (orbitType == OrbitType.KOLNIYA || orbitType == OrbitType.TUNDRA)
                {
                    if (generator.Next(0, 100) > 50)
                        inc = 116.6;
                    else
                        inc = 63.4;
                }
            }

            o.inclination = inc;
            o.meanAnomalyAtEpoch = (double)0.999f + generator.NextDouble() * (1.001 - 0.999);
            o.epoch = (double)0.999f + generator.NextDouble() * (1.001 - 0.999);
            o.Init();
            PostProcessOrbit(ref o);
            return o;
        }

        public static void PostProcessOrbit(ref Orbit o)
        {
            if (HighLogic.LoadedSceneHasPlanetarium)
            {
                //These values can only be set in an appropriate scene. Setting them when the contract is generated is meaningless and will cause bad things to happen.
                o.UpdateFromStateVectors(o.getRelativePositionAtUT(0.0), o.getOrbitalVelocityAtUT(0.0), o.referenceBody, 0.0);
            }
        }

        public static string TitleCase(string str)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
        }

        public static CelestialBody RandomNeighbor(int seed, CelestialBody body, bool allowSun)
        {
            bool hasChildren = true;
            bool hasParent = true;

            System.Random generator = new System.Random(seed);

            if (body.orbitingBodies.Count <= 0)
                hasChildren = false;

            if (body.referenceBody == Planetarium.fetch.Sun && !allowSun)
                hasParent = false;

            if (body == Planetarium.fetch.Sun)
                hasParent = false;

            if (hasParent && hasChildren)
            {
                if (generator.Next(0, 100) > 50)
                    return body.orbitingBodies[generator.Next(0, body.orbitingBodies.Count)];
                else
                    return body.referenceBody;
            }
            else if (!hasParent && hasChildren)
                return body.orbitingBodies[generator.Next(0, body.orbitingBodies.Count)];
            else if (hasParent && !hasChildren)
                return body.referenceBody;
            else
                return null;
        }

        public static bool IsGasGiant(CelestialBody body)
        {
            if (body == null)
                return false;

            return (body.pqsController == null);
        }

        public static Vessel.Situations ApplicableSituation(int seed, CelestialBody body, bool splashAllowed)
        {
            System.Random generator = new System.Random(seed);
            List<Vessel.Situations> sitList = new List<Vessel.Situations>();

            sitList.Add(Vessel.Situations.ORBITING);

            if (body.ocean && splashAllowed)
                sitList.Add(Vessel.Situations.SPLASHED);

            if (!IsGasGiant(body))
                sitList.Add(Vessel.Situations.LANDED);

            return sitList[generator.Next(0, sitList.Count)];
        }

        public static double ResourcesOnVessel(Vessel v, string resourceName)
        {
            double totalAmount = 0.0;

            if (v != null)
            {
                foreach (Part part in v.Parts)
                {
                    if (part.Resources.Contains(resourceName))
                    {
                        totalAmount += part.Resources[resourceName].amount;
                    }
                }
            }

            return totalAmount;
        }

        public static string PossessiveString(string thing)
        {
            if (thing.EndsWith("s"))
                thing += "'";
            else
                thing += "'s";

            return thing;
        }
    }
}
