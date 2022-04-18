using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace InstallPackage
{
    class DetectInstalls
    {
        List<string> m_progs0 = new List<string>();
        List<Tuple<string, string>> m_progs1 = new List<Tuple<string, string>>();
        List<Tuple<string, string>> m_progs2 = new List<Tuple<string, string>>();
        List<string> m_progs3 = new List<string>();
        

        private string Read(RegistryKey baseKey, string keyName, string valueName)
        {
            RegistryKey sk1 = baseKey.OpenSubKey(keyName);            
            // If the RegistrySubKey doesn't exist -> (null)
            if (sk1 == null)
            {
                return null;
            }
            else
            {
                try
                {
                    // If the RegistryKey exists I get its value
                    // or null is returned.
                    return (string)sk1.GetValue(valueName.ToUpper());
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        private int ReadNumber(RegistryKey baseKey, string keyName, string valueName)
        {
            RegistryKey sk1 = baseKey.OpenSubKey(keyName);
            // If the RegistrySubKey doesn't exist -> (null)
            if (sk1 != null)
            {
                try
                {
                    // If the RegistryKey exists I get its value
                    // or null is returned.
                    return (int)sk1.GetValue(valueName.ToUpper());
                }
                catch (Exception e)
                {
                    return -1;
                }
            }
            return -1;
        }
        private List<string> get_installers()
        {
            RegistryKey baseKey = Registry.ClassesRoot;
            RegistryKey rk = baseKey.OpenSubKey(@"Installer\Products");
            var subKeys = rk.GetSubKeyNames();

            Dictionary<string, int> progsMap = new Dictionary<string, int>();
            foreach (var subkey in subKeys)
            {
                var prog = Read(rk, subkey, "ProductName");
                if (prog == null) continue;
                int nVal = 0;
                if (progsMap.TryGetValue(prog, out nVal)) continue;
                progsMap.Add(prog, 1);
            }


            List<string> progs = new List<string>();
            foreach (KeyValuePair<string, int> ele in progsMap)
            {
                progs.Add(ele.Key);
            }
            return progs;
        }

        private List<Tuple<string, string>> get_x86_installs()
        {
            RegistryKey baseKey = Registry.LocalMachine;
            RegistryKey rk = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            var subKeys = rk.GetSubKeyNames();

            Dictionary<string, string> progsMap = new Dictionary<string, string>();
            foreach (var subkey in subKeys)
            {
                var prog = Read(rk, subkey, "DisplayName");
                if (prog == null) continue;

                var version = Read(rk, subkey, "DisplayVersion");
                string strTmp;
                if (progsMap.TryGetValue(prog, out strTmp)) continue;
                progsMap.Add(prog, version);
            }

            List<Tuple<string, string>> progs = new List<Tuple<string, string>>();
            foreach (KeyValuePair<string, string> ele in progsMap)
            {
                progs.Add(new Tuple<string, string>(ele.Key, ele.Value));
            }
            return progs;
        }

        private List<Tuple<string, string>> get_wow64_installs()
        {
            RegistryKey baseKey = Registry.LocalMachine;            
            RegistryKey rk = baseKey.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            var subKeys = rk.GetSubKeyNames();

            Dictionary<string, string> progsMap = new Dictionary<string, string>();
            foreach (var subkey in subKeys)
            {
                var prog = Read(rk, subkey, "DisplayName");
                if (prog == null) continue;
                var version = Read(rk, subkey, "DisplayVersion");
                string strTmp;
                if (progsMap.TryGetValue(prog, out strTmp)) continue;
                progsMap.Add(prog, version);
            }

            List< Tuple<string, string>> progs = new List<Tuple<string, string>>();
            foreach (KeyValuePair<string, string> ele in progsMap)
            {
                progs.Add(new Tuple<string, string>(ele.Key, ele.Value));
            }
            return progs;
        }

        private List<string> get_net_framework_installs()
        {
            RegistryKey baseKey = Registry.LocalMachine;
            RegistryKey rk = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");
            var subKeys = rk.GetSubKeyNames();

            List<string> progs = new List<string>();
            foreach(var subkey in subKeys)
            {
                if (!subkey.StartsWith("v")) continue;
                progs.Add("Microsoft .NET Framework " + subkey.Substring(1));
            }


            if(Read(baseKey, @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client", "Version")!=null)
            {
                progs.Add("Microsoft .NET Framework 4 Client Profile");
            }

            if (ReadNumber(baseKey, @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5", "SP") > 0)
            {
                progs.Add("Microsoft .NET Framework 3.5 Client Profile");
            }
            return progs;
        }

        public void init()
        {
            m_progs0 = get_installers();
            m_progs1 = get_x86_installs();
            m_progs2 = get_wow64_installs();
            m_progs3 = get_net_framework_installs();
        }

        public bool is_installed(string strProg, bool checkVersion = false)
        {
            foreach (var prog in m_progs0)
            {
                if (prog.Contains(strProg))
                {
                    return true;
                }
            }

            foreach (var prog in m_progs1)
            {                
                if (prog.Item1.Contains(strProg) || strProg.Contains(prog.Item1))
                {
                    if(checkVersion==false)
                        return true;
                    string strProg1 = prog.Item1 + " v" + prog.Item2;
                    return strProg == strProg1;
                }
            }

            foreach (var prog in m_progs2)
            {
                if (prog.Item1.Contains(strProg) || strProg.Contains(prog.Item1))
                {
                    if (checkVersion == false)
                        return true;
                    string strProg1 = prog.Item1 + " v" + prog.Item2;
                    return strProg == strProg1;
                }
            }

            foreach (var prog in m_progs3)
            {
                if (prog.Contains(strProg))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
