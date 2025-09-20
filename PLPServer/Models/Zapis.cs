using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PLPServer.Models;

public class Zapis
{
    //[Key]
    public Guid Id { get; set; }

    [DataType(DataType.DateTime)]
    [Required]
    public required DateTime CasVoznje { get; set; }

    [Required]
    public required Pogodba Pogodba { get; set; }
}

public class Linija
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public required string NazivNarocnika { get; set; }


    public required ICollection<Pogodba> Pogodbe { get; set; }
}

[PrimaryKey(nameof(LinijaId), nameof(PrevoznikId))]
public class Pogodba
{
    public Guid LinijaId { get; set; }
    public Guid PrevoznikId { get; set; }

    public required Linija Linija { get; set; }
    public required  Prevoznik Prevoznik { get; set; }

    public required string Narocnik { get; set; } //TODO: naredi v pravega naročnika

    public double Znesek { get; set; }
}