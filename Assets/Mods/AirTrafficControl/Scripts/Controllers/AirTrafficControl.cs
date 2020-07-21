using System.Collections;
using UnityEngine;

namespace WillemMeijer.NMAirTrafficControl
{
    [RequireComponent(typeof(KMNeedyModule))]
    public class AirTrafficControl : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private bool oneShot = false;
        [SerializeField] private bool instaStart = false;
#endif

        [Space, SerializeField] private MessageField messageField;
        [SerializeField] private SelectionMenu selectionMenu;
        [SerializeField] private InteractableButton okButton;
        [SerializeField] private InteractableButton upButton;
        [SerializeField] private InteractableButton downButton;
        [SerializeField] private LandingLane[] lanes;

        [Space, SerializeField] private int eventIntervalMin;
        [SerializeField] private int eventIntervalMax;
        [SerializeField, Multiline] private string incomingPlaneMessage;
        [SerializeField] private int startingDelay;
        [SerializeField, Multiline] private string notYetStartedMessage;
        [SerializeField, Multiline] private string startedMessage;

        private Lock screenLock;
        private Lock interactionLock; 
        private KMNeedyModule needyModule;
        private int lastIncomingPlaneLane;
        private int current;


        private void Start()
        {
            needyModule = GetComponent<KMNeedyModule>();
            screenLock = new Lock();
            interactionLock = new Lock();
            StartCoroutine(DelayedModuleActivation());
        }

        private IEnumerator DelayedModuleActivation()
        {
            // Everything is initialized.
            okButton.AddListener(OnOkClicked);
            upButton.AddListener(OnUpClicked);
            downButton.AddListener(OnDownClicked);

            // interactionLock is initially claimed to prevent everything from interacting.
            interactionLock.Claim(this);
            messageField.Initialize(interactionLock, screenLock, okButton);
            selectionMenu.Initialize(this, interactionLock, screenLock, okButton, upButton, downButton);

            for (int i = 0; i < lanes.Length; i++)
            {
                lanes[i].Intialize(i, lanes, this);
            }



            // Disables needymodule timer that automatically spawns. 
            // The Object only appears after the first frame.
            yield return new WaitForEndOfFrame();
            int c = transform.childCount;
            for (int i = 0; i < c; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.gameObject.name == "NeedyTimer(Clone)")
                {
                    child.gameObject.SetActive(false);
                }
            }

            // Shows not yet started message every 1 seconds. 
            int t = startingDelay;

#if UNITY_EDITOR
            if (instaStart)
            {
                t = 2;
            }
#endif

            while (t > 0)
            {
                string message = string.Format(notYetStartedMessage, t);
                messageField.ShowMessage(message);
                t--;
                yield return new WaitForSeconds(1);
            }

            // The delayed initialization.
            messageField.ShowMessage(startedMessage);
            lanes[current].Select();
            StartCoroutine(EventInvoker());
            interactionLock.Unclaim(this);
        }


        private IEnumerator EventInvoker()
        {
            while (true)
            {
                int d = Random.Range(eventIntervalMin, eventIntervalMax);
#if UNITY_EDITOR
                if (oneShot)
                {
                    yield return new WaitForSeconds(1);
                }
                else
                {
#endif
                    yield return new WaitForSeconds(d);
#if UNITY_EDITOR
                }
#endif

                PlaneData incoming = AirTrafficControlData.GeneratePlane();

                int laneIndex = lastIncomingPlaneLane;
                while (laneIndex == lastIncomingPlaneLane
                    || lanes[laneIndex].IsWrecked)
                {
                    laneIndex = Random.Range(0, lanes.Length);
                }
                lastIncomingPlaneLane = laneIndex;
                
#if UNITY_EDITOR
                if (oneShot)
                {
                    laneIndex = 0;
                }
#endif

                LandingLane lane = lanes[laneIndex];

                lane.Incoming(incoming);

                string message = string.Format(
                    incomingPlaneMessage, 
                    incoming.Serial, 
                    incoming.Origin, 
                    incoming.PassengerCount, 
                    incoming.LuggageCount, 
                    laneIndex + 1); // incremented to match the lane's visual numbers.

                selectionMenu.Disable();
                messageField.ShowMessage(message);

#if UNITY_EDITOR
                if (oneShot)
                {
                    break;
                }
#endif
            }
        }


        public void OnPlaneCrash()
        {
            needyModule.HandleStrike();
        }


        public void OnSelect(int index)
        {
            if (interactionLock.Available)
            {
                LandingLane lane = lanes[current];
                lane.SetNext(index);
            }
        }


        private void OnOkClicked()
        {
            if(!interactionLock.Available
                || !screenLock.Available)
            {
                return;
            }

            LandingLane lane = lanes[current];

            if(!lane.ContainsPlane)
            {
                return;
            }

            switch (lane.State)
            {
                case 0:
                    selectionMenu.Show(AirTrafficControlData.Hangars);
                    break;
                case 1:
                    selectionMenu.Show(AirTrafficControlData.ShuttleSerials);
                    break;
                case 2:
                    selectionMenu.Show(AirTrafficControlData.LuggageSerials);
                    break;
            }
        }

        private void OnUpClicked()
        {
            if (!interactionLock.Available
                || !screenLock.Available)
            {
                return;
            }

            int next = current - 1;
            if(next < 0)
            {
                next = lanes.Length + next;
            }

            lanes[current].Deselect();
            lanes[next].Select();
            current = next;
        }

        private void OnDownClicked()
        {
            if (!interactionLock.Available
                || !screenLock.Available)
            {
                return;
            }

            int next = (current + 1) % lanes.Length;

            lanes[current].Deselect();
            lanes[next].Select();
            current = next;
        }
    }
}
