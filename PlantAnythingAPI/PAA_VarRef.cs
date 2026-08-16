using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace PlantAnythingAPI
{
    internal class PAA_VarRef
    {
        internal const string ModName = "PlantAnythingLib";
        internal const string ModName_Trunc = "PAL";

        //globally used API callers
        internal static ICoreAPI API;
        internal static ICoreClientAPI API_Client;
        internal static ICoreServerAPI API_Server;
    }
}
