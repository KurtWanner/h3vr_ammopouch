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

    public class AmmoPouch : FVRFireArmMagazine
    {

        [Header("Pouch Attributes")]

        [HideInInspector]
        public int currentCapacity = 0;
        [HideInInspector]
        public int currentRoundCapacity = 0;
        [HideInInspector]
        public int totalCapacity = 10;
        [HideInInspector]
        public int containerCapacity = 10;
        [HideInInspector]
        public int proxyCount = 1;
        [HideInInspector]
        public int proxyCap = 5;
        [HideInInspector]
        public bool lockID = false;

        [HideInInspector]
        public bool vaultLoaded = false;

        public List<List<FireArmRoundClass>> roundClassData = new List<List<FireArmRoundClass>>();

        public int speedLoaderCapacity = 10;
        public int clipCapacity = 10;
        public int magazineCapacity = 10;
        public int roundCapacity = 40;
        public string lastObjId = null;
        public Dictionary<string, string> camoCodes = new Dictionary<string, string>();

        public AmmoPouch_UI UI = null;

        public AmmoPouch_Reception reception = null;
        public Transform displayMagContainer;
        [HideInInspector]
        public FVRPhysicalObject displayMag;

        [HideInInspector]
        public FireArmRoundClass None = (FireArmRoundClass)(-1);

        [HideInInspector]
        public enum ContainerType
        {
            Round,
            SpeedLoader,
            Clip,
            Magazine,
            None
        }

        [HideInInspector]
        public ContainerType container = ContainerType.None;

        public enum PouchSorting
        {
            LIFO,
            Fullest,
            Emptiest,
            Retention
        }

        [HideInInspector]
        public PouchSorting sortMethod = PouchSorting.Retention;

        public new void Awake()
        {
            base.Awake();
            sortMethod = PouchSorting.Retention;
        }

        public new void Start()
        {
            base.Start();
            UI.updateDisplay();

        }

        // @requires this.isRetention
        public int fullMags()
        {

            if (roundClassData.Count < 1)
                return 0;

            // Ceiling integer
            return (currentRoundCapacity + containerCapacity - 1) / containerCapacity;
        }


        public void resetPouch()
        {
            lastObjId = null;
            camoCodes = new Dictionary<string, string>(); ;
            currentCapacity = 0;
            currentRoundCapacity = 0;
            container = ContainerType.None;
            roundClassData = new List<List<FireArmRoundClass>>();
            if (!object.Equals(displayMag, null))
            {
                UnityEngine.Object.Destroy(displayMag.GameObject);
                displayMag = null;
            }
            sortMethod = PouchSorting.Retention;
            UI.updateDisplay();
        }



        public void convertToRetention()
        {

            // Print values
            for (int i = 0; i < roundClassData.Count; i++)
            {
                string s = "";
                for (int p = 0; p < roundClassData[i].Count; p++)
                {
                    s += roundClassData[i][p] + " ";
                }
                Console.WriteLine(s);
            }

            if (roundClassData.Count == 0)
            {
                roundClassData.Clear();
                currentCapacity = 0;
                currentRoundCapacity = 0;
                // If already retention / has mags
            }
            else
            {

                // New full round data
                List<FireArmRoundClass> newData = new List<FireArmRoundClass>();

                for (int mag = 0; mag < roundClassData.Count; mag++)
                {
                    for (int round = 0; round < roundClassData[mag].Count; round++)
                    {
                        if (roundClassData[mag][round] != None)
                        {
                            newData.Add(roundClassData[mag][round]);
                        }
                    }
                }

                // Set new data
                currentRoundCapacity = newData.Count;
                roundClassData.Clear();

                int fullMags = currentRoundCapacity / containerCapacity;
                int remainder = currentRoundCapacity % containerCapacity;
                int emptyMags = currentCapacity - (fullMags + ((remainder > 0) ? 1 : 0));


                // Add all empty mags
                for (int i = 0; i < emptyMags; i++)
                {
                    List<FireArmRoundClass> empty = new List<FireArmRoundClass>();

                    // If speedloader, fill with empty
                    if (container == ContainerType.SpeedLoader)
                    {
                        for (int round = 0; round < containerCapacity; round++)
                            empty.Add(None);
                    }

                    roundClassData.Add(empty);

                }


                // Add partial mag
                if (remainder > 0)
                {
                    List<FireArmRoundClass> partial = new List<FireArmRoundClass>();
                    for (int i = 0; i < remainder; i++)
                    {
                        partial.Add(newData[i]);
                    }

                    // Fill the rest with None
                    if (container == ContainerType.SpeedLoader)
                    {
                        for (int i = remainder; i < containerCapacity; i++)
                            partial.Add(None);
                    }

                    roundClassData.Add(partial);
                }


                // Add full mags
                for (int i = 0; i < fullMags; i++)
                {
                    List<FireArmRoundClass> mag_data = new List<FireArmRoundClass>();
                    for (int round = 0; round < containerCapacity; round++)
                    {
                        mag_data.Add(newData[i * containerCapacity + round + remainder]);
                    }
                    roundClassData.Add(mag_data);
                }

            }
        }

        public void convertToContainers()
        {
            if (roundClassData.Count > 0)
            {
                currentRoundCapacity = 0;
                return;
            }

            // Reset to 0
            currentCapacity = 0;
            currentRoundCapacity = 0;
        }

        public void sortLIFO()
        {

            if (sortMethod == PouchSorting.LIFO)
                return;

            if (sortMethod == PouchSorting.Retention)
            {
                convertToContainers();
            }
            sortMethod = PouchSorting.LIFO;
            updateDisplayMag();
            UI.updateDisplay();
        }

        public void sortFullestFirst()
        {
            if (sortMethod == PouchSorting.Fullest)
                return;

            if (sortMethod == PouchSorting.Retention)
            {
                convertToContainers();
            }

            roundClassData.Sort(AmmoPouch_Helpers.CompareMagContainersAscending);

            sortMethod = PouchSorting.Fullest;
            updateDisplayMag();
            UI.updateDisplay();
        }

        public void sortEmptiestFirst()
        {
            if (sortMethod == PouchSorting.Emptiest)
                return;

            if (sortMethod == PouchSorting.Retention)
            {
                convertToContainers();
            }

            roundClassData.Sort(AmmoPouch_Helpers.CompareMagContainersDescending);

            sortMethod = PouchSorting.Emptiest;
            updateDisplayMag();
            UI.updateDisplay();
        }

        public void sortRetention()
        {
            sortMethod = PouchSorting.Retention;
            convertToRetention();
            updateDisplayMag();
            UI.updateDisplay();
        }

        // Returns true if successful
        public bool attemptAddProxy()
        {
            if (proxyCount < proxyCap)
            {
                proxyCount++;
                if (currentCapacity > 0)
                    putRoundsInDisplay();
                return true;
            }
            return false;
        }

        // Returns true if successful
        public bool attemptSubProxy()
        {
            if (proxyCount > 1)
            {
                proxyCount--;
                if (currentCapacity > 0)
                    putRoundsInDisplay();
                return true;
            }
            return false;
        }


        public void updateDisplayMag()
        {

            if (currentCapacity == 0)
                return;

            // Put rounds in
            if(container == ContainerType.Round) {
                putRoundsInDisplay();
            } else {
                 if(displayMag != null && displayMag.ObjectWrapper.ItemID.Equals(lastObjId)) {
                    AmmoPouch_Helpers.fillContainer(displayMag, getNewClassData(), container);
                } else {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(IM.OD[lastObjId].GetGameObject(), displayMagContainer.transform.position, displayMagContainer.transform.rotation);
                    FVRPhysicalObject component = gameObject.GetComponent<FVRPhysicalObject>();
                    setDisplayMag(component);
                }
            }

            if (displayMag != null) {
                displayMag.ConfigureFromFlagDic(camoCodes);
            }
        }

        // Begin interaction stays the same as long as
        // this.m_isSpawnlockable is true
        // this.duplicatefromSpawnLock is implemented

        public int getTotalCapacity()
        {
            return this.totalCapacity;
        }

        public int getCurrentCapacity()
        {
            return this.currentCapacity;
        }

        public void testCamoCodes(FVRPhysicalObject obj)
        {
            if (lockID)
                return;
            camoCodes.Clear();
            Dictionary<string, string> flags = obj.GetFlagDic();
            List<string> keys = flags.Keys.Where(key => key.StartsWith("nga_mc")).ToList();
            foreach (string s in keys)
            {
                camoCodes.Add(s, flags[s]);
            }
        }

        public void addClassData(List<FireArmRoundClass> data)
        {
            switch (sortMethod)
            {
                case PouchSorting.LIFO:
                    roundClassData.Add(data);
                    currentCapacity = roundClassData.Count;
                    break;
                case PouchSorting.Retention:

                    roundClassData.Add(data);
                    currentCapacity++;
                    convertToRetention();
                    break;
                default:
                    int num_rounds = data.Count;

                    int cur_rounds;

                    // Iterate through all mags
                    for (int i = 0; i < currentCapacity; i++)
                    {
                        cur_rounds = roundClassData[i].Count;

                        // Go to next loop if not right position
                        if (sortMethod == PouchSorting.Fullest && num_rounds > cur_rounds)
                            continue;
                        if (sortMethod == PouchSorting.Emptiest && num_rounds < cur_rounds)
                            continue;

                        roundClassData.Insert(i, data);
                        currentCapacity++;
                        return;
                    }

                    // Else, add to end
                    roundClassData.Add(data);
                    currentCapacity++;
                    break;
            }
        }

        public void refillWithType(FireArmRoundClass rClass)
        {
            Console.WriteLine("Attempted to reload all with " + rClass);
            if (currentCapacity == 0)
                return;
            roundClassData.Clear();
            if (container != ContainerType.Round)
            {
                for (int mag = 0; mag < currentCapacity; mag++)
                {
                    List<FireArmRoundClass> newMag = new List<FireArmRoundClass>();
                    for (int round = 0; round < containerCapacity; round++)
                    {
                        newMag.Add(rClass);
                    }
                    roundClassData.Add(newMag);
                }

                // Set round count
                if (sortMethod == PouchSorting.Retention)
                {
                    currentRoundCapacity = containerCapacity * currentCapacity;
                }
            }
            else
            {
                roundClassData.Add(new List<FireArmRoundClass>());
                for (int i = 0; i < totalCapacity; i++)
                {
                    roundClassData[0].Add(rClass);
                }
                currentCapacity = totalCapacity;
            }
            updateDisplayMag();
            UI.updateDisplay();
        }

        public void duplicateMag()
        {
            if (currentCapacity >= totalCapacity)
            {
                return;
            }

            currentCapacity++;

            roundClassData.Insert(0, new List<FireArmRoundClass>());
            
            updateDisplayMag();
            UI.updateDisplay();
        }

        public void upgradeMag(FVRObject upgrade)
        {
            lastObjId = upgrade.ItemID;
            containerCapacity = upgrade.MagazineCapacity;
            if (sortMethod == PouchSorting.Retention)
            {
                convertToRetention();
            }
            updateDisplayMag();
            UI.updateDisplay();
        }

        //@requires lastObjId == loader.id
        public void addSpeedLoader(Speedloader loader)
        {
            if (currentCapacity >= totalCapacity || loader.gameObject == null || !loader.m_isVisible)
                return;

            // Get round class data
            int capacity = loader.Chambers.Count;
            List<FireArmRoundClass> classData = new List<FireArmRoundClass>();
            for (int i = 0; i < capacity; i++)
            {
                if (loader.Chambers[i].IsLoaded)
                {
                    classData.Add(loader.Chambers[i].LoadedClass);
                }
            }

            addClassData(classData);

            testCamoCodes(loader);

            loader.m_isVisible = false;
            Destroy(loader.gameObject);

            updateDisplayMag();

        }

        public void addClip(FVRFireArmClip clip)
        {
            if (currentCapacity >= totalCapacity || clip.gameObject == null || !clip.m_isVisible)
                return;

            // Get round data
            int capacity = clip.m_numRounds;
            List<FireArmRoundClass> classData = new List<FireArmRoundClass>();
            for (int i = 0; i < capacity; i++)
                if (clip.LoadedRounds[i] != null)
                    classData.Add(clip.LoadedRounds[i].LR_Class);

            addClassData(classData);

            testCamoCodes(clip);

            clip.m_isVisible = false;
            Destroy(clip.gameObject);

            updateDisplayMag();
        }



        public void addMag(FVRFireArmMagazine mag)
        {
            if (currentCapacity >= totalCapacity || mag.gameObject == null || !mag.m_isVisible)
                return;

            // Get class data
            int capacity = mag.m_numRounds;
            List<FireArmRoundClass> classData = new List<FireArmRoundClass>();
            for (int i = 0; i < capacity; i++)
                if (mag.LoadedRounds[i] != null)
                    classData.Add(mag.LoadedRounds[i].LR_Class);

            addClassData(classData);

            testCamoCodes(mag);

            mag.m_isVisible = false;
            Destroy(mag.gameObject);

            updateDisplayMag();
        }

        public void addRound(FVRFireArmRound round)
        {
            if (round.gameObject == null || !round.m_isVisible)
            {
                return;
            }

            if (currentCapacity >= totalCapacity || round.IsSpent)
                return;

            if (currentCapacity == 0)
            {
                roundClassData.Clear();
                roundClassData.Add(new List<FireArmRoundClass>());
            }

            roundClassData[0].Add(round.RoundClass);

            currentCapacity++;

            testCamoCodes(round);

            round.m_isVisible = false;
            Destroy(round.gameObject);

            putRoundsInDisplay();
        }

        public void setDisplayMag(FVRPhysicalObject obj)
        {

            if (currentCapacity == 0)
            {
                Destroy(obj.gameObject);
                return;
            }

            // Delete display mag if not null
            if (displayMag != null)
                Destroy(displayMag.gameObject);

            if (obj == null)
                return;

            //Console.WriteLine ("Setting display mag with " + obj.ObjectWrapper.ItemID);
            AmmoPouch_Helpers.fillContainer(obj, getNewClassData(), container);

            // Set camos
            obj.ConfigureFromFlagDic(camoCodes);

            // Turn off rigidbody to improve performance
            Rigidbody rb = obj.GameObject.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.detectCollisions = false;

            // Set parent of object so movement follow
            obj.GameObject.transform.SetParent(displayMagContainer);

            // Set position and rotation
            if (obj.QBPoseOverride != null)
            {
                obj.GameObject.transform.localPosition = -obj.QBPoseOverride.localPosition;
                obj.GameObject.transform.localRotation = obj.QBPoseOverride.localRotation;
            }
            else if (obj.PoseOverride != null)
            {
                obj.GameObject.transform.localPosition = -obj.PoseOverride.localPosition;
                obj.GameObject.transform.localRotation = obj.PoseOverride.localRotation;
            }
            else
            {
                obj.GameObject.transform.localPosition = new Vector3();
                obj.GameObject.transform.localRotation = Quaternion.identity;
            }


            // Adjust local position to see it easier
            switch (container)
            {
                case ContainerType.Round:
                    obj.GameObject.transform.localPosition += new Vector3(0f, 0.045f, 0f);
                    break;
                case ContainerType.SpeedLoader:
                    obj.GameObject.transform.localPosition += new Vector3(0f, 0.03f, 0f);
                    break;
                default:
                    obj.GameObject.transform.localPosition += new Vector3(0f, 0.015f, 0f);
                    break;
            }


            obj.GameObject.layer = LayerMask.NameToLayer("Default"); // Non interact

            displayMag = obj;

        }

        public List<FireArmRoundClass> getNewClassData()
        {

            // Class data for rounds is not handled here
            if (container == ContainerType.Round || currentCapacity == 0)
            {
                return new List<FireArmRoundClass>();
            }

            return roundClassData[currentCapacity - 1];
        }

        public List<FireArmRoundClass> removeRoundClassData()
        {
            List<FireArmRoundClass> data = getNewClassData();

            currentRoundCapacity = Math.Max(0, currentRoundCapacity - data.Count);
            roundClassData.RemoveAt(roundClassData.Count - 1);
            currentCapacity--;

            return data;
        }

        public FVRPhysicalObject duplicateDisplayMag()
        {
            GameObject gameObject;
            if(displayMag != null)
            {
                gameObject = UnityEngine.Object.Instantiate<GameObject>(displayMag.ObjectWrapper.GetGameObject(), displayMag.Transform.position, displayMag.Transform.rotation);
            }
            else
            {
                gameObject = UnityEngine.Object.Instantiate<GameObject>(IM.OD[lastObjId].GetGameObject(), displayMagContainer.position, displayMagContainer.rotation);
            }

            FVRPhysicalObject component = gameObject.GetComponent<FVRPhysicalObject>();

            return component;
        }

        // @requires displayMag != null 
        public FVRPhysicalObject removeDisplayMag(FVRViveHand hand)
        {
            if(displayMag == null && currentCapacity > 0)
            {
                updateDisplayMag();
            } else if(displayMag == null)
            {
                return null;
            }
            
            FVRPhysicalObject dup = null;
            if (container != ContainerType.Round)
            {
                dup = duplicateDisplayMag();
            }

            //Console.WriteLine("Current mag display: " + displayMag.ObjectWrapper.ItemID + " " + displayMag.Transform.position + " " + displayMag.Transform.rotation);

            // Detach display mag
            FVRPhysicalObject obj = displayMag;
            displayMagContainer.DetachChildren();
            displayMag = null;

            // Return rigid body
            Rigidbody rb = obj.GameObject.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.detectCollisions = true;
            obj.GameObject.layer = LayerMask.NameToLayer("Interactable");

            // Handle next display
            // Rounds
            if (container == ContainerType.Round)
            {

                // Only take what's needed
                int max_pull;
                if (this.IsHeld)
                {
                    max_pull = proxyCount;
                }
                else
                {
                    max_pull = ((FVRFireArmRound)obj).GetNumRoundsPulled(hand); 
                }
                int pull_count = Math.Min(proxyCount, max_pull);

                // Generate new proxy
                List<FireArmRoundClass> data = roundClassData[0];
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(AM.GetRoundSelfPrefab(RoundType, data[currentCapacity - 1]).GetGameObject(), displayMagContainer.transform.position, displayMagContainer.transform.rotation);
                FVRFireArmRound component = gameObject.GetComponent<FVRFireArmRound>();
                for (int i = currentCapacity - 2; i >= Math.Max(0, currentCapacity - pull_count); i--)
                {
                    component.AddProxy(data[i], AM.GetRoundSelfPrefab(RoundType, data[i]));
                }
                component.UpdateProxyDisplay();

                // Set camos on new obj
                component.ConfigureFromFlagDic(camoCodes);
                // Swap 
                Destroy(obj.GameObject);
                obj = component;

                // Update next rounds
                currentCapacity -= component.ProxyRounds.Count + 1;
                putRoundsInDisplay();

                // Mags/SL/Clips
            }
            else
            {
                List<FireArmRoundClass> data = removeRoundClassData();
                AmmoPouch_Helpers.fillContainer(dup, data, container);
                if (currentCapacity > 0)
                {
                    setDisplayMag(dup);
                }
                else
                {
                    Destroy(dup.gameObject);
                }
            }

            UI.updateDisplay();

            return obj;
        }

        // @requires currentCapacity > 0
        public void putRoundsInDisplay()
        {
            if (currentCapacity == 0)
                return;
            List<FireArmRoundClass> data = roundClassData[0];
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(AM.GetRoundSelfPrefab(RoundType, data[currentCapacity - 1]).GetGameObject(), displayMagContainer.transform.position, displayMagContainer.transform.rotation);
            FVRFireArmRound component = gameObject.GetComponent<FVRFireArmRound>();
            for (int i = currentCapacity - 2; i >= Math.Max(0, currentCapacity - proxyCount); i--)
            {
                component.AddProxy(data[i], AM.GetRoundSelfPrefab(RoundType, data[i]));
            }
            component.UpdateProxyDisplay();
            setDisplayMag(component);

        }

        // Returns true iff mag is not null, not being held and capacity is open
        bool basicChecks(FVRPhysicalObject mag)
        {
            if (object.Equals(mag, null))
                return false;

            //Console.WriteLine ("Mag attributes: " + mag.m_isHeld + " " + currentCapacity + " ");
            //Console.WriteLine("Item Id: " + mag.ObjectWrapper.ItemID);
            return mag != null && !mag.IsHeld && mag.Size == FVRPhysicalObjectSize.Small && object.Equals(mag.m_quickbeltSlot, null) &&
                (currentCapacity < totalCapacity || currentCapacity == 0);
        }

        public void putDisplayMagInHand(FVRViveHand hand)
        {
            FVRPhysicalObject component = removeDisplayMag(hand);
            if (component == null) return;

            // Force interaction of component into hand
            if (hand != null)
            {
                hand.ForceSetInteractable(component);
            }
            component.SetQuickBeltSlot(null);
            if (hand != null)
            {
                component.BeginInteraction(hand);
            }
        }

        public override void BeginInteraction(FVRViveHand hand)
        {

            // If being held by other hand
            if (this.IsHeld && !this.m_isHardnessed)
            {

                // If any capacity, put mag into hand
                if (currentCapacity > 0)
                {
                    putDisplayMagInHand(hand);
                    return;

                    // If no capacity, swap to other hand
                }
                else
                {
                    this.ForceBreakInteraction();
                    if (hand != null)
                    {
                        hand.ForceSetInteractable(this);
                        this.BeginInteraction(hand);
                    }
                }

                // Not held, but is hardnessed
            }
            else if (this.m_isHardnessed)
            {

                // If any capacity, put mag into hand
                if (currentCapacity > 0)
                {
                    putDisplayMagInHand(hand);

                    // If no capacity, put pouch into hand
                }
                else
                {
                    base.BeginInteraction(hand);
                }

                // Default case, put pouch in hand
            }
            else
            {
                base.BeginInteraction(hand);
            }
        }

        public void collisionTest(Collider col)
        {

            // Test the parent object of the collider
            Collider collider = col;
            Speedloader speedload = collider.gameObject.GetComponent<Speedloader>();
            FVRFireArmClip clip = collider.gameObject.GetComponent<FVRFireArmClip>();
            FVRFireArmMagazine mag = collider.gameObject.GetComponent<FVRFireArmMagazine>();
            FVRFireArmRound round = collider.gameObject.GetComponent<FVRFireArmRound>();

            FVRPhysicalObject obj;

            ContainerType contType;
            FireArmRoundType rndType;
            string itemID;

            if (basicChecks(speedload))
            {
                contType = ContainerType.SpeedLoader;
                rndType = speedload.Chambers[0].Type;
                obj = speedload;
                itemID = speedload.ObjectWrapper.ItemID;
            }
            else if (basicChecks(clip) && clip.FireArm == null)
            {
                contType = ContainerType.Clip;
                rndType = clip.RoundType;
                obj = clip;
                itemID = clip.ObjectWrapper.ItemID;
            }
            else if (basicChecks(mag) && mag.FireArm == null && !(mag is AmmoPouch))
            {
                contType = ContainerType.Magazine;
                rndType = mag.RoundType;
                obj = mag;
                itemID = mag.ObjectWrapper.ItemID;
            }
            else if (basicChecks(round))
            {
                contType = ContainerType.Round;
                rndType = round.RoundType;
                obj = round;
                itemID = round.ObjectWrapper.ItemID;
            }
            else
            {
                return; // TODO make sure magazines from Melon works
            }

            // Prevents other itemIDs from being accepted when locked
            if (lockID && !lastObjId.Equals(itemID) && container != ContainerType.Round)
                return;

            // Allow rounds of same type but different class to still work with lockID
            if (lockID && container == ContainerType.Round && RoundType != rndType) 
                return;

            // If the pouch is empty
            if (currentCapacity == 0)
            {

                // Only used for retention mode
                currentRoundCapacity = 0;

                // For rounds: if the round type is the same, don't reset the proxy count
                bool resetProxy = true;
                if (round != null)
                {
                    resetProxy = RoundType != round.RoundType;
                }

                lastObjId = obj.ObjectWrapper.ItemID;

                switch (contType)
                {
                    
                    // Set up new speedloader
                    case ContainerType.SpeedLoader:
                        

                        container = ContainerType.SpeedLoader;
                        containerCapacity = speedload.Chambers.Count;
                        RoundType = speedload.Chambers[0].Type;
                        totalCapacity = speedLoaderCapacity;
                        addSpeedLoader(speedload);
                        break;

                    // Set up new clip
                    case ContainerType.Clip:
                        containerCapacity = clip.m_capacity;
                        container = ContainerType.Clip;
                        RoundType = clip.RoundType;
                        totalCapacity = clipCapacity;
                        addClip(clip);
                        break;

                    // Set up new magazine
                    case ContainerType.Magazine:
                        containerCapacity = mag.m_capacity;
                        container = ContainerType.Magazine;
                        RoundType = mag.RoundType;
                        totalCapacity = magazineCapacity;
                        addMag(mag);
                        break;

                    // Set up new round
                    case ContainerType.Round:
                        containerCapacity = 1;
                        container = ContainerType.Round;
                        RoundType = round.RoundType;
                        totalCapacity = roundCapacity;
                        proxyCap = round.MaxPalmedAmount;
                        addRound(round);
                        if (resetProxy)
                            proxyCount = 1;
                        break;
                }

            }

            // Check round
            else if (container == ContainerType.Round && contType == container && rndType == RoundType)
            {
                addRound(round);
            }

            // If same object and empty capacity, add
            else if (lastObjId.Equals(itemID) && currentCapacity < totalCapacity)
            {
                switch (contType)
                {
                    case ContainerType.SpeedLoader:
                        addSpeedLoader(speedload);
                        break;
                    case ContainerType.Clip:
                        addClip(clip);
                        break;
                    case ContainerType.Magazine:
                        addMag(mag);
                        break;
                    case ContainerType.Round:
                        addRound(round);
                        break;
                }
            }

            // If has same type of rounds, steal that shit
            else if (RoundType == rndType)
            {
                stealRounds(obj, contType);
            }

            UI.updateDisplay();

        }

        public void stealRounds(FVRPhysicalObject obj, ContainerType type)
        {
            //Console.WriteLine("Stealing rounds.");
        }

        public override void ConfigureFromFlagDic(Dictionary<string, string> f)
        {

            base.ConfigureFromFlagDic(f);
            string key = string.Empty;
            string value = string.Empty;

            key = "currentCapacity";
            if (f.ContainsKey(key))
            {
                value = f[key];
                currentCapacity = Convert.ToInt32(value);
            }

            key = "currentRoundCapacity";
            if (f.ContainsKey(key))
            {
                value = f[key];
                currentRoundCapacity = Convert.ToInt32(value);
            }

            key = "containerCapacity";
            if (f.ContainsKey(key))
            {
                value = f[key];
                containerCapacity = Convert.ToInt32(value);
            }

            key = "containerType";
            if (f.ContainsKey(key))
            {
                value = f[key];
                container = (ContainerType)(Convert.ToInt32(value));
                switch (container)
                {
                    case ContainerType.Magazine:
                        totalCapacity = magazineCapacity;
                        break;
                    case ContainerType.Clip:
                        totalCapacity = clipCapacity;
                        break;
                    case ContainerType.SpeedLoader:
                        totalCapacity = speedLoaderCapacity;
                        break;
                    case ContainerType.Round:
                        totalCapacity = roundCapacity;
                        break;
                }
            }

            key = "sortMethod";
            if (f.ContainsKey(key))
            {
                value = f[key];

                sortMethod = (PouchSorting)Convert.ToInt32(value);

            }

            key = "roundClassData";
            if (f.ContainsKey(key))
            {
                value = f[key];
                roundClassData = AmmoPouch_Helpers.roundClassConfigure(value);
            }

            key = "lastObjID";
            if (f.ContainsKey(key))
            {
                value = f[key];
                lastObjId = value;
            }

            key = "proxyCount";
            if (f.ContainsKey(key))
            {
                value = f[key];
                proxyCount = Convert.ToInt32(value);
            }

            key = "proxyCap";
            if (f.ContainsKey(key))
            {
                value = f[key];
                proxyCap = Convert.ToInt32(value);
            }

            key = "lockID";
            if (f.ContainsKey(key))
            {
                value = f[key];
                lockID = Convert.ToBoolean(value);
            }

            key = "roundType";
            if (f.ContainsKey(key))
            {
                value = f[key];
                RoundType = (FireArmRoundType)Convert.ToInt32(value);
            }

            List<string> camos = f.Keys.Where(i => i.StartsWith("item_nga_mc")).ToList();

            foreach (string s in camos)
            {
                camoCodes.Add(s.Substring(5), f[s]);
            }

            updateDisplayMag();
            UI.updateDisplay();
            UI.setContainerText(container);

        }

        public override Dictionary<string, string> GetFlagDic()
        {
            Dictionary<string, string> flagDic = base.GetFlagDic();

            flagDic.Add("currentCapacity", currentCapacity.ToString());
            flagDic.Add("currentRoundCapacity", currentRoundCapacity.ToString());
            flagDic.Add("containerCapacity", containerCapacity.ToString());
            flagDic.Add("containerType", ((int)container).ToString());
            flagDic.Add("sortMethod", ((int)sortMethod).ToString());
            flagDic.Add("roundClassData", AmmoPouch_Helpers.roundClassGet(roundClassData));
            flagDic.Add("lastObjID", lastObjId);
            flagDic.Add("proxyCount", proxyCount.ToString());
            flagDic.Add("proxyCap", proxyCap.ToString());
            flagDic.Add("lockID", lockID.ToString());
            flagDic.Add("roundType", ((int)RoundType).ToString());

            foreach (string s in camoCodes.Keys)
            {
                flagDic.Add("item_" + s, camoCodes[s]);
            }

            return flagDic;
        }


    }

}