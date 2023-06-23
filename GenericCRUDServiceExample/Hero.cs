using MassTransit;

namespace GenericCRUDServiceExample;

public enum HeroType
{
    Student = 0,
    Teacher = 1,
    ProHero = 2,
    Villain = 3,
    Vigilante = 4
}

public class Hero : Entity<HeroId>
{
    public override HeroId Id { get; set; } = NewId.NextGuid();
    public string Name { get; set; } = null!;

    public string? Nickname { get; set; }
    public string? Individuality { get; set; } = null!;
    public int? Age { get; set; }

    public HeroType HeroType { get; set; }

    public string? Team { get; set; }
}