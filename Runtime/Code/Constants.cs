using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOP
{
    public static class Constants
    {
        public static class AssetGUIDS
        {
            public static string accessNodeGUID = "383d1a1eefe41db4eb8708719f86fc03";

            public static string iscHardwareProgPortalGUID = "c4e7a9ed6153edf488cf434d843311f5";
            public static string iscHardwareProgPortalHauntGUID = "7c01731f4ba8cb548af2a35bce9105d3";
            public static string iscSolusShopPortalGUID = "07fc379a8d5d3c44c9211730bf4e1572";

            public static string cscSolusAmalgamator = "14bf22df446f37549aa65eb724c1ddda";

            public static string dlc1ExpansionGUID = "d4f30c23b971a9b428e2796dc04ae099";
            public static string dlc2ExpansionGUID = "851f234056d389b42822523d1be6a167";
            public static string dlc3ExpansionGUID = "234e83997deed274291470be69e7662e";
            
            public enum Expansion
            {
                DLC1 = 1,
                DLC2 = 2,
                DLC3 = 3
            }

            public static string GetVanillaExpansion(Expansion expansion)
            {
                switch (expansion)
                {
                    case Expansion.DLC1:
                        return dlc1ExpansionGUID;
                    case Expansion.DLC2:
                        return dlc1ExpansionGUID;
                    case Expansion.DLC3:
                        return dlc3ExpansionGUID;
                    default:
                        LOPLog.Error($"Invalid Expansion provided.");
                        break;
                }
                return null;
            }
        }
    }
}
