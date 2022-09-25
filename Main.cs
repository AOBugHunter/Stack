using System;
using System.Collections.Generic;
using System.Linq;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Core.Inventory;
using AOSharp.Common.GameData;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.GameData;

namespace Stack
{
    public class Main : AOPluginEntry
    {
        public static bool SwitchBool = false;
        public static List<Item> characterItems = Inventory.Items;
        public static Container stackBag = Inventory.Backpacks.FirstOrDefault(x => x.Name == "stack");
        public static bool StackSwitch = false;
        public static bool SplitStack;
        Item stackitem;
        int stackingslot;
        public override void Run(string pluginDir)
        {
            try
            {
                Chat.WriteLine("Stack loaded");

                Chat.WriteLine("Name an empty bag 'stack', eject if named bag while injected.");
                Chat.WriteLine("Then put item into it and type /setitem then /stack to toggle on and off.");
                Chat.WriteLine(" ");
                Chat.WriteLine("Splitting stacks of stims/kits in bank - type /splitstack to toggle on and off");

                Game.OnUpdate += OnUpdate;
                Network.N3MessageReceived += Network_N3MessageReceived;

                Chat.RegisterCommand("stack", StackToggle);
                Chat.RegisterCommand("setitem", SetItem);
                Chat.RegisterCommand("splitstack", (string command, string[] param, ChatWindow chatWindow) =>
                {
                    SplitStack = !SplitStack;
                    Chat.WriteLine($"Splitting : {SplitStack}");
                });
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        private void Network_N3MessageReceived(object s, N3Message n3Msg)
        {
            if (SplitStack)
            {
                if (n3Msg.N3MessageType == N3MessageType.Bank)
                {
                    BankMessage bankMsg = (BankMessage)n3Msg;

                    BankSlot Stim = bankMsg.BankSlots.FirstOrDefault(x =>
                    x.ItemHighId == 204103 || x.ItemHighId == 204104 || x.ItemHighId == 204105 ||
                    x.ItemHighId == 204106 || x.ItemHighId == 204107 || x.ItemHighId == 291082 ||
                    x.ItemHighId == 291083 || x.ItemHighId == 291084 || x.ItemHighId == 293296 ||
                    x.ItemHighId == 291043 || x.ItemHighId == 291044 || x.ItemHighId == 291045);

                    for (int i = 0; i <= 60; i++)
                    {
                        Network.Send(new CharacterActionMessage()
                        {
                            Action = CharacterActionType.SplitItem,
                            Target = new Identity(IdentityType.BankByRef, Stim.ItemFlags),
                            Parameter2 = 1
                        });
                    }
                }
            }
        }

        private void StackToggle(string command, string[] param, ChatWindow chatWindow)
        {
            StackSwitch = !StackSwitch;
            Chat.WriteLine(string.Format("Toggled Stack to ", StackSwitch));
        }

        private void SetItem(string command, string[] param, ChatWindow chatWindow)
        {
            if (stackBag != null)
            {
                if (stackBag.Items.FirstOrDefault() != null)
                {
                    stackitem = stackBag.Items.FirstOrDefault();
                }
                else
                {
                    Chat.WriteLine("Set");
                }

                stackingslot = (int)stackitem.EquipSlots.FirstOrDefault();
            }
            else
            {
                Chat.WriteLine("No bag named 'stack'");
            }
        }

        private static void StripItem(Identity bank, Container stackBag)
        {
            Network.Send(new ClientContainerAddItem()
            {
                Target = stackBag.Identity,
                Source = bank
            });
        }

        private static void EquipItem(Container stackBag, EquipSlot slotToStack)
        {
            foreach (Item item in stackBag.Items)
            {
                item.Equip(slotToStack);
                break;
            }
        }

        private void OnUpdate(object s, float deltaTime)
        {
            if (stackBag != null && StackSwitch)
            {
                for (int i = 1; i <= 20; i++)
                {
                    Identity stackBagId = stackBag.Identity;
                    Identity bank = new Identity();
                    bank.Type = IdentityType.BankByRef;
                    bank.Instance = (int)stackingslot;

                    EquipItem(stackBag, stackitem.EquipSlots.First());
                    StripItem(bank, stackBag);
                }
            }
        }

        public override void Teardown()
        {
        }
    }
}
