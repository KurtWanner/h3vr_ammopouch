using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using UnityEngine.UI;
using System;

namespace Ammo_Pouch
{
    public class AmmoPouch_UI : MonoBehaviour
    {

        public Text CapacityTxt;

        public AudioEvent SortSound;
        public AudioEvent BtnSound;

        [Header("Default Page")]
        public GameObject DefaultPage;

        public Text ContainerTypeTxt;

        public GameObject SettingsBtn;

        // Reset Btns
        public GameObject ResetBtn;
        public GameObject ResetConfirmBtn;
        public GameObject ResetCancelBtn;

        public GameObject IDLockOffBtn;
        public GameObject IDLockOnBtn;

        [Header("Settings")]

        public GameObject SettingsPage;

        public Text SettingsContainerTxt;

        public GameObject SortingTab;
        public OptionsPanel_ButtonSet SortingSet;
        public GameObject[] SortButtons;

        public int _selectedSort = 1;

        public GameObject ProxyTab;
        public Text ProxyCount;
        public GameObject AddProxyBtn;
        public GameObject SubProxyBtn;

        public GameObject ReturnBtn;

        public AmmoPouch AmmoPouch;

        [HideInInspector]
        public enum UIPage
        {
            Default,
            Settings,
            Cheats
        }

        [HideInInspector]
        UIPage currentPage = UIPage.Default;

        [HideInInspector]
        const int LIFO = 0;
        const int RETENTION = 1;
        const int FULLEST = 2;
        const int EMPTIEST = 3;


        public void resetCheck()
        {
            ResetBtn.SetActive(false);
            ResetCancelBtn.SetActive(true);
            ResetConfirmBtn.SetActive(true);
        }

        public void resetConfirm()
        {

            ResetBtn.SetActive(true);
            ResetCancelBtn.SetActive(false);
            ResetConfirmBtn.SetActive(false);
            AmmoPouch.resetPouch();
        }

        public void resetCancel()
        {

            ResetBtn.SetActive(true);
            ResetCancelBtn.SetActive(false);
            ResetConfirmBtn.SetActive(false);

        }

        public void SetDefaultUI()
        {
            currentPage = UIPage.Default;
            SettingsPage.SetActive(false);
            DefaultPage.SetActive(true);
            SettingsBtn.SetActive(true);
            ResetBtn.SetActive(true);
            ResetConfirmBtn.SetActive(false);
            ResetCancelBtn.SetActive(false);

            setLockIDColor();

        }

        public void SetSettingsUI()
        {

            currentPage = UIPage.Settings;
            DefaultPage.SetActive(false);
            SettingsPage.SetActive(true);
            setContainerText(AmmoPouch.container);

            switch (AmmoPouch.container)
            {
                case AmmoPouch.ContainerType.Magazine:
                case AmmoPouch.ContainerType.Clip:
                case AmmoPouch.ContainerType.SpeedLoader:
                case AmmoPouch.ContainerType.None:
                    SortingTab.SetActive(true);
                    ProxyTab.SetActive(false);
                    setSortingButton(AmmoPouch.sortMethod);
                    break;
                case AmmoPouch.ContainerType.Round:
                    SortingTab.SetActive(false);
                    ProxyTab.SetActive(true);
                    updateProxyText();
                    break;
            }

            ReturnBtn.SetActive(true);
        }

        public void updateDisplay()
        {
            switch (currentPage)
            {
                case UIPage.Default:
                    SetDefaultUI();
                    break;
                case UIPage.Settings:
                    SetSettingsUI();
                    break;
            }
            setCapacityTxt();
            setContainerText(AmmoPouch.container);
        }

        // Use this for initialization
        public void Start()
        {
            SetDefaultUI();
        }

        public void setLockIDColor()
        {
            IDLockOnBtn.SetActive(AmmoPouch.lockID);
            IDLockOffBtn.SetActive(!AmmoPouch.lockID);
        }

        public void setCapacityTxt()
        {
            if (AmmoPouch.sortMethod != AmmoPouch.PouchSorting.Retention)
            {
                CapacityTxt.text = AmmoPouch.currentCapacity + " / " + AmmoPouch.totalCapacity;
            }
            else
            {
                CapacityTxt.text = AmmoPouch.fullMags() + " (" + AmmoPouch.currentCapacity + ") / " + AmmoPouch.totalCapacity;
            }
        }

        public void setSortingButton(AmmoPouch.PouchSorting sort)
        {
            if (sort == AmmoPouch.PouchSorting.LIFO)
            {
                SortingSet.SetSelectedButton(LIFO);
            }
            else if (sort == AmmoPouch.PouchSorting.Retention)
            {
                SortingSet.SetSelectedButton(RETENTION);
            }
            else if (sort == AmmoPouch.PouchSorting.Fullest)
            {
                SortingSet.SetSelectedButton(FULLEST);
            }
            else
            {
                SortingSet.SetSelectedButton(EMPTIEST);
            }
        }

        public void sortLIFO()
        {
            SortingSet.SetSelectedButton(LIFO);
            AmmoPouch.sortLIFO();
        }

        public void sortRetention()
        {
            SortingSet.SetSelectedButton(RETENTION);
            AmmoPouch.sortRetention();
        }

        public void sortFullest()
        {
            SortingSet.SetSelectedButton(FULLEST);
            AmmoPouch.sortFullestFirst();
        }

        public void sortEmptiest()
        {
            SortingSet.SetSelectedButton(EMPTIEST);
            AmmoPouch.sortEmptiestFirst();
        }

        public void lockIDToggle()
        {
            AmmoPouch.lockID = !AmmoPouch.lockID;
            setLockIDColor();
        }

        public void addProxy()
        {
            AmmoPouch.attemptAddProxy();
            updateProxyText();
        }

        public void subProxy()
        {
            AmmoPouch.attemptSubProxy();
            updateProxyText();
        }

        public void updateProxyText()
        {
            ProxyCount.text = "" + AmmoPouch.proxyCount;
        }

        public void setContainerText(AmmoPouch.ContainerType type)
        {
            switch (type)
            {
                case AmmoPouch.ContainerType.Magazine:
                    ContainerTypeTxt.text = "Magazines";
                    SettingsContainerTxt.text = "Container: \nMagazine";
                    break;
                case AmmoPouch.ContainerType.SpeedLoader:
                    ContainerTypeTxt.text = "Speed loaders";
                    SettingsContainerTxt.text = "Container: \nSpeedloader";
                    break;
                case AmmoPouch.ContainerType.Clip:
                    ContainerTypeTxt.text = "Clips";
                    SettingsContainerTxt.text = "Container: \nClip";
                    break;
                case AmmoPouch.ContainerType.Round:
                    ContainerTypeTxt.text = "Rounds";
                    SettingsContainerTxt.text = "Container: \nRound";
                    break;
                default:
                    ContainerTypeTxt.text = "Empty";
                    SettingsContainerTxt.text = "Container: \nEmpty";
                    break;
            }
        }

    }

}