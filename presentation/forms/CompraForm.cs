using tallerc.domain.entities;
using System.Windows.Forms;


private void btnGuardarCompra_Click(object sender, EventArgs e)
{
    var compra = new Compra
    {
        ProveedorId = (int)cmbProveedor.SelectedValue,
        Productos = listaProductosSeleccionados,
        // ... otros campos
    };

    var (success, message) = _compraService.RegistrarCompra(compra);
    MessageBox.Show(message);
}