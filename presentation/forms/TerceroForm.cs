using System;
using System.Windows.Forms;

namespace tallerc.presentation.forms
{
    public partial class TercerosForm : Form
    {
        public TercerosForm()
        {
            InitializeComponent();
            this.Text = "Gestión de Terceros";
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Tercero guardado:\nNombre: {txtNombre.Text}\nTipo: {cmbTipo.Text}");
        }
    }
}