# DotExperiment

Dot experiments is a list of over engineered functions in dotnet.

## Expressions

A set of functions to convert and rewrite expressions. This allow users to transform expression's parameters & return type and rewrite accessors.

Expression Convertor Usage:
```csharp
Expression<Func<int, bool>> func = x => x > 100;
Expression<Func<long, bool>> converted = ExpressionUtility.Convert<Func<long, bool>>(func);
```

Expression Rewrite Usage:
```csharp
public class Parent
{
    public ICollection<GenreLink> GenreLinks { get; set; }
    
    [ExpressionRewrite(nameof(GenreLinks), nameof(GenreLink.Genre))]
    public ICollection<Genre> Genres
    {
        get => GenreLinks?.Select(x => x.Genre);
        set => GenreLinks = value?.Select(x => new GenreLink(this, x)).ToList();
    }
}

public class Genre 
{
    public bool Enabled { get; set; }
}

public class GenreLink
{
    public Parent Parent { get ; set; }
    public Genre Genre { get; set; }
    
    public GenreLink(Parent parent, Genre genre)
    {
        Parent = parent;
        Genre = genre;
    }
}

Expression<Func<Parent, ICollection<Genre>>> = x => x.Genres.Any(y => y.Enabled);
Expession rewrited = Expression.ExpressionRewrite(expression);
// rewrited contains: x => x.GenreLinks.Any(y => y.Genre.Enabled);
```