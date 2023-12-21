# DoCMovieTool
This small tool allows you to unpack and repack Dirge of Cerberus game's movie archive files. the program should be launched from command prompt with a few argument switches to perform a function. 
<br><br>The list of valid argument switches are given below:
<br>``-u`` To unpack a movie archive file
<br>``-r`` To repack an unpacked movie archive folder
<br>
<br>After specifying either one of the above switches, specify the archive file path or the extracted archive folder path.


# Info
## General Info
- This tool will only allow you to replace the existing files inside a movie archive.
  
- Valid movie files will have a `.pss` extension and the video alone can be played in media players that support the mpeg format. if you want the video to play with the audio, then the pss file would have to be demultiplexed with pssplex tool which will split the video as a `.m2v` file and the audio as a `.wav` file. you can then play the video and audio together in a media player that supports the playback of two media files at the same time. 
## Important Info
- Do not rename the movie archive file or the unpacked folder. the tool uses the archive file and the unpacked folder names to use an appropriate set of keys for decrypting and encrypting processes.
  
- Do not repack any of the PAL (EU region) version's movie files into NTSC-U (USA region) or NTSC-J (Japan / Japan international region) version's archive files. the game is strict about the movie file's framerate during runtime and as the PAL region movie files are encoded at 25.000fps, the game will get stuck in a black screen when it tries playing this file. you can freely interchange movie files between NTSC-U and NTSC-J versions of the game as the movie files in these two versions run at the same framerate.

- Do not remove any of the UNK files extracted along with the movie files as they are required for the repacking process.
