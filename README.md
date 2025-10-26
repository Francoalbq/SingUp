# 🔐 Sistema de Login y Registro con ASP.NET Core MVC + SQL Server + Autenticación con Google

Este proyecto implementa un **sistema de autenticación completo y seguro** utilizando **ASP.NET Core MVC** con **SQL Server** como base de datos y soporte para **autenticación externa con Google (OAuth 2.0)**.  
Además, las contraseñas se almacenan de manera cifrada usando **BCrypt**, garantizando la seguridad de los usuarios.

---

## 🧠 Objetivo del proyecto

El objetivo fue crear un sistema de login funcional y escalable que permita:
- Registrar usuarios nuevos en una base de datos SQL Server.
- Iniciar sesión con credenciales locales (email y contraseña).
- Iniciar sesión con cuentas de Google.
- Validar los datos de entrada con anotaciones de modelo.
- Mantener sesiones mediante cookies de autenticación.
- Almacenar contraseñas en formato **hash seguro (BCrypt)**.

---

## ⚙️ Tecnologías utilizadas

- **ASP.NET Core MVC 7.0**
- **C#**
- **Microsoft SQL Server**
- **ADO.NET**
- **BCrypt.Net-Next** (para cifrado de contraseñas)
- **Google OAuth 2.0** (para autenticación externa)
- **Razor Views (HTML, CSS, Bootstrap 5)**

---

## 📁 Estructura del proyecto

Probar_hacer_login/
│
├── appsettings.json # Configuración de conexión a SQL Server
├── Program.cs # Configuración de autenticación y middleware
│
├── Controllers/
│ └── LoginController.cs # Controlador principal de autenticación
│
├── Datos/
│ ├── Conexion.cs # Obtiene la cadena de conexión desde appsettings.json
│ └── UsuarioDatos.cs # Lógica de acceso a datos y procedimientos almacenados
│
├── Models/
│ └── UsuarioModels.cs # Modelo de datos y validaciones del usuario
│
└── Views/
└── Login/
├── Login.cshtml # Formulario de inicio de sesión
└── Registrar.cshtml # Formulario de registro de nuevos usuarios



---

## 🧩 Descripción técnica detallada

### 🧱 1. appsettings.json
Archivo de configuración donde se define la cadena de conexión a SQL Server:

```json
{
  "ConnectionStrings": {
    "CadenaSQL": "Data Source=(local); initial Catalog=login;Integrated Security=true;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
🔍 Esta configuración permite conectar de forma local con el servidor SQL sin necesidad de credenciales explícitas, usando autenticación integrada de Windows.



🔌 2. Conexion.cs
Encargada de leer y exponer la cadena de conexión.
Esto desacopla la configuración del código, facilitando el mantenimiento y la portabilidad.
public class Conexion
{
    private string cadenaSQL = string.Empty;

    public Conexion()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        cadenaSQL = builder.GetSection("ConnectionStrings:CadenaSQL").Value;
    }

    public string getCadenaSQL()
    {
        return cadenaSQL;
    }
}


👤 3. UsuarioModels.cs
Modelo de usuario con validaciones mediante Data Annotations.
Estas restricciones se aplican directamente sobre los formularios Razor (Registrar.cshtml y Login.cshtml).

public class UsuarioModels
{
    [Required(ErrorMessage = "El Campo Email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del Email no es válido")]
    [StringLength(50)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "El Campo Contraseña es obligatorio")]
    [StringLength(50, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string? Contraseña { get; set; }

    [Required(ErrorMessage = "El Campo Documento es obligatorio")]
    public string? Documento { get; set; }

    [Required(ErrorMessage = "El Campo Nombre es obligatorio")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El Campo Apellido es obligatorio")]
    public string? Apellido { get; set; }

    [NotMapped]
    [Compare("Contraseña", ErrorMessage = "La Contraseña y la Confirmación no coinciden.")]
    [DataType(DataType.Password)]
    public string? ConfirmarContraseña { get; set; }
}

💾 4. UsuarioDatos.cs

Esta clase maneja toda la comunicación con SQL Server usando ADO.NET y procedimientos almacenados.
Incluye tres métodos principales:

📍 Registrar()

Registra un nuevo usuario aplicando hasheo de contraseña con BCrypt antes de guardarla en la base de datos:
string contraseñaHasheada = BCrypt.Net.BCrypt.HashPassword(oUsuario.Contraseña);

Luego llama al procedimiento almacenado sp_RegistrarUsuario:

SqlCommand cmd = new SqlCommand("sp_RegistrarUsuario", conexion);
cmd.Parameters.AddWithValue("Email", oUsuario.Email);
cmd.Parameters.AddWithValue("Contraseña", contraseñaHasheada);
cmd.Parameters.AddWithValue("Documento", oUsuario.Documento);
cmd.Parameters.AddWithValue("Nombre", oUsuario.Nombre);
cmd.Parameters.AddWithValue("Apellido", oUsuario.Apellido);
cmd.CommandType = CommandType.StoredProcedure;
cmd.ExecuteNonQuery();

🔑 ValidarCredenciales()
Valida si el email existe y compara la contraseña ingresada con el hash almacenado usando BCrypt.Verify:

bool esCorrecta = BCrypt.Net.BCrypt.Verify(contraseña, oUsuario.Contraseña);
if (!esCorrecta)
{
    oUsuario = null;
}
Esto asegura que las contraseñas nunca se almacenen en texto plano.

🔎 ObtenerPorEmail()

Devuelve un usuario existente mediante una consulta SQL directa, útil para comprobar si ya está registrado (por ejemplo, desde el login de Google).


🧠 Procedimientos almacenados en SQL Server

sp_RegistrarUsuario
CREATE PROCEDURE sp_RegistrarUsuario
    @Email NVARCHAR(50),
    @Contraseña NVARCHAR(255),
    @Documento NVARCHAR(20),
    @Nombre NVARCHAR(20),
    @Apellido NVARCHAR(20)
AS
BEGIN
    INSERT INTO Usuario (Email, Contraseña, Documento, Nombre, Apellido)
    VALUES (@Email, @Contraseña, @Documento, @Nombre, @Apellido)
END


sp_ValidarUsuario
CREATE PROCEDURE sp_ValidarUsuario
    @Email NVARCHAR(50)
AS
BEGIN
    SELECT * FROM Usuario WHERE Email = @Email
END
🔒 Ambos procedimientos previenen inyección SQL al usar parámetros tipados (@Email, @Contraseña, etc.).



🔐 5. LoginController.cs
Controlador principal que maneja todo el flujo de autenticación:

-Registro e inicio de sesión tradicional.
-Inicio de sesión con Google.
-Creación y persistencia de cookies de sesión.

🔸 Registro local

Llama a UsuarioDatos.Registrar() y redirige al login si es exitoso.

🔸 Login local

Valida las credenciales mediante ValidarCredenciales() y redirige al Home si son correctas.

🔸 Google Login

Configura un Challenge de autenticación y recibe la respuesta en el callback.
Si el usuario no existe, lo registra automáticamente y crea una cookie persistente por 7 días:

var authProperties = new AuthenticationProperties()
{
    IsPersistent = true,
    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
};
await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme,
    new ClaimsPrincipal(claimsIdentity),
    authProperties);


🧭 6. Program.cs
Registra los servicios de autenticación y configura el middleware de Google:
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Google";
})
.AddCookie("Cookies")
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"];
    options.ClientSecret = builder.Configuration["Google:ClientSecret"];
});
|Esto permite iniciar sesión tanto con credenciales locales como con cuentas de Google.


🧱 7. Vistas Razor

🔸 Login.cshtml
Formulario simple que envía el email y la contraseña al controlador Login.

Incluye enlace para registrarse y opción de inicio con Google:
<a class="btn btn-outline-danger" href="/Login/GoogleLogin">
    Iniciar sesión con Google
</a>


🔸 Registrar.cshtml

Formulario de registro con validaciones del modelo.
Si la validación falla, se muestran mensajes de error mediante asp-validation-for.


🔒 Seguridad implementada

✅ Contraseñas cifradas con BCrypt (no reversibles).
✅ Validaciones en el lado del servidor (DataAnnotations).
✅ Autenticación persistente con Cookies.
✅ Prevención de inyección SQL mediante parámetros en los procedimientos almacenados.
✅ Sesión de usuario segura, válida por 7 días.


🚀 Ejecución del proyecto

-git clone https://github.com/tuusuario/Probar_hacer_login.git

-Crear la base de datos login en SQL Server.

-Ejecutar los procedimientos almacenados sp_RegistrarUsuario y sp_ValidarUsuario.

-Configurar tu cadena de conexión en appsettings.json.

-Ingresar tus credenciales de Google API (ClientId y ClientSecret) en appsettings.json.

