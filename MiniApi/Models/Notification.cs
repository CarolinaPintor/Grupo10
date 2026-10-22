namespace MiniApi.Models;

public class Notification
{
    // Identificador único de la notificación.
    public Guid Id { get; set; }


    // Identificador del usuario destinatario.
    public Guid UsuarioId { get; set; }
    public string Mensaje { get; set; } = string.Empty;

    // Tipo de notificación: Email | Push | SMS.
    public string Tipo { get; set; } = string.Empty;

    // Estado de la notificación: Pendiente | Enviada | Fallida.
    public string Estado { get; set; } = string.Empty;

    // Fecha y hora de envío (Asignado automáticamente al registrar/enviar).
    public DateTime FechaEnvio { get; set; }

    // Constructor sin parámetros requerido por Dapper
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