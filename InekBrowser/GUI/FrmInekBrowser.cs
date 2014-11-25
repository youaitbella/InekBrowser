﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using combit.ListLabel20;
using org.inek.InekBrowser.Data;
using org.inek.controls.gui;
using org.inek.controls.helper;
using org.inek.InekBrowser.Data.Entities;
using Application = System.Windows.Forms.Application;
using DataTable = System.Data.DataTable;
using Point = System.Drawing.Point;

namespace org.inek.InekBrowser.GUI {
    public partial class FrmInekBrowser : Form {

        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TitleBar));

        /* Use for window moving */
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        /* ################## */

        private FrmList dlg;
        private SelectionDrg selectionDrg;

        public static string PEPP {
            get; set;
        }

        public string TextCode {
            set { cbxPepp.Text = value; }
        }

        private FrmSearch search;

        public FrmInekBrowser() {
            InitializeComponent();
            UpdateGuiForSystem();
            SelectionPepp.Parent = this;
            cbxPepp.InputField.Click += cbxPepp_ButtonClicked;
            SetRechercheHelp();
        }

        private void UpdateGuiForSystem() {
            if (Program.SystemBrowser == Program.System.Drg) {
                titleBar.BackColor = BrowserColors.DrgTitleBar;
                mnuMain.BackColor = BrowserColors.DrgMenuBand;
                BackColor = BrowserColors.DrgBrowser;
                pnlContentBackground.BackColor = BrowserColors.DrgBackgroundPanel;
                peppData.BackColor = BrowserColors.DrgDataBackground;
                peppData.ColorTextFields(BrowserColors.DrgDataTextField);
                titleBar.Title = "DRG-Browser " + Program.Year;
                pnlContentBackground.Controls.RemoveByKey("peppData");
                pnlContentBackground.Controls.RemoveByKey("peppSelection");
                initDrgSelection();
                selectionDrg.BackColor = BrowserColors.DrgSelection;
                lblSystem.Text = "DRG:";
            } else if (Program.SystemBrowser == Program.System.Pepp) {
                titleBar.BackColor = BrowserColors.PeppTitleBar;
                mnuMain.BackColor = BrowserColors.PeppMenuBand;
                BackColor = BrowserColors.PeppBrowser;
                selectionPepp.BackColor = BrowserColors.PeppSelection;
                pnlContentBackground.BackColor = BrowserColors.PeppBackgroundPanel;
                peppData.BackColor = BrowserColors.PeppDataBackground;
                peppData.ColorTextFields(BrowserColors.PeppDataTextField);
                titleBar.Title = "PEPP-Browser " + Program.Year;
                pnlContentBackground.Controls.RemoveByKey("drgData");
                lblSystem.Text = "PEPP:";
                mnuPepp.Text = "PEPPs";
                peppData.CatalogueActive = false;
            }
        }

        private void initDrgSelection() {
            selectionDrg.Anchor =
                ((System.Windows.Forms.AnchorStyles)
                    (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
            selectionDrg.BackColor = System.Drawing.Color.Transparent;
            selectionDrg.Location = new System.Drawing.Point(0, 27);
            selectionDrg.Name = "selectionDrg";
            selectionDrg.Size = new System.Drawing.Size(1182, 52);
            selectionDrg.TabIndex = 13;
        }

        private void SetRechercheHelp() {
            search = new FrmSearch(this) {helpProvider1 = {HelpNamespace = helpProvider1.HelpNamespace}};
            search.helpProvider1.SetHelpNavigator(search, HelpNavigator.Topic);
            search.helpProvider1.SetShowHelp(search, true);
            search.helpProvider1.SetHelpKeyword(search, "Navigieren.htm");
        }

        private void SetDataHelpProvider(FrmList dlg) {
            dlg.helpProvider1.HelpNamespace = helpProvider1.HelpNamespace;
            dlg.helpProvider1.SetHelpNavigator(dlg, HelpNavigator.Topic);
            dlg.helpProvider1.SetShowHelp(dlg, true);
            dlg.helpProvider1.SetHelpKeyword(dlg, "Daten.htm");
        }

        private void mnuPepp_Click(object sender, EventArgs e) {
            dlg = new FrmList();
            SetDataHelpProvider(dlg);
            dlg.StartPosition = FormStartPosition.CenterParent;
            var q = CsvData.Context().System.Select(p => new { pe_SK = p.Category, pe_Pepp = p.Code, pe_Text = p.Text });
            dlg.SetDataSource(q);
            dlg.Text = "PEPPS";
            dlg.Show();
        }

        private void mnuStructureCategories_Click(object sender, EventArgs e) {
            dlg = new FrmList(); 
            SetDataHelpProvider(dlg);
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.Text = "Strukturkategorien";
            var q =
                CsvData.Context().StructureCategories.OrderBy(sk => sk.Order)
                    .Select(sk => new {
                                          st_Strukturkategorie = sk.Category,
                                          st_Order = sk.Order,
                                          st_Text = sk.Text,
                                          st_PeppAnzahl = sk.PeppCount,
                                          st_FaelleAnzahl = sk.CaseCount,
                                          st_TageAnzahl = sk.DayCount
                                      });
            dlg.DataSource = new DataTable();
            dlg.SetDataSource(q);
            dlg.ShowDialog(this);
        }

        private void mnuPeppInfo_Click(object sender, EventArgs e) {
            dlg = new FrmList();
            SetDataHelpProvider(dlg);
            dlg.StartPosition = FormStartPosition.CenterParent;
            var q = CsvData.Context().SystemInfo.Select(pi => new {
                                                                     kd_Pepp = pi.Code, kd_FaelleAnzahl = pi.CaseCount,
                                                                     kd_VwdSummeTage = pi.LosSumDays,
                                                                     kd_VwdMw = pi.LosAverage,
                                                                     kd_VwdStd = pi.LosStandard,
                                                                     kd_VwdHK = pi.LosStandard,
                                                                     kd_GeschlechtM = pi.GenderMale,
                                                                     kd_GeschlechtW = pi.GenderFemale,
                                                                     kd_AlterMw = pi.AgeAverage,
                                                                     kd_AlterStd = pi.AgeStandard,
                                                                     kd_AlterU28T = pi.AgeBelow28Days,
                                                                     kd_AlterU01 = pi.AgeBelow1Year,
                                                                     kd_AlterU03 = pi.AgeBelow3Years,
                                                                     kd_AlterU06 = pi.AgeBelow6Years,
                                                                     kd_AlterU10 = pi.AgeBelow10Years,
                                                                     kd_AlterU16 = pi.AgeBelow16Years,
                                                                     kd_AlterU18 = pi.AgeBelow18Years,
                                                                     kd_AlterU30 = pi.AgeBelow30Years,
                                                                     kd_AlterU40 = pi.AgeBelow40Years,
                                                                     kd_AlterU50 = pi.AgeBelow50Years,
                                                                     kd_AlterU55 = pi.AgeBelow55Years,
                                                                     kd_AlterU60 = pi.AgeBelow60Years,
                                                                     kd_AlterU65 = pi.AgeBelow65Years,
                                                                     kd_AlterU75 = pi.AgeBelow75Years,
                                                                     kd_AlterU80 = pi.AgeBelow80Years,
                                                                     kd_AlterU99 = pi.AgeBelow99Years,
                                                                     kd_TageskostenMw = pi.DayCostsAverage,
                                                                     kd_TageskostenStd = pi.DayCostsStandard,
                                                                     kd_TageskostenHK = pi.DayCostsHc
                                                                 });
            dlg.SetDataSource(q);
            dlg.Text = "Kopfdaten";
            dlg.ShowDialog(this);

        }

        private void mnuPrimaryDiagnoses_Click(object sender, EventArgs e) {
            dlg = new FrmList();
            SetDataHelpProvider(dlg);
            dlg.StartPosition = FormStartPosition.CenterParent;
            var q = CsvData.Context().PrimaryDiagnoses.Select(p => new { hd_Pepp = p.SystemCode, hd_Code = p.DiagCode, hd_FaelleAnzahl = p.Count, hd_FaelleAnteil = p.Fraction});
            dlg.SetDataSource(q);
            dlg.Text = "Hauptdiagnosen";
            dlg.ShowDialog(this);

        }

        private void mnuSecondaryDiagnoses_Click(object sender, EventArgs e) {
            dlg = new FrmList();
            SetDataHelpProvider(dlg);
            dlg.StartPosition = FormStartPosition.CenterParent;
            var q =
                CsvData.Context()
                    .SecondaryDiagnoses.Select(
                        s =>
                            new {
                                    nd_Pepp = s.System,
                                    nd_Code = s.DiagCode,
                                    nd_FaelleAnzahl = s.CaseCount,
                                    nd_FaelleAnteil = s.CaseFraction,
                                    nd_NennungenAnzahl = s.EntryCount,
                                    nd_NennungenAnteil = s.EntryFraction
                                });
            dlg.SetDataSource(q);
            dlg.Text = "Nebendiagnosen";
            dlg.ShowDialog(this);

        }

        private void mnuProcedures_Click(object sender, System.EventArgs e) {
            dlg = new FrmList();
            SetDataHelpProvider(dlg);
            dlg.StartPosition = FormStartPosition.CenterParent;
            var q = CsvData.Context().Procedures.Select(p => new {
                                                                     pr_Pepp = p.System,
                                                                     pr_Code = p.ProcCode,
                                                                     pr_FaelleAnzahl = p.CaseCount,
                                                                     pr_FaelleAnteil = p.CaseFraction,
                                                                     pr_NennungenAnzahl = p.EntryCount,
                                                                     pr_NennungenAnteil = p.EntryFraction
                                                                 });
            dlg.SetDataSource(q);
            dlg.Text = "Prozeduren";
            dlg.ShowDialog();

        }

        private void mnuCosts_Click(object sender, System.EventArgs e) {
            dlg = new FrmList();
            SetDataHelpProvider(dlg);
            dlg.StartPosition = FormStartPosition.CenterParent;
            var q = CsvData.Context().Costs.Select(c => new {
                                                                ko_Pepp = c.Code,
                                                                ko_BereichNr = c.CostDomain,
                                                                ko_KArt1 = c.CostType1,
                                                                ko_KArt2 = c.CostType2,
                                                                ko_KArt3a = c.CostType3a,
                                                                ko_KArt3b = c.CostType3b,
                                                                ko_KArt3c = c.CostType3c,
                                                                ko_KArt3 = c.CostType3,
                                                                ko_KArt4a = c.CostType4a,
                                                                ko_KArt4b = c.CostType4b,
                                                                ko_KArt5 = c.CostType5,
                                                                ko_KArt6a = c.CostType6a,
                                                                ko_KArt6b = c.CostType6b,
                                                                ko_KArt7 = c.CostType7,
                                                                ko_KArt8 = c.CostType8
                                                            });
            dlg.SetDataSource(q);
            dlg.Text = "Kosten";
            dlg.ShowDialog(this);

        }

        private void mnuRecherche_Click(object sender, System.EventArgs e) {
            dlg = new FrmList();
            SetDataHelpProvider(dlg);
            dlg.StartPosition = FormStartPosition.CenterParent;
            var q = CsvData.Context().Recherche.Select(d => new {
                                                                    re_Code = d.Code,
                                                                    re_Text = d.Text,
                                                                    re_Hauptdiagnose = d.PrimaryDiagnosis,
                                                                    re_Nebendiagnose = d.SecondaryDiagnosis,
                                                                    re_Prozedur = d.Procedure
                                                                  });
            dlg.SetDataSource(q);
            dlg.Text = "Recherche";
            dlg.ShowDialog();
        }

        private void FrmPeppBrowser_MouseMove(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }


        //***********************************************************
        //This gives us the ability to resize the borderless from any borders instead of just the lower right corner
        protected override void WndProc(ref Message m) {
            const int wmNcHitTest = 0x84;
            const int htLeft = 10;
            const int htRight = 11;
            const int htTop = 12;
            const int htTopLeft = 13;
            const int htTopRight = 14;
            const int htBottom = 15;
            const int htBottomLeft = 16;
            const int htBottomRight = 17;

            if (m.Msg == wmNcHitTest) {
                int x = (int)(m.LParam.ToInt64() & 0xFFFF);
                int y = (int)((m.LParam.ToInt64() & 0xFFFF0000) >> 16);
                Point pt = PointToClient(new Point(x, y));
                Size clientSize = ClientSize;
                ///allow resize on the lower right corner
                if (pt.X >= clientSize.Width - 16 && pt.Y >= clientSize.Height - 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(IsMirrored ? htBottomLeft : htBottomRight);
                    return;
                }
                ///allow resize on the lower left corner
                if (pt.X <= 16 && pt.Y >= clientSize.Height - 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(IsMirrored ? htBottomRight : htBottomLeft);
                    return;
                }
                ///allow resize on the upper right corner
                if (pt.X <= 16 && pt.Y <= 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(IsMirrored ? htTopRight : htTopLeft);
                    return;
                }
                ///allow resize on the upper left corner
                if (pt.X >= clientSize.Width - 16 && pt.Y <= 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(IsMirrored ? htTopLeft : htTopRight);
                    return;
                }
                ///allow resize on the top border
                if (pt.Y <= 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(htTop);
                    return;
                }
                ///allow resize on the bottom border
                if (pt.Y >= clientSize.Height - 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(htBottom);
                    return;
                }
                ///allow resize on the left border
                if (pt.X <= 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(htLeft);
                    return;
                }
                ///allow resize on the right border
                if (pt.X >= clientSize.Width - 16 && clientSize.Height >= 16) {
                    m.Result = (IntPtr)(htRight);
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private void titleBar_ClickedExit(object sender, EventArgs e) {
            Application.Exit();
        }

        private void titleBar_ClickedIconify(object sender, EventArgs e) {
            WindowState = FormWindowState.Minimized;
        }

        private void titleBar_ClickedMinMax(object sender, EventArgs e) {
            if (WindowState == FormWindowState.Normal) {
                WindowState = FormWindowState.Maximized;
            } else {
                WindowState = FormWindowState.Normal;
            }
        }

        private void mnuManual_Click(object sender, EventArgs e) {
            try {
                Process.Start("PeppBrowser.pdf");
            } catch (Exception) {
                MessageBox.Show("Kein Handbuch verfügbar.");
            }

        }

        private void mnuHelp_Click(object sender, EventArgs e) {
            try {
                Process.Start("PeppBrowser.chm");
            } catch (Exception) {
                MessageBox.Show("Keine Hilfe verfügbar.");
            }

        }

        private void cbxPepp_ButtonClicked(object sender, EventArgs e) {
            FrmSearch dlg = new FrmSearch(cbxPepp);
            dlg.helpProvider1.HelpNamespace = helpProvider1.HelpNamespace;
            dlg.helpProvider1.SetHelpNavigator(dlg, HelpNavigator.Topic);
            dlg.helpProvider1.SetShowHelp(dlg, true);
            dlg.helpProvider1.SetHelpKeyword(dlg, "Filter.htm");
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.Text = "PEPP-Suche";
            var q = CsvData.Context().System.Select(pepp => new {Strukturkategorie = pepp.Category, PEPP = pepp.Code, Text = pepp.Text});
            if (SelectionPepp.Category != "") {
                q = q.Where(pepp => pepp.Strukturkategorie == SelectionPepp.Category);
            }
            if (SelectionPepp.PrimaryDiagnosis != "") {
                List<string> pepps =
                    CsvData.Context()
                        .PrimaryDiagnoses.Where(hd => hd.DiagCode == SelectionPepp.PrimaryDiagnosis)
                        .Select(pepp => pepp.SystemCode)
                        .ToList();
                q = q.Where(pepp => pepps.Contains(pepp.PEPP));
            } else if (SelectionPepp.SecondaryDiagnosis != "") {
                List<string> pepps =
                    CsvData.Context()
                        .SecondaryDiagnoses.Where(sd => sd.DiagCode == SelectionPepp.SecondaryDiagnosis)
                        .Select(pepp => pepp.System)
                        .ToList();
                q = q.Where(pepp => pepps.Contains(pepp.PEPP));
            } else if (SelectionPepp.Procedure != "") {
                List<string> pepps =
                    CsvData.Context()
                        .Procedures.Where(proc => proc.ProcCode == SelectionPepp.Procedure)
                        .Select(pepp => pepp.System)
                        .ToList();
                q = q.Where(pepp => pepps.Contains(pepp.PEPP));
            }
            if (!q.Any()) {
                MessageBox.Show(this, "Es gibt keine PEPP zu Ihren Filtereinstellungen.", "Keine PEPP gefunden.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            dlg.SetDataSource(q);
            dlg.KeyColumns = new[] {"Pepp", "Text"};
            dlg.ButtonShowIsVisible = false;
            if (dlg.ShowDialog() == DialogResult.OK) {
                List<object> cells = (List<object>)dlg.Id;
                PEPP = cells[0].ToString();
                cbxPepp.Text = PEPP + ": ";
                cbxPepp.Text += cells[1].ToString();
                LoadPeppData();
                peppData.CatalogueActive = true;
            }
        }

        private void LoadPeppData() {
            tabControl.Enabled = true;
            FillHeadData();
            _mainLoaded = false;
            _secLoaded = false;
            _procLoaded = false;
            _matrixLoaded = false;
            FillActiveTab(ActiveGrid());
        }

        private bool _mainLoaded = false;
        private bool _secLoaded = false;
        private bool _procLoaded = false;
        private bool _matrixLoaded = false;
        private void FillActiveTab(DataGridView grid) {
            CsvData dataContext = CsvData.Context();
            Cursor = Cursors.WaitCursor;
            if (grid == grdMainDiagnosis && !_mainLoaded) {
                var q =
                    dataContext.PrimaryDiagnoses.Where(d => d.SystemCode == PEPP)
                        .Join(dataContext.Recherche.Where(r => r.PrimaryDiagnosis == 1), d => d.DiagCode, r => r.Code,
                            (d, r) => new {
                                              PEPP = d.SystemCode,
                                              Kode = d.DiagCode,
                                              HDBezeichnung = r.Text,
                                              AnzahlFälle = d.Count,
                                              AnteilFälle = d.Fraction
                                          });
                grdMainDiagnosis.DataSource = Helper.ConvertToDataTable(q);
                SetHdGridColumnStyle();
                _mainLoaded = true;
            } else if (grid == grdSecondaryDiagnosis && !_secLoaded) {
                var q = dataContext.SecondaryDiagnoses.Where(d => d.System == PEPP)
                        .Join(dataContext.Recherche.Where(r => r.SecondaryDiagnosis == 1), d => d.DiagCode, r => r.Code,
                            (d, r) => new {
                                PEPP = d.System,
                                Kode = d.DiagCode,
                                Nebendiagnose = r.Text,
                                AnzahlFälle = d.CaseCount,
                                AnteilFälle = d.CaseFraction,
                                AnzahlNennungen = d.EntryCount,
                                AnteilNennungen = d.EntryFraction
                            });
                grdSecondaryDiagnosis.DataSource = Helper.ConvertToDataTable(q);
                SetSdGridColumnStyle();
                _secLoaded = true;
            } else if (grid == grdProcedures && !_procLoaded) {
                var q = dataContext.Procedures.Where(d => d.System == PEPP)
                        .Join(dataContext.Recherche.Where(r => r.Procedure == 1), d => d.ProcCode, r => r.Code,
                            (d, r) => new {
                                PEPP = d.System,
                                Kode = d.ProcCode,
                                Prozedur = r.Text,
                                AnzahlFälle = d.CaseCount,
                                AnteilFälle = d.CaseFraction,
                                AnzahlNennungen = d.EntryCount,
                                AnteilNennungen = d.EntryFraction
                            });
                grdProcedures.DataSource = Helper.ConvertToDataTable(q);
                SetProcedureGridColumnStyle();
                _procLoaded = true;
            } else if (grid == grdCosts && !_matrixLoaded) {
                BuildCostMatrix();
                _matrixLoaded = true;
                grdCosts.ClearSelection();
            }
            Cursor = Cursors.Default;
        }

        private void SetHdGridColumnStyle() {
            //grdMainDiagnosis.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            //grdMainDiagnosis.Columns[2].Width = 700;
            grdMainDiagnosis.Columns[2].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grdMainDiagnosis.Columns[0].Width = 50;
            grdMainDiagnosis.Columns[1].Width = 52;
            grdMainDiagnosis.Columns[2].Width = 720;
            grdMainDiagnosis.Columns[3].MinimumWidth = 80;
            grdMainDiagnosis.Columns[4].MinimumWidth = 80;
            grdMainDiagnosis.Columns[2].HeaderText = "Hauptdiagnose-Bezeichnung";
            grdMainDiagnosis.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grdMainDiagnosis.Columns[3].DefaultCellStyle.Format = "##,###";
            grdMainDiagnosis.Columns[3].HeaderText = "Anzahl Fälle";
            grdMainDiagnosis.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grdMainDiagnosis.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grdMainDiagnosis.Columns[4].DefaultCellStyle.Format = "P";
            grdMainDiagnosis.Columns[4].HeaderText = "Anteil Fälle";
            grdMainDiagnosis.Columns[4].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void SetSdGridColumnStyle() {
            //grdSecondaryDiagnosis.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            //grdSecondaryDiagnosis.Columns[2].Width = 600;
            grdSecondaryDiagnosis.Columns[2].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grdSecondaryDiagnosis.Columns[0].Width = 50;
            grdSecondaryDiagnosis.Columns[1].Width = 52;
            grdSecondaryDiagnosis.Columns[2].Width = 678;
            grdSecondaryDiagnosis.Columns[3].Width = 80;
            grdSecondaryDiagnosis.Columns[4].Width = 80;
            grdSecondaryDiagnosis.Columns[5].MinimumWidth = 100;
            grdSecondaryDiagnosis.Columns[6].MinimumWidth = 100;
            grdSecondaryDiagnosis.Columns[2].HeaderText = "Nebendiagnose-Bezeichnung";
            grdSecondaryDiagnosis.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grdSecondaryDiagnosis.Columns[3].DefaultCellStyle.Format = "##,###";
            grdSecondaryDiagnosis.Columns[3].HeaderText = "Anzahl Fälle";
            grdSecondaryDiagnosis.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grdSecondaryDiagnosis.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grdSecondaryDiagnosis.Columns[4].DefaultCellStyle.Format = "P";
            grdSecondaryDiagnosis.Columns[4].HeaderText = "Anteil Fälle";
            grdSecondaryDiagnosis.Columns[4].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grdSecondaryDiagnosis.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grdSecondaryDiagnosis.Columns[5].DefaultCellStyle.Format = "##,###";
            grdSecondaryDiagnosis.Columns[5].HeaderText = "Anzahl Nennungen";
            grdSecondaryDiagnosis.Columns[5].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grdSecondaryDiagnosis.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grdSecondaryDiagnosis.Columns[6].DefaultCellStyle.Format = "P";
            grdSecondaryDiagnosis.Columns[6].HeaderText = "Anteil Nennungen";
            grdSecondaryDiagnosis.Columns[6].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void SetProcedureGridColumnStyle() {
            //grdProcedures.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            //grdProcedures.Columns[2].Width = 600;
            grdProcedures.Columns[2].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grdProcedures.Columns[0].Width = 50;
            grdProcedures.Columns[1].Width = 52;
            grdProcedures.Columns[2].Width = 678;
            grdProcedures.Columns[3].Width = 80;
            grdProcedures.Columns[4].Width = 80;
            grdProcedures.Columns[5].MinimumWidth = 100;
            grdProcedures.Columns[6].MinimumWidth = 100;
            grdProcedures.Columns[2].HeaderText = "OPS-Bezeichnung";
            grdProcedures.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grdProcedures.Columns[3].DefaultCellStyle.Format = "##,###";
            grdProcedures.Columns[3].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grdProcedures.Columns[3].HeaderText = "Anzahl Fälle";
            grdProcedures.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grdProcedures.Columns[4].DefaultCellStyle.Format = "P";
            grdProcedures.Columns[4].HeaderText = "Anteil Fälle";
            grdProcedures.Columns[4].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grdProcedures.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grdProcedures.Columns[5].DefaultCellStyle.Format = "##,###";
            grdProcedures.Columns[5].HeaderText = "Anzahl Nennungen";
            grdProcedures.Columns[5].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grdProcedures.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grdProcedures.Columns[6].DefaultCellStyle.Format = "P";
            grdProcedures.Columns[6].HeaderText = "Anteil Nennungen";
            grdProcedures.Columns[6].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void BuildCostMatrix() {
            var q =
                CsvData.Context()
                    .Costs.Where(pepp => pepp.Code == PEPP)
                    .Select(c => new {
                        KostenArt1 = (c.CostType1.ToString("F").Equals("0,00") ? "" : c.CostType1.ToString("F")),
                        KostenArt2 = (c.CostType2.ToString("F").Equals("0,00") ? "" : c.CostType2.ToString("F")),
                        KostenArt3a = (c.CostType3a.ToString("F").Equals("0,00") ? "" : c.CostType3a.ToString("F")),
                        KostenArt3b = (c.CostType3b.ToString("F").Equals("0,00") ? "" : c.CostType3b.ToString("F")),
                        KostenArt3c = (c.CostType3c.ToString("F").Equals("0,00") ? "" : c.CostType3c.ToString("F")),
                        KostenArt3 = (c.CostType3.ToString("F").Equals("0,00") ? "" : c.CostType3.ToString("F")),
                        KostenArt4a = (c.CostType4a.ToString("F").Equals("0,00") ? "" : c.CostType4a.ToString("F")),
                        KostenArt4b = (c.CostType4b.ToString("F").Equals("0,00") ? "" : c.CostType4b.ToString("F")),
                        KostenArt5 = (c.CostType5.ToString("F").Equals("0,00") ? "" : c.CostType5.ToString("F")),
                        KostenArt6a = (c.CostType6a.ToString("F").Equals("0,00") ? "" : c.CostType6a.ToString("F")),
                        KostenArt6b = (c.CostType6b.ToString("F").Equals("0,00") ? "" : c.CostType6b.ToString("F")),
                        KostenArt7 = (c.CostType7.ToString("F").Equals("0,00") ? "" : c.CostType7.ToString("F")),
                        KostenArt8 = (c.CostType8.ToString("F").Equals("0,00") ? "" : c.CostType8.ToString("F")),
                                     });
            grdCosts.DataSource = Helper.ConvertToDataTable(q);
            List<int> rowIds = CsvData.Context()
                .Costs.Where(pepp => pepp.Code == PEPP)
                .Select(ri => ri.CostDomain).ToList();
            BuildCostMatrixColHeaders(Color.LightGreen);
            BuildCostMatrixRowHeaders(rowIds, Color.LightGreen);
            BuildCostMatrixColSum(Color.MediumSeaGreen);
            decimal sum = 0;
            BuildCostMatrixRowSum(Color.MediumSeaGreen, ref sum);
            BuildCostMatrixMasterSum(Color.White, Color.SeaGreen, sum);
            CostMatrixRightToLeft();
            SetMatrixSortMode();
        }

        private void SetMatrixSortMode() {
            foreach (DataGridViewColumn c in grdCosts.Columns) {
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void CostMatrixRightToLeft() {
            int startCol = grdCosts.Columns["KostenArt1"].Index;
            int startRow = 1;
            for (int cols = startCol; cols < grdCosts.Columns.Count; cols++) {
                if (cols == grdCosts.Columns["rowHeaders"].Index) {
                    continue;
                }
                for (int rows = startRow; rows < grdCosts.Rows.Count; rows++) {
                    DataGridViewCellStyle style = new DataGridViewCellStyle(grdCosts.Rows[rows].Cells[cols].Style);
                    style.Alignment = DataGridViewContentAlignment.MiddleRight;
                    grdCosts.Rows[rows].Cells[cols].Style = style;
                }
            }
            startCol = grdCosts.Columns["rowSums"].Index;
            for (int i = 1; i < grdCosts.Rows.Count; i++) {
                DataGridViewCellStyle style = new DataGridViewCellStyle(grdCosts.Rows[i].Cells[startCol].Style);
                style.Alignment = DataGridViewContentAlignment.MiddleRight;
                grdCosts.Rows[i].Cells[startCol].Style = style;
            }
        }

        private void BuildCostMatrixMasterSum(Color fontColor, Color backColor, decimal sum) {
            DataGridViewCellStyle cellStyle = new DataGridViewCellStyle();
            cellStyle.ForeColor = fontColor;
            cellStyle.BackColor = backColor;
            int cellCol = grdCosts.Columns["rowSums"].Index;
            grdCosts.Rows[grdCosts.Rows.Count - 1].Cells[cellCol].Value = sum.ToString();
            grdCosts.Rows[grdCosts.Rows.Count - 1].Cells[cellCol].Style = cellStyle;
        }

        private void BuildCostMatrixRowSum(Color backColor, ref decimal sumsum) {
            DataGridViewCellStyle cellStyle = new DataGridViewCellStyle();
            cellStyle.BackColor = backColor;
            DataTable table = ((DataTable)grdCosts.DataSource);
            DataRow row = table.NewRow();
            table.Rows.Add(row);
            int sumRow = table.Rows.Count - 1;
            decimal sum = 0;
            int colStart = grdCosts.Columns["KostenArt1"].Index;
            int colEnd = grdCosts.Columns["KostenArt8"].Index;
            for (int cols = colStart; cols <= colEnd; cols++) {
                for (int rows = 1; rows < grdCosts.Rows.Count-1; rows++) {
                    if (grdCosts.Rows[rows].Cells[cols].Value.ToString() != "") {
                        sum += decimal.Parse(grdCosts.Rows[rows].Cells[cols].Value.ToString());
                    }
                }
                grdCosts.Rows[sumRow].Cells[cols].Value = sum.ToString("F");
                grdCosts.Rows[sumRow].Cells[cols].Style = cellStyle;
                sumsum += sum;
                sum = 0;
            }
            int sumCell = grdCosts.Columns["rowHeaders"].Index;
            grdCosts.Rows[sumRow].Cells[sumCell].Value = "Summe";
            grdCosts.Rows[sumRow].Cells[sumCell].Style = cellStyle;
        }

        private void BuildCostMatrixColSum(Color sumColor) {
            DataGridViewCellStyle cellStyle = new DataGridViewCellStyle();
            cellStyle.BackColor = sumColor;
            int sumCol = 1;
            if (!grdCosts.Columns.Contains("rowSums")) {
                grdCosts.Columns.Add("rowSums", "");
                sumCol = grdCosts.Columns.Count - 1;
            }
            grdCosts.Rows[0].Cells[sumCol].Value = "Summe";
            grdCosts.Rows[0].Cells[sumCol].Style = cellStyle;
            decimal sum = 0;
            int colStart = grdCosts.Columns["KostenArt1"].Index;
            int colEnd = grdCosts.Columns["KostenArt8"].Index;
            for (int rows = 1; rows < grdCosts.Rows.Count; rows++) {
                for (int cols = colStart; cols <= colEnd; cols++) {
                    if (grdCosts.Rows[rows].Cells[cols].Value.ToString() != "") {
                        sum += decimal.Parse(grdCosts.Rows[rows].Cells[cols].Value.ToString());   
                    }
                }
                grdCosts.Rows[rows].Cells[sumCol].Value = sum.ToString("F");
                grdCosts.Rows[rows].Cells[sumCol].Style = cellStyle;
                sum = 0;
            }
        }

        private void BuildCostMatrixRowHeaders(List<int> rowIds, Color headColor) {
            int colHeaderId = 0;
            if (!grdCosts.Columns.Contains("rowHeaders")) {
                grdCosts.Columns.Add("rowHeaders", "");
                colHeaderId = grdCosts.Columns.Count - 1;
                grdCosts.Columns[colHeaderId].MinimumWidth = 280;
                grdCosts.Columns[colHeaderId].DisplayIndex = 0;
            }
            var rowMap = CreateCostCenterMap();
            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            headerStyle.BackColor = headColor;
            for (int i = 0; i < grdCosts.Rows.Count - 1; i++) {
                if (i == grdCosts.Rows.Count - 2) {
                    grdCosts.Rows[i + 1].Cells[colHeaderId].Style = headerStyle;
                }
                grdCosts.Rows[i].Cells[colHeaderId].Style = headerStyle;
                grdCosts.Rows[i + 1].Cells[colHeaderId].Value = rowMap[rowIds[i]];
            }
        }

        private static Dictionary<int, string> CreateCostCenterMap() {
            Dictionary<int, string> rowMap = new Dictionary<int, string>();
            rowMap.Add(21, "21. Station - Regelbehandlung");
            rowMap.Add(22, "22. Station - Intensivbehandlung");
            rowMap.Add(23, "23. Psychotherapie");
            rowMap.Add(24, "24. Physikalische Therapie");
            rowMap.Add(25, "25. Ergotherapie");
            rowMap.Add(26, "26. Andere Therapie");
            rowMap.Add(4, "04. OP-Bereich");
            rowMap.Add(5, "05. Anästhesie");
            rowMap.Add(7, "07. Kardiologische Diagnostik / Therapie");
            rowMap.Add(8, "08. Endoskopische Diagnostik / Therapie");
            rowMap.Add(9, "09. Radiologie");
            rowMap.Add(10, "10. Laboratorien");
            rowMap.Add(11, "11. Übrige diagnostische und therapeutische Bereiche");
            return rowMap;
        }

        private void BuildCostMatrixColHeaders(Color headColor) {
            DataTable table = ((DataTable) grdCosts.DataSource);
            DataRow row = table.NewRow();
            string[] headers = {
                      "1", "2", "3a", "3b", "3c", "3",
                      "4a", "4b", "5", "6a", "6b", "7", "8"
                  };
            for (int i = 0; i < 13; i++) {
                row[i] = headers[i];
            }
            table.Rows.InsertAt(row, 0);
            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            headerStyle.BackColor = headColor;
            headerStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            for (int i = 0; i < grdCosts.Columns.Count; i++) {
                grdCosts.Rows[0].Cells[i].Style = headerStyle;
                CreateCostMatrixHeaderTooltips(i);
            }
        }

        private void CreateCostMatrixHeaderTooltips(int i) {
            if (i == grdCosts.Columns["KostenArt1"].Index) {
                grdCosts.Rows[0].Cells[i].ToolTipText = "Personalkosten Ärztlicher Dienst";
            } else if (i == grdCosts.Columns["KostenArt2"].Index) {
                grdCosts.Rows[0].Cells[i].ToolTipText = "Personalkosten Pflege-/Erziehungsdienst";
            } else if (i == grdCosts.Columns["KostenArt3a"].Index) {
                grdCosts.Rows[0].Cells[i].ToolTipText = "Personalkosten Psychologen";
            } else if (i == grdCosts.Columns["KostenArt3b"].Index) {
                grdCosts.Rows[0].Cells[i].ToolTipText = "Personalkosten Sozialarbeiter/Sozial-/Heilpädagogen";
            } else if (i == grdCosts.Columns["KostenArt3c"].Index) {
                grdCosts.Rows[0].Cells[i].ToolTipText = "Personalkosten Spezialtherapeuten";
            } else if (i == grdCosts.Columns["KostenArt3"].Index) {
                grdCosts.Rows[0].Cells[i].ToolTipText = "Personalkosten Med.-techn. Dienst/Funktionsdienst";
            } else if (i == grdCosts.Columns["KostenArt4a"].Index) {
                grdCosts.Rows[0].Cells[i].ToolTipText = "Sachkosten Arzneimittel";
            } else if (i == grdCosts.Columns["KostenArt4b"].Index) {
                grdCosts.Rows[0].Cells[i].ToolTipText = "Sachkosten Arzneimittel";
            } else if (i == grdCosts.Columns["KostenArt5"].Index) {
                grdCosts.Rows[0].Cells[i].ToolTipText = "Sachkosten Implantate/Transplantate";
            } else if (i == grdCosts.Columns["KostenArt6a"].Index) {
                grdCosts.Rows[0].Cells[i].ToolTipText = "Sachkosten Übriger medizinischer Bedarf";
            } else if (i == grdCosts.Columns["KostenArt6b"].Index) {
                grdCosts.Rows[0].Cells[i].ToolTipText = "Sachkosten Übriger medizinischer Bedarf";
            } else if (i == grdCosts.Columns["KostenArt7"].Index) {
                grdCosts.Rows[0].Cells[i].ToolTipText = "Personal- und Sachkosten med. Infrastruktur";
            } else if (i == grdCosts.Columns["KostenArt8"].Index) {
                grdCosts.Rows[0].Cells[i].ToolTipText = "Personal- und Sachkosten nicht med. Infrastruktur";
            }
        }

        private DataGridView ActiveGrid() {
            switch (tabControl.SelectedIndex) {
                case 0:
                    return grdMainDiagnosis;
                case 1:
                    return grdSecondaryDiagnosis;
                case 2:
                    return grdProcedures;
                case 3:
                    return grdCosts;
            }
            return null;
        } 

        private void FillHeadData() {
            var q = CsvData.Context().SystemInfo.Where(pepp => pepp.Code == PEPP);
            peppData.CasesNumSummary = q.Select(p => p.CaseCount.ToString("##,###")).ElementAt(0);
            peppData.DaysSummary = q.Select(p => p.LosSumDays.ToString("##,###")).ElementAt(0);
            peppData.LosAverage = q.Select(p => Math.Round(p.LosAverage, 1).ToString()).ElementAt(0);               // einstellig Dezimal
            peppData.LosStandardDeviation = q.Select(p => Math.Round(p.LosStandard, 1).ToString()).ElementAt(0);    // einstellig Dezimal
            peppData.LosHomogeneityCoeff = q.Select(p => Math.Round((p.LosHc*100),2).ToString()+"%").ElementAt(0);      // Prozent
            peppData.GenderMale = q.Select(p => Math.Round((p.GenderMale * 100), 2).ToString()+"%").ElementAt(0);         // Prozent
            peppData.GenderFemale = q.Select(p => Math.Round((p.GenderFemale * 100), 2).ToString()+"%").ElementAt(0);     // Prozent
            peppData.AgeAverage = q.Select(p => Math.Round(p.AgeAverage, 1).ToString()).ElementAt(0);               // einstellig Dezimal
            peppData.AgeStandardDeviation = q.Select(p => Math.Round(p.AgeStandard,1).ToString()).ElementAt(0);     // einstellig Dezimal
            peppData.LT28Days = q.Select(p => Math.Round((p.AgeBelow28Days * 100), 2).ToString()+"%").ElementAt(0);        // Prozent
            peppData.Bt28Days1Year = q.Select(p => Math.Round((p.AgeBelow1Year * 100), 2).ToString()+"%").ElementAt(0);    // Prozent
            peppData.Bt1Year2 = q.Select(p => Math.Round((p.AgeBelow3Years * 100), 2).ToString()+"%").ElementAt(0);        // Prozent
            peppData.Bt3Year5 = q.Select(p => Math.Round((p.AgeBelow6Years * 100), 2).ToString()+"%").ElementAt(0);        // Prozent
            peppData.Bt6Year9 = q.Select(p => Math.Round((p.AgeBelow10Years * 100), 2).ToString()+"%").ElementAt(0);       // Prozent
            peppData.Bt10Year15 = q.Select(p => Math.Round((p.AgeBelow16Years * 100), 2).ToString()+"%").ElementAt(0);     // Prozent
            peppData.Bt16Year17 = q.Select(p => Math.Round((p.AgeBelow18Years * 100), 2).ToString()+"%").ElementAt(0);     // Prozent
            peppData.Bt18Year29 = q.Select(p => Math.Round((p.AgeBelow30Years * 100), 2).ToString()+"%").ElementAt(0);     // Prozent
            peppData.Bt30Year39 = q.Select(p => Math.Round((p.AgeBelow40Years * 100), 2).ToString()+"%").ElementAt(0);     // Prozent
            peppData.Bt40Year49 = q.Select(p => Math.Round((p.AgeBelow50Years * 100), 2).ToString()+"%").ElementAt(0);     // Prozent
            peppData.Bt50Year54 = q.Select(p => Math.Round((p.AgeBelow55Years * 100), 2).ToString()+"%").ElementAt(0);     // Prozent
            peppData.Bt55Year59 = q.Select(p => Math.Round((p.AgeBelow60Years * 100), 2).ToString()+"%").ElementAt(0);     // Prozent
            peppData.Bt60Year64 = q.Select(p => Math.Round((p.AgeBelow65Years * 100), 2).ToString()+"%").ElementAt(0);     // Prozent
            peppData.Bt65Year74 = q.Select(p => Math.Round((p.AgeBelow75Years * 100), 2).ToString()+"%").ElementAt(0);     // Prozent
            peppData.Bt75Year79 = q.Select(p => Math.Round((p.AgeBelow80Years * 100), 2).ToString()+"%").ElementAt(0);     // Prozent
            peppData.Gt79Year = q.Select(p => Math.Round((p.AgeBelow99Years * 100), 2).ToString()+"%").ElementAt(0);       // Prozent
            peppData.DailyCostsAverage = q.Select(p => Math.Round(p.DayCostsAverage, 2).ToString()).ElementAt(0);   // zweistellig Dezimal (money)
            peppData.DailyCostsStandardDeviation = q.Select(p => Math.Round(p.DayCostsStandard, 2).ToString()).ElementAt(0);    // zweistellig Dezimal (money)
            peppData.DailyCostsHomogeneityCoeff = q.Select(p => Math.Round((p.DayCostsHc * 100), 2).ToString()+"%").ElementAt(0);  // Prozent
            peppData.Degression = q.Select(p => p.Degression).Single().ToString();
            peppData.TitlePEPP = "Kennzahlen - " + PEPP;
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e) {
            FillActiveTab(ActiveGrid());
        }


        private void beendenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void druckenToolStripMenuItem_Click(object sender, EventArgs e) {
            if (string.IsNullOrEmpty(PEPP)) {
                MessageBox.Show("Keine PEPP gewählt. Druck nicht möglich!");
                return;
            }
            if (MessageBox.Show(this, "Wollen Sie die PEPP " + PEPP + " jetzt ausdrucken?\n\n" +
                                      "Hinweis: Es wird die komplette PEPP ausgedruckt. Um einzelne Seiten auszudrucken, " +
                                      "benutzen Sie bitte den PDF-Export und einen geeigneten PDF-Reader (z.B. Adobe Acrobat Reader)",
                                      "Drucken - " + PEPP, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK) {
                CreateReport(OutputType.Print);   
            }
        }

        private void CreateReport(OutputType outputType) {
                Reporter reporter = new Reporter();
                //helpProvider1.SetHelpKeyword(this, "Drucken.htm");
                //timerPrintWindow.Start();
                reporter.Perform(LlProject.List, LlAutoMasterMode.AsVariables, outputType, "peppDruck.lst",
                                 setReportData(PEPP), "data");
        }

        private void pDFExportToolStripMenuItem_Click(object sender, EventArgs e) {
            if (string.IsNullOrEmpty(PEPP)) {
                MessageBox.Show("Keine PEPP gewählt. PDF-Export nicht möglich!");
                return;
            }
            CreateReport(OutputType.PDF);
        }

        private void designerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(PEPP)){
                MessageBox.Show("Keine PEPP gewählt. Druck nicht möglich!");
            }
            else{
                Reporter reporter = new Reporter();
                reporter.Perform(LlProject.List, LlAutoMasterMode.AsVariables, OutputType.Design, "peppDruck", setReportData(PEPP), "data");    
            }
            
        }


        private void grdMainDiagnosis_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (grdMainDiagnosis.SelectedRows.Count > 0) {
                SetRechercheHelp();
                string hd = grdMainDiagnosis.SelectedRows[0].Cells[1].Value.ToString();
                List<string> pepps =
                    CsvData.Context()
                        .PrimaryDiagnoses
                        .Where(pepp => pepp.DiagCode == hd)
                        .Select(pepp => pepp.SystemCode)
                        .ToList();
                var q = CsvData.Context().PrimaryDiagnoses.Where(pd => pepps.Contains(pd.SystemCode) && pd.DiagCode == hd)
                    .Select(pd => new {PEPP = pd.SystemCode, HD = pd.DiagCode, AnzahlFälle = pd.Count, AnteilFälle = pd.Fraction});
                search.StartPosition = FormStartPosition.CenterParent;
                search.Text = "PEPPs zu Hauptdiagnosen";
                search.ButtonShowIsVisible = false;
                search.SetDataSource(q);
                search.ColumnTextAlign(2, DataGridViewContentAlignment.MiddleRight);
                search.ColumnFormat("##,##0", 2);
                search.ColumnTextAlign(3, DataGridViewContentAlignment.MiddleRight);
                search.ColumnFormat("P", 3);
                if (search.ShowDialog() == DialogResult.OK) {
                    LoadPeppByTabControl(search);
                }
            }
        }

        private void grdSecondaryDiagnosis_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (grdSecondaryDiagnosis.SelectedRows.Count > 0) {
                SetRechercheHelp();
                string sd = grdSecondaryDiagnosis.SelectedRows[0].Cells[1].Value.ToString();
                List<string> pepps =
                    CsvData.Context()
                        .SecondaryDiagnoses
                        .Where(pepp => pepp.DiagCode == sd)
                        .Select(pepp => pepp.System)
                        .ToList();
                var q = CsvData.Context().SecondaryDiagnoses.Where(pd => pepps.Contains(pd.System) && pd.DiagCode == sd)
                    .Select(nd => new {
                        PEPP = nd.System,
                        ND = nd.DiagCode,
                        AnzahlFälle = nd.CaseCount,
                        AnteilFälle = nd.CaseFraction,
                        AnzahlNennungen = nd.EntryCount,
                        AnteilNennungen = nd.EntryFraction
                                      });
                search.StartPosition = FormStartPosition.CenterParent;
                search.Text = "PEPPs zu Nebendiagnosen";
                search.ButtonShowIsVisible = false;
                search.SetDataSource(q);
                search.ColumnTextAlign(2, DataGridViewContentAlignment.MiddleRight);
                search.ColumnFormat("##,##0", 2);
                search.ColumnTextAlign(3, DataGridViewContentAlignment.MiddleRight);
                search.ColumnFormat("P", 3);
                search.ColumnTextAlign(4, DataGridViewContentAlignment.MiddleRight);
                search.ColumnFormat("##,##0", 4);
                search.ColumnTextAlign(5, DataGridViewContentAlignment.MiddleRight);
                search.ColumnFormat("P", 5);
                if (search.ShowDialog() == DialogResult.OK) {
                    LoadPeppByTabControl(search);
                }
            }
        }

        private void grdProcedures_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (grdProcedures.SelectedRows.Count > 0) {
                SetRechercheHelp();
                string proc = grdProcedures.SelectedRows[0].Cells[1].Value.ToString();
                List<string> pepps =
                    CsvData.Context()
                        .Procedures
                        .Where(pepp => pepp.ProcCode == proc)
                        .Select(pepp => pepp.System)
                        .ToList();
                var q = CsvData.Context().Procedures.Where(pd => pepps.Contains(pd.System) && pd.ProcCode == proc)
                    .Select(p => new {
                        PEPP = p.System,
                        Prozedur = p.ProcCode,
                        AnzahlFälle = p.CaseCount,
                        AnteilFälle = p.CaseFraction,
                        AnzahlNennungen = p.EntryCount,
                        AnteilNennungen = p.EntryFraction
                                     });
                search.StartPosition = FormStartPosition.CenterParent;
                search.Text = "PEPPs zu Prozeduren";
                search.ButtonShowIsVisible = false;
                search.SetDataSource(q);
                search.ColumnTextAlign(2, DataGridViewContentAlignment.MiddleRight);
                search.ColumnFormat("##,##0", 2);
                search.ColumnTextAlign(3, DataGridViewContentAlignment.MiddleRight);
                search.ColumnFormat("P", 3);
                search.ColumnTextAlign(4, DataGridViewContentAlignment.MiddleRight);
                search.ColumnFormat("##,##0", 4);
                search.ColumnTextAlign(5, DataGridViewContentAlignment.MiddleRight);
                search.ColumnFormat("P", 5);
                if (search.ShowDialog() == DialogResult.OK) {
                    LoadPeppByTabControl(search);
                }
            }
        }

        private List<Data.SystemData> setReportData(string pepp)
        {
            SystemInfo info = CsvData.Context().SystemInfo.Single(p => p.Code == pepp);
            string skTag = CsvData.Context().System.Where(p => p.Code == PEPP).Select(p => p.Category).Single();
            info.StruCat = CsvData.Context().StructureCategories.Where(sk => sk.Category == skTag).Select(sk => sk.Text).Single();
            info.PeppTxt = cbxPepp.Text;
            var peppData = new InekBrowser.Data.SystemData(info);
            //Primary Diagnoses
            peppData.PrimDiag = CsvData.Context().PrimaryDiagnoses.Where(p => p.SystemCode == pepp)
                .Join(CsvData.Context().Recherche.Where(r => r.PrimaryDiagnosis == 1), d => d.DiagCode, r => r.Code,
                            (d, r) => new PrimaryDiagnosis() {
                                SystemCode = d.SystemCode,
                                DiagCode = d.DiagCode,
                                Hauptdiagnose = r.Text,
                                Count = d.Count,
                                Fraction = d.Fraction
                            }).ToList();
            //Secondary Diagnoses
            peppData.SecDiag = CsvData.Context().SecondaryDiagnoses.Where(p => p.System == pepp)
                .Join(CsvData.Context().Recherche.Where(r => r.SecondaryDiagnosis == 1), d => d.DiagCode, r => r.Code,
                            (d, r) => new SecondaryDiagnosis(){
                                System = d.System,
                                DiagCode = d.DiagCode,
                                Nebendiagnose = r.Text,
                                CaseCount = d.CaseCount,
                                CaseFraction = d.CaseFraction,
                                EntryCount = d.EntryCount,
                                EntryFraction = d.EntryFraction
                            }).ToList();
            //Procedures
            peppData.Proc = CsvData.Context().Procedures.Where(p => p.System == pepp)
                 .Join(CsvData.Context().Recherche.Where(r => r.Procedure == 1), d => d.ProcCode, r => r.Code,
                            (d, r) => new Procedure() {
                                System = d.System,
                                ProcCode = d.ProcCode,
                                Prozedur = r.Text,
                                CaseCount = d.CaseCount,
                                CaseFraction = d.CaseFraction,
                                EntryCount = d.EntryCount,
                                EntryFraction = d.EntryFraction
                            }).ToList();
            peppData.Cost = CsvData.Context().Costs.Where(p => p.Code== pepp)
                .Join(CsvData.Context().CostDomains, c => c.CostDomain, d => d.DomainId,
                (c, d) => new Cost()
                              {
                                  Code = c.Code,
                                  CostDomain = c.CostDomain,
                                  CostType1 = c.CostType1,
                                  CostType2 = c.CostType2,
                                  CostType3 = c.CostType3,
                                  CostType3a = c.CostType3a,
                                  CostType3b = c.CostType3b,
                                  CostType3c = c.CostType3c,
                                  CostType4a = c.CostType4a,
                                  CostType4b = c.CostType4b,
                                  CostType5 = c.CostType5,
                                  CostType6a = c.CostType6a,
                                  CostType6b = c.CostType6b,
                                  CostType7 = c.CostType7,
                                  CostType8 = c.CostType8,
                                  TxtBez = d.DomainText
                              }).ToList();
            var dataSet = new List<Data.SystemData> {peppData};

            return dataSet;
        }


        private void LoadPeppByTabControl(FrmSearch search) {
            selectionPepp.ClearSelection();
            PEPP = search.Id.ToString();
            cbxPepp.Text = PEPP + ": ";
            cbxPepp.Text += CsvData.Context().System.Where(p => p.Code == PEPP).Select(p => p.Text).Single();
            LoadPeppData();
        }

        private void mnuInfo_Click(object sender, EventArgs e) {
            var about = new FrmAbout();
            about.ShowDialog(this);
        }

        private void FrmPeppBrowser_Resize(object sender, EventArgs e) {
            if (WindowState == FormWindowState.Maximized) {
                titleBar.MinMaxImage = (Image)resources.GetObject("peppo_minimize");
            } else {
                titleBar.MinMaxImage = (Image) resources.GetObject("picMinMax.BackgroundImage");
            }
        }

        private int ticks = 0;
        private void tabControl_Click(object sender, EventArgs e) {
            if (PEPP == null) {
                ticks = 0;
                timerPeppBlink.Start();
            }
        }

        private void timerPeppBlink_Tick(object sender, EventArgs e) {
            if (ticks%2 == 0) {
                cbxPepp.BackColor = Color.Red;
            } else {
                cbxPepp.BackColor = Color.White;
            }
            if (ticks == 5) {
                timerPeppBlink.Stop();
            }
            ticks++;
        }

        private void grdCosts_CellClick(object sender, DataGridViewCellEventArgs e) {
            grdCosts.SelectionMode = DataGridViewSelectionMode.CellSelect;
            if (e.RowIndex == 0) {
                grdCosts.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
                grdCosts.Columns[e.ColumnIndex].Selected = true;
            } else {
                grdCosts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                grdCosts.Rows[e.RowIndex].Selected = true;
            }
        }

        private void timerPrintWindow_Tick(object sender, EventArgs e) {
            IntPtr previewHandle = FindWindowByCaption(IntPtr.Zero, "Preview");
            if (previewHandle == IntPtr.Zero) {
                helpProvider1.SetHelpKeyword(this, "Introduction.htm");
                timerPrintWindow.Stop();
            }
        }

        private void mnuDataDir_Click(object sender, EventArgs e) {
            Process.Start("explorer.exe", Application.StartupPath + "\\" + ResourceController.RESOURCE_DIR + Program.Year);
        }

        private void peppData_ClickedCatalogue(object sender, EventArgs e) {
            FrmSearch popup = new FrmSearch(this);
            popup.StartPosition = FormStartPosition.CenterParent;
            popup.Text = "Katalog";
            popup.ButtonShowIsVisible = false;
            var q = CsvData.Context().Catalogs.Where(c => c.Pepp == PEPP).Select(c => new {PEPP = c.Pepp, Vergütungsklasse = c.RemunerationClass, Relativgewicht = c.RelativeWeight});
            popup.SetDataSource(q);
            if (popup.ShowDialog(this) == DialogResult.OK) {
                
            }
        }

        private void cbxPepp_Load(object sender, EventArgs e) {

        }

    }
}
