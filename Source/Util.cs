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

        // LoadNode does a variety of things. It fixes broken saves, for one. It keeps consistency and helps me root out bugs, for two.
        // It could be implemented as a template, but then I'd need to do a bunch of reflection, so instead I have a lot of overloads.

        public static void LoadNode(ConfigNode node, string nameOfClass, string nameOfValue, ref double value, double defaultValue)
        {
            bool hasValue = node.HasValue(nameOfValue);
            bool parsed = false;

            if (hasValue)
                parsed = double.TryParse(node.GetValue(nameOfValue), out value);

            if (!hasValue || !parsed)
            {
                Debug.LogWarning("Fine Print" + nameOfClass + " failed to load " + nameOfValue + ", initializing with default of " + defaultValue + "!");
                value = defaultValue;
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
            }
        }
    }
}
