namespace FamilyTree.Dtos;

public abstract class BaseDto
{
    public long Id { get; set; }

    public DateTime DateCreated { get; set; }

    public long? OwnerId { get; set; }

    public int? ItemOrder { get; set; }

    public bool Archived { get; set; }
}
