/**
 * 
 * Original Source Name: MysteryModuleService
 * This version is slightly altered to avoid integration conflicts
 * Authored by: Goofy1807, Timwi, and rocket0634
 * Original Github Repository: https://github.com/Goofy1807/Mystery-Module
 * Date of Copy: 18-08-2020
 * 
 */

using System.Collections.Generic;

namespace wmeijer {
    public sealed class TechSupportSettings
    {
        public string SiteUrl = @"https://ktane.timwi.de/json/raw";

        public Dictionary<string, string> RememberedCompatibilities = new Dictionary<string, string>();

        public bool HideUberSouvenir = false;

        public bool AutomaticUpdate = true;

        public int Version = 3;
    }
}
