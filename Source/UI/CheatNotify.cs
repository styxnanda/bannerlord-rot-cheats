using System;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace RoTCheats.UI
{
    public static class CheatNotify
    {
        public static void Show(string message)
        {
            try
            {
                MBInformationManager.AddQuickInformation(new TextObject(message), 0, null, null, "");
            }
            catch
            {
            }
        }
    }
}
