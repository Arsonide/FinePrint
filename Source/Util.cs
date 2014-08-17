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

namespace FinePrint
{
	class Util
    {
        public const int frameSuccessDelay = 5;
        static public bool patchReset = false;

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
			tech.Replace('_', '.');
			AvailablePart ap = PartLoader.getPartInfoByName(tech);

			if (ap != null)
			{
				if (ResearchAndDevelopment.PartTechAvailable(ap))
					return true;
			}
			else
				Debug.LogWarning("Fine Print: Attempted to check for nonexistent technology.");

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
			List<string> suffix = new List<string> { "Folly", "Hope", "Legacy", "Doom", "Rock", "Gambit", "Bane", "End", "Drift", "Frontier", "Pride", "Retreat", "Escape", "Legend", "Sector", "Abyss", "Void", "Vision", "Wisdom", "Refuge", "Doubt", "Redemption", "Anomaly", "Trek", "Monolith", "Emitter", "Wonder", "Lament", "Hindsight", "Mistake", "Isolation", "Hole", "Jest", "Stretch", "Scar", "Surprise", "Whim", "Whimsy", "Target", "Insanity", "Goal", "Dirge", "Adventure", "Fate", "Point", "Decent", "Ascent", "Dawn", "Dusk" };
			List<string> kerbinSuffix = new List<string> { "Backyard", "Bar and Grill", "Junkyard", "Lab", };
			List<string> alphaNumeric = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", };
			List<string> developerNames = new List<string> { "Ayarza's", "Goya's", "Falanghe's", "Mora's", "Geelan's", "Salcedo's", "Jenkins'", "Rosas'", "Safi's", "Benjaminsen's", "Piña's", "Montaño's", "Holtzman's", "Everett's", "Guzzardo's", "Reyes'", "Dominguez'", "Gutiérrez'", "Demeneghi's", "Vázquez'", "Rosas'", "Maqueo's", "Silisko's", "Keeton's", "Kupperian's", "Chiarello's", "Zuev's", "Nelson's" };
			System.Random generator = new System.Random(seed);
			string siteName = "";

			if (generator.Next(0, 101) > 75)
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

		public static CelestialBody RandomJoolianMoon()
		{
			int randomMoon = UnityEngine.Random.Range(0, 5);
			string targetMoon = "Laythe";

			switch (randomMoon)
			{
				case 0:
					targetMoon = "Laythe";
					break;
				case 1:
					targetMoon = "Vall";
					break;
				case 2:
					targetMoon = "Tylo";
					break;
				case 3:
					targetMoon = "Bop";
					break;
				case 4:
					targetMoon = "Pol";
					break;
				default:
					targetMoon = "Laythe";
					break;
			}

			foreach (CelestialBody body in FlightGlobals.Bodies)
			{
				if (body.GetName() == targetMoon)
					return body;
			}

			return null;
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

        #region LoadNode Overloads
        // LoadNode does a variety of things. It fixes broken saves, for one. It keeps consistency and helps me root out bugs, for two.
		// It could be implemented as a template, but then I'd need to do a bunch of reflection, so instead I have a lot of overloads.

		public static void LoadNode(ConfigNode node, string nameOfClass, string nameOfValue, ref double value, double defaultValue)
		{
			bool hasValue = node.HasValue(nameOfValue);
			bool parsed = false;

			if (hasValue)
				parsed = double.TryParse(node.GetValue(nameOfValue), out value);

            if (!hasValue)
                Debug.LogWarning("Fine Print" + nameOfClass + " does not have " + nameOfValue + "!");

            if (!parsed)
                Debug.LogWarning("Fine Print" + nameOfClass + " could not parse " + nameOfValue +"! String: " + node.GetValue(nameOfValue));

			if (!hasValue || !parsed)
			{
				Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of " + defaultValue + "!");
				value = defaultValue;
                resetBoard();
			}
		}

		public static void LoadNode(ConfigNode node, string nameOfClass, string nameOfValue, ref float value, float defaultValue)
		{
			bool hasValue = node.HasValue(nameOfValue);
			bool parsed = false;

			if (hasValue)
				parsed = float.TryParse(node.GetValue(nameOfValue), out value);

			if (!hasValue || !parsed)
			{
				Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of " + defaultValue + "!");
				value = defaultValue;
                resetBoard();
			}
		}

		public static void LoadNode(ConfigNode node, string nameOfClass, string nameOfValue, ref int value, int defaultValue)
		{
			bool hasValue = node.HasValue(nameOfValue);
			bool parsed = false;

			if (hasValue)
				parsed = int.TryParse(node.GetValue(nameOfValue), out value);

			if (!hasValue || !parsed)
			{
				Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of " + defaultValue + "!");
				value = defaultValue;
                resetBoard();
			}
		}

		public static void LoadNode(ConfigNode node, string nameOfClass, string nameOfValue, ref bool value, bool defaultValue)
		{
			bool hasValue = node.HasValue(nameOfValue);
			bool parsed = false;

			if (hasValue)
				parsed = bool.TryParse(node.GetValue(nameOfValue), out value);

			if (!hasValue || !parsed)
			{
				Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of " + defaultValue + "!");
				value = defaultValue;
                resetBoard();
			}
		}

		public static void LoadNode(ConfigNode node, string nameOfClass, string nameOfValue, ref string value, string defaultValue)
		{
			if (node.HasValue(nameOfValue))
				value = node.GetValue(nameOfValue);
			else
			{
				Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of " + defaultValue + "!");
				value = defaultValue;
                resetBoard();
			}
		}

		public static void LoadNode(ConfigNode node, string nameOfClass, string nameOfValue, ref CelestialBody body, CelestialBody defaultBody)
		{
			bool hasValue = node.HasValue(nameOfValue);
			bool parsed = false;

			if (hasValue)
			{
				int bodyID = 0;
				parsed = int.TryParse(node.GetValue(nameOfValue), out bodyID);

				if (parsed)
				{
					foreach (var cb in FlightGlobals.Bodies)
					{
						if (cb.flightGlobalsIndex == bodyID)
							body = cb;
					}
				}
			}

			if (!hasValue || !parsed)
			{
				body = Planetarium.fetch.Home;
				Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of " + defaultBody.GetName() + "!");
                resetBoard();
            }
		}

		public static void LoadNode(ConfigNode node, string nameOfClass, string nameOfValue, ref Vessel.Situations situation, Vessel.Situations defaultSituation)
		{
			bool hasValue = node.HasValue(nameOfValue);
			bool parsed = false;

			if (hasValue)
			{
				int loadedSituationInteger = 0;
				parsed = int.TryParse(node.GetValue(nameOfValue), out loadedSituationInteger);
				situation = (Vessel.Situations)loadedSituationInteger;
			}

			if (!hasValue || !parsed)
			{
				switch (defaultSituation)
				{
					case Vessel.Situations.PRELAUNCH:
						Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of PRELAUNCH!");
						break;
					case Vessel.Situations.SPLASHED:
						Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of SPLASHED!");
						break;
					case Vessel.Situations.SUB_ORBITAL:
						Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of SUB_ORBITAL!");
						break;
					case Vessel.Situations.ORBITING:
						Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of ORBITING!");
						break;
					case Vessel.Situations.LANDED:
						Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of LANDED!");
						break;
					case Vessel.Situations.FLYING:
						Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of FLYING!");
						break;
					case Vessel.Situations.ESCAPING:
						Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of ESCAPING!");
						break;
					case Vessel.Situations.DOCKED:
						Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of DOCKED!");
						break;
					default:
						Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of UNKNOWN!");
						break;
				}

				situation = defaultSituation;
                resetBoard();
			}
        }

        public static void LoadNode(ConfigNode node, string nameOfClass, string nameOfValue, ref OrbitType orbitType, OrbitType defaultOrbitType)
        {
            bool hasValue = node.HasValue(nameOfValue);
            bool parsed = false;

            if (hasValue)
            {
                int loadedOrbitTypeInteger = 0;
                parsed = int.TryParse(node.GetValue(nameOfValue), out loadedOrbitTypeInteger);
                orbitType = (OrbitType)loadedOrbitTypeInteger;
            }

            if (!hasValue || !parsed)
            {
                switch (defaultOrbitType)
                {
                    case OrbitType.EQUATORIAL:
                        Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of EQUATORIAL!");
						break;
                    case OrbitType.POLAR:
                        Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of POLAR!");
						break;
                    case OrbitType.RANDOM:
                        Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of RANDOM!");
						break;
                    case OrbitType.STATIONARY:
                        Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of STATIONARY!");
						break;
                    case OrbitType.SYNCHRONOUS:
                        Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of SYNCHRONOUS!");
						break;
                    case OrbitType.KOLNIYA:
                        Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of KOLNIYA!");
                        break;
                    case OrbitType.TUNDRA:
                        Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of TUNDRA!");
                        break;
                    default:
                        Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of UNKNOWN!");
                        break;
                }

                orbitType = defaultOrbitType;
                resetBoard();
            }
        }
        #endregion

        public static void resetBoard()
        {
            if (!patchReset)
            {
                Debug.LogError("Fine Print has detected save game incompatibilities and is resetting the contract board to fix them. This usually happens after a patch.");
                ContractSystem.Instance.ClearContractsCurrent();
                patchReset = true;
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
            if ((object)body == null)
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
            if ((object)body == null)
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
            if ((object)body == null)
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
            double inc = (generator.NextDouble() * 90.0) * difficultyFactor;
            double desiredPeriapsis = 0.0;
            double desiredApoapsis = 0.0;
            double pointA = 0.0;
            double pointB = 0.0;
            float maximumAltitude = 0f;
            double easeFactor = 1.0 - difficultyFactor;
            double minimumAltitude = Util.getMinimumOrbitalAltitude(targetBody);
            o.referenceBody = targetBody;

            //If it chooses the sun, the infinite SOI can cause NAN, so choose Eeloo's altitude instead.
            //Use 90% of the SOI to give a little leeway for error correction.
            if (targetBody == Planetarium.fetch.Sun)
                maximumAltitude = 113549713200f;
            else
                maximumAltitude = Math.Max((float)minimumAltitude, (float)targetBody.sphereOfInfluence * (float)difficultyFactor);

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
                inc = 0;

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
            o.LAN = generator.NextDouble() * 360.0;
            o.meanAnomalyAtEpoch = (double)0.999f + generator.NextDouble() * (1.001 - 0.999);
            o.epoch = (double)0.999f + generator.NextDouble() * (1.001 - 0.999);
            o.Init();
            Vector3d pos = o.getRelativePositionAtUT(0.0);
            Vector3d vel = o.getOrbitalVelocityAtUT(0.0);
            o.h = Vector3d.Cross(pos, vel);

            return o;
        }
    }
}
