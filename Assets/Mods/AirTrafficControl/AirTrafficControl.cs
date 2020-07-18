using System.Collections;
using UnityEngine;

namespace WillemMeijer.NMAirTrafficControl
{
    public class AirTrafficControl : MonoBehaviour
    {
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
        [SerializeField, Multiline] private string notYetStartedMessage;
        [SerializeField, Multiline] private string startedMessage;

        private int lastIncomingPlaneLane;
        private int current;


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
                messageField.ShowMessage(message);
                t--;
                yield return new WaitForSeconds(1);
            }

            messageField.ShowMessage(startedMessage);
            okButton.AddListener(OnOkClicked);
            upButton.AddListener(OnUpClicked);
            downButton.AddListener(OnDownClicked);

            lanes[current].Select();

            messageField.Initialize(okButton);
            selectionMenu.Initialize(this, okButton, upButton, downButton);

            StartCoroutine(EventInvoker());
        }


        private IEnumerator EventInvoker()
        {
            while (true)
            {
                int d = UnityEngine.Random.Range(eventIntervalMin, eventIntervalMax);
                yield return new WaitForSeconds(d);

                PlaneData incoming = AirTrafficControlData.GeneratePlane();

                int laneIndex = lastIncomingPlaneLane;
                while (laneIndex == lastIncomingPlaneLane)
                {
                    laneIndex = UnityEngine.Random.Range(0, lanes.Length);
                }
                lastIncomingPlaneLane = laneIndex;

                LandingLane lane = lanes[laneIndex];
                lane.Incoming(incoming);

                string message = string.Format(
                    incomingPlaneMessage, 
                    incoming.Serial, 
                    incoming.Origin, 
                    incoming.PassengerCount, 
                    incoming.LuggageCount, 
                    laneIndex);

                messageField.ShowMessage(message);
            }
        }


        public void OnSelect(int index)
        {
            LandingLane lane = lanes[current];
            lane.SetNext(index);
        }


        private void OnOkClicked()
        {
            if(messageField.IsDisplaying || selectionMenu.IsShowing)
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
            if (messageField.IsDisplaying || selectionMenu.IsShowing)
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
            if (messageField.IsDisplaying || selectionMenu.IsShowing)
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
