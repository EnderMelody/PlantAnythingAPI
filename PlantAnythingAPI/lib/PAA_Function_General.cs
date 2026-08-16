using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Vintagestory.API.Common;

namespace PlantAnythingAPI.lib
{
    internal class PAA_Function_General
    {
        private static ICoreAPI API = PAA_VarRef.API;

        internal static void Log_Debug(string DebugMessage, [CallerMemberName] string caller = "", params object[] loggers) { try { if (true) { API.Logger.Debug($"[{PAA_VarRef.ModName_Trunc}]: ({caller}) {string.Format(DebugMessage, [.. loggers.Select(a => a ?? "null")])}"); } } catch (Exception e) { PAA_Function_General.Log_Error("Caught exception: {0}", loggers: [e.Message]); } }
        internal static void Log_Error(string ErrorMessage, [CallerMemberName] string caller = "", params object[] loggers) { try { API?.Logger?.Error($"[{PAA_VarRef.ModName_Trunc}]: ({caller}) {string.Format(ErrorMessage, [.. loggers.Select(a => a ?? "null")])}"); } catch (Exception e) { API.Logger.Error(e); } }
        internal static void Log_Debug_Verbose(string DebugMessage, [CallerMemberName] string caller = "", params object[] loggers) { try { if (true) { API.Logger.Debug($"[{PAA_VarRef.ModName_Trunc}][V]: ({caller}) {string.Format(DebugMessage, [.. loggers.Select(a => a ?? "null")])}"); } } catch (Exception e) { PAA_Function_General.Log_Error("Caught exception: {0}", loggers: [e.Message]); } }
    }
}
