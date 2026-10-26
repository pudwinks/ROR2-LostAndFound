using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace LostAndFound
{
    // Something the recap can browse to. Each subclass wraps one kind of game component.
    public abstract class Interactable
    {
        public abstract Vector3 GetPosition();

        // False once the component is destroyed or can no longer be used.
        public abstract bool IsAvailable { get; }

        public virtual void SimulateDrop() { }

        internal void CreateDropOrb(Vector3 effectOrigin, GameObject targetObject, PickupDef pickupDef)
        {
            EffectData effectData = new EffectData
            {
                origin = effectOrigin,
                genericFloat = 1.5f,
                genericUInt = (uint)(pickupDef.pickupIndex.value + 1)
            };

            effectData.SetNetworkedObjectReference(targetObject);
            EffectManager.SpawnEffect(LostAndFoundPlugin.effectPrefab, effectData, transmit: false);
        }

        // Shared by PurchaseInteractable and HollowChestInteractable.
        protected void SimulatePurchaseDrop(PurchaseInteraction pi)
        {
            if (pi.displayNameToken.Contains("CHEST") || pi.displayNameToken.Contains("EQUIPMENTBARREL") || pi.displayNameToken == "LOCKBOX_NAME")
            {
                ChestBehavior component = pi.GetComponent<ChestBehavior>();
                if (component)
                {
                    CreateDropOrb(pi.gameObject.transform.position + new Vector3(0, 3.5f, 0), pi.gameObject, PickupCatalog.GetPickupDef(component.currentPickup.pickupIndex));
                }
            }
            else if (pi.displayNameToken.Contains("CHANCE"))
            {
                ShrineChanceBehavior component = pi.GetComponent<ShrineChanceBehavior>();
                if (component)
                {
                    var anglePerOrb = 360f / component.maxPurchaseCount;
                    var posOffset = Quaternion.AngleAxis(90, Vector3.up) * pi.gameObject.transform.forward * 2; //facing the shrine, the one on the left should be first
                    int succcs = component.successfulPurchaseCount;
                    while (succcs < component.maxPurchaseCount)
                    {
                        if (component.rng.nextNormalizedFloat > component.failureChance)
                        {
                            var angle = anglePerOrb * succcs;
                            CreateDropOrb(pi.gameObject.transform.position + new Vector3(0, 3.5f, 0) + (Quaternion.AngleAxis(angle, Vector3.up) * posOffset), pi.gameObject, PickupCatalog.GetPickupDef(component.dropTable.GeneratePickup(component.rng).pickupIndex));
                            succcs++;
                        }
                    }
                }
            }
            else if (pi.displayNameToken == "VOID_TRIPLE_NAME" || pi.displayNameToken == "VOIDLOCKBOX_NAME" && !LostAndFoundPlugin.showVoidKeyContents.Value)
            {
                OptionChestBehavior component = pi.GetComponent<OptionChestBehavior>();
                if (component)
                {
                    var anglePerOrb = 360f / component.generatedPickups.Count;
                    var posOffset = new Vector3(0, 0, 1f + (.16f * component.generatedPickups.Count));

                    for (int i = 0; i < component.generatedPickups.Count; i++)
                    {
                        var angle = anglePerOrb * i;
                        CreateDropOrb(pi.gameObject.transform.position + new Vector3(0, 3.5f, 0) + (Quaternion.AngleAxis(angle, Vector3.up) * posOffset), pi.gameObject, PickupCatalog.GetPickupDef(component.generatedPickups[i].pickupIndex));
                    }
                }
            }
            else if (pi.displayNameToken == "SCAVBACKPACK_NAME")
            {
                ChestBehavior component = pi.GetComponent<ChestBehavior>();
                if (component)
                {
                    float anglePerOrb = 360f / 10;
                    var posOffset = new Vector3(0, 0, 4f);
                    for (int i = 0; i < 10; i++)
                    {
                        var angle = anglePerOrb * i;
                        component.RollItem(); //scav drops just roll twice, don't ask why
                        CreateDropOrb(pi.gameObject.transform.position + new Vector3(0, 6.5f, 0) + (Quaternion.AngleAxis(angle, Vector3.up) * posOffset), pi.gameObject, PickupCatalog.GetPickupDef(component.currentPickup.pickupIndex));
                        for (int j = 0; j < component.dropCount; j++)
                        {
                            component.Roll();
                        }
                    }
                }
            }
        }
    }

    public class PurchaseInteractable : Interactable
    {
        public readonly PurchaseInteraction pi;

        public PurchaseInteractable(PurchaseInteraction purchase)
        {
            this.pi = purchase;
        }

        public override Vector3 GetPosition()
        {
            return pi.transform.position;
        }

        public override bool IsAvailable => pi && pi.available;

        public override void SimulateDrop()
        {
            SimulatePurchaseDrop(pi);
        }
    }

    // Printers stay available after printing, so they leave the recap once used instead.
    public class Printer3DInteractable : Interactable
    {
        // Display name tokens of white/green, red and boss printers.
        public static readonly HashSet<string> printerNames = new HashSet<string>
        {
            "DUPLICATOR_NAME",
            "DUPLICATOR_MILITARY_NAME",
            "DUPLICATOR_WILD_NAME",

            // Quality mod
            "DUPLICATOR_QUALITY_NAME",
            "DUPLICATOR_MILITARY_QUALITY_NAME",
            "DUPLICATOR_WILD_QUALITY_NAME",
        };

        public readonly PurchaseInteraction pi;

        public bool usedPrinter;

        public Printer3DInteractable(PurchaseInteraction purchase)
        {
            this.pi = purchase;
        }

        public override Vector3 GetPosition()
        {
            return pi.transform.position;
        }

        public override bool IsAvailable => pi && !usedPrinter;
    }

    // Hallowed Concepts' Concept Reliquary turns unavailable when the teleporter starts,
    // so it leaves the recap once purchased instead.
    public class HollowChestInteractable : Interactable
    {
        public const string hollowChestName = "HOLLOWCHEST_NAME";

        public readonly PurchaseInteraction pi;

        public bool purchased;

        public HollowChestInteractable(PurchaseInteraction purchase)
        {
            this.pi = purchase;
        }

        public override Vector3 GetPosition()
        {
            return pi.transform.position;
        }

        public override bool IsAvailable => pi && !purchased;

        public override void SimulateDrop()
        {
            SimulatePurchaseDrop(pi);
        }
    }

    public class MultiShopInteractable : Interactable
    {
        public readonly MultiShopController msc;

        public MultiShopInteractable(MultiShopController multiShop)
        {
            this.msc = multiShop;
        }

        public override Vector3 GetPosition()
        {
            return msc.transform.position;
        }

        public override bool IsAvailable => msc && msc.available;

        public override void SimulateDrop()
        {
            foreach (var terminal in msc.terminalGameObjects)
            {
                ShopTerminalBehavior component = terminal.GetComponent<ShopTerminalBehavior>();
                if (component)
                {
                    CreateDropOrb(terminal.transform.position + new Vector3(0, 5f, 0), terminal, PickupCatalog.GetPickupDef(component.Networkpickup.pickupIndex));
                }
            }
        }
    }

    public class DroneMultiShopInteractable : Interactable
    {
        public readonly DroneVendorMultiShopController dvmsc;

        public DroneMultiShopInteractable(DroneVendorMultiShopController droneMultiShop)
        {
            this.dvmsc = droneMultiShop;
        }

        public override Vector3 GetPosition()
        {
            return dvmsc.transform.position;
        }

        public override bool IsAvailable => dvmsc && dvmsc.available;
    }

    public class BarrelInteractable : Interactable
    {
        public readonly BarrelInteraction bi;

        public BarrelInteractable(BarrelInteraction barrel)
        {
            this.bi = barrel;
        }

        public override Vector3 GetPosition()
        {
            return bi.transform.position;
        }

        public override bool IsAvailable => bi && !bi.opened;
    }

    // Every scrapper leaves the recap once any of them has been used this stage.
    public class ScrapperInteractable : Interactable
    {
        public readonly ScrapperController sc;

        public ScrapperInteractable(ScrapperController scrapper)
        {
            this.sc = scrapper;
        }

        public override Vector3 GetPosition()
        {
            return sc.transform.position;
        }

        public override bool IsAvailable => sc && sc.gameObject.activeInHierarchy && !LostAndFoundPlugin.didScrap;
    }

    // An item left on the ground.
    public class GenericPickupControllerInteractable : Interactable
    {
        public readonly GenericPickupController gpc;

        public GenericPickupControllerInteractable(GenericPickupController pickup)
        {
            this.gpc = pickup;
        }

        public override Vector3 GetPosition()
        {
            return gpc.transform.position;
        }

        public override bool IsAvailable => gpc && !gpc.consumed;
    }

    public class NtormatPageInteractable : GenericPickupControllerInteractable
    {
        public NtormatPageInteractable(GenericPickupController pickup) : base(pickup) { }

        public override void SimulateDrop()
        {
            PickupDef pickupDef = PickupCatalog.GetPickupDef(gpc.pickup.pickupIndex);
            if (pickupDef != null)
            {
                CreateDropOrb(gpc.transform.position + new Vector3(0, 3.5f, 0), gpc.gameObject, pickupDef);
            }
        }
    }

    // A void potential or halcyonite fragment pickup on the ground. It's destroyed once picked.
    public class PotentialInteractable : Interactable
    {
        public readonly PickupPickerController ppc;

        public PotentialInteractable(PickupPickerController picker)
        {
            this.ppc = picker;
        }

        public override Vector3 GetPosition()
        {
            return ppc.transform.position;
        }

        public override bool IsAvailable => ppc;

        public override void SimulateDrop()
        {
            var anglePerOrb = 360f / ppc.options.Length;
            var posOffset = new Vector3(0, 0, 1f + (.16f * ppc.options.Length)); //expand radius as options added

            for (int i = 0; i < ppc.options.Length; i++)
            {
                if (ppc.options[i].available) //leaves a hole for picked halc fragment loot options
                {
                    var angle = anglePerOrb * i;
                    CreateDropOrb(ppc.gameObject.transform.position + new Vector3(0, 3.5f, 0) + (Quaternion.AngleAxis(angle, Vector3.up) * posOffset), ppc.gameObject, PickupCatalog.GetPickupDef(ppc.options[i].pickup.pickupIndex));
                }
            }
        }
    }
}
