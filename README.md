
[css:main.css]

# stasigen - static site generator

stasigen is short for static site generator. And yes, this is yet another static site generator.
There seems to be an infinite number of these things, so here is another one.

## Why I wrote this

The main purpose for this generator is to help myself document things during work where I mostly use a text editor for taking notes.
Occasionally I have some screenshots laying around which I want to combine with the notes. So I could either use something like Microsoft Word (no thanks) or a Website editor like "nvu", "Dreamweaver" or alike. But these seem to big for the task and I like it simple - so I created stasigen.

With stasigen you can create a folder structure without any limitations, place your Markdown files, put some images here and there.
To link files with images stasigen uses its own "commands" which get replaced with some values.

### Example 1:

``` 
[css:main.css]
```

This command inserts the HTML CSS stylesheet tag for the given stylesheet.

```
<link rel="stylesheet" type="text/css" href="../../dh/css/main.css">
```

### Example 2:

``` 
[css:logo.png]
```

This command inserts the HTML img tag for the given image.

```
<img src="../../dh/img/logo.png" alt="logo.png" /></p>

```

## Features

- Stupid simple. Creates HTML files out of Markdown files.
- Open source, .NET Core Framework, written in C#
- Dynamic image and css file integration

## Demo run

``` 
stasigen generate ~/dh/Websites/static
```

## Thanks

Thanks go out to:

- xooxf the author of [Markdig](https://github.com/xoofx/markdig) the markdown parser I used.

## Compile / requirements

This application requires mono or the .NET Framework 5 Core.

```
dotnet build && dotnet run
```

## Todos?

- Command? $navbar
- Command? $posts.last(10)
- Command? $posts.random(100)