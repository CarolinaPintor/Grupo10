public class Notification
{
    // Identificador único de la notificación.
    public Guid Id;


    // Identificador del usuario destinatario.
    public Guid UsuarioId;
    public string Mensaje;

    // Tipo de notificación: Email | Push | SMS.
    public string Tipo;

    // Estado de la notificación: Pendiente | Enviada | Fallida.
    public string Estado;

    // Fecha y hora de envío (Asignado automáticamente al registrar/enviar).

    public DateTime FechaEnvio;

    public Notification()
    {
        Id = Guid.NewGuid();
        Estado = "Pendiente";
        FechaEnvio = DateTime.UtcNow;
    }

    public Notification(Guid usuarioId, string mensaje, string tipo, string estado = "Enviada")
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Mensaje = mensaje;
        Tipo = tipo;
        Estado = estado;
        FechaEnvio = DateTime.UtcNow;
    }
}