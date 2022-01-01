
[css:main.css]

# stasigen - static site generator

stasigen is short for static site generator. And yes, this is yet another static site generator.
There seems to be an infinite number of these things, so here is another one.

## Features

- Stupid simple. Creates HTML files out of Markdown files.
- Open source, .NET Core Framework, written in C#
- Dynamic image and css file integration

## Why I wrote this

The main purpose for this generator is to help myself document things during work where I mostly use a text editor for taking notes. Occasionally I have some screenshots laying around which I want to combine with the notes. So I could either use something like a word processor or a website editor like. But these seem to big for the task at hand and I like to keep it simple - so I created stasigen.

With stasigen you can create a folder structure without any restrictions to the application itself. That means you can place your Markdown files anythere you like. In your main documents folder or separated into sub folders for each topic. You can create a main image folder or put the images in the folders with your notes. Either way you can easily link them together without remembering any paths. stasigen does that for you.

You just have to declare the images like:

```
[img:filename_of_the_image.jpg]
```

stasigen will then look for that file and create the appropiate Markdown syntax for this. This happens accordingly to css stylesheets.

To see this in action look at the folder ["Examples\dh\"](https://github.com/dahead/stasigen/tree/master/Examples) and the "compiled" output ["Examples\output"](https://github.com/dahead/stasigen/tree/master/Output/dh) folder.

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
[img:logo.png]
```

This command inserts the HTML img tag for the given image.

```
<img src="../../dh/img/logo.png" alt="logo.png" /></p>

```

## Demo run

``` 
stasigen generate ~/dh/Websites/static
```

## Thanks

Thanks go out to:

- xooxf the author of [Markdig](https://github.com/xoofx/markdig) the markdown parser I used.

## Compilation

This application requires mono or the .NET Framework 5 Core.

```
dotnet build && dotnet run
```

## Todos?

- Command? $navbar
- Command? $posts.last(10)
- Command? $posts.random(100)