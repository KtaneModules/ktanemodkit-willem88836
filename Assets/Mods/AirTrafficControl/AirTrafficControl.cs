using UnityEngine;

namespace WillemMeijer.NMAirTrafficControl
{
    public class AirTrafficControl : MonoBehaviour
    {
        void Awake()
        {
            GetComponent<KMNeedyModule>().OnNeedyActivation += OnNeedyActivation;
            GetComponent<KMNeedyModule>().OnNeedyDeactivation += OnNeedyDeactivation;
            GetComponent<KMNeedyModule>().OnTimerExpired += OnTimerExpired;
        }

        protected bool Solve()
        {
            GetComponent<KMNeedyModule>().OnPass();

            return false;
        }

        protected void OnNeedyActivation()
        {
            Debug.Log("Needy activated");
        }

        protected void OnNeedyDeactivation()
        {
            Debug.Log("Needy deactivated");
        }

        protected void OnTimerExpired()
        {
            Debug.Log("Timer expired");
            GetComponent<KMNeedyModule>().OnStrike();
        }

        protected bool AddTime()
        {
            float time = GetComponent<KMNeedyModule>().GetNeedyTimeRemaining();
            if (time > 0)
            {
                GetComponent<KMNeedyModule>().SetNeedyTimeRemaining(time + 5f);
            }

            return false;
        }
    }
}
