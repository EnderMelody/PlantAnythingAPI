using PlantAnythingAPI.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
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
            private List<Block> plantOn = [];
            private Block? plantAs;

            public override void Initialize(JsonObject properties)
            {
                //itemProps = properties.AsObject<ItemProps_Plantable>(null, collObj.Code.Domain);
                if (properties["plantAs"].Exists)
                {
                    if (properties["plantOn"].Exists)
                    {
                        foreach (var i in properties["plantOn"])
                        {
                            plantOn.Add(API.World.GetBlock(new AssetLocation(i.ToString())));
                        }
                    }
                    plantAs = API.World.GetBlock(new AssetLocation(properties["plantAs"].ToString()));
                    PAA_Function_General.Log_Debug("found Behavior_Plantable on item; plantOn: list[{0}], plantAs: {1}", loggers: [plantOn?.Count ?? 0, plantAs?.Code.GetNameWithDomain() ?? "null"]);
                }
                base.Initialize(properties);
            }

            public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
            {
                if (plantAs is null) { return; }
                if (byEntity is EntityPlayer && (byEntity.Controls.Sneak || byEntity.Controls.Sprint)) { return; } //bypass interaction when crouching to allow other ineraction systems
                PAA_Function_General.Log_Debug_Verbose("ran with item: {0} on selected block: {1}", loggers: [slot.Itemstack?.GetName() ?? "empty", blockSel.Block.Code.Path.ToString() ?? "null"]);


                bool isListEmpty = (plantOn.Count == 0 || plantOn is null);
                if (isListEmpty || plantOn.Any(block => block is BlockFarmland || block is null))
                {
                    if (byEntity.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityFarmland farmland) { farmland.TryPlant(plantAs, slot, byEntity, blockSel); handling = EnumHandling.PreventDefault; }
                }
                if (!isListEmpty && plantOn.Any(block => (block is not null && block.Code.Path == byEntity.World.BlockAccessor.GetBlock(blockSel.Position).Code.Path)))
                {
                    //slightly modified vanilla .TryPlant code to not explicitly require a seed item or item cropbehavior or farmland block target
                    BlockPos checkBlock = blockSel.Position.UpCopy();
                    if (API.World.BlockAccessor.GetBlock(checkBlock) is not null && API.World.BlockAccessor.GetBlock(checkBlock).BlockMaterial == EnumBlockMaterial.Air)
                    {
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


                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
            }
        }
    }
}
