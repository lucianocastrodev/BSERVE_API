using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSERVE_API.Models
{
    public class Produto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título não pode ser vazio")]
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço de compra deve ser maior que zero")]
        public decimal PrecoCompra { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço de venda deve ser maior que zero")]
        public decimal PrecoVenda { get; set; }
        public int Estoque { get; set; }
        public string? CodigoBarras { get; set; }
    }
}