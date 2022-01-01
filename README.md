
[css:main.css]

# stasigen - static site generator

stasigen is short for static site generator. And yes, this is yet another static site generator.
There seems to be an infinite number of these things, so here is another one.

## Why I wrote this

The main purpose for this generator is to help myself document things during work where I mostly use a text editor for taking notes.
Occasionally I have some screenshots laying around which I want to combine with the notes. So I could either use something like Microsoft Word (no thanks) or a Website editor like "nvu", "Dreamweaver" or alike. But these seem to big for the task and I like it simple - so I created stasigen.

With stasigen you can create a folder structure without any limitations, place your Markdown files, put some images here and there.
To link files with images stasigen uses variables which get replaced with some values.

### Example:

``` 
[css:main.css]
```

Gets replaced with the HTML css stylesheet tag for.

## Features

- Stupid simple. Creates HTML files out of Markdown files.
- Open source, .NET Core Framework, written in C#

## Demo run

``` 
stasigen generate ~/dh/Websites/static
```

## Thanks

Thanks go out to:

- xooxf the author of [Markdig](https://github.com/xoofx/markdig) the markdown parser I used.

## Documentation

will follow.

## Todos?

- Command? $navbar
- Command? $posts.last(10)
- Command? $posts.random(100)