using PlantAnythingAPI.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace PlantAnythingAPI.handler
{
    /*public class ItemProps_Plantable() //maybe come back and make this work with proper json deserialization
    {
        public string? plantOn;
        public string? plantAs;

        public ItemProps_Plantable Clone()
        {
            return new ItemProps_Plantable
            {
                plantOn = plantOn,
                plantAs = plantAs
            };
        }
    }*/

    internal class PAA_Handle_Plant
    {
        private static ICoreAPI API = PAA_VarRef.API;
        private static ICoreClientAPI API_Client = PAA_VarRef.API_Client;
        private static ICoreServerAPI API_Server = PAA_VarRef.API_Server;

        //public ItemProps_Plantable itemProps = { get; protected set; }


        public class Behavior_Plantable : CollectibleBehavior
        {
            //public ItemProps_Plantable itemProps { get; protected set; }
            public Behavior_Plantable(CollectibleObject collObj) : base(collObj) { }
            private string[] plantOn = [];
            private Block? plantAs;

            public override void Initialize(JsonObject properties)
            {
                //itemProps = properties.AsObject<ItemProps_Plantable>(null, collObj.Code.Domain);
                if (properties["plantAs"].Exists)
                {
                    if (properties["plantOn"].Exists)
                    {
                        List<string> plantOnList = [];
                        foreach (var i in properties["plantOn"])
                        {
                            if (i is null) { continue; }
                            plantOnList.Add(i.ToString());
                        }
                        plantOn = [.. plantOnList];
                    }

                    plantAs = API.World.GetBlock(new AssetLocation(properties["plantAs"].ToString()));
                }
                PAA_Function_General.Log_Debug_Verbose("found Behavior_Plantable on item: {0}; plantOn: list[{1}], plantAs: {2}", loggers: [collObj.Code, plantOn?.Length ?? 0, plantAs?.Code.GetNameWithDomain() ?? "null"]);
                base.Initialize(properties);
            }

            public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
            {
                if (plantAs is null || blockSel is null || slot is null) { return; }
                if (byEntity is EntityPlayer && (byEntity.Controls.Sneak || byEntity.Controls.Sprint)) { return; } //bypass interaction when crouching to allow other ineraction systems
                PAA_Function_General.Log_Debug_Verbose("ran with item: {0} on selected block: {1}", loggers: [slot?.Itemstack?.GetName() ?? "null", blockSel?.Block?.Code?.Path?.ToString() ?? "null"]);


                bool isListEmpty = (plantOn is null || plantOn.Length < 1);
                if (isListEmpty && blockSel.Block is BlockFarmland) { PAA_Plant(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling); }
                if (!isListEmpty && PAA_Match_Wildcard(plantOn, blockSel.Block))
                {
                    PAA_Plant(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
                }


                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
            }


            /// <summary>
            /// compares an array of strings of block codes to a single block's code. Strings do not require having a Code.Domain.
            /// </summary>
            /// <param name="wildcardCheck">array of strings of block codes to compare to the block.</param>
            /// <param name="blockCheck">block whos code is being compared against</param>
            /// <returns></returns>
            private bool PAA_Match_Wildcard(string[] wildcardCheck, Block blockCheck)
            {
                if (wildcardCheck is null || wildcardCheck.Length < 1 || blockCheck is null) { return false; }
                foreach (var i in wildcardCheck)
                {
                    string wildcardDomain = (i.IndexOf(':') > -1) ? i[.. (i.IndexOf(':'))] : blockCheck.Code.Domain;
                    string wildcardCode = (i.IndexOf(':') > -1) ? i[(i.IndexOf(':') + 1) ..] : i;

                    if (wildcardDomain == blockCheck.Code.Domain)
                    {
                        if (blockCheck.WildCardMatch(wildcardCode)) { return true; }
                    }
                }

                return false;
            }


            /// <summary>
            /// slightly modified vanilla .TryPlant code to not explicitly require a seed item or item cropbehavior or farmland block target
            /// </summary>
            private void PAA_Plant(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
            {
                if (plantAs is null || blockSel is null) { return; }

                BlockPos checkBlock = blockSel.Position.UpCopy();
                if (API.World.BlockAccessor.GetBlock(checkBlock) is not null && API.World.BlockAccessor.GetBlock(checkBlock).BlockMaterial == EnumBlockMaterial.Air)
                {
                    if (plantAs.CollisionBoxes is not null && plantAs.CollisionBoxes.Length != 0 && API.World.GetIntersectingEntities(checkBlock, plantAs.CollisionBoxes, (Entity e) => e.IsInteractable).Length != 0)
                    {
                        return;
                    }
                    API.World.BlockAccessor.SetBlock(plantAs.BlockId, checkBlock);
                    if (plantAs.CropProps is not null)
                    {
                        CropBehavior[] cropBehaviors = plantAs.CropProps.Behaviors;
                        for (int i = 0; i < cropBehaviors.Length; i++)
                        {
                            cropBehaviors[i].OnPlanted(API, slot, byEntity, blockSel);
                        }
                    }
                    if (byEntity is EntityPlayer)
                    {
                        IPlayer player = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
                        byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/block/plant"), checkBlock, 0.4375, player);
                        if (player == null || player.WorldData?.CurrentGameMode != EnumGameMode.Creative)
                        {
                            slot.TakeOut(1);
                            slot.MarkDirty();
                        }
                    }
                    handling = EnumHandling.PreventDefault;
                }
            }
        }
    }
}
