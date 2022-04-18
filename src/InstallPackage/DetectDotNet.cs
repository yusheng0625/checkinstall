using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallPackage
{
    class DetectDotNet
    {
        public List<string> _versions;
        public int _version45 = 0;
        public bool checkVersion(string ver)
        {
            foreach(string v in _versions)
            {
                if (v.StartsWith(ver))
                    return true;
            }
            return false;
        }
        public bool checkVersion45(int ver)
        {
            return _version45 >= ver;
        }

        public void init()
        {
            _versions = Get1To45VersionFromRegistry();
            _version45 = Get45PlusFromRegistry();
        }

        private List<string> Get1To45VersionFromRegistry()
        {
            List<string> versions = new List<string>();
            // Opens the registry key for the .NET Framework entry.
            using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    // Skip .NET Framework 4.5 version information.
                    if (versionKeyName == "v4")
                    {
                        continue;
                    }

                    if (versionKeyName.StartsWith("v"))
                    {
                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        // Get the .NET Framework version value.
                        string name = (string)versionKey.GetValue("Version", "");
                        if (!string.IsNullOrEmpty(name))
                        {
                            versions.Add(name);
                            continue;
                        }

                        foreach (string subKeyName in versionKey.GetSubKeyNames())
                        {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            name = (string)subKey.GetValue("Version", "");
                            if (string.IsNullOrEmpty(name)) continue;
                            versions.Add(name);
                        }
                    }
                }
            }
            return versions;
        }

        private int Get45PlusFromRegistry()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(subkey))
            {
                if (ndpKey == null)
                    return 0;
                //First check if there's an specific version indicated
                if (ndpKey.GetValue("Release") != null)
                {
                    return (int)ndpKey.GetValue("Release");
                }
            }
            return 0;
        }

        // Checking the version using >= enables forward compatibility.
        private string CheckFor45PlusVersion(int releaseKey)
        {
            if (releaseKey >= 528040)
                return "4.8";
            if (releaseKey >= 461808)
                return "4.7.2";
            if (releaseKey >= 461308)
                return "4.7.1";
            if (releaseKey >= 460798)
                return "4.7";
            if (releaseKey >= 394802)
                return "4.6.2";
            if (releaseKey >= 394254)
                return "4.6.1";
            if (releaseKey >= 393295)
                return "4.6";
            if (releaseKey >= 379893)
                return "4.5.2";
            if (releaseKey >= 378675)
                return "4.5.1";
            if (releaseKey >= 378389)
                return "4.5";
            // This code should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "";
        }


    }
}
