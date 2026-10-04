public class Cart
{ 
    public Guid UsuarioId;
    public CartItem[] Items;
    public DateTime FechaActualizacion;

    public Cart(Guid UsuarioId, CartItem[] Items, DateTime FechaActualizacion)
    {
        this.UsuarioId = UsuarioId;
        this.Items = Items;
        this.FechaActualizacion = FechaActualizacion;
    }
}