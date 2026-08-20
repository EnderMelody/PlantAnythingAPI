using PlantAnythingAPI.handler;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace PlantAnythingAPI
{
    public class PAA_Main_ModSystem : ModSystem
    {

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void StartPre(ICoreAPI api)
        {
            PAA_VarRef.API = api;
            switch (api.Side)
            {
                case (EnumAppSide.Server): { PAA_VarRef.API_Server = (ICoreServerAPI)api; break; }
                case (EnumAppSide.Client): { PAA_VarRef.API_Client = (ICoreClientAPI)api; break; }
                default: { break; }
            }
        }

        public override void Start(ICoreAPI api)
        {
            api.RegisterCollectibleBehaviorClass("Behavior_Plantable", typeof(PAA_Handle_Plant.Behavior_Plantable));
        }

    }
}
