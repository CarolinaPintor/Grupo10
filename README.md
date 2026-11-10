# **Grupo 10 — E-Commerce REST API (.NET 10\)**

Trabajo Práctico de **Construcción de Aplicaciones Informáticas — Facultad de Ciencias Económicas, Universidad de Buenos Aires**.

Sistema de E-Commerce desarrollado en C\# con ASP.NET Core Minimal APIs sobre .NET 10\. La aplicación incluye los dominios de Products, Users, Orders, Cart y Notifications, persistencia mediante SQLite y Dapper, manejo centralizado de excepciones, documentación interactiva con Swagger/OpenAPI, logging estructurado con Serilog, trazabilidad mediante Correlation ID, Health Checks e integración continua con GitHub Actions.

> **Decisión de arquitectura:** el enunciado plantea cinco microservicios independientes que se comunican mediante HTTP. El equipo implementó una única API modular (`MiniApi`), organizada internamente en los cinco dominios requeridos. Cada módulo cuenta con sus propios endpoints, servicios, repositorios y DTOs. Las interacciones entre dominios se resuelven dentro del mismo proceso mediante inyección de dependencias, sin comunicación HTTP entre servicios.

> Esta decisión constituye una diferencia respecto de la arquitectura distribuida planteada originalmente en la consigna y debe considerarse al evaluar el alcance de la implementación.

## **Contenido**

1. Arquitectura del sistema  
2. Estructura del repositorio  
3. Requisitos e instrucciones de ejecución  
4. Puertos y URLs  
5. Servicios y endpoints  
6. Contrato y catálogo de errores  
7. Reglas de negocio y seguridad  
8. Observabilidad  
9. Evidencias de Swagger  
10. Decisiones de diseño  
11. Tecnologías utilizadas  
12. Integrantes

---

## **1\. Arquitectura del sistema**

La aplicación está organizada en capas para separar la recepción de solicitudes HTTP, la lógica de negocio, el acceso a datos y los mecanismos transversales.

```
                    CLIENTE HTTP
              Swagger / Postman / REST Client
                         |
                         v
                  MiniApi (.NET 10)
                         |
          +--------------+--------------+
          |                             |
          v                             v
   Middleware y observabilidad    Manejo de excepciones
   - Correlation ID               - IExceptionHandler
   - Request logging              - ErrorResponseWriter
   - Auditoría                    - ProblemDetails
          |
          v
                 ENDPOINTS (Minimal APIs)
                         |
       +-----------------+------------------+
       |         |        |        |         |
       v         v        v        v         v
   Products    Users    Orders    Cart   Notifications
       |         |        |        |         |
       v         v        v        v         v
   Product    User     Order     Cart   Notification
   Service    Service  Service   Service  Service
       |         |        |        |         |
       +---------+--------+--------+---------+
                         |
                         v
                  REPOSITORIOS
                    (Dapper)
                         |
                         v
                   SQLite (app.db)
```

Los cinco dominios son:

* **Products:** administración y consulta de productos.  
* **Users:** registro, autenticación y control de intentos fallidos.  
* **Orders:** creación, consulta y actualización de órdenes.  
* **Cart:** administración de los carritos de compra.  
* **Notifications:** registro y consulta de notificaciones asociadas a usuarios.

### **Interacción entre dominios**

Algunos servicios necesitan consultar información perteneciente a otros dominios. Como todos funcionan dentro de la misma aplicación, estas interacciones se realizan mediante dependencias inyectadas.

| Dominio | Dependencia | Finalidad |
| ----- | ----- | ----- |
| Cart | ProductService | Verificar la existencia de productos y disponibilidad de stock. |
| Orders | UserRepository y ProductRepository | Validar usuarios, productos y stock al crear órdenes. |
| Notifications | UserRepository | Comprobar que el usuario destinatario exista. |
| Products | OrderRepository | Evitar eliminar productos asociados a órdenes activas. |

## **2\. Estructura del repositorio**

La estructura principal del proyecto es la siguiente:

```
Grupo10/
├── MiniApi.slnx
├── README.md
├── .github/
│   └── workflows/
│       └── dotnet-build.yml
└── MiniApi/
    ├── Program.cs
    ├── MiniApi.csproj
    ├── appsettings.json
    ├── MiniApi.http
    ├── docs/
    │   └── swagger/
    ├── Common/
    ├── Data/
    ├── Models/
    ├── DTOs/
    ├── Services/
    ├── Exceptions/
    ├── ExceptionHandlers/
    ├── Middleware/
    ├── HealthChecks/
    ├── Extensions/
    │   └── Endpoints/
    └── logs/
```

Responsabilidad de las principales carpetas:

| Carpeta | Responsabilidad |
| ----- | ----- |
| `Common` | Clases compartidas, como PasswordHasher y filtros de documentación Swagger. |
| `Data` | Inicialización de SQLite, configuración de mapeo y repositorios de acceso a datos. |
| `Models` | Entidades que representan los objetos del dominio. |
| `DTOs` | Objetos utilizados para recibir solicitudes y construir respuestas HTTP. |
| `Services` | Lógica de negocio y validaciones de cada dominio. |
| `Exceptions` | Excepciones personalizadas y catálogo de códigos de error. |
| `ExceptionHandlers` | Manejo centralizado de excepciones mediante `IExceptionHandler`. |
| `Middleware` | Trazabilidad, contexto de logging y auditoría de solicitudes. |
| `HealthChecks` | Comprobaciones del estado de la aplicación y de SQLite. |
| `Extensions` | Métodos de extensión para registrar servicios, middleware y endpoints. |
| `logs` | Archivos generados por Serilog durante la ejecución. |

## **3\. Requisitos e instrucciones de ejecución**

### **Requisitos previos**

* .NET 10 SDK.  
* Git para clonar el repositorio.  
* No se necesita instalar un motor de base de datos externo: el proyecto utiliza SQLite.

### **Clonar el repositorio**

```
git clone https://github.com/CarolinaPintor/Grupo10.git
cd Grupo10/MiniApi
```

### **Compilar y ejecutar**

```
dotnet build ../MiniApi.slnx
dotnet run --launch-profile http
```

También se puede ejecutar con `dotnet run`, utilizando el perfil predeterminado configurado en el proyecto.

### **Inicialización de la base de datos**

La aplicación utiliza SQLite y el archivo `app.db` para persistir la información.

Al iniciar la API, `DatabaseInitializer` ejecuta las sentencias de creación de tablas mediante `CREATE TABLE IF NOT EXISTS`. De esta manera, las tablas se crean si todavía no existen, sin volver a generarlas en cada ejecución.

Entre las tablas utilizadas se encuentran:

* `products`  
* `users`  
* `orders`  
* `order_items`  
* `cart`  
* `cart_items`  
* `notifications`

Para comenzar con una base de datos vacía, se debe detener la aplicación y eliminar el archivo `app.db`. Al volver a ejecutarla, se inicializará nuevamente.

## **4\. Puertos y URLs**

Todos los dominios se encuentran expuestos desde la misma aplicación y comparten el host y los puertos configurados.

Los puertos están definidos en `MiniApi/Properties/launchSettings.json`:

| Perfil | Dirección | Puerto |
| ----- | ----- | ----- |
| HTTP | `http://localhost:5112` | 5112 |
| HTTPS | `https://localhost:7048` | 7048 |

El perfil HTTPS también habilita el acceso mediante HTTP en el puerto 5112\.

### **Rutas generales**

| Ruta | Descripción |
| ----- | ----- |
| `http://localhost:5112/swagger` | Documentación interactiva de la API mediante HTTP. |
| `https://localhost:7048/swagger` | Documentación interactiva de la API mediante HTTPS. |
| `/health` | Estado general de la aplicación. |
| `/health/live` | Comprueba que la aplicación esté activa. |
| `/health/ready` | Comprueba la disponibilidad de la base de datos. |
| `/health-ui` | Interfaz visual de Health Checks UI. |

**5\. Servicios y endpoints**

A continuación se detallan las operaciones disponibles en cada dominio.

### **5.1. Products**

Prefijo: `/api/products`

| Método | Endpoint | Descripción | Respuestas |
| ----- | ----- | ----- | ----- |
| GET | `/api/products` | Lista productos. Permite filtrar por categoría y nombre. | 200, 500 |
| GET | `/api/products/{id}` | Obtiene un producto por su identificador. | 200, 404, 500 |
| POST | `/api/products` | Registra un producto nuevo. | 201, 400, 409, 500 |
| PUT | `/api/products/{id}` | Actualiza un producto existente. | 200, 400, 404, 409, 500 |
| DELETE | `/api/products/{id}` | Elimina un producto si no tiene órdenes activas asociadas. | 204, 404, 409, 500 |

### **5.2. Users**

Prefijo: `/api/users`

| Método | Endpoint | Descripción | Respuestas |
| ----- | ----- | ----- | ----- |
| POST | `/api/users/register` | Registra un usuario nuevo. | 201, 400, 409, 500 |
| POST | `/api/users/login` | Autentica al usuario y controla los intentos fallidos. | 200, 400, 401, 403, 500 |

Las respuestas no exponen información sensible como `PasswordHash`.

### **5.3. Orders**

Prefijo: `/api/orders`

| Método | Endpoint | Descripción | Respuestas |
| ----- | ----- | ----- | ----- |
| GET | `/api/orders` | Lista órdenes. Permite filtrar por `usuarioId`. | 200, 500 |
| GET | `/api/orders/{id}` | Obtiene una orden con sus ítems. | 200, 404, 500 |
| POST | `/api/orders` | Crea una orden validando usuario, productos y stock. | 201, 400, 404, 409, 422, 500 |
| PUT | `/api/orders/{id}/status` | Actualiza el estado de una orden. | 200, 400, 404, 409, 500 |

### **5.4. Cart**

Prefijo: `/api/cart`

| Método | Endpoint | Descripción | Respuestas |
| ----- | ----- | ----- | ----- |
| GET | `/api/cart/{userId}` | Obtiene el carrito de un usuario. | 200, 404, 500 |
| POST | `/api/cart/{userId}/items` | Agrega un producto al carrito. Si ya existe, acumula la cantidad. | 200, 400, 404, 422, 500 |
| PUT | `/api/cart/{userId}/items/{productId}` | Actualiza la cantidad de un producto. | 200, 400, 404, 422, 500 |
| DELETE | `/api/cart/{userId}/items/{productId}` | Elimina un producto del carrito. | 204, 404, 500 |
| DELETE | `/api/cart/{userId}` | Vacía el carrito. | 204, 404, 500 |

### **5.5. Notifications**

Prefijo: `/api/notifications`

| Método | Endpoint | Descripción | Respuestas |
| ----- | ----- | ----- | ----- |
| POST | `/api/notifications/send` | Registra una notificación y simula su envío. | 201, 400, 404, 500 |
| GET | `/api/notifications/{userId}` | Consulta el historial de notificaciones de un usuario. | 200, 404, 500 |

Los tipos de notificación admitidos son `Email`, `Push` y `SMS`.

## **6\. Contrato y catálogo de errores**

La API utiliza un mecanismo centralizado para gestionar las excepciones y mantener un formato uniforme en las respuestas de error.

Los manejadores implementan `IExceptionHandler` y delegan la construcción de la respuesta en `ErrorResponseWriter`.

Las respuestas siguen el formato Problem Details (`application/problem+json`) e incorporan los campos propios del proyecto:

```
{
  "type": "about:blank",
  "title": "Not Found",
  "status": 404,
  "detail": "Descripción general del error.",
  "instance": "/api/products/00000000-0000-0000-0000-000000000099",
  "errorCode": "PRD-001",
  "errorMessage": "No se encontró el producto solicitado.",
  "correlationId": "10a0faf0-0047-49e5-9243-1674673ade4a"
}
```

El contenido de `type`, `title`, `detail` e `instance` puede variar según la excepción y el endpoint. Los campos `errorCode`, `errorMessage` y `correlationId` permiten identificar el error concreto y relacionarlo con los registros de la aplicación.

### **Catálogo de códigos de error**

| Código | HTTP | Descripción |
| ----- | ----- | ----- |
| PRD-001 | 404 | Producto no encontrado. |
| PRD-002 | 400 | Datos del producto inválidos. |
| PRD-003 | 409 | Producto duplicado dentro de la misma categoría. |
| PRD-004 | 409 | No se puede eliminar un producto asociado a órdenes activas. |
| PRD-005 | 500 | Error interno al procesar productos. |
| USR-001 | 409 | El email ya se encuentra registrado. |
| USR-002 | 400 | Datos del usuario inválidos. |
| USR-003 | 401 | Credenciales incorrectas. |
| USR-004 | 403 | Cuenta bloqueada por intentos fallidos. |
| USR-005 | 403 | Cuenta suspendida por razones de seguridad. |
| USR-006 | 500 | Error interno al procesar usuarios. |
| ORD-001 | 404 | Orden no encontrada. |
| ORD-002 | 400 | Datos de la orden inválidos. |
| ORD-003 | 404 | Usuario inexistente o inactivo al crear una orden. |
| ORD-004 | 404 | Producto inexistente al crear una orden. |
| ORD-005 | 422 | Stock insuficiente para completar la orden. |
| ORD-006 | 409 | Transición de estado no permitida. |
| ORD-007 | 500 | Error interno al procesar órdenes. |
| CRT-001 | 404 | El usuario no posee un carrito activo. |
| CRT-002 | 404 | Producto inexistente o no perteneciente al carrito. |
| CRT-003 | 422 | Stock insuficiente para la cantidad solicitada. |
| CRT-004 | 400 | Cantidad o datos de entrada inválidos. |
| CRT-005 | 500 | Error interno al procesar el carrito. |
| NTF-001 | 404 | Usuario destinatario no encontrado. |
| NTF-002 | 400 | Datos de notificación inválidos. |
| NTF-003 | 404 | El usuario existe, pero no tiene notificaciones registradas. |
| NTF-004 | 500 | Error interno al procesar notificaciones. |

### **Manejo de errores de formato**

Las solicitudes con datos inválidos o JSON malformado se gestionan mediante las validaciones correspondientes y el manejador `BadRequestExceptionHandler`, cuando corresponda al código de error del dominio.

Los errores inesperados son tratados por el manejador global, que evita exponer detalles internos de la aplicación en las respuestas al cliente.
## **7\. Reglas de negocio y seguridad**

### **Usuarios y contraseñas**

* Las contraseñas no se almacenan en texto plano.  
* Se utiliza PBKDF2 con SHA-256 y un salt criptográfico aleatorio para generar los hashes.  
* La comparación de hashes utiliza `CryptographicOperations.FixedTimeEquals`.  
* Los DTOs de respuesta no incluyen `PasswordHash`.  
* Luego de tres intentos fallidos consecutivos, la cuenta se bloquea.  
* Si el inicio de sesión es correcto, el contador de intentos fallidos vuelve a cero.  
* Se diferencian las credenciales incorrectas, las cuentas bloqueadas automáticamente y las cuentas suspendidas.

### **Productos**

* El nombre de un producto debe ser único dentro de su categoría.  
* El precio debe ser mayor que cero.  
* El stock no puede ser negativo.  
* No se permite eliminar un producto que esté asociado a órdenes pendientes o confirmadas.

### **Órdenes**

* Para crear una orden se valida que el usuario exista y esté activo.  
* Se comprueba la existencia de cada producto y la disponibilidad de stock.  
* El precio unitario se obtiene del producto registrado en el sistema, no del valor enviado por el cliente.  
* El total se calcula a partir de las cantidades y los precios unitarios.  
* La creación de la orden y sus ítems se realiza dentro de una transacción para mantener la integridad de los datos.  
* Las transiciones de estado tienen que respetar las reglas de negocio definidas.

### **Carrito**

* Un usuario puede agregar productos a su carrito.  
* Si agrega un producto que ya existe, se acumula la cantidad.  
* Se valida que el producto exista y que haya stock suficiente.  
* Se permite actualizar cantidades y eliminar productos del carrito.

### **Notificaciones**

* Solo se aceptan los tipos `Email`, `Push` y `SMS`.  
* Se verifica que el usuario destinatario exista.  
* Se valida que el mensaje y los datos recibidos sean correctos.  
* Se registra el estado y la fecha de envío de la notificación.  
* Se puede ver el historial de notificaciones de un usuario.

## **8\. Observabilidad**

La aplicación incorpora mecanismos de trazabilidad, logging y monitoreo para facilitar el seguimiento de las solicitudes y detectar problemas durante la ejecución.

### **Correlation ID**

El middleware `CorrelationIdMiddleware` permite asociar un identificador único a cada solicitud HTTP.

* Si la solicitud incluye el header `X-Correlation-Id`, se utiliza ese valor.  
* Si no lo incluye, se genera un nuevo identificador.  
* El valor se devuelve en el header de respuesta.  
* También se incorpora en las respuestas de error y en los registros de Serilog.

Esto permite relacionar una solicitud con los eventos registrados durante su procesamiento.

### **Logging con Serilog**

Se utiliza Serilog para registrar eventos.

Los registros contemplan información como:

* Fecha y hora.  
* Nivel del evento.  
* Servicio.  
* Endpoint.  
* Correlation ID.  
* Código de error, cuando corresponde.

Se utilizan distintos destinos de logging:

* **Consola:** muestra los eventos durante la ejecución.  
* **Auditoría:** registra información de las solicitudes HTTP.  
* **Archivo JSON:** guarda eventos estructurados para facilitar su análisis.

Los registros de auditoría y los archivos JSON se generan en el directorio `logs/`, según la configuración de Serilog.

### **Health Checks**

La aplicación dispone de endpoints para comprobar su estado:

| Endpoint | Finalidad |
| ----- | ----- |
| `/health` | Consulta general del estado de la aplicación. |
| `/health/live` | Comprueba que el proceso se encuentre activo. |
| `/health/ready` | Verifica la disponibilidad de SQLite. |
| `/health-ui` | Presenta el estado mediante una interfaz visual. |
## **9\. Evidencias de Swagger**

En el directorio `MiniApi/docs/swagger/` se incluyen capturas de la documentación interactiva y de distintos escenarios de respuesta de la API.

| Archivo | Escenario | Código |
| ----- | ----- | ----- |
| `01-swagger-vista-general.jpg` | Vista general de Swagger y los módulos. | OpenAPI |
| `02-PRD-001-producto-no-encontrado-404.jpg` | Consulta de un producto inexistente. | 404 |
| `03-PRD-003-producto-duplicado-409.jpg` | Registro de un producto duplicado. | 409 |
| `04-PRD-004-producto-con-ordenes-activas-409.jpg` | Eliminación de un producto asociado a órdenes activas. | 409 |
| `05-USR-001-email-duplicado-409.jpg` | Registro de un email existente. | 409 |
| `06-USR-003-credenciales-incorrectas-401.jpg` | Inicio de sesión con credenciales incorrectas. | 401 |
| `07-USR-004-usuario-bloqueado-403.jpg` | Bloqueo por intentos fallidos. | 403 |
| `08-ORD-005-stock-insuficiente-422.jpg` | Creación de una orden sin stock suficiente. | 422 |
| `09-ORD-006-transicion-invalida-409.jpg` | Transición de estado no permitida. | 409 |
| `10-CRT-003-stock-insuficiente-carrito-422.jpg` | Agregado de un producto sin stock suficiente. | 422 |
| `11-NTF-001-usuario-no-encontrado-404.jpg` | Envío de una notificación a un usuario inexistente. | 404 |

Estas capturas documentan escenarios seleccionados de la API. Para probar otras operaciones y respuestas se pueden utilizar Swagger UI o el archivo `MiniApi/MiniApi.http`.

## **10\. Decisiones de diseño**

### **Organización modular**

Se separaron los cinco dominios en carpetas y componentes propios para mantener las responsabilidades organizadas. La implementación se realizó dentro de una única API, en lugar de desplegar cinco procesos independientes.

### **Minimal APIs**

Se utilizaron Minimal APIs para definir las rutas HTTP. Los endpoints reciben las solicitudes, delegan la lógica en los servicios y construyen las respuestas correspondientes.

### **Patrón Repository**

El acceso a datos se separó de la lógica de negocio mediante repositorios. De esta forma, los servicios no necesitan ejecutar directamente las consultas SQL.

### **Dapper y SQLite**

Se eligieron Dapper y SQLite para resolver la persistencia con consultas SQL explícitas y parametrizadas.

Debido a que SQLite no dispone de un tipo nativo `Guid`, los identificadores se guardan como texto. Se utiliza `GuidTypeHandler` para facilitar el mapeo entre los tipos de .NET y SQLite mediante Dapper.

### **Manejo centralizado de excepciones**

Implementamos excepciones y manejadores específicos para los distintos tipos de errores. Esto permite mantener el mismo formato de respuesta sin repetir bloques `try/catch` en cada endpoint.

### **Documentación Swagger**

Se utiliza Swagger/OpenAPI para describir las rutas, los parámetros, las respuestas HTTP y ejemplos de solicitudes y errores. Los filtros de operación nos dejan incorporar ejemplos específicos sin sobrecargar los archivos de endpoints.

## **11\. Tecnologías utilizadas**

| Propósito | Tecnología |
| ----- | ----- |
| Framework | .NET 10 / ASP.NET Core |
| API | Minimal APIs |
| Base de datos | SQLite |
| Acceso a datos | Dapper y Microsoft.Data.Sqlite |
| Documentación | Swashbuckle.AspNetCore / OpenAPI |
| Logging | Serilog |
| Validaciones | Data Annotations y validaciones de negocio |
| Seguridad de contraseñas | PBKDF2 y SHA-256 |
| Manejo de errores | `IExceptionHandler` y ProblemDetails |
| Monitoreo | Health Checks / HealthChecks UI |
| Integración continua | GitHub Actions |
| Control de versiones | Git y GitHub |

## **12\. Integrantes**

**Grupo 10**

* **Carolina Pintor**  
* **Jonas Pesco** 
* **Tomás Galacho**

---

**Repositorio:** [CarolinaPintor/Grupo10](https://github.com/CarolinaPintor/Grupo10)
