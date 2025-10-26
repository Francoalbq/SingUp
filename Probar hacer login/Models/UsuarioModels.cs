using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Probar_hacer_login.Models
{
    public class UsuarioModels
    {
        [Required(ErrorMessage = "El Campo Email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del Email no es válido")]
        [StringLength(50, ErrorMessage = "El Email no puede exceder los 50 caracteres")]
        public string? Email { get; set; }



        [Required(ErrorMessage = "El Campo Contraseña es obligatorio")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 50 caracteres")]
        [DataType(DataType.Password)]
        public string? Contraseña { get; set; }


        [Required(ErrorMessage = "El Campo Documento es obligatorio")]
        [StringLength(20, ErrorMessage = "El Documento no puede exceder los 20 caracteres")]
        public string? Documento { get; set; }



        [Required(ErrorMessage = "El Campo Nombre es obligatorio")]
        [StringLength(20, ErrorMessage = "El Nombre no puede exceder los 20 caracteres")]
        public string? Nombre { get; set; }



        [Required(ErrorMessage = "El Campo Apellido es obligatorio")]
        [StringLength(20, ErrorMessage = "El Apellido no puede exceder los 20 caracteres")]
        public string? Apellido { get; set; }



        [NotMapped]
        [Compare("Contraseña", ErrorMessage = "La Contraseña y la Confirmación de Contraseña no coinciden.")]
        [DataType(DataType.Password)]
        public string? ConfirmarContraseña { get; set; }



    }
}
