using System;
using System.Collections.Generic;

namespace Coslen.RogueTiler.Domain.Engine
{
    public class Element
    {
        public string Name { get; set; }

        public Element(string name)
        {
            Name = name;
        }
    }

    public class ElementFactory
    {
        private static ElementFactory instance;
        public static ElementFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ElementFactory();
                }
                return instance;
            }
        }
        private ElementFactory()
        {
            None = new Element("none");
            Air = new Element("air");
            Earth = new Element("earth");
            Fire = new Element("fire");
            Water = new Element("water");
            Acid = new Element("acid");
            Cold = new Element("cold");
            Lightning = new Element("lightning");
            Poison = new Element("poison");
            Dark = new Element("dark");
            Light = new Element("light");
            Spirit = new Element("spirit");
            Dire = new Element("dire");
        }


        public Element None { get; private set; }
        public Element Air { get; private set; }
        public Element Earth { get; private set; }
        public Element Fire { get; private set; }
        public Element Water { get; private set; }
        public Element Acid { get; private set; }
        public Element Cold { get; private set; }
        public Element Lightning { get; private set; }
        public Element Poison { get; private set; }
        public Element Dark { get; private set; }
        public Element Light { get; private set; }
        public Element Spirit { get; private set; }
        public Element Dire { get; private set; }

        public Element GetByName(string name)
        {
            switch (name)
            {
                case "None": return None;
                case "Air": return Air;
                case "Earth": return Earth;
                case "Fire": return Fire;
                case "Water": return Water;
                case "Acid": return Acid;
                case "Cold": return Cold;
                case "Lightning": return Lightning;
                case "Poison": return Poison;
                case "Dark": return Dark;
                case "Light": return Light;
                case "Spirit": return Spirit;
                case "Dire": return Dire;
                default:
                    throw new ApplicationException($"Unknown Element Requested = [{name}]");
            }
        }

        public Element GetByIndex(int index)
        {
            switch (index)
            {
                case 0: return None;
                case 1: return Air;
                case 2: return Earth;
                case 3: return Fire;
                case 4: return Water;
                case 5: return Acid;
                case 6: return Cold;
                case 7: return Lightning;
                case 8: return Poison;
                case 9: return Dark;
                case 10: return Light;
                case 11: return Spirit;
                case 12: return Dire;
                default:
                    throw new ApplicationException($"Unknown Element Requested = [{index}]");
            }
        }

        public Element this[int index]
        {
            get { return GetByIndex(index); }
        }

        public List<Element> All
        {
            get
            {
                var result = new List<Element>();
                result.Add(None);
                result.Add(Air);
                result.Add(Earth);
                result.Add(Fire);
                result.Add(Water);
                result.Add(Acid);
                result.Add(Cold);
                result.Add(Lightning);
                result.Add(Poison);
                result.Add(Dark);
                result.Add(Light);
                result.Add(Spirit);
                result.Add(Dire);
                return result;
            }
        }
    }
}