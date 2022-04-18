using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InstallPackage
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        static DotNetFrameWorksInfo g_dotnetInfo = new DotNetFrameWorksInfo();
        static PackageGroup[] g_packageGroups = new PackageGroup[]
        {
            new PackageGroup()
            {
                groupName = "DirectX",
                bExclude = false,
                progs = new CheckInfo[]
                {
                    new CheckInfo("Microsoft DirectX SDK (June 2010)", "DXSDK_Jun10.exe")
                }
            },
            new PackageGroup()
            {
                groupName = "Visual C++ Redist",
                bExclude = false,
                progs = new CheckInfo[]
                {
                    new CheckInfo("Microsoft Visual C++ 2005 Redistributable", "vcredist/vcredist2005_x86.exe"),
                    new CheckInfo("Microsoft Visual C++ 2005 Redistributable (x64)", "vcredist/vcredist2005_x64.exe"),

                    new CheckInfo("Microsoft Visual C++ 2008 Redistributable - x86", "vcredist/vcredist2008_x86.exe"),
                    new CheckInfo("Microsoft Visual C++ 2008 Redistributable - x64", "vcredist/vcredist2008_x64.exe"),

                    new CheckInfo("Microsoft Visual C++ 2010  x86 Redistributable", "vcredist/vcredist2010_x86.exe"),
                    new CheckInfo("Microsoft Visual C++ 2010  x64 Redistributable", "vcredist/vcredist2010_x64.exe"),

                    new CheckInfo("Microsoft Visual C++ 2012 Redistributable (x86)", "vcredist/vcredist2012_x86.exe"),
                    new CheckInfo("Microsoft Visual C++ 2012 Redistributable (x64)", "vcredist/vcredist2012_x64.exe"),

                    new CheckInfo("Microsoft Visual C++ 2013 Redistributable (x86)", "vcredist/vcredist2013_x86.exe"),
                    new CheckInfo("Microsoft Visual C++ 2013 Redistributable (x64)", "vcredist/vcredist2013_x64.exe"),

                    new CheckInfo("Microsoft Visual C++ 2015-2022 Redistributable (x86)", "vcredist/vcredist2015_2017_2019_2022_x86.exe"),
                    new CheckInfo("Microsoft Visual C++ 2015-2022 Redistributable (x64)", "vcredist/vcredist2015_2017_2019_2022_x64.exe")
                }
            },
            new PackageGroup()
            {
                groupName = "OpenAL",
                bExclude = false,
                progs = new CheckInfo[]
                {
                    new CheckInfo("OpenAL", "oalinst.exe")
                }
            },
            //new PackageGroup()
            //{
            //    groupName = ".Net Frameworks",
            //    bExclude = false,
            //    progs = new CheckInfo[]
            //    {
            //        new CheckInfo("Microsoft .NET Framework 4.7.2", "dotnet/ndp472-kb4054531-web.exe"),
            //        new CheckInfo("Microsoft .NET Framework 4.6.2", "dotnet/ndp462-kb3151802-web.exe"),
            //        new CheckInfo("Microsoft .NET Framework 4.5.2", "dotnet/ndp452-kb2901954-web.exe"),

            //        new CheckInfo("Microsoft .NET Framework 4 Client Profile", "dotnet/dotnetfx40_client_setup.exe"),
            //        new CheckInfo("Microsoft .NET Framework 4 ", "dotnet/dotnetfx40_full_setup.exe"),

            //        new CheckInfo("Microsoft .NET Framework 3.5 Client Profile", "dotnet/dotnetfx35setup.exe"),
            //        new CheckInfo("Microsoft .NET Framework 3.5", "dotnet/dotnetfx35setup.exe"),
            //    }
            //},
            new PackageGroup()
            {
                groupName = "XNA",
                bExclude = false,
                progs = new CheckInfo[]
                {
                    new CheckInfo("Microsoft XNA Framework Redistributable 3.0", "xna/xnafx30_redist.msi"),
                    new CheckInfo("Microsoft XNA Framework Redistributable 3.1", "xna/xnafx31_redist.msi"),
                    new CheckInfo("Microsoft XNA Framework Redistributable 4.0", "xna/xnafx40_redist.msi")
                }
            },
            new PackageGroup()
            {
                groupName = "PhysX",
                bExclude = true,
                progs = new CheckInfo[]
                {
                    new CheckInfo("NVIDIA PhysX v8.09.04", "physx/PhysX_8.09.04_SystemSoftware.exe", true),
                    new CheckInfo("NVIDIA PhysX v9.12.1031", "physx/PhysX-9.12.1031-SystemSoftware.msi",true),
                    new CheckInfo("NVIDIA PhysX v9.13.1220", "physx/PhysX-9.13.1220-SystemSoftware.msi",true),
                    new CheckInfo("NVIDIA PhysX v9.14.0702", "physx/PhysX-9.14.0702-SystemSoftware.msi",true)
                }
            },
        };


        private void createButton(int y, CheckInfo prog, bool installed, bool bRadio=false)
        {
            if (bRadio)
            {
                var chk = new RadioButton();
                chk.Text = prog.strProg;
                chk.Location = new Point(20, y);
                chk.Checked = installed;
                chk.Enabled = !installed;
                chk.Size = new Size(300, 20);
                _container.Controls.Add(chk);
            }
            else
            {
                var chk = new CheckBox();
                chk.Text = prog.strProg;
                chk.Location = new Point(20, y);
                chk.Checked = installed;
                chk.Enabled = !installed;
                chk.Size = new Size(300, 20);
                _container.Controls.Add(chk);
            }
            if (!installed)
            {
                var btn = new Button();
                btn.Text = "Install";
                btn.Tag = prog;
                btn.Location = new Point(330, y);
                btn.Click += new EventHandler(this.onInstall);
                _container.Controls.Add(btn);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DetectDotNet dotnetDetector = new DetectDotNet();
            dotnetDetector.init();

            foreach (Control c in _container.Controls)
                _container.Controls.Remove(c);


            DetectInstalls detector = new DetectInstalls();
            detector.init();

            List<bool> exists = new List<bool>();
            int y = 10;
            foreach(var grp in g_packageGroups)
            {
                var lbl = new Label();
                lbl.Text = grp.groupName;
                lbl.Location = new Point(10, y);
                lbl.Size = new Size(300, 20);
                lbl.ForeColor = Color.DarkBlue;
                _container.Controls.Add(lbl);
                y += 24;
                foreach(var prog in grp.progs)
                {
                    bool bExist = detector.is_installed(prog.strProg, prog.checkVersion);
                    createButton(y, prog, bExist, grp.bExclude);
                    y += 24;
                }
            }


            var lbl1 = new Label();
            lbl1.Text = ".NET Framework";
            lbl1.Location = new Point(10, y);
            lbl1.Size = new Size(300, 20);
            lbl1.ForeColor = Color.DarkBlue;
            _container.Controls.Add(lbl1);
            y += 24;

            bool installed = dotnetDetector.checkVersion(g_dotnetInfo.ver_35.strProg);
            createButton(y, g_dotnetInfo.ver_35, installed);
            y += 24;

            installed = dotnetDetector.checkVersion(g_dotnetInfo.ver_40.strProg);
            createButton(y, g_dotnetInfo.ver_40, installed);
            y += 24;

            foreach(var prog in g_dotnetInfo.ver_45s)
            {
                installed = dotnetDetector.checkVersion45(prog.Item1);
                createButton(y, prog.Item2, installed);
                y += 24;
            }
        }

        private void onInstall(object sender, EventArgs e)
        {
            CheckInfo info = (CheckInfo)((Button)sender).Tag;
            downloader.start(info.strProg, "http://my2starserver.com/remote/download/" + info.strPath, progressBar1, info.executable);
        }

        Downloader downloader = new Downloader();
        private void button2_Click(object sender, EventArgs e)
        {
            
        }
    }
}


public class CheckInfo
{
    public CheckInfo(string s, string path, bool b = false)
    {
        strProg = s;
        checkVersion = b;
        strPath = path;
        executable = path.EndsWith(".exe");
    }
    public string strProg;
    public string strPath;
    public bool   executable;
    public bool   checkVersion;
}

public class PackageGroup
{
    public string groupName;
    public bool bExclude;
    public CheckInfo[] progs;
}


public class DotNetFrameWorksInfo
{
    public CheckInfo ver_35 = new CheckInfo("3.5", "dotnet/dotnetfx35setup.exe");
    public CheckInfo ver_40 = new CheckInfo("4.0", "dotnet/dotnetfx40_full_setup.exe");
    public List<Tuple<int, CheckInfo>> ver_45s = new List<Tuple<int, CheckInfo>>()
    {
        new Tuple<int, CheckInfo>(379893, new CheckInfo("4.5.2", "dotnet/ndp452-kb2901954-web.exe")),
        new Tuple<int, CheckInfo>(394802, new CheckInfo("4.6.2", "dotnet/ndp462-kb3151802-web.exe")),
        new Tuple<int, CheckInfo>(461808, new CheckInfo("4.7.2", "dotnet/ndp472-kb4054531-web.exe")),
    };
}



