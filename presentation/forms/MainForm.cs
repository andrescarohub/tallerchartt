namespace tallerc.presentation.forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            // Configura botones o menús aquí
        }

        private void InitializeComponent()
        {
            throw new NotImplementedException();
        }

        // Ejemplo: Botón para abrir el formulario de Terceros
        private void btnTerceros_Click(object sender, EventArgs e)
        {
            var form = new TercerosForm();
            form.ShowDialog();
        }

        // Botón para abrir el formulario de Caja
        private void btnCaja_Click(object sender, EventArgs e)
        {
            var form = new CajaForm();
            form.ShowDialog();
        }
        // ... (métodos similares para otros módulos)
    }

    public class Form
    {
    }

    internal class CajaForm
    {
        public CajaForm()
        {
        }

        internal void ShowDialog()
        {
            throw new NotImplementedException();
        }
    }

    internal class TercerosForm
    {
        public TercerosForm()
        {
        }

        internal void ShowDialog()
        {
            throw new NotImplementedException();
        }
    }
}