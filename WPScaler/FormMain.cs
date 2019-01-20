using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace WPScaler
{
    public partial class FormMain : Form
    {
        private const string UPDATE_URL = "";

        private static Properties.Settings settings = Properties.Settings.Default;

        private string partsbndPath
        {
            get => toolStripStatusLabel1.Text;
            set => toolStripStatusLabel1.Text = value;
        }

        private BND4 partsbnd;
        private Dictionary<string, FLVER> flvers;

        public FormMain()
        {
            InitializeComponent();
            dgvModels.AutoGenerateColumns = false;
            dgvBones.AutoGenerateColumns = false;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Text = "WPScaler " + Application.ProductVersion;
            Location = settings.WindowLocation;
            if (settings.WindowSize.Width >= MinimumSize.Width && settings.WindowSize.Height >= MinimumSize.Height)
                Size = settings.WindowSize;
            if (settings.WindowMaximized)
                WindowState = FormWindowState.Maximized;

            splitContainer1.SplitterDistance = settings.SplitterDistance;
            partsbndPath = settings.PartsbndPath;
            LoadPartsbnd(partsbndPath, true);
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            settings.WindowMaximized = WindowState == FormWindowState.Maximized;
            if (WindowState == FormWindowState.Normal)
            {
                settings.WindowLocation = Location;
                settings.WindowSize = Size;
            }
            else
            {
                settings.WindowLocation = RestoreBounds.Location;
                settings.WindowSize = RestoreBounds.Size;
            }

            settings.SplitterDistance = splitContainer1.SplitterDistance;
            settings.PartsbndPath = partsbndPath;
        }

        private void FormMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void FormMain_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                LoadPartsbnd(files[0]);
            }
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(UPDATE_URL);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadPartsbnd(openFileDialog1.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (BND4.File file in partsbnd.Files.Where(f => f.Name.EndsWith(".flver")))
                {
                    file.Bytes = flvers[Path.GetFileName(file.Name)].Write();
                }

                if (!File.Exists(partsbndPath + ".bak"))
                    File.Copy(partsbndPath, partsbndPath + ".bak");

                partsbnd.Write(partsbndPath);
                SystemSounds.Asterisk.Play();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to save partsbnd:\r\n{partsbndPath}\r\n\r\n{ex}");
            }
        }

        private void restoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!File.Exists(partsbndPath + ".bak"))
            {
                ShowError($"Backup not found:\r\n{partsbndPath}.bak");
                return;
            }

            try
            {
                File.Copy(partsbndPath + ".bak", partsbndPath, true);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to restore backup:\r\n{partsbndPath}.bak\r\n\r\n{ex}");
            }

            LoadPartsbnd(partsbndPath);
            SystemSounds.Asterisk.Play();
        }

        private void dgvModels_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvModels.SelectedCells.Count == 0)
            {
                dgvBones.DataSource = null;
            }
            else
            {
                DataGridViewCell cell = dgvModels.SelectedCells[0];
                dgvBones.DataSource = flvers[(string)cell.Value].Bones.Select(bone => new BoneWrapper(bone)).ToList();
            }
        }

        private void LoadPartsbnd(string path, bool silent = false)
        {
            BND4 bnd = null;
            Dictionary<string, FLVER> flvers = null;
            try
            {
                bnd = BND4.Read(path);
                flvers = new Dictionary<string, FLVER>();
                foreach (BND4.File file in bnd.Files.Where(f => f.Name.EndsWith(".flver")))
                {
                    FLVER flv = FLVER.Read(file.Bytes);
                    flvers[Path.GetFileName(file.Name)] = flv;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load partsbnd:\r\n{path}\r\n\r\n{ex}", silent);
            }

            partsbnd = bnd;
            this.flvers = flvers;
            partsbndPath = path;
            dgvModels.Rows.Clear();
            foreach (string flver in flvers.Keys)
                dgvModels.Rows.Add(flver);
            if (dgvModels.Rows.Count > 0)
                dgvModels.Rows[0].Selected = true;
        }

        private void ShowError(string message, bool silent = false)
        {
            if (!silent)
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
