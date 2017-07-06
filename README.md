# BatchThat
An automated image processing application

## About BatchThat
BatchThat is the Washington State Archives' first open source project. We wanted to release software out to the public that would help others prepare scanned documents for digital preservation. This application is a bit rough around the edges but will evolve over time, it is our hope that the digital archiving community will help make this application better as well.

## What does BatchThat allow users to do?
BatchThat allows users to apply a set of manipulations to images, these manipulations or "filters" as we call them include:
- Auto Cropping
- Auto Deskewing
- Auto Enhancing
- Auto Greyscale
- Auto Sharpening
- Auto Trimming

## What file formats are supported?
Currently BatchThat has been tested with single page and multipage TIFF files. BatchThat *should* work most image formats but it has not been tested by us. BatchThat always outputs its work as single page or multipage tiff and preserves the name of the original file. As the application evolves, these constraints may (will likely) change.

## Do you have an example of how BatchThat works?
Below are two images, the first image is a scanned document we received from one of our volunteers that scanned a large amount of documents at the Washington State Archives. As you can see, this doucment is a bit skewed, has a large black background, and is a bit dull.

![Initial Document](https://raw.githubusercontent.com/washington-state-archives/batch-that/master/batch-that-inital.png)

Applying auto crop, deskew, enhance, sharpen, and trim with BatchThat we now have an image that is displayed below. We believe this is a large improvement over the initial image and it took little work for our software to create this for us.

![Initial Document](https://raw.githubusercontent.com/washington-state-archives/batch-that/master/batch-that-finished.png)
