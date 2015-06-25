# TagParser

A tag-formatted document parser,mainly used to process HTML documents.

## Getting started

```cs
using (TextReader reader = File.OpenText(@"C:\Test.html"))
{
    var parser = new TagParser(new ParseReader(reader));
    
    ParseToken token;
    while ((token = parser.GetNextToken()) != null)
    {
        Console.WriteLine(token.GetType().Name);
    }
}
```
