OrderTypes@Biller
==============
Plugin for Biller (V2) implementing the most common document types in a small business


Introduction
--------------
This repository is used as submodule in the original Biller V2 Repository (https://github.com/LastElb/BillerV2).
To build this project you need a few dependencies as stated below.

Dependencies
--------------
* Biller
* Biller.Controls
* Biller.Core
* Fluent
* MigraDoc.DocumentObjectModel.WPF
* MigraDoc.Rendering.WPF
* MigraDoc.RtfRendering.WPF
* PdfSharp.WPF
* PdfSharp.Rendering.WPF

How to use
--------------
Once you build the project copy the "OrderTypes@Biller.dll" to your Biller V2 executable directory and rename the extension to "bdll".
Biller will load the module automatically with the next start.