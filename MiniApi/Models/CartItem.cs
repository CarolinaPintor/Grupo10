public class CartItem
{

    public Guid ProductoId;

    public int Cantidad;
    //Constructor por defecto
    public CartItem()
    {
    }

    /// Constructor parametrizado para instanciar items en el carrito.
    public CartItem(Guid productoId, int cantidad)
    {
        ProductoId = productoId;
        Cantidad = cantidad;
    }
}