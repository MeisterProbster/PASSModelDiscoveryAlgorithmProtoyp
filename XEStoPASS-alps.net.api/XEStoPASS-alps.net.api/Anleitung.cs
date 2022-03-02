using System;
using System.Collections.Generic;
using System.Text;

namespace XEStoPASS_alps.net.api
{
    class Anleitung
    {

        /*Erstelle PASS-Modell
                 * 
                 * Lese .xes ein
                 * 
                 * Ordne allen Ressourcen ihre Events inkl. Vorgänger, Nachfolger (und timestamp/Reihenfolge im Trace zu)
                 *         
                 *         Wenn Ressource noch nicht vorhanden:
                 *              speichere zusammen Eventname, Vorgänger, Nachfolger
                 *              
                 *              Klasse Ressourcen
                 *                  Eigenschaften Name, Vorgänger, Nachfolger, Eventname
                 *                  
                 *                 Methoden get, set
                 *                 
                 *                 events sortieren
                 *                 
                 *                 RessourcenListe anlegen
                 *              
                 *         Wenn Ressource schon vorhanden
                 *              kontrollieren ob Eventname schon existiert
                 *                  falls ja: 
                 *                      füge Vorgänger zu Vorgänger hinzu, falls noch nicht vorhanden
                 *                      füge Nachfolger zu Nachfolger hinzu, falls noch nicht vorhanden
                 *                  falls nein: 
                 *                      speichere zusammen Eventname, Vorgänger, Nachfolger
                 * 
             * Vergleiche Ressourcen und fasse Ressourcen mit gleichen Events zusammen + aktualisiere Vorgänger/NachfolgerListe
             * 
                 *Erstelle Subjekte der zusammengefassten Ressourcen
             * 
             * Erstelle Footprintmatrizen für die Subjekte und ihrer Events
             * 
                 *Erstelle SBDs der Subjekte
                 *
                 *Erstelle für jedes Event einen DoState
                 *
                 *Erstelle für jeden anderen Nachfolger eines Events als das Subjekt selbst einen SendState
                 *      
                 *Erstelle für jeden anderen Vorgänger eines Events als das Subjekt selbst einen ReceiveState
                 *
                 *Erstelle DoTransitions
                 *      falls Nachfolger ein anderer, verbinde mit SendState
                 *      
                 *      falls Nachfolger gleiches Subjekt, verbinde mit nächstem DoState(s) (laut Footprintmatrix?)
                 * 
                 * Erstelle ReceiveTransitions
                 *      verbinde mit DoState
                 * 
                 * Erstelle SendTransitions
                 *      verbinde mit nächstem ReceiveState
                 *      falls keines mehr vorhanden --> erstelle DoState "End"
                 * 
                 * 
                 *
                 *
                 *Messages: 
                 *
                 *erstelle für jeden anderen Nachfolger eines Events eine Message (und bringe sie an die SendTransition an?)
                 *
             *
             *
             *Fragen: 
             *
             *AddMessage / AddSBD / AddState / AddSubject / AddTransition
             *
             *übergeben von string
             *
             *
             *
             *
             *Startevents
             *
             *
             *
             *
             * methoden definieren um vorheriges und next zu resource hinzufügen ( generell evnt hinzufügen : gibts das event schon in der Resource? Ausgabe an welcher Stelle das Event in der Liste vorkommt, an der Stelle in der Liste der vorherigen und nachfolgenden anlegen )
             *
             *
             *08.02.2022
             *Nur started events einlesen??
             *
             *Bei den Vorgängern(vor start event), erstes vorgängerobjekt, welches completed, ist vorgänger 
             *
             *Bei den Nachfolgern(nach completed event), erstes Nachfolgerobjekt, das started ist nachfolger
             *
             *Vergleichen
             *
             *vorgänger werde nicht gelistet
             *
             *Was passiert wenn keine ressource vorhanden?
             *
             *Anpassen der Subjektlisten: Nachfolger und vorgängerlisten werden zu RessourcenListen umgewandelt
             *  --> Messages können erstellt werden (da Nachfolgersubjekt bekannt)
             *  --> States können
             *
             *
             *
             *
             *
             *Do-State Gruppen bilden
             *          Ressourcen durch gleiche/ähnliche Events zu Subjekten zusammenfassen
             *              -Vorgänger/Nachfolgerbeziehungen über Petrinetz klären
             *                  --> Vorgänger über die Transitionen vor der/den Stelle/n ausmachen
             *                  --> Nachfolger über die Stellen und ihre anschließenden Transitionen ausmachen
             *          SBDs erstellen
             *          Do-States bilden
             *          SendStates + ReceiveStates
             *
             *Reihenfolge der Do-State Gruppen im SBD
             *          Alle möglichen Kombinationen der Events eines Subjektes in den Traces suchen
             *              -> sobald in einer Kombi ein Event nach einem anderen kommt --> Verbindung von First to Second (Falls noch keine vorhanden)
             *              -> falls alle Kombinationen durchlaufen füge nach jedem letzten Event ein EndDoState ein (Falls noch nicht vorhanden)
             *
             *
             *
             *
             * */
    }
}
