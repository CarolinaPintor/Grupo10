using System.ComponentModel.DataAnnotations;

namespace MiniApi.DTOs;
// Request para el registro de un nuevo usuario (POST /api/users/register).
// Acá llegan los datos limpios que ingresa el usuario en el formulario.

public record RegisterUserRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    string Nombre,

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
    string Apellido,

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    string Email,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    string Password
);

// Request para la autenticación de un usuario (POST /api/users/login).
public record LoginRequest(
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    string Email,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    string Password
);

// Respuesta devuelta tras un registro exitoso (201 Created).
public record RegisterResponse(
    Guid Id,
    string Nombre,
    string Apellido,
    string Email,
    DateTime FechaRegistro,
    bool Activo
);
// Respuesta devuelta tras un login exitoso (200 OK).
public record LoginResponse(
    Guid Id,
    string Nombre,
    string Apellido,
    string Email
);
