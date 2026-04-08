using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TruckFruitReceiver : MonoBehaviour
{
    [SerializeField]
    private string successMessage = "Sawit masuk ke bak!";
    [SerializeField]
    private bool showPopupOnDelivery = true;

    [SerializeField]
    private int deliveredCount;

    public int DeliveredCount => deliveredCount;

    void Reset()
    {
        BoxCollider trigger = GetComponent<BoxCollider>();
        trigger.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        PalmFruitCargo cargo = ResolveCargo(other);
        if (cargo == null || !cargo.CanBeDelivered())
            return;

        deliveredCount++;
        cargo.Deliver();

        if (showPopupOnDelivery)
            FruitPickupPopup.ShowMessage(successMessage);
    }

    private PalmFruitCargo ResolveCargo(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            PalmFruitCargo cargoFromBody = other.attachedRigidbody.GetComponent<PalmFruitCargo>();
            if (cargoFromBody != null)
                return cargoFromBody;
        }

        return other.GetComponentInParent<PalmFruitCargo>();
    }
}
