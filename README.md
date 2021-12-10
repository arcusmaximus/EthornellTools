# EthornellTools
Tools for the Buriko General Interpreter (BGI)/Ethornell visual novel engine.

**BgiDisassembler**: disassembles .\_bp script files.

**BgiImageEncoder**: encodes images to the engine's proprietary "CompressedBG" format. (To decode existing images, use e.g. [GARbro](https://github.com/morkt/GARbro/))

If you're looking to translate an Ethornell game, take a look at [VNTextPatch](https://github.com/arcusmaximus/VNTranslationTools).

## Script patching
Ethornell has two script formats, each with its own VM and instruction set:

* One format for internal system scripts (setting up the UI, executing scenarios...). These are
  more or less the same for every game. The VM is implemented in C++ in BGI.exe, and the scripts have
  extension .\_bp; it's these scripts that BgiDisassembler targets.

* Another format for scenario files, which contain the narration, dialogue, choices etc.
  specific to the game. This VM is implemented in scrmsg.\_bp, and the scenario files have no extension.
  Their text content can be extracted and patched using VNTextPatch.

### Fixing half-width text rendering
Ethornell games tend to make a mess when displaying half-width characters (making them overlap).
Interestingly enough, the engine is very much capable of displaying them correctly; you just need
to tell it to do so.

The steps are as follows:

* Disassemble all .\_bp scripts.

* Open the disassembly of scrmsg.\_bp, find the `graphcall 91:88` instruction (which calls the
  "ConfigureFormatInfo" system function), and look where its second argument from the bottom
  (bProportional) comes from. Then find the script which writes to this address and change
  the value to 1.
  
  For example: the second argument of the graphcall instruction is the byte at address 198FC+12928+6.
  Searching the folder of disassembled scripts for the text "12928" shows that userdata.\_bp writes
  to this address. Open the .\_bp file in a hex editor, go to the offset shown in the disassembly,
  and change the value loading instruction from `push 0` (`00 00`) to `push 1` (`00 01`).

* Find relevant `graphcall 92:9C` instructions (which call the "RenderText" system function) in
  bitmap.\_bp/scrmsg.\_bp (character name), logwnd.\_bp (backlog) and scrslct.\_bp (choice screen)
  and change their 9th argument (counting from the bottom) to 1.
  
### Changing font size
To change font size (for games with V1 scenario files, that is, files with the header "BurikoCompiledScriptVer1.00"):

* Optionally, disassemble the script that contains the scenario VM (scrmsg.\_bp), find the `graphcall 91:88` instruction,
  and confirm that it's called from scenario opcode 14C (which sets formatting options for the message window).
  To mark the scenario opcode handlers in the disassembly, you can uncomment the AssignOpcodeHandlerNames()
  function in the disassembler's Program.cs and change the `push 14FE0` in the regular expression to the
  correct address of the opcode handler table.

* Find the scenario file that calls this scenario opcode (e.g. "function") and change the second argument. For example:
```
  00 00 00 00 00 00 00 00    // push 0       <- bold
  00 00 00 00 1C 00 00 00    // push 1C      <- font size
  00 00 00 00 00 00 00 00    // push 0       <- font family
  4C 01 00 00                // set message window format
```
  Note that the opcode may appear multiple times with different font sizes: one for regular lines, one for whispering,
  one for yelling etc. If changing one font size has no effect ingame, try another.

* Find the two `graphcall 92:9C` instructions in logwnd.\_bp and change their 12th argument
  (counting from the bottom) to the new font size.
  
## Image patching
Most images can be extracted using GARbro and re-encoded with the "BgiImageEncoder" tool in this repository.
Some other images, however, are bitmap files with part of the header stripped (e.g. the GUI images in sysgrp.arc).
These can instead be converted to regular .bmp files using the [AE VN Tools](http://wks.arai-kibou.ru/ae.php) and then
edited using a tool capable of handling BMPs with an alpha channel (e.g. [Pixelformer](http://www.qualibyte.com/pixelformer/)).
Afterwards, simply remove the .bmp extension and you're done; Ethornell will accept the file even if the
full header is present.
