# cge-tools

## Summary

A collection of tools that allows the modification (and translation) of cge/cge2-games, namely Sołtys and Sfinx.

## Usage "cft-converter"

This tool is used to convert fonts for further editing. The process involves the conversion of the font to png first, editing afterwards and conversion back to ".cft".

You can get a help description with "cft-converter --help".

In order to convert a font to png, you need to use:

```
cft-converter --input C:\soltys\cge.cft --output C:\soltys\font.png
```

You will get a png that looks like this:

![Font PNG](https://abload.de/img/cge-font-edit5pfw6.jpg)

There are a couple of things to notice.

- The background color #8fd4ff is interpreted as transparent
- The boundaries of a glyph are fixed and can never exceed 6px x 8px (W x H)
- White (#ffffff) is interpreted as a "spacer", so you should always have one white "line" after your glyph
- Black (#000000) is interpreted as "it should be drawn"

Please stay with the previously mentioned colors and disable any unsharpness / antialiasing that your favorite image editing software might introduce.

When you are done with editing, you may invoke the program like this

```
cft-converter --input "C:\soltys\font.png" --output "C:\soltys\cge.cft"
```

This will convert the font image back to the native CGE format.

## Usage "vbm-converter"

This tool is used to convert CGE images to png and back. It is 100% compatible with Sołtys but there are issues with fullscreen images (320x200) in Sfinx, as it will not render correctly in ScummVM. This is still an issue to be solved.

You can get a help description with "vbm-converter --help".

### Step 1: Extract palettes

This is required because the tool uses the embedded palettes in those fullscreen images (320x200) for rendering the sprites. It will write .act files to the output directory.

.act files can be used within Photoshop for example.

```
cft-converter pal --input "C:\soltys" --output "C:\soltys_pal"
```

### Step 2: Convert to .png

There are a couple of things to consider. First, you need to supply a fallback palette if the image does not embed it. For Sołtys, this should be "welcome.act". For Sfinx, you should use "01.act". This will still not be 100% correct for all cases.

The process will also create .mct files along the png's. Don't delete them! They are required for the conversion back to .vbm and contain the used colors and which position in the palette they represent.

- "input" refers to the path where the .vbm files are. This can also be a single file
- "output" refers to the path where the .png files are stored. It has to be a directory for single and batch processing
- "palette-path" is the path where the previously extracted .act files are stored
- "g" or "game" refers to the game that you are going to convert from. There are slight differences in palette handling between both.

Here is an example with batch processing and fallback palette:

```
vbm-converter png --fallback-palette "C:\soltys_pal\welcome.act" --input "C:\soltys" --output "C:\soltys_out" --palette-path "C:\soltys_pal" -g Soltys
```

Here is an example with single file processing and forced palette:

```
vbm-converter png --force-palette "C:\soltys_pal\welcome.act" --input "C:\soltys\welcome.vbm" --output "C:\soltys_out" -g Soltys
```

So why is it important to define the "palette-path"? The tool uses a filename-pattern-matching in order to find the correct palette for a file.

For example, if the file is named "03drzwi0.vbm" it will search for palettes in the following order:

- If "force-palette" is enabled, use the file that is defined
- If a palette is embedded in the .vbm file, use it
- Look for an .act file which matches the file name. In that case, "03.act" would match "03drzwi0.vbm" because the .vbm file starts with the palette name.
- If nothing is found, use the palette defined as "fallback-palette"

### Step 3: Editing

Just use your favorite image tool to edit the files. The converter is strict when it comes to the palette. You have to stick to the predefined palette of colors as only for those it is guaranteed that they are loaded in the scene where the sprites are loaded.

### Step 4: Convert back to .vbm

Here is an example with batch processing and fallback palette:

```
vbm-converter vbm --input "C:\soltys_out" --output "C:\soltys_modified"
```

Here is an example with single file processing and forced palette:

```
vbm-converter vbm --input "C:\soltys_out\03drzwi0.png" --output "C:\soltys_modified"
```

In case you have to edit a full screen image (those with 320x200 dimensions), you need to embed the palette that the original file came with and which has been extracted in Step 1. Here is an example:

```
vbm-converter vbm --input "C:\soltys_out\welcome.png" --output "C:\soltys_modified" --embed-palette "C:\soltys_pal\welcome.act"
```

or another

```
vbm-converter vbm --input "C:\soltys_out\01.png" --output "C:\soltys_modified" --embed-palette "C:\soltys_pal\01.act"
```

# Credits

Credits go to this blog for the font conversion:
https://criezy.blogspot.com/2014/09/do-you-play-english-part-3.html

and to ScummVM which inspired the conversion routine for the images.