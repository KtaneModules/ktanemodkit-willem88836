using System.Collections;
using UnityEngine;

namespace WillemMeijer.NMAirTrafficControl
{
    [RequireComponent(typeof(KMNeedyModule))]
    [RequireComponent(typeof(KMBombInfo))]
    public class AirTrafficControl : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("Debugging")]
        [SerializeField] private bool oneShot = false;
        [SerializeField] private bool instantStart = false;
        [SerializeField] private bool earlyEnd = false;
        [SerializeField] private bool skipSpawnDelay = false;
        [SerializeField] private bool skipEndMessage = false;
        [SerializeField] private bool forceLane0 = false;
        [SerializeField] private bool onlyAllowLane0 = false;
        [SerializeField] private bool ignoreCloseIntervalModifier = false;
#endif

        [Header("Module Settings")]
        [SerializeField] private MessageField messageField;
        [SerializeField] private SelectionMenu selectionMenu;
        [SerializeField] private InteractableButton okButton;
        [SerializeField] private InteractableButton upButton;
        [SerializeField] private InteractableButton downButton;
        [SerializeField] private LandingLane[] lanes;

        [Space, SerializeField] private int eventIntervalMin;
        [SerializeField] private int eventIntervalMax;
        [SerializeField, Multiline] private string incomingPlaneMessage;
        [SerializeField] private int startingDelay;
        [SerializeField] private float deactivationAlpha;
        [SerializeField, Multiline] private string notYetStartedMessage;
        [SerializeField, Multiline] private string startedMessage;
        [SerializeField] private int closeMessageDuration;
        [SerializeField, Multiline] private string closedMessage;
        [SerializeField] private float afterCloseEventIntervalModifier;

        private Lock screenLock;
        private Lock interactionLock; 
        private KMNeedyModule needyModule;
        private KMBombInfo bombInfo;
        private float bombTime;
        private bool isDeactivated;
        private int lastIncomingPlaneLane;
        private int current;


        private void Start()
        {
            needyModule = GetComponent<KMNeedyModule>();
            bombInfo = GetComponent<KMBombInfo>();
            screenLock = new Lock();
            interactionLock = new Lock();
            bombTime = bombInfo.GetTime();
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



            //// Disables needymodule timer that automatically spawns. 
            //// The Object only appears after the first frame.
            //yield return new WaitForEndOfFrame();
            //int c = transform.childCount;
            //for (int i = 0; i < c; i++)
            //{
            //    Transform child = transform.GetChild(i);
            //    if (child.gameObject.name == "NeedyTimer(Clone)")
            //    {
            //        child.gameObject.SetActive(false);
            //    }
            //}

            // Shows not yet started message every 1 seconds. 
            int t = startingDelay;

#if UNITY_EDITOR
            if (instantStart)
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
                if (skipSpawnDelay)
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

                // Selects the landing lane. this cannot be the previous one.
                int laneIndex = lastIncomingPlaneLane;
                while (laneIndex == lastIncomingPlaneLane
                    || lanes[laneIndex].IsWrecked)
                {
                    laneIndex = Random.Range(0, lanes.Length);
                }
                lastIncomingPlaneLane = laneIndex;
                
#if UNITY_EDITOR
                if (forceLane0)
                {
                    laneIndex = 0;
                }
                if (onlyAllowLane0 && laneIndex != 0)
                {
                    continue;
                }
#endif

                // generates incoming plane.
                PlaneData incoming = AirTrafficControlData.GeneratePlane();

                // Notifies all the elements involved. 
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
                if (!isDeactivated)
                {
                    messageField.ShowMessage(message);
                }

                // if the timer reaches the lowerbound, 
                // this module is deactivated. 
                if (!isDeactivated && (bombInfo.GetTime()
                    < bombTime * deactivationAlpha
#if UNITY_EDITOR
                    || earlyEnd
#endif
                    ))
                {
                    isDeactivated = true;

                    foreach(LandingLane l in lanes)
                    {
                        l.ModuleEnded();
                    }

                    lanes[current].Deselect();
                    interactionLock.Claim(this);
                    messageField.ShowMessage(closedMessage);
#if UNITY_EDITOR
                    if (!skipEndMessage)
                    {
#endif
                        yield return new WaitForSeconds(closeMessageDuration);
#if UNITY_EDITOR
                    }
#endif
                    messageField.Close();

#if UNITY_EDITOR
                    if (!ignoreCloseIntervalModifier)
                    {
#endif
                        eventIntervalMin = (int)(eventIntervalMin * afterCloseEventIntervalModifier);
                        eventIntervalMax = (int)(eventIntervalMax * afterCloseEventIntervalModifier);
#if UNITY_EDITOR
                    }
#endif
                }    

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
            if (interactionLock.Available)
            {
                needyModule.HandleStrike();
            }
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
