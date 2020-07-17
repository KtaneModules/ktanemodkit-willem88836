using System.Collections;
using UnityEngine;

namespace WillemMeijer.NMAirTrafficControl
{
    public class AirTrafficControl : MonoBehaviour
    {
        public static readonly string[] PlaneSerials = new string[] { "GN3-63", "WR-857", "BS-788", "DE4-86", "SG-325", "LR2-67", "OK-248", "ET7-31", "KP-820", "AL0-34", "WK4-11", "UQ-633", "SA-567", "CR-653", "ES-607", "WK2-60", "WA-664", "LR1-44", "NG2-82", "JY-286", "RH-611", "AX1-30", "QC-270", "CG-765", "BJ-832" };
        public static readonly string[] ShuttleSerials = new string[] { "DG-780", "DO5-62", "GC-120", "GN-726", "LF-705", "MQ3-17", "JV6-32", "CQ-306", "HK4-82" };
        public static readonly string[] LuggageSerials = new string[] { "SQ2-16", "CQ3-01", "VG5-44", "LF2-01", "CS-612", "FK2-81", "BF-453", "RP3-48", "HO0-46" };
        public static readonly string[] Hangars = new string[] { "Hangar 1", "Hangar 2", "Hangar 3", "Hangar 4", "Hangar 5", "Hangar 6" };


        [SerializeField] private MessageField messageField;
        [SerializeField] private SelectionMenu selectionMenu;
        [SerializeField] private InteractableButton okButton;
        [SerializeField] private InteractableButton upButton;
        [SerializeField] private InteractableButton downButton;
        [SerializeField] private LandingLane[] landingLanes;

        [Space, SerializeField] private int startingDelay;
        [SerializeField] private string notYetStartedMessage;
        [SerializeField] private string startedMessage;

        private int currentLane;


        private void Start()
        {
            StartCoroutine(DelayedModuleActivation());
        }

        private IEnumerator DelayedModuleActivation()
        {
            yield return new WaitForEndOfFrame();

            // Disables needymodule timer that automatically spawns. 
            int c = transform.childCount;
            for (int i = 0; i < c; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.gameObject.name == "NeedyTimer(Clone)")
                {
                    child.gameObject.SetActive(false);
                }
            }


            int t = startingDelay;
            while (t > 0)
            {
                string message = string.Format(notYetStartedMessage, t);
                messageField.Set(message);
                t--;
                yield return new WaitForSeconds(1);
            }

            messageField.Set(startedMessage);
            okButton.AddListener(OnOkClicked);
            upButton.AddListener(OnUpClicked);
            downButton.AddListener(OnDownClicked);

            landingLanes[currentLane].Select();

            messageField.Initialize(okButton);
            selectionMenu.Initialize(this, okButton, upButton, downButton);
        }


        public void OnSelect(int index)
        {
            LandingLane lane = landingLanes[currentLane];
            lane.SetNext(index);
        }


        private void OnOkClicked()
        {
            if(messageField.IsDisplaying || selectionMenu.IsShowing)
            {
                return;
            }

            LandingLane lane = landingLanes[currentLane];

            if(!lane.ContainsPlane)
            {
                return;
            }

            switch (lane.State)
            {
                case 0:
                    selectionMenu.Show(Hangars);
                    break;
                case 1:
                    selectionMenu.Show(ShuttleSerials);
                    break;
                case 2:
                    selectionMenu.Show(LuggageSerials);
                    break;
            }
        }

        private void OnUpClicked()
        {
            if (messageField.IsDisplaying || selectionMenu.IsShowing)
            {
                return;
            }

            landingLanes[currentLane].Deselect();
            currentLane--;
            if(currentLane < 0)
            {
                currentLane += landingLanes.Length;
            }
            landingLanes[currentLane].Select();
        }

        private void OnDownClicked()
        {
            if (messageField.IsDisplaying || selectionMenu.IsShowing)
            {
                return;
            }

            landingLanes[currentLane].Deselect();
            currentLane = (currentLane + 1) % landingLanes.Length;
            landingLanes[currentLane].Select();
        }
    }
}
