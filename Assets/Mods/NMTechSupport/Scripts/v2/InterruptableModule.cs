using UnityEngine;

public sealed class InterruptableModule
{
    public KMBombModule BombModule;
    public KMSelectable Selectable;
    public GameObject PassLight;
    public GameObject StrikeLight;
    public GameObject ErrorLight;
    public GameObject OffLight;

    public bool IsFocussed { get; private set; }

    public InterruptableModule(
        KMBombModule bombModule,
        KMSelectable selectable,
        GameObject passLight,
        GameObject strikeLight,
        GameObject errorLight,
        GameObject offLight)
    {
        BombModule = bombModule;
        Selectable = selectable;
        PassLight = passLight;
        StrikeLight = strikeLight;
        ErrorLight = errorLight;
        OffLight = offLight;

        Selectable.OnFocus += delegate { IsFocussed = true; };
        Selectable.OnDefocus += delegate { IsFocussed = false; };
    }

    public override string ToString()
    {
        return this.BombModule.ModuleDisplayName;
    }
}
