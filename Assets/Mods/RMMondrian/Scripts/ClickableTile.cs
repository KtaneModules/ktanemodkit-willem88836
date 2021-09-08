using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Mods.RMMondrian.Scripts
{
    [RequireComponent(typeof(KMSelectable))]
    public class ClickableTile : MonoBehaviour
    {
        private void Awake()
        {
            KMSelectable selectable = GetComponent<KMSelectable>();
            Array.Resize(ref selectable.Parent.Children, selectable.Parent.Children.Length + 1);
            selectable.Parent.Children[selectable.Parent.Children.Length - 1] = selectable;
        }

        public void Click()
        {
            Debug.Log("CLIKING");
        }


    }
}
