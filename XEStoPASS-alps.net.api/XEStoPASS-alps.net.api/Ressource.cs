using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace XEStoPASS_alps.net.api
{
    public class Ressource
    {
        public string Name { get; set; }

        public string SubjectName { get; set; }

        public List<Event> Events { get; set; }

        public List<List<Event>> Vorgänger { get; set; }

        public List<List<Event>> Nachfolger { get; set; }

        public List<string> EventNameList { get; set; }

        public Ressource(string name)
        {
            Name = name;
            Events = new List<Event>();
            Vorgänger = new List<List<Event>>();
            Nachfolger = new List<List<Event>>();
            EventNameList = new List<string>();
        }

        /// <summary>
        /// Gibt den Übereinstimmungswert der EventNameLists zurück
        /// </summary>
        /// <param name="compareResource"></param>
        /// <param name="considerSubsets">bestimmt ob beim Vergleich untermengen berücksichtigt werden oder nicht</param>
        /// <returns>1 wenn 100% übereinstimmung, 0 wenn keine</returns>
        public double compareEventNameList(Ressource compareResource, bool considerSubsets = false)
        {
            double result = 0;
            double numberOfMyEventNamesInCompareResource = 0;

            foreach(string tempString in this.EventNameList)
            {
                if (compareResource.EventNameList.Contains(tempString))
                {
                    numberOfMyEventNamesInCompareResource = numberOfMyEventNamesInCompareResource + 1;
                }
            }

            //Szenario 1: AnzahlEvents gleich und numberofmyeventsincompareresource --> retunr 1
            //Szenario 6: AnzahlEvents gleich und numberofmyeventsincompareresource < Anzahlevents return numberofmyeventsincompareresource/Anzahlevents
            if (this.EventNameList.Count() == compareResource.EventNameList.Count())
            {
                result = numberOfMyEventNamesInCompareResource / this.EventNameList.Count();
            }

            //Szenario 2: MyAnzahlevents < comparedResourceAnzahlEvents und MyAnzahlevents = numberofmyeventsincompareresource
            //Szenario 4: MyAnzahlevents < comparedResourceAnzahlEvents und MyAnzahlevents > numberofmyeventsincompareresource
            else if (this.EventNameList.Count() < compareResource.EventNameList.Count())
            {
                if (considerSubsets)
                {
                    result = numberOfMyEventNamesInCompareResource / this.EventNameList.Count();
                }
                else
                {
                    result = numberOfMyEventNamesInCompareResource / compareResource.EventNameList.Count();
                }
            }

            //Szenario 5: MyAnzahlevents > comparedResourceAnzahlEvents und comparedResourceAnzahlEvents > numberofmyeventsincompareresource
            //Szenario 3: MyAnzahlevents > comparedResourceAnzahlEvents und comparedResourceAnzahlEvents = numberofmyeventsincompareresource
            else if (this.EventNameList.Count() > compareResource.EventNameList.Count())
            {
                if (considerSubsets)
                {
                    result = numberOfMyEventNamesInCompareResource / compareResource.EventNameList.Count();
                }
                else
                {
                    result = numberOfMyEventNamesInCompareResource / this.EventNameList.Count();
                }
            }
            return result;
        }


        //leere Übergaben + wahrheitswerte überprüfen
        public void AddEvent(Event current, Event previous, Event next)
        {
            if (!this.Events.Any(p => p.Name == current.Name && p.Resource == current.Resource))
            {
                this.Events.Add(current);
                this.EventNameList.Add(current.Name);
                this.Vorgänger.Add(new List<Event> { });
                this.Nachfolger.Add(new List<Event> { });

                if (previous != null)
                {
                    this.Vorgänger.Last().Add(previous);
                }

                if (next != null)
                {
                    this.Nachfolger.Last().Add(next);
                }
            }
            else
            { //Funktion, die die Liste der Ressourcen durchläuft  

                int i = this.Events.FindIndex(p => p.Name == current.Name);

                if (previous != null)
                {
                    if (!this.Vorgänger[i].Any(p => p.Name == previous.Name && p.Resource == previous.Resource))
                    {
                        this.Vorgänger[i].Add(previous);
                    }
                }

                if (next != null)
                {
                    if(!this.Nachfolger[i].Any(p => p.Name == next.Name && p.Resource == next.Resource))
                    {
                        this.Nachfolger[i].Add(next);
                    }
                }
            }
        }
    }
}
