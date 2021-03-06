﻿using NppLogGazer.PatternTracer;
using NppLogGazer.PatternTracer.View;
using NppLogGazer.QuickSearch;
using NppPluginNET;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace NppLogGazer
{
    class Main
    {
        #region " Fields "
        internal const string PluginName = "NppLogGazer"; 
        public const string PluginVersion = "0.8.2";
        static string iniFilePath = null;
        static string defaultKeywordListFile = null;
        static string defaultPatternListFile = null;
        static bool someSetting = false;
        static Form frmPatternTracer = null;
        static Form frmQuickSearch = null;
        static int idPatternTracerDlg = -1;
        static int idQuickSearchDlg = -1;
        static Bitmap tbPatternTracerBmp = Properties.Resources.pattern_tracer;
        static Bitmap tbPatternTracerBmp_tbTab = Properties.Resources.pattern_tracer_bmp;
        static Bitmap tbQuickSearchBmp = Properties.Resources.magnifier;
        static Bitmap tbQuickSearchBmp_tbTab = Properties.Resources.magnifier_bmp;
        static Icon tbPatternTracerIcon = null;
        static Icon tbQuickSearchIcon = null;
        #endregion

        #region " StartUp/CleanUp "
        internal static void CommandMenuInit()
        {
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            iniFilePath = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFilePath)) Directory.CreateDirectory(iniFilePath);
            defaultKeywordListFile = Path.Combine(iniFilePath, PluginName + "Keywords.xml");
            defaultPatternListFile = Path.Combine(iniFilePath, PluginName + "Patterns.xml");
            LoadConfig(iniFilePath);
            
            someSetting = (Win32.GetPrivateProfileInt("SomeSection", "SomeKey", 0, iniFilePath) != 0);

            PluginBase.SetCommand(0, Properties.Resources.show_pattern_tracer, ShowPatternTracerDlg); idPatternTracerDlg = 0;
            PluginBase.SetCommand(1, Properties.Resources.show_quick_search_panel, ShowQuickSearchDlg); idQuickSearchDlg = 1;
            PluginBase.SetCommand(2, Properties.Resources.show_about, ShowAbout); 
        }
        internal static void SetToolBarIcon()
        {
            SetToolBarIcon(tbPatternTracerBmp, idPatternTracerDlg);
            SetToolBarIcon(tbQuickSearchBmp, idQuickSearchDlg);
        }
        internal static void SetToolBarIcon(Bitmap bmp, int id)
        {
            toolbarIcons tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = bmp.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[id]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }
        internal static void PluginCleanUp()
        {
            if (frmQuickSearch != null)
            {
                if (frmQuickSearch.Visible)
                    QuickSearchSettings.Instance.Configs.ShowOnStartup = true;
                else
                    QuickSearchSettings.Instance.Configs.ShowOnStartup = false;

                frmQuickSearch.Close();
            }

            if (frmPatternTracer != null)
            {
                if (frmPatternTracer.Visible)
                    PatternTracerSettings.Instance.Configs.ShowOnStartup = true;
                else
                    PatternTracerSettings.Instance.Configs.ShowOnStartup = false;

                frmPatternTracer.Close();
            }

            SaveConfig(iniFilePath);
        }
        public static string GetDefaultKeywordListPath()
        {
            return defaultKeywordListFile;
        }
        public static string GetDefaultPatternListPath()
        {
            return defaultPatternListFile;
        }
        public static int GetQuickSearchDlgId()
        {
            return idQuickSearchDlg;
        }
        public static int GetPatternTracerDlgId()
        {
            return idPatternTracerDlg;
        }
        internal static void LoadConfig(string iniFilePath)
        {
            QuickSearchSettings.Instance.ConfigDir = iniFilePath;
            QuickSearchSettings.Instance.LoadConfigs();

            PatternTracerSettings.Instance.ConfigDir = iniFilePath;
            PatternTracerSettings.Instance.LoadConfigs();
        }
        internal static void SaveConfig(string iniFilePath)
        {
            QuickSearchSettings.Instance.SaveConfigs();
            PatternTracerSettings.Instance.SaveConfigs();
        }
        #endregion

        #region " Menu functions "
        internal static void ShowPatternTracerDlg()
        {
            if (frmPatternTracer == null)
            {
                frmPatternTracer = PatternTracerPanel.Instance.Form;

                using (Bitmap newBmp = new Bitmap(16, 16))
                {
                    Graphics g = Graphics.FromImage(newBmp);
                    ColorMap[] colorMap = new ColorMap[1];
                    colorMap[0] = new ColorMap();
                    colorMap[0].OldColor = Color.Fuchsia;
                    colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetRemapTable(colorMap);
                    g.DrawImage(tbPatternTracerBmp_tbTab, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                    tbPatternTracerIcon = Icon.FromHandle(newBmp.GetHicon());
                }

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = frmPatternTracer.Handle;
                _nppTbData.pszName = Properties.Resources.pattern_tracer_dlg_label;
                _nppTbData.dlgID = idPatternTracerDlg;
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                _nppTbData.hIconTab = (uint)tbPatternTracerIcon.Handle;
                _nppTbData.pszModuleName = PluginName;
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[idPatternTracerDlg]._cmdID, 1);

            }
            else
            {
                if (frmPatternTracer.Visible)
                {
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMHIDE, 0, frmPatternTracer.Handle);
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[idPatternTracerDlg]._cmdID, 0);
                }
                else
                {
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMSHOW, 0, frmPatternTracer.Handle);
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[idPatternTracerDlg]._cmdID, 1);
                }
            }
        }

        internal static void ShowQuickSearchDlg()
        {
            if (frmQuickSearch == null)
            {
                frmQuickSearch = QuickSearchPanel.Instance.Form;

                using (Bitmap newBmp = new Bitmap(16, 16))
                {
                    Graphics g = Graphics.FromImage(newBmp);
                    ColorMap[] colorMap = new ColorMap[1];
                    colorMap[0] = new ColorMap();
                    colorMap[0].OldColor = Color.Fuchsia;
                    colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetRemapTable(colorMap);
                    g.DrawImage(tbQuickSearchBmp_tbTab, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                    tbQuickSearchIcon = Icon.FromHandle(newBmp.GetHicon());
                }

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = frmQuickSearch.Handle;
                _nppTbData.pszName = Properties.Resources.quick_search_dlg_label;
                _nppTbData.dlgID = idQuickSearchDlg;
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                _nppTbData.hIconTab = (uint)tbQuickSearchIcon.Handle;
                _nppTbData.pszModuleName = PluginName;
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[idQuickSearchDlg]._cmdID, 1);
   
            }
            else
            {
                if (frmQuickSearch.Visible)
                {
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMHIDE, 0, frmQuickSearch.Handle);
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[idQuickSearchDlg]._cmdID, 0);
                }
                else
                {
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMSHOW, 0, frmQuickSearch.Handle);
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[idQuickSearchDlg]._cmdID, 1);
                }
            }
        }
        internal static void ShowAbout()
        {
            FrmAbout about = new FrmAbout();
            about.Text = PluginName + " v" + PluginVersion;
            about.ShowDialog();
        }
        #endregion
    }
}