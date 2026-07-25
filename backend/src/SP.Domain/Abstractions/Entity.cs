namespace SP.Domain.Abstractions;

public abstract class Entity
{
    //important for ef core
    protected Entity (){}
    public Guid Id { get; init ; }
    protected Entity (Guid id)=> Id = id;
    
}