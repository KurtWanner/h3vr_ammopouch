using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System;
using System.Linq;
using System.Text;
using HarmonyLib;

namespace Ammo_Pouch
{
    public static class AmmoPouch_Helpers
    {

        const char roundSeparator = ',';
        const char magSeparator = ';';

        public static int CompareMagContainersAscending(List<FireArmRoundClass> x, List<FireArmRoundClass> y)
        {
            if (x.Count < y.Count)
            {
                return -1;
            }
            else if (x.Count == y.Count)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        public static int CompareMagContainersDescending(List<FireArmRoundClass> x, List<FireArmRoundClass> y)
        {
            if (x.Count < y.Count)
            {
                return 1;
            }
            else if (x.Count == y.Count)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        public static int CompareSLContainersAscending(List<FireArmRoundClass> x, List<FireArmRoundClass> y)
        {

            // In case of accidents
            if (x.Count != y.Count)
                return CompareMagContainersAscending(x, y);

            int x_count = 0;
            int y_count = 0;

            FireArmRoundClass none = (FireArmRoundClass)(-1);

            for (int i = 0; i < x.Count; i++)
            {
                if (x[i] != none)
                    x_count++;
                if (y[i] != none)
                    y_count++;
            }

            if (x_count < y_count)
            {
                return -1;
            }
            else if (x_count == y_count)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        public static int CompareSLContainersDescending(List<FireArmRoundClass> x, List<FireArmRoundClass> y)
        {

            // In case of accidents
            if (x.Count != y.Count)
                return CompareMagContainersDescending(x, y);

            int x_count = 0;
            int y_count = 0;

            FireArmRoundClass none = (FireArmRoundClass)(-1);

            for (int i = 0; i < x.Count; i++)
            {
                if (x[i] != none)
                    x_count++;
                if (y[i] != none)
                    y_count++;
            }

            if (x_count < y_count)
            {
                return 1;
            }
            else if (x_count == y_count)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        public static void fillContainer(FVRPhysicalObject obj, List<FireArmRoundClass> classData, AmmoPouch.ContainerType container)
        {
            switch (container)
            {
                case AmmoPouch.ContainerType.SpeedLoader:
                    fillSpeedLoader((Speedloader)obj, classData);
                    break;
                case AmmoPouch.ContainerType.Clip:
                    fillClip((FVRFireArmClip)obj, classData);
                    break;
                case AmmoPouch.ContainerType.Magazine:
                    fillMagazine((FVRFireArmMagazine)obj, classData);
                    break;
                default:
                    break;
            }
        }

        public static void fillClip(FVRFireArmClip clip, List<FireArmRoundClass> classData)
        {

            // Unload clip
            while (clip.m_numRounds > 0)
            {
                clip.LoadedRounds[clip.m_numRounds - 1] = null;
                clip.m_numRounds--;
            }
            int count = Mathf.Min(clip.m_capacity, classData.Count);
            for (int i = count - 1; i >= 0; i--)
            {
                clip.AddRound(classData[i], false, false);
            }
            clip.UpdateBulletDisplay();
        }

        public static void fillMagazine(FVRFireArmMagazine mag, List<FireArmRoundClass> classData)
        {
            mag.m_numRounds = 0;
            int num = Mathf.Min(classData.Count, mag.m_capacity);
            for (int i = num - 1; i >= 0; i--)
                mag.AddRound(classData[i], false, false);

            mag.UpdateBulletDisplay();
        }

        public static void fillSpeedLoader(Speedloader loader, List<FireArmRoundClass> classData)
        {

            // Unload speedloader
            for (int i = 0; i < loader.Chambers.Count; i++)
            {
                loader.Chambers[i].Unload();
            }

            // Load class data
            for (int i = 0; i < classData.Count; i++)
            {
                if (classData[i] != (FireArmRoundClass)(-1))
                {
                    loader.Chambers[i].Load(classData[i]);
                }
            }
        }

        public static void printRoundClassData(List<FireArmRoundClass> data)
        {

            string s = "";
            for (int i = 0; i < data.Count; i++)
            {
                s += data[i] + " ";
            }
            Console.WriteLine(s);
        }

        public static List<List<FireArmRoundClass>> roundClassConfigure(string s)
        {

            Console.WriteLine("Configure from: " + s);

            List<List<FireArmRoundClass>> data = new List<List<FireArmRoundClass>>();

            // If empty string, then empty data
            if (string.IsNullOrEmpty(s))
            {
                return data;
            }

            string[] mags = s.Split(magSeparator);

            // Go through each list
            foreach (string mag in mags)
            {
                List<FireArmRoundClass> newMag = new List<FireArmRoundClass>();
                string[] rounds = mag.Split(roundSeparator);

                // Mag may be empty, which means the string is ""
                if (!string.IsNullOrEmpty(mag))
                {
                    // Go through each round in mag
                    foreach (string round in rounds)
                    {

                        // Attempt to parse, if not valid, skip
                        int n;
                        bool isValid = int.TryParse(round, out n);
                        if (isValid)
                        {
                            newMag.Add((FireArmRoundClass)(n));
                        }
                    }
                }

                // Insert at end
                data.Add(newMag);
            }

            return data;
        }

        public static string roundClassGet(List<List<FireArmRoundClass>> data)
        {
            StringBuilder sb = new StringBuilder();
            for (int mag = 0; mag < data.Count; mag++)
            {

                // CSV of round data
                for (int round = 0; round < data[mag].Count; round++)
                {
                    sb.Append(((int)data[mag][round]).ToString());
                    if (round < data[mag].Count - 1)
                    {
                        sb.Append(roundSeparator);
                    }
                }

                if (mag < data.Count - 1)
                {
                    sb.Append(magSeparator);
                }
            }

            return sb.ToString();
        }

    }

}