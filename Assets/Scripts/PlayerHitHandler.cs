using UnityEngine;

public class PlayerHitHandler : MonoBehaviour, IPlayerHitReceiver
{
    public OVRController LHand;
    public OVRController RHand;

    public void OnHitByBoulder()
    {
        LHand.ReduceStamina(0.5f);
        RHand.ReduceStamina(0.5f);

    }
}
