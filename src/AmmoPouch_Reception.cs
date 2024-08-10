using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ammo_Pouch
{
    public class AmmoPouch_Reception : MonoBehaviour
    {
        public AmmoPouch pouch;

        public void OnTriggerStay(Collider other)
        {
            pouch.collisionTest(other);
        }
    }
}