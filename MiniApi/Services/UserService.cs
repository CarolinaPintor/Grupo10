using System.ComponentModel.DataAnnotations;
using MiniApi.Common;
using MiniApi.Data;
using MiniApi.DTOs;
using MiniApi.Exceptions;
using MiniApi.Models;

namespace MiniApi.Services;

public class UserService
{
    private readonly UserRepository _userRepository;

    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterUserRequest request)
    {
        // Validar que el email tenga un formato básico correcto
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
        {
            // Corrección: Usamos una List<string> y la constante ErrorCodes.USR_002
            var errores = new List<string> { "El formato del correo electrónico es inválido." };
            throw new MiniApi.Exceptions.ValidationException(ErrorCodes.USR_002, errores);
        }

        //  Validar formato de datos (USR-002)
        List<ValidationResult> resultadosValidacion = new List<ValidationResult>();
        ValidationContext contextoValidacion = new ValidationContext(request);
        bool esValido = Validator.TryValidateObject(request, contextoValidacion, resultadosValidacion, true);

        if (!esValido)
        {
            IEnumerable<string> errores = resultadosValidacion.Select(r => r.ErrorMessage ?? "Dato inválido");
            throw new MiniApi.Exceptions.ValidationException(ErrorCodes.USR_002, errores);
        }

        // Validar la regla de email duplicado (USR-001)
        User? usuarioExistente = await _userRepository.GetByEmailAsync(request.Email);
        if (usuarioExistente != null)
        {
            throw new BusinessRuleException(ErrorCodes.USR_001, "El email ya está registrado.");
        }

        // Hashear la contraseña 
        string contrasenaHasheada = PasswordHasher.Hash(request.Password);

        // Creo la entidad User 
        User nuevoUsuario = new User
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            PasswordHash = contrasenaHasheada,
            FechaRegistro = DateTime.UtcNow,
            Activo = true,
            IntentosFallidos = 0
        };

        // Guardar el nuevo usuario en la base de datos
        await _userRepository.CreateAsync(nuevoUsuario);

        // Retornar el DTO seguro (sin la contraseña)
        return new RegisterResponse(
            nuevoUsuario.Id,
            nuevoUsuario.Nombre,
            nuevoUsuario.Apellido,
            nuevoUsuario.Email,
            nuevoUsuario.FechaRegistro,
            nuevoUsuario.Activo
        );
    }
}