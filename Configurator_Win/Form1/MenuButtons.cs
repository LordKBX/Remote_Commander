using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Configurator_Win
{
    public partial class Form1 : Form
    {
        private void enregistrerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.File.WriteAllText(Program.configDirectory + "\\grids.json", JsonConvert.SerializeObject(GridsList));
                MessageBox.Show("Le Fichier des Grilles à été enregistré avec succès", "Information");
            }
            catch (Exception error) { MessageBox.Show("Le Fichier des Grilles na pas été enregistré Correctement ou à complètement échoué", "Erreur"); }
        }

        private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void gestionImagestoolStripMenuItem2_Click(object sender, EventArgs e)
        {
            ImagesManager.ImagesManager images = new ImagesManager.ImagesManager();
            images.StartPosition = FormStartPosition.CenterScreen;
            images.ShowDialog();
        }

        private void gestionSondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SoundsManager.SoundsManager sndm = new SoundsManager.SoundsManager();
            sndm.StartPosition = FormStartPosition.CenterScreen;
            sndm.ShowDialog();
        }

        private void gestionMacrosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MacrosManager.MacrosManager mc = new MacrosManager.MacrosManager();
            mc.StartPosition = FormStartPosition.CenterScreen;
            mc.ShowDialog();
        }



        private void BtnGestionImages_Click(object sender, EventArgs e)
        {

        }

        private void BtnGestionSonds_Click(object sender, EventArgs e)
        {

        }

        private void BtnGestionMacros_Click(object sender, EventArgs e)
        {

        }
    }
}
