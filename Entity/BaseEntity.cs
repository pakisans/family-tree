using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyTree.Entity;

public abstract class BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateUpdated { get; set; }

    public bool Deleted { get; set; } = false;

    public bool Archived { get; set; } = false;

    public long? OwnerId { get; set; }

    public int? ItemOrder { get; set; } = 0;
}
