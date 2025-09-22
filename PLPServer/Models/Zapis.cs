using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PLPServer.Models;

public class Zapis
{
    [Key]
    public Guid Id { get; set; }
    public Guid PogodbaId { get; set; }

    [DataType(DataType.DateTime)]
    [Required]
    public DateTime ZacetekVoznje { get; set; }

    [DataType(DataType.DateTime)]
    [Required]
    public DateTime KonecVoznje { get; set; }

    [Required]
    public Pogodba? Pogodba { get; set; }
}

public class Linija
{
    [Key]
    public Guid Id { get; set; }

    public Guid NarocnikId { get; set; }

    [Required]
    public required string Ime { get; set; }

    [Required]
    public Narocnik? Narocnik { get; set; }
    public ICollection<Pogodba>? Pogodbe { get; set; }
}

//[PrimaryKey(nameof(LinijaId), nameof(PrevoznikId))]
public class Pogodba
{
    [Key]
    public Guid Id { get; set; }
    public Guid LinijaId { get; set; }
    public Guid PrevoznikId { get; set; }

    public Linija? Linija { get; set; }
    public Prevoznik? Prevoznik { get; set; }

    [Required]
    [DataType(DataType.Currency)]
    public required double Znesek { get; set; }


    public ICollection<Zapis>? Voznje { get; set; }
}